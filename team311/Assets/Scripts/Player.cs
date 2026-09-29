using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float jumpForce = 7f;
    public float fallMultiplier = 3.0f;
    public float lowJumpMultiplier = 2.0f;

    [Header("Hold Settings")]
    public Transform holdPoint;
    public float pickupRange = 1.5f;
    Box heldBox;

    [SerializeField] private KeyCode grabKey = KeyCode.E;

    [Header("Split Settings")]
    [Tooltip("切断された後に生成する上半身のプレハブ")]
    public GameObject upperBodyPrefab;
    [Tooltip("切断された後に生成する下半身のプレハブ")]
    public GameObject lowerBodyPrefab;

    bool isSplit = false;

    Rigidbody rb;
    Collider col; // 自身のコライダー
    bool isGrounded;
    bool isCrouching;

    // プレイヤーが操作出来るか
    private bool canMove = true;


    [Header("Ground Check")]
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer; // 地面とみなすレイヤー

    [Header("One Way Floor Settings")]
    [Tooltip("すり抜ける床に設定したレイヤーを選択してください")]
    public LayerMask oneWayFloorLayer;
    [Tooltip("足元からどれくらい下までRayを飛ばすか")]
    public float rayDistance = 0.3f;

    // しゃがみ時の設定
    float originalColliderHeight;
    Vector3 originalColliderCenter;
    float colliderBottomY; // コライダーの底面のローカルY座標

    // ジャンプ入力を物理ステップで処理するためのフラグ
    bool jumpRequested = false;
    // 特殊アクションの勢いを消さないためのタイマー
    private float specialActionTimer = 0f;

    // カウンター方式をシンプルに再定義
    private int jumpCount = 0;       // 今、空中ジャンプを何回消費したか
    private int maxAirJumpCount = 0; // 空中で追加でジャンプできる回数（通常は0回、空気入れで1回に）

    // --- カメラ固定用フィールド ---
    private Camera cachedCamera;
    private Vector3 cameraWorldOffset;

    [Header("Animation & Visual Settings")]
    [Tooltip("MAYAのモデル（見た目）のオブジェクトをここにドラッグ＆ドロップしてください")]
    public Transform visualTransform;

    // アニメーターを制御するための変数
    private Animator anim;

    private void Awake()
    {
        Debug.Log(
            $"Player Awake: {name}, Scene: {gameObject.scene.name}",
            this
        );
    }

    private void OnDestroy()
    {
        Debug.LogWarning(
            $"Player Destroy: {name}, Scene: {gameObject.scene.name}",
            this
        );
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // モデルについているAnimatorを自動で取得する
        anim = GetComponentInChildren<Animator>();

        // カメラのワールドオフセットをキャッシュ（プレイヤーのスケールに影響されない位置保持のため）
        cachedCamera = GetComponentInChildren<Camera>();
        if (cachedCamera == null) cachedCamera = Camera.main;
        if (cachedCamera != null)
        {
            cameraWorldOffset = cachedCamera.transform.position - transform.position;
        }

        // 当たり判定の初値を記録
        if (col is BoxCollider box)
        {
            originalColliderHeight = box.size.y;
            originalColliderCenter = box.center;
        }
        else if (col is CapsuleCollider cap)
        {
            originalColliderHeight = cap.height;
            originalColliderCenter = cap.center;
        }

        // 底面の位置を計算（ここを固定する）
        colliderBottomY = originalColliderCenter.y - (originalColliderHeight / 2f);
    }

    void LateUpdate()
    {
        // 毎フレーム、カメラのワールド位置をプレイヤー位置 + キャッシュしたオフセットに保つ
        if (cachedCamera == null)
        {
            cachedCamera = GetComponentInChildren<Camera>();
            if (cachedCamera == null) cachedCamera = Camera.main;
            if (cachedCamera == null) return;
            cameraWorldOffset = cachedCamera.transform.position - transform.position;
        }

        cachedCamera.transform.position = transform.position + cameraWorldOffset;
    }

    // プレイヤーの向き（1: 右, -1: 左）
    public int FacingDir { get; private set; } = 1;

    // しゃがみ状態を外部から参照できるように公開
    public bool IsCrouching => isCrouching;

    [Header("空気入れギミックの設定")]
    [SerializeField] private Vector3 normalScale = new Vector3(1, 1, 1); // 通常のサイズ
    [SerializeField] private Vector3 inflatedScale = new Vector3(1.5f, 1.5f, 1.5f); // 膨らんだサイズ
    [SerializeField] private float airDashSpeed = 150.0f; // 横ダッシュの速度

    private bool isInflated = false; // 膨らんでいるかどうかのフラグ

    void Update()
    {
        // 接地判定を毎フレーム実行
        CheckGrounded();

        // 追加：一方通行の床（すり抜け床）をRayで制御する処理
        HandleOneWayFloor();

        // 地面に着いていたら、空中ジャンプの消費数を「0」にリセットする
        if (isGrounded)
        {
            jumpCount = 0;
        }

        // 特殊アクション用のタイマーカウントダウン
        if (specialActionTimer > 0f)
        {
            specialActionTimer -= Time.deltaTime;
        }

        // --- 移動処理 ---
        float h = Input.GetAxis("Horizontal");

        if (canMove && specialActionTimer <= 0f)
        {
            Vector3 move = new Vector3(h, 0, 0) * moveSpeed;
            rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);

            if (h > 0.1f) FacingDir = 1;
            else if (h < -0.1f) FacingDir = -1;

            // プレイヤー自身（transform）ではなく、見た目（visualTransform）だけを回転させる
            if (visualTransform != null)
            {
                if (FacingDir == 1)
                {
                    visualTransform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                }
                else if (FacingDir == -1)
                {
                    visualTransform.localRotation = Quaternion.Euler(0f, -90f, 0f);
                }
            }

            // キー入力（h）が左右どちらかにあれば歩きアニメーションをON、なければOFF
            if (anim != null)
            {
                anim.SetBool("isWalking", Mathf.Abs(h) > 0.1f);
            }
        }
        else
        {
            if (anim != null)
                anim.SetBool("isWalking", false);
        }

        // ジャンプの入力判定
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                jumpRequested = true;
                //地上ジャンプ時にジャンプアニメーションを再生
                if (anim != null) anim.SetTrigger("doJump");
                AudioManager.PlaySE("Jump");
            }
            else if (jumpCount < maxAirJumpCount && specialActionTimer <= 0f)
            {
                AirJump();
            }
        }

        // --- スマートジャンプ（滞空をなくす） ---
        if (!isGrounded)
        {
            if (rb.linearVelocity.y < 0)
            {
                rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            }
            else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
            {
                rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }
        }

        // 空気入れ状態の時の「横ダッシュ（Shift）」
        if (isInflated)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                AirDash();
            }
        }

        // ==========================================
        // 🛠️ デバッグ機能（テスト用）
        // ==========================================

        // 【Bキー】強制クリア（GoalSceneへ遷移）
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("【デバッグ】Bキーが押されたため強制クリアを実行します。");
            UnityEngine.SceneManagement.SceneManager.LoadScene("GoalScene");
        }

        // 【Rキー】シーンリトライ（現在のステージを再読み込み）
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("【デバッグ】Rキーが押されたため現在のシーンを再読み込みします。");
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
        }
    }

    void FixedUpdate()
    {
        // 通常ジャンプの物理実行
        if (jumpRequested)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            jumpRequested = false;
        }
    }

    // 足元からのRayで一方通行の床を制御する関数
    void HandleOneWayFloor()
    {
        if (col == null) return;

        // プレイヤーのコライダーのワールド底面（足元）の位置を計算
        float bottomWorldY = col.bounds.min.y;
        Vector3 footPos = new Vector3(transform.position.x, bottomWorldY + 0.01f, transform.position.z);

        // 足元から真下に向けてRay（光線）を飛ばす
        RaycastHit hit;
        bool rayHitFloor = Physics.Raycast(footPos, Vector3.down, out hit, rayDistance, oneWayFloorLayer);

        // デバッグ用にエディタ上でRayを可視化（当たったら緑、外れたら赤）
        Debug.DrawRay(footPos, Vector3.down * rayDistance, rayHitFloor ? Color.green : Color.red);

        if (rayHitFloor)
        {
            Collider floorCollider = hit.collider;

            // プレイヤーが「下に落ちてきている（着地体制）」かつ「足元が床の上面より高い位置にある」ときだけ乗れる
            if (rb.linearVelocity.y <= 0.1f && footPos.y >= hit.point.y - 0.05f)
            {
                // 当たり判定をONにする（衝突無視を解除）
                Physics.IgnoreCollision(col, floorCollider, false);
            }
            else
            {
                // それ以外（ジャンプで上昇中など）はすり抜ける
                Physics.IgnoreCollision(col, floorCollider, true);
            }
        }
    }

    // 空気入れから呼び出される、膨らむ関数
    public void Inflate()
    {
        if (isInflated) return;

        isInflated = true;
        maxAirJumpCount = 1;
        transform.localScale = inflatedScale;
        Debug.Log("プレイヤーが膨らんだ！空中2段ジャンプ or Shiftダッシュが解禁！");
    }

    private void AirJump()
    {
        if (rb != null)
        {
            specialActionTimer = 0.3f;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * 14f, ForceMode.Impulse);
            jumpCount++;
        }
        AudioManager.PlaySE("Jump");
        Deflate();
    }

    private void AirDash()
    {
        if (rb != null)
        {
            specialActionTimer = 1.0f;
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            Vector3 dashDirection = Mathf.Abs(horizontalInput) > 0.1f
                ? new Vector3(horizontalInput, 0f, 0f).normalized
                : transform.forward;

            rb.linearVelocity = new Vector3(dashDirection.x * airDashSpeed, 2f, dashDirection.z * airDashSpeed);
        }
        Deflate();
    }

    private void Deflate()
    {
        isInflated = false;
        maxAirJumpCount = 0;
        transform.localScale = normalScale;
        Debug.Log("空気が抜けて元に戻った。");
    }


    public void Split()
    {
        if (isSplit) return;
        isSplit = true;


        Camera cam = GetComponentInChildren<Camera>();

        if (upperBodyPrefab != null)
        {
            GameObject upper = Instantiate(upperBodyPrefab, transform.position + Vector3.up * 0.5f, transform.rotation);
            Rigidbody upperRb = upper.GetComponent<Rigidbody>();
            if (upperRb) upperRb.AddForce(Vector3.up * 2f, ForceMode.Impulse);

            if (cam != null)
            {
                cam.transform.SetParent(upper.transform);
                cam.transform.localPosition = new Vector3(0, 2, -5);
                cam.transform.localRotation = Quaternion.Euler(15, 0, 0);
            }
        }

        if (lowerBodyPrefab != null)
        {
            GameObject lower = Instantiate(lowerBodyPrefab, transform.position, transform.rotation);
        }

        if (cam != null && cam.transform.parent == transform)
        {
            cam.transform.SetParent(null);
        }

        Destroy(gameObject);
    }

    void CheckGrounded()
    {
        if (col == null)
        {
            isGrounded = false;
            return;
        }

        // ワールド空間でのコライダー底面の位置を使う（transform とローカル値のズレ対策）
        float bottomWorldY = col.bounds.min.y;
        Vector3 footPos = new Vector3(transform.position.x, bottomWorldY + 0.01f, transform.position.z);

        // groundLayer が未設定（0）のときは全レイヤーをチェックする
        int layerMask = (groundLayer == 0) ? ~0 : (int)groundLayer;

        Collider[] cols = Physics.OverlapSphere(footPos, groundCheckRadius, layerMask);

        bool lastGrounded = isGrounded;
        isGrounded = false;
        foreach (var c in cols)
        {
            if (c.gameObject != gameObject && !c.isTrigger && !c.name.Contains("Visual") && !c.name.Contains("Hand"))
            {
                isGrounded = true;
                break;
            }
        }

        if (isGrounded != lastGrounded)
        {
            Debug.Log($"CheckGrounded: isGrounded changed -> {isGrounded} (overlapCount={cols.Length})");
        }

        Debug.DrawLine(footPos, footPos + Vector3.up * 0.1f, isGrounded ? Color.green : Color.red, 0.1f);
        Debug.DrawRay(transform.position, Vector3.up * (originalColliderHeight * 0.8f), Color.yellow, 0.1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // Gizmosにワールド底面位置を描画
        if (col != null)
        {
            Vector3 footPos = new Vector3(transform.position.x, col.bounds.min.y + 0.01f, transform.position.z);
            Gizmos.DrawWireSphere(footPos, groundCheckRadius);
        }

        Gizmos.color = Color.yellow;
        Vector3 checkPos = transform.position + transform.forward * 0.5f;
        Gizmos.DrawWireSphere(checkPos, pickupRange);

        // 潰されている間、上のブロックチェック範囲をシーン上に表示する（緑=検出可、実際に色分けはしないが位置確認用）
        if (isSquashed)
        {
            Gizmos.color = Color.cyan;
            Vector3 halfExtents = new Vector3(
                preSquashExtents.x * squashCheckSizeMultiplier,
                squashCheckThickness * 0.5f,
                preSquashExtents.z * squashCheckSizeMultiplier
            );
            Vector3 topCenter = new Vector3(transform.position.x, transform.position.y + preSquashTopOffsetY, transform.position.z);
            Gizmos.matrix = Matrix4x4.TRS(topCenter, preSquashRotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }

    public bool IsGrounded
    {
        get { return isGrounded; }
    }
    public void SetMoveEnabled(bool enabled)
    {
        canMove = enabled;

        if (!enabled && rb != null)
        {
            // 横方向の移動を止める
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }




    // --- 省略されていた残りのメンバ変数や関数群 ---
    [Header("Squash Settings")]
    public float squashedScaleY = 0.2f;
    public float squashedScaleX = 2.0f;
    [Tooltip("潰されてから、上下のブロックチェックを始めるまでの待ち時間（秒）")]
    public float recoveryDelay = 8f;
    [Tooltip("上下のブロック判定で、元のコライダーの横幅・奥行きに対してどれくらい余裕を持たせるか（1.0で元のサイズそのまま）")]
    public float squashCheckSizeMultiplier = 1.0f;
    [Tooltip("上下のブロック判定の厚み（薄すぎるとすり抜け判定になりやすいので0.1〜0.3程度を推奨）")]
    public float squashCheckThickness = 0.2f;
    [Tooltip("「挟んでいるブロック」とみなすレイヤー。未設定(Nothing)なら全レイヤーを対象にする")]
    public LayerMask squashBlockLayer;

    bool isSquashed = false;
    Vector3 originalScale;
    // 潰す直前（まだ元の大きさの時）の、コライダーの情報一式
    Vector3 preSquashCenter;
    Vector3 preSquashExtents;
    Quaternion preSquashRotation;
    float preSquashTopY;
    float preSquashBottomY;
    // 潰す前の「中心から頭上までの高さ」。これを現在位置に足すことで、
    // プレイヤーが動いてもちゃんと追従してチェックできるようにする。
    float preSquashTopOffsetY;
    private Coroutine squashRoutine;

    // 潰れた見た目にする。
    public void Squash()
    {
        if (isSquashed) return; // 見た目はすでに潰れているので、これ以上は何もしない

        // 潰れて縮む前に、今の（元の大きさの）コライダー情報を記録しておく。
        // 潰れた後はコライダーが縮んで隙間ができてしまうので、サイズ・厚みの基準は
        // 常にこの「元の状態」を使う。ただし位置は毎フレーム現在地を追従させる。
        if (col != null)
        {
            preSquashCenter = col.bounds.center;
            preSquashExtents = col.bounds.extents;
            preSquashRotation = transform.rotation;
            preSquashTopY = col.bounds.max.y;
            preSquashBottomY = col.bounds.min.y;
            preSquashTopOffsetY = preSquashTopY - transform.position.y;
        }

        isSquashed = true;
        isGrounded = true;

        Debug.Log("Player was squashed!");
        originalScale = transform.localScale;
        transform.localScale = new Vector3(
            originalScale.x * squashedScaleX,
            originalScale.y * squashedScaleY,
            originalScale.z
        );

        if (squashRoutine != null) StopCoroutine(squashRoutine);
        squashRoutine = StartCoroutine(SquashRecoverRoutine());
    }

    // 【互換用】PressSquash.cs等から呼ばれる可能性があるため残してあるが、
    // 回復判定は今はSquash()側のコルーチンで完全に自動化されているので、
    // このメソッド自体は何もしない。
    public void ReleaseSquash()
    {
    }

    // Recovery Delay秒待ってから、上下にブロックが無くなるまで待ち続け、
    // 無くなった瞬間に元のサイズへ戻す。
    private IEnumerator SquashRecoverRoutine()
    {
        yield return new WaitForSeconds(recoveryDelay);
        Debug.Log($"Squash: {recoveryDelay}秒経過。チェック開始時点でブロック有り={IsStillBlockedAbove()}");

        while (IsStillBlockedAbove())
        {
            yield return null;
        }

        Debug.Log("Squash: 上にブロックが無いと判定したので、これから元に戻します。");
        RevertSquash();
        squashRoutine = null;
    }

    // 潰す直前に記録した「元のコライダーの範囲」を基準に、今も上にブロックがあるかどうかを判定する
    // （下は見ない）。1点だけのチェックだと、プレス側の当たり判定と微妙にズレて取りこぼすことが
    // あるため、横幅・奥行き全体を薄い箱（CheckBox）でチェックする。
    private bool IsStillBlockedAbove()
    {
        int layerMask = (squashBlockLayer == 0) ? ~0 : (int)squashBlockLayer;

        Vector3 halfExtents = new Vector3(
            preSquashExtents.x * squashCheckSizeMultiplier,
            squashCheckThickness * 0.5f,
            preSquashExtents.z * squashCheckSizeMultiplier
        );

        Vector3 topCenter = new Vector3(transform.position.x, transform.position.y + preSquashTopOffsetY, transform.position.z);

        return Physics.CheckBox(topCenter, halfExtents, preSquashRotation, layerMask, QueryTriggerInteraction.Collide);
    }

    // 実際に元のサイズへ戻す処理
    private void RevertSquash()
    {
        transform.localScale = originalScale;
        isSquashed = false;
        isGrounded = true;
        Debug.Log("Player recovered from squash!");
    }

    public bool HasKey { get; private set; } = false;

    public void PickUpKey()
    {
        HasKey = true;
        Debug.Log("鍵をゲットした！");
    }

    public void UseKey()
    {
        HasKey = false;
        Debug.Log("鍵を使った！");
    }
}