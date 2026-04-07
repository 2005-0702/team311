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
    Collider myCollider;
    Collider[] playerColliders; // 持ち主の全コライダーを記憶用

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();
    }

    // Playerから呼ばめるメソッド
    public void TryPickup(Transform player, Transform playerHoldPoint)
    {
        if (isHeld) return;

        // 物理を停止して親子付け
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // プレイヤーと箱の間の当たり判定を無視する（自分自身を突き抜けないように）
        playerColliders = player.GetComponentsInChildren<Collider>();
        if (playerColliders != null && myCollider != null)
        {
            foreach (var pc in playerColliders)
            {
                Physics.IgnoreCollision(myCollider, pc, true);
            }
        }

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

        // 当たり判定の無視を解除
        if (playerColliders != null && myCollider != null)
        {
            foreach (var pc in playerColliders)
            {
                if (pc != null) Physics.IgnoreCollision(myCollider, pc, false);
            }
            playerColliders = null;
        }

        isHeld = false;
    }
}