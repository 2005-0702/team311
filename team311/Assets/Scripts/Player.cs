using UnityEngine;

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
        if (isSplit) return; // 切断済みの場合は操作不可（必要に応じて変更）

        // --- 移動処理 ---
        float h = Input.GetAxis("Horizontal");
        Vector3 move = new Vector3(h, 0, 0) * moveSpeed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);

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

        // 上半身と下半身を生成
        if (upperBodyPrefab != null)
        {
            GameObject upper = Instantiate(upperBodyPrefab, transform.position + Vector3.up * 0.5f, transform.rotation);
            // 必要に応じて物理的な衝撃を加える
            Rigidbody upperRb = upper.GetComponent<Rigidbody>();
            if (upperRb) upperRb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
        }

        if (lowerBodyPrefab != null)
        {
            GameObject lower = Instantiate(lowerBodyPrefab, transform.position, transform.rotation);
        }

        // カメラが子供にいる場合の対策：カメラを親子関係から切り離して保護する
        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null)
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
}