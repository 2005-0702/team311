using UnityEngine;

/// <summary>
/// 下からジャンプして通り抜け、上には着地できる「一方向の足場」を作成するスクリプト。
/// 足場のオブジェクトにアサインし、2つのコライダー（実体用とトリガー用）を準備してください。
/// </summary>
public class OneWayPlatform : MonoBehaviour
{
    [Tooltip("実際にキャラクターが乗るための、Is Triggerがオフのコライダーを指定してください。")]
    public Collider solidCollider;

    // 触れた瞬間に即座に無視を開始する
    private void OnTriggerEnter(Collider other)
    {
        CheckCollision(other);
    }

    private void OnTriggerStay(Collider other)
    {
        CheckCollision(other);
    }

    private void CheckCollision(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) rb = other.GetComponentInParent<Rigidbody>();

        // 足元の高さ
        float objectBottom = other.bounds.min.y;
        // 足場の表面の高さ
        float platformTop = solidCollider.bounds.max.y;

        // 次のいずれかの条件を満たせば「すり抜け」にする：
        // 1. 足元が足場の表面より低い位置にある
        // 2. あるいは、上方向にある程度の速度で移動している
        bool isBelow = objectBottom < platformTop - 0.1f;
        bool isMovingUp = (rb != null && rb.linearVelocity.y > 0.1f);

        if (isBelow || isMovingUp)
        {
            Physics.IgnoreCollision(solidCollider, other, true);
        }
        else
        {
            Physics.IgnoreCollision(solidCollider, other, false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // トリガーから完全に離れたら、念のため衝突判定を戻しておく
        Physics.IgnoreCollision(solidCollider, other, false);
    }
}
