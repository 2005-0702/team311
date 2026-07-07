using UnityEngine;

/// <summary>
/// 【最終安定版】一方通行の足場スクリプト。
/// 金網オブジェクト（RigidbodyのIs Kinematicオン）にアタッチしてください。
/// </summary>
public class OneWayPlatform : MonoBehaviour
{
    private Collider blockCollider;

    void Start()
    {
        // 自身のコライダー（1個に整理した正しいコライダー）を取得
        blockCollider = GetComponent<Collider>();
    }

    // プレイヤーが下から接触している間、常にチェック
    private void OnCollisionEnter(Collision collision)
    {
        CheckOneWay(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        CheckOneWay(collision);
    }

    private void CheckOneWay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();

            if (playerRb != null)
            {
                // プレイヤーが上に向かって移動している（ジャンプ中）なら
                if (playerRb.linearVelocity.y > 0.1f)
                {
                    // 衝突を無視してすり抜けさせる
                    if (blockCollider != null)
                    {
                        Physics.IgnoreCollision(collision.collider, blockCollider, true);
                    }
                }
            }
        }
    }

    // プレイヤーが金網から完全に抜け切ったら、上から乗れるように衝突無視を解除
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (blockCollider != null)
            {
                Physics.IgnoreCollision(collision.collider, blockCollider, false);
            }
        }
    }
}