using UnityEngine;

/// <summary>
/// 下半身プレハブにアタッチすると、自動で前方に進み続けるスクリプト。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class LowerBodyMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // 回転が不安定にならないように、XYZ軸の回転を固定することも検討（任意）
        // rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        // X軸方向に一定の速度を与える（右方向：Vector3.right）
        Vector3 targetVelocity = Vector3.right * moveSpeed;
        
        // Y軸の速度（重力分）は維持しつつ、X軸の速度を更新
        // Z軸の速度は 0 にリセットします
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, 0f);
    }
}
