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

        // 親子解除
        transform.SetParent(null);

        // ==========================================
        // プレイヤーの目の前に箱を配置する処理
        // ==========================================
        // トカゲくんの正面方向（player.forward）を取得
        Vector3 forwardDir = player.forward;

        // プレイヤーの中心から、正面に「1.2メートル」離した位置を計算
        // （※箱のサイズに合わせて 1.0f 〜 1.5f あたりで微調整）
        Vector3 spawnPosition = player.position + (forwardDir * 1.0f);

        // Y軸（高さ）はトカゲくんと同じか、少しだけ上（お腹の高さなど）に、
        // ここではプレイヤーの足元より少し上（+0.5f）に調整
        spawnPosition.y = player.position.y + 2.5f;
        spawnPosition.x = player.position.x + 0.5f; // プレイヤーの右側に少しずらす

        // 計算した安全な位置に箱を瞬間移動させる
        transform.position = spawnPosition;
        // ==========================================

        // 物理復帰
        rb.isKinematic = false;

        // 少し前方へ投げる力を与える（重くした分、ちょっと強めに飛ぶようになる）
        rb.AddForce(forwardDir * dropForwardImpulse, ForceMode.Impulse);

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