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
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        Collider[] allPlayerColliders = rb.GetComponentsInChildren<Collider>();

        // 【修正】足元の高さ（一番低い位置）を正しく計算
        float objectBottom = Mathf.Infinity;
        foreach (var c in allPlayerColliders)
        {
            if (!c.isTrigger) objectBottom = Mathf.Min(objectBottom, c.bounds.min.y);
        }
        if (objectBottom == Mathf.Infinity) objectBottom = other.bounds.min.y;

        // 足場の表面の高さ
        float platformTop = solidCollider.bounds.max.y;

        Player player = rb.GetComponent<Player>();
        bool isCrouching = (player != null && player.IsCrouching);

        // 条件判定
        // 下から来ているか、上向きに動いているか、しゃがんでいるなら「無視」
        bool isBelow = objectBottom < platformTop - 0.1f;
        bool isMovingUp = rb.linearVelocity.y > 0.01f;

        if (isBelow || isMovingUp || isCrouching)
        {
            foreach (var pc in allPlayerColliders)
            {
                if (pc != solidCollider) Physics.IgnoreCollision(solidCollider, pc, true);
            }
        }
        else
        {
            // 完全に上に乗り切っている時だけ衝突を有効にする
            if (objectBottom > platformTop - 0.05f)
            {
                foreach (var pc in allPlayerColliders)
                {
                    if (pc != solidCollider) Physics.IgnoreCollision(solidCollider, pc, false);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            Collider[] allPlayerColliders = rb.GetComponentsInChildren<Collider>();
            
            // 出口での判定：足元が床より高い位置にある時だけ当たり判定を戻す
            // これにより、下から上に突き抜ける瞬間に引っかかるのを防ぎます
            float objectBottom = Mathf.Infinity;
            foreach (var c in allPlayerColliders)
            {
                if (!c.isTrigger) objectBottom = Mathf.Min(objectBottom, c.bounds.min.y);
            }

            if (objectBottom > solidCollider.bounds.max.y - 0.1f)
            {
                foreach (var pc in allPlayerColliders)
                {
                    if (pc != solidCollider) Physics.IgnoreCollision(solidCollider, pc, false);
                }
            }
        }
    }
}
