using UnityEngine;

/// <summary>
/// 落下物やプレス機にアタッチするスクリプト。
/// プレイヤーを地面との間で挟んだ時に「ぺしゃんこ」にします。
/// </summary>
public class Squasher : MonoBehaviour
{
    [Tooltip("押しつぶすために必要な最低速度（0なら触れるだけでOK）")]
    public float crushThreshold = 0.5f;

    private void OnCollisionEnter(Collision collision)
    {
        // 衝突相手がプレイヤーかどうか確認
        Player player = collision.gameObject.GetComponent<Player>();
        if (player == null) player = collision.gameObject.GetComponentInParent<Player>();

        if (player != null)
        {
            // 条件1：プレイヤーが地面に足をつけている（Player.cs のプロパティを使用）
            bool grounded = player.IsGrounded;
            
            // 条件2：このオブジェクトが下方向に移動している
            Rigidbody rb = GetComponent<Rigidbody>();
            bool isFalling = rb != null && !rb.isKinematic && rb.linearVelocity.y < -crushThreshold;
            
            // 条件3：あるいは、MovingHazard スクリプトによる強制的な下移動中である
            MovingHazard movingHazard = GetComponent<MovingHazard>();
            bool isMovingPosDown = movingHazard != null && movingHazard.IsMovingDown();

            if (grounded && (isFalling || isMovingPosDown))
            {
                // プレイヤー側のメソッドを呼ぶ
                player.Squash();
            }
        }
    }
}
