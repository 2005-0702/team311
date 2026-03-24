using UnityEngine;
using System.Collections;

/// <summary>
/// プレイヤーにつかまれたり離されたりする箱のプログラム。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Box : MonoBehaviour
{
    [Tooltip("拾ったときに前方向へ少し投げる力（ドロップ時の慣性）。")]
    public float dropForwardImpulse = 1.5f;

    Rigidbody rb;
    bool isHeld = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Playerから呼ばれるメソッド
    public void TryPickup(Transform player, Transform playerHoldPoint)
    {
        if (isHeld) return;

        // 物理を停止して親子付け
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (playerHoldPoint != null)
        {
            transform.SetParent(playerHoldPoint);
            transform.position = playerHoldPoint.position;
            transform.rotation = playerHoldPoint.rotation;
        }
        else
        {
            transform.SetParent(player);
            // HoldPointがない場合は少し上に持ち上げる
            transform.localPosition = new Vector3(0f, 0.6f, 0.8f);
            transform.localRotation = Quaternion.identity;
        }

        isHeld = true;
    }

    // Playerから呼ばれるメソッド
    public void Drop(Transform player)
    {
        if (!isHeld) return;

        // 親子解除して物理復帰
        transform.SetParent(null);
        rb.isKinematic = false;

        // 少し前方に力を与えて自然に落ちるようにする（プレイヤーが動いている場合の慣性）
        var forward = player.forward;
        rb.AddForce(forward * dropForwardImpulse, ForceMode.Impulse);

        isHeld = false;
    }
}