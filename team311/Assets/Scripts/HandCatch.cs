using UnityEngine;
using UnityEngine.UIElements;

public class HandCatch : MonoBehaviour
{
    [SerializeField]
    private float pickupRange = 3f; // プレイヤーが箱を持ち上げられる距離

    private Box heldBox; // 現在プレイヤーが持っている箱
    [SerializeField]
    private Transform holdPoint; // 箱を持つ位置

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // --- 箱を持つ・離す処理（Eキー） ---
        if (Input.GetKeyDown(KeyCode.E))
        {
            HandleGrabDrop();
        }
    }
    
        // 箱を持つ・離す処理を行うメソッド
        private void HandleGrabDrop()
        {
        // プレイヤーの前方にある箱を検出
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, pickupRange))
        {
            Box box = hit.collider.GetComponent<Box>();
            if (box != null)
            {
                if (heldBox == null)
                {
                    // 箱を持つ
                    heldBox = box;
                    heldBox.transform.SetParent(holdPoint);
                    heldBox.transform.localPosition = Vector3.zero;
                    heldBox.GetComponent<Rigidbody>().isKinematic = true;
                }
                else
                {
                    // 箱を離す
                    heldBox.transform.SetParent(null);
                    Rigidbody boxRb = heldBox.GetComponent<Rigidbody>();
                    boxRb.isKinematic = false;
                    boxRb.AddForce(transform.forward * 5f, ForceMode.Impulse); // 離すときに少し前方に力を加える
                    heldBox = null;
                }
            }
        }

    }
}