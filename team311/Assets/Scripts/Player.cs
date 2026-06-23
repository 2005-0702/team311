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

    [Header("Ground Check")]
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer; // 地面とみなすレイヤー

    // しゃがみ時の設定
    float originalColliderHeight;
    Vector3 originalColliderCenter;
    float colliderBottomY; // コライダーの底面のローカルY座標

    // ジャンプ入力を物理ステップで処理するためのフラグ
    bool jumpRequested = false;
    // 特殊アクションの勢いを消さないためのタイマー
    private float specialActionTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // 当たり判定の初期値を記録
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

    // プレイヤーの向き（1: 右, -1: 左）
    public int FacingDir { get; private set; } = 1;

    // しゃがみ状態を外部から参照できるように公開
    public bool IsCrouching => isCrouching;

    //Player.cs の中に追加・統合するコード

    [Header("空気入れギミックの設定")]
    [SerializeField] private Vector3 normalScale = new Vector3(1, 1, 1); // 通常のサイズ
    [SerializeField] private Vector3 inflatedScale = new Vector3(1.5f, 1.5f, 1.5f); // 膨らんだサイズ
    [SerializeField] private float airDashSpeed = 15f; // 横ダッシュの速度

    private bool isInflated = false; // 膨らんでいるかどうかのフラグ

    void Update()
    {
        // 接地判定を毎フレーム実行
        CheckGrounded();

        // --- 移動処理 ---
        float h = Input.GetAxis("Horizontal");
        Vector3 move = new Vector3(h, 0, 0) * moveSpeed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);

        // --- しゃがみ処理 ---
        // Sキーが押されているか、あるいは頭上に障害物がある場合はしゃがみ状態を維持
        bool sKeyPressed = Input.GetKey(KeyCode.S);
        bool spaceAbove = !Physics.Raycast(transform.position, Vector3.up, originalColliderHeight * 0.8f, groundLayer == 0 ? ~0 : groundLayer);

        isCrouching = sKeyPressed || !spaceAbove;
        float visualScaleY = isCrouching ? 0.5f : 1.0f;

        // --- 向きとしゃがみのビジュアル反映 ---
        if (!isSquashed)
        {
            if (h > 0) FacingDir = 1;
            else if (h < 0) FacingDir = -1;

            foreach (Transform child in transform)
            {
                if (child.name.Contains("Visual"))
                {
                    child.localScale = new Vector3(FacingDir, visualScaleY, 1f);
                }
            }
        }

        // しゃがみに合わせて当たり判定のサイズを変える
        if (col != null)
        {
            float newHeight = originalColliderHeight * visualScaleY;
            // 底面(colliderBottomY)が変わらないように中心(center)を再計算
            float newCenterY = colliderBottomY + (newHeight / 2f);

            if (col is BoxCollider box)
            {
                box.size = new Vector3(box.size.x, newHeight, box.size.z);
                box.center = new Vector3(originalColliderCenter.x, newCenterY, originalColliderCenter.z);
            }
            else if (col is CapsuleCollider cap)
            {
                cap.height = newHeight;
                cap.center = new Vector3(originalColliderCenter.x, newCenterY, originalColliderCenter.z);
            }
        }

        // --- ジャンプ入力取得（物理ステップで処理） ---
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // フラグを立てて FixedUpdate で実際にジャンプを発生させる
            jumpRequested = true;
        }

        // --- スマートジャンプ（滞空をなくす） ---
        if (!isGrounded)
        {
            // 落下を速くする（滞空を消す）
            if (rb.linearVelocity.y < 0)
            {
                rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            }
            // 上昇を短く切る（ふわっと上がらない）
            else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
            {
                rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }
        }

        // --- 箱を持つ・離す処理（Eキー） ---
        if (Input.GetKeyDown(KeyCode.E))
        {
            HandleGrabDrop();
        }
        // 💡 膨らんでいる時だけ使える特殊アクションの入力チェック
        if (isInflated)
        {
            // パターンA：空中でもう一度ジャンプキー（Spaceなど）を押したら「2段ジャンプ」
            if (Input.GetButtonDown("Jump"))
            {
                AirJump();
            }
            // パターンB：Shiftキー（またはお好みのキー）を押したら「横ダッシュ」
            else if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                AirDash();
            }
        }
    }
    // 🎈 空気入れから呼び出される、膨らむ関数
    public void Inflate()
    {
        if (isInflated) return; // すでに膨らんでいたら重ならない

        isInflated = true;
        transform.localScale = inflatedScale; // 見た目を大きく（プレジ機で小さくした時の逆）
        Debug.Log("プレイヤーが膨らんだ！特殊アクションが1回使えます。");
    }
    // 💨 2段ジャンプを実行
    // 💡 既存の「地面にいるか」を管理している変数（IsGroundedなど）があれば、
    // 特殊ジャンプの瞬間にそれを一時的に false に書き換える必要があります。

    private void AirJump()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 通常移動を 0.3秒間 ストップさせる
            specialActionTimer = 0.3f;

            // 速度を一度リセットして、上方向に強烈な速度を直接ぶち込む
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 12f, rb.linearVelocity.z); // 12fは高さに合わせて調整してね
        }

        Deflate();
    }

    private void AirDash()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 通常移動を 0.25秒間 ストップさせる（この間は勢いよく飛ぶ）
            specialActionTimer = 0.25f;

            float horizontalInput = Input.GetAxisRaw("Horizontal");
            Vector3 dashDirection = Mathf.Abs(horizontalInput) > 0.1f
                ? new Vector3(horizontalInput, 0f, 0f).normalized
                : transform.forward;

            // 横方向に強烈な速度を直接ぶち込む（Y軸の重さは残すか、0fにして真横に飛ばす）
            rb.linearVelocity = new Vector3(dashDirection.x * airDashSpeed, 2f, dashDirection.z * airDashSpeed);
        }

        Deflate();
    }

    // 🧍 元の姿に戻る関数
    private void Deflate()
    {
        isInflated = false;
        transform.localScale = normalScale; // サイズを元に戻す
        Debug.Log("空気が抜けて元に戻った。");
    }

    [System.Obsolete]
    void FixedUpdate()
    {
        // ==========================================
        // 1. 【いつでも受け付ける処理】ボタンの入力チェック
        // ==========================================
        // 💡 通常のジャンプボタンが押されたらフラグを立てる（警告が出ていた変数です！）
        if (Input.GetButtonDown("Jump") && IsGrounded) // 地面にいる時だけ通常ジャンプ
        {
            jumpRequested = true;
        }

        // 🎈 空気入れで膨らんでいる時の特殊アクション入力
        if (isInflated)
        {
            if (Input.GetButtonDown("Jump")) AirJump();
            else if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) AirDash();
        }


        // ==========================================
        // 2. 【タイマーの管理】空気入れダッシュの勢いを残す壁
        // ==========================================
        if (specialActionTimer > 0f)
        {
            specialActionTimer -= Time.deltaTime;
            // 💡 ここでリターン(終了)せず、下に進むけど「通常移動の速度上書き」だけをパスさせる構造にします
        }


        // ==========================================
        // 3. 【実際の移動・ジャンプ処理】
        // ==========================================
        Rigidbody rb = GetComponent<Rigidbody>();

        // 🏃 通常の左右移動（タイマーが0の時だけ実行する）
        if (specialActionTimer <= 0f)
        {
            float x = Input.GetAxis("Horizontal");
            rb.linearVelocity = new Vector3(x * moveSpeed, rb.linearVelocity.y, rb.linearVelocity.z);
        }

        // 🦘 通常のジャンプの実行（タイマーに関係なく、地面から跳べるようにここに書く！）
        if (jumpRequested)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // 速度リセット
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // 上に跳ぶ

            jumpRequested = false; // 💡 使い終わったら必ず false に戻す（これで警告も消えます！）
        }
    }

    void HandleGrabDrop()
    {
        if (heldBox != null)
        {
            heldBox.Drop(transform);
            heldBox = null;
        }
        else
        {
            // --- 掴み判定の位置を決定 ---
            Vector3 checkPos;
            ExtendableArm arm = GetComponentInChildren<ExtendableArm>();

            // 腕が伸びている（手の位置が肩から離れている）場合は、手の位置で判定
            if (arm != null && arm.handVisual != null && Vector3.Distance(arm.transform.position, arm.handVisual.position) > 0.5f)
            {
                checkPos = arm.handVisual.position;
            }
            else
            {
                // 腕が縮んでいる時は、通常通りプレイヤーの正面で判定
                checkPos = transform.position + transform.forward * 0.5f;
            }

            Collider[] colliders = Physics.OverlapSphere(checkPos, pickupRange);
            foreach (var col in colliders)
            {
                Box box = col.GetComponent<Box>();
                if (box == null) box = col.GetComponentInParent<Box>();
                if (box != null)
                {
                    // 腕が伸びている場合は「手の先（arm.handVisual）」に付けるように指定
                    bool isUsingArm = arm != null && arm.handVisual != null && Vector3.Distance(arm.transform.position, arm.handVisual.position) > 0.5f;

                    if (isUsingArm)
                    {
                        box.TryPickup(transform, holdPoint, arm.handVisual);
                    }
                    else
                    {
                        box.TryPickup(transform, holdPoint);
                    }

                    heldBox = box;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// プレイヤーを上半身と下半身に切り離す処理。
    /// Slicerから呼び出されます。
    /// </summary>
    public void Split()
    {
        if (isSplit) return;
        isSplit = true;

        // 持っている箱があれば強制的に離す
        if (heldBox != null)
        {
            heldBox.Drop(transform);
            heldBox = null;
        }

        // カメラを事前に取得しておく
        Camera cam = GetComponentInChildren<Camera>();

        // 上半身と下半身を生成
        if (upperBodyPrefab != null)
        {
            GameObject upper = Instantiate(upperBodyPrefab, transform.position + Vector3.up * 0.5f, transform.rotation);
            Rigidbody upperRb = upper.GetComponent<Rigidbody>();
            if (upperRb) upperRb.AddForce(Vector3.up * 2f, ForceMode.Impulse);

            // 上半身をカメラで追いかけるようにする
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

        // カメラがまだ親子関係にある場合は切り離しておく（念のため）
        if (cam != null && cam.transform.parent == transform)
        {
            cam.transform.SetParent(null);
        }

        // 元のプレイヤーを消す
        Destroy(gameObject);
    }

    void CheckGrounded()
    {
        // 足元の位置を計算（少しだけ内側に浮かせて判定の安定性を高める）
        Vector3 footPos = transform.TransformPoint(new Vector3(0, colliderBottomY + 0.1f, 0));

        // 足元に小さな球を作って判定
        Collider[] cols = Physics.OverlapSphere(footPos, groundCheckRadius, groundLayer == 0 ? ~0 : groundLayer);

        bool lastGrounded = isGrounded;
        isGrounded = false;
        foreach (var c in cols)
        {
            // 自分自身やトリガー、または「のびーる腕」のパーツは除外
            if (c.gameObject != gameObject && !c.isTrigger && !c.name.Contains("Visual") && !c.name.Contains("Hand"))
            {
                isGrounded = true;
                break;
            }
        }

        // 接地状態が変化したときのみログを出す（ログの洪水を防ぐ）
        if (isGrounded != lastGrounded)
        {
            Debug.Log($"CheckGrounded: isGrounded changed -> {isGrounded} (overlapCount={cols.Length})");
        }

        // エディタで確認しやすいようにワイヤー表示（OnDrawGizmosSelectedでも確認可）
        Debug.DrawLine(footPos, footPos + Vector3.up * 0.1f, isGrounded ? Color.green : Color.red, 0.1f);
        Debug.DrawRay(transform.position, Vector3.up * (originalColliderHeight * 0.8f), Color.yellow, 0.1f);
    }

    private void OnDrawGizmosSelected()
    {
        // エディタ上で接地判定の範囲を可視化（デバッグ用）
        Gizmos.color = Color.red;
        Vector3 footPos = transform.TransformPoint(new Vector3(0, colliderBottomY, 0));
        Gizmos.DrawWireSphere(footPos, groundCheckRadius);
    }

    // プロパティとして isGrounded を公開
    public bool IsGrounded
    {
        get { return isGrounded; }
    }

    // プレイヤーが押しつぶされた時に呼ばれるメソッドを追加
    [Header("Squash Settings")]
    public float squashedScaleY = 0.2f;   // 縦の潰れ具合
    public float squashedScaleX = 2.0f;   // 横の広がり
    public float recoveryDelay = 8f;    // 復活までの秒数

    bool isSquashed = false;
    Vector3 originalScale;
    public void Squash()
    {
        if (isSquashed) return;
        isSquashed = true;

        isGrounded = true;

        // ここに「ぺしゃんこ」になった時の処理を記述
        // 例: ゲームオーバー処理やアニメーション再生など
        Debug.Log("Player was squashed!");
        // 必要に応じて追加の処理を実装してください
        originalScale = transform.localScale;
        transform.localScale = new Vector3(
            originalScale.x * 2.0f,
            originalScale.y * 0.2f,
            originalScale.z
        );
        // 一定時間後に元に戻すコルーチン開始
        StartCoroutine(RecoverFromSquash());
    }

    IEnumerator RecoverFromSquash()
    {
        // 待機
        yield return new WaitForSeconds(recoveryDelay);

        // スケールを元に戻す
        transform.localScale = originalScale;

        // 状態フラグ解除
        isSquashed = false;

        isGrounded = true;

        Debug.Log("Player recovered from squash!");
    }


    // 鍵を持っているかどうかのフラグ
    public bool HasKey { get; private set; } = false;

    // 鍵を拾った時に呼び出す関数
    public void PickUpKey()
    {
        HasKey = true;
        Debug.Log("鍵をゲットした！");
    }
    // 鍵を使った時に呼び出す関数（これを追加）
    public void UseKey()
    {
        HasKey = false;
        Debug.Log("鍵を使った！");
    }
}