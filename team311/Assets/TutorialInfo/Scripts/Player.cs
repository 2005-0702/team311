using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float jumpForce = 7f;

    Rigidbody rb;
    bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        // 移動は AddForce で行う
        float h = Input.GetAxis("Horizontal");
        //float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, 0, 0) * moveSpeed;

        // XZ の速度だけ固定
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);

        // ジャンプ
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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
