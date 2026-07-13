using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Box : MonoBehaviour
{
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // 箱がプレイヤーに押されたときに、傾いたり奥（Z軸）にズレたりしないように固定する
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;

        // プレイヤーの押し心地に合わせて重さを調整（軽すぎたら10、重すぎたら50などにインスペクターで調整してね）
        rb.mass = 20f;
    }
}