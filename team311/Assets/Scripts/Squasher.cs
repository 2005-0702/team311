using UnityEngine;

/// <summary>
/// 落下物やプレス機にアタッチするスクリプト。
/// プレイヤーに触れたら（移動条件を満たしていれば）いつでもぺしゃんこにします。
/// </summary>
public class Squasher : MonoBehaviour
{
    [Tooltip("押しつぶすために必要な最低速度（0なら動いていなくても、触れるだけでOK）")]
    public float crushThreshold = 0.5f;

    // 「OnTriggerStay」または「OnCollisionStay」にします。
    // プレス機のコライダーの「Is Trigger」にチェックを入れている場合は、こちらを使います。
    private void OnTriggerStay(Collider other)
    {
        // 衝突相手がプレイヤーかどうか確認
        Player player = other.GetComponent<Player>();
        if (player == null) player = other.GetComponentInParent<Player>();

        if (player != null)
        {
            // 【変更】grounded（地面判定）を削除しました！

            // 条件1：このオブジェクトが物理演算で下方向に移動している
            Rigidbody rb = GetComponent<Rigidbody>();
            bool isFalling = rb != null && rb.linearVelocity.y < -crushThreshold;

            // 条件2：あるいは、MovingHazard スクリプトによる強制的な下移動中である
            MovingHazard movingHazard = GetComponent<MovingHazard>();
            bool isMovingPosDown = movingHazard != null && movingHazard.IsMovingDown();

            // 💡 速度チェックが必要ない（0設定）か、下移動中であること
            bool isCrushingSpeed = (crushThreshold <= 0) || isFalling || isMovingPosDown;

            if (isCrushingSpeed)
            {
                // プレイヤー側のメソッドを呼ぶ
                player.Squash();
            }
        }
    }

    // 💡もしプレス機の「Is Trigger」にチェックを入れていない（物理的な壁のままにしている）場合は、
    // 上の OnTriggerStay ではなく、下の OnCollisionStay を有効にしてください。
    /*
    private void OnCollisionStay(Collision collision)
    {
        Player player = collision.gameObject.GetComponent<Player>();
        if (player == null) player = collision.gameObject.GetComponentInParent<Player>();

        if (player != null)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            bool isFalling = rb != null && rb.linearVelocity.y < -crushThreshold;
            
            MovingHazard movingHazard = GetComponent<MovingHazard>();
            bool isMovingPosDown = movingHazard != null && movingHazard.IsMovingDown();

            bool isCrushingSpeed = (crushThreshold <= 0) || isFalling || isMovingPosDown;

            if (isCrushingSpeed)
            {
                player.Squash();
            }
        }
    }
    */
}