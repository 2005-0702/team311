using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float jumpForce = 7f;

    [Header("Hold Settings")]
    [Tooltip("箱を持つためのポイントを作ってここにアタッチします（未指定でも動作します）")]
    public Transform holdPoint;
    [Tooltip("箱を探す範囲（半径）")]
    public float pickupRange = 1.5f;
    Box heldBox;

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

        // XZ の速度だけ固定
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);

        // --- ジャンプ処理 ---
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // --- 箱を持つ・離す処理（Eキー） ---
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldBox != null)
            {
                // すでに持っている場合は離す
                heldBox.Drop(transform);
                heldBox = null;
            }
            else
            {
                // 持っていない場合は近くの箱(Box)を探す
                // プレイヤーの少し前方を基準に球状に探す
                Vector3 checkPos = transform.position + transform.forward * 0.5f;
                Collider[] colliders = Physics.OverlapSphere(checkPos, pickupRange);
                
                foreach (var col in colliders)
                {
                    // 当たったオブジェクトが Box なのか判定
                    Box box = col.GetComponent<Box>();
                    if (box == null) box = col.GetComponentInParent<Box>();
                    
                    if (box != null)
                    {
                        // 見つけたら拾う処理を呼び出す
                        box.TryPickup(transform, holdPoint);
                        heldBox = box;
                        break; // 1つ拾ったら終了
                    }
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}