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
        if (specialActionTimer <= 0f)
        {
            Vector3 move = new Vector3(h, 0, 0) * moveSpeed;
            rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);

            if (h > 0.1f) FacingDir = 1;
            else if (h < -0.1f) FacingDir = -1;
        }

        // 箱の「つかむ・離す」のキー入力チェック
        if (Input.GetKeyDown(grabKey))
        {
            HandleGrabDrop();
        }

        // ジャンプの入力判定
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                jumpRequested = true;
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
        // プレイヤーのコライダーの一番底面（足元）の位置を計算
        Vector3 footPos = transform.TransformPoint(new Vector3(0, colliderBottomY, 0));

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
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            specialActionTimer = 0.3f;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * 14f, ForceMode.Impulse);
            jumpCount++;
        }
        Deflate();
    }

    private void AirDash()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
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

    void HandleGrabDrop()
    {
        if (heldBox != null)
        {
            heldBox.Drop(transform);
            heldBox = null;
        }
        else
        {
            Vector3 checkPos = transform.position + transform.forward * 0.5f;

            Collider[] colliders = Physics.OverlapSphere(checkPos, pickupRange);
            foreach (var col in colliders)
            {
                Box box = col.GetComponent<Box>();
                if (box == null) box = col.GetComponentInParent<Box>();

                if (box != null)
                {
                    box.TryPickup(transform, holdPoint);
                    heldBox = box;
                    break;
                }
            }
        }
    }

    public void Split()
    {
        if (isSplit) return;
        isSplit = true;

        if (heldBox != null)
        {
            heldBox.Drop(transform);
            heldBox = null;
        }

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
        Vector3 footPos = transform.TransformPoint(new Vector3(0, colliderBottomY + 0.1f, 0));
        Collider[] cols = Physics.OverlapSphere(footPos, groundCheckRadius, groundLayer == 0 ? ~0 : groundLayer);

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
        Vector3 footPos = transform.TransformPoint(new Vector3(0, colliderBottomY, 0));
        Gizmos.DrawWireSphere(footPos, groundCheckRadius);

        Gizmos.color = Color.yellow;
        Vector3 checkPos = transform.position + transform.forward * 0.5f;
        Gizmos.DrawWireSphere(checkPos, pickupRange);
    }

    public bool IsGrounded
    {
        get { return isGrounded; }
    }

    // --- 省略されていた残りのメンバ変数や関数群 ---
    [Header("Squash Settings")]
    public float squashedScaleY = 0.2f;
    public float squashedScaleX = 2.0f;
    public float recoveryDelay = 8f;

    bool isSquashed = false;
    Vector3 originalScale;

    public void Squash()
    {
        if (isSquashed) return;
        isSquashed = true;

        isGrounded = true;

        Debug.Log("Player was squashed!");
        originalScale = transform.localScale;
        transform.localScale = new Vector3(
            originalScale.x * 2.0f,
            originalScale.y * 0.2f,
            originalScale.z
        );
        StartCoroutine(RecoverFromSquash());
    }

    IEnumerator RecoverFromSquash()
    {
        yield return new WaitForSeconds(recoveryDelay);
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