using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float jumpForce = 7f;

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

        // --- ジャンプ処理 ---
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // --- 箱を持つ・離す処理（Eキー） ---
        if (Input.GetKeyDown(KeyCode.E))
        {
            HandleGrabDrop();
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
}