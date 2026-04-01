using UnityEngine;

/// <summary>
/// 切断後の上半身を操作するためのスクリプト。
/// Player.cs とほぼ同じ操作感で動かせます。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class UpperBodyController : MonoBehaviour
{
    public float moveSpeed = 6f; // 上半身なので少し遅めにするのもアリ
    public float jumpForce = 5f;

    [Header("Hold Settings")]
    public Transform holdPoint;
    public float pickupRange = 1.2f;
    Box heldBox;

    Rigidbody rb;
    bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 倒れないように回転を固定（必要に応じて）
        rb.freezeRotation = true;
    }

    void Update()
    {
        // --- 移動処理 ---
        float h = Input.GetAxis("Horizontal");
        Vector3 move = new Vector3(h, 0, 0) * moveSpeed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, 0f); // X軸移動

        // --- ジャンプ処理 ---
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // --- 箱の操作 ---
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

    private void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}
