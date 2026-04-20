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
    bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // --- 移動処理 ---
        float h = Input.GetAxis("Horizontal");
        Vector3 move = new Vector3(h, 0, 0) * moveSpeed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);

        // --- 向き反転処理 ---
        if (h > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (h < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
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

    private void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;

        
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
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
    public float recoveryDelay = 4f;    // 復活までの秒数

    bool isSquashed = false;
    Vector3 originalScale;
    public void Squash()
    {
        if (isSquashed) return;
        isSquashed = true;


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