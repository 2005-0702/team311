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
    Transform targetHoldPoint;  // 最終的な持ち位置

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();
    }

    // Playerから呼ばめるメソッド
    public void TryPickup(Transform player, Transform playerHoldPoint, Transform hand = null)
    {
        if (isHeld) return;

        // 物理を停止
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 当たり判定の無視
        playerColliders = player.GetComponentsInChildren<Collider>();
        if (playerColliders != null && myCollider != null)
        {
            foreach (var pc in playerColliders)
            {
                Physics.IgnoreCollision(myCollider, pc, true);
            }
        }

        targetHoldPoint = playerHoldPoint;

        if (hand != null)
        {
            // まずは手の先に親子付け（引き寄せ演出用）
            transform.SetParent(hand);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        else if (playerHoldPoint != null)
        {
            // 通常のキャッチ：手元に親子付け
            transform.SetParent(playerHoldPoint);
            transform.position = playerHoldPoint.position;
            transform.rotation = playerHoldPoint.rotation;
        }
        else
        {
            transform.SetParent(player);
            transform.localPosition = new Vector3(0f, 0.6f, 0.8f);
            transform.localRotation = Quaternion.identity;
        }

        isHeld = true;
    }

    void Update()
    {
        if (isHeld && targetHoldPoint != null && transform.parent != targetHoldPoint)
        {
            // 手元（HoldPoint）に十分近づいたら、正式に持ち位置へ移動
            if (Vector3.Distance(transform.position, targetHoldPoint.position) < 0.2f)
            {
                transform.SetParent(targetHoldPoint);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
        }
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