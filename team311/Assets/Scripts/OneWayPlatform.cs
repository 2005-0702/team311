using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    [Header("実体化させる足場用のコライダー（Is TriggerがOFFの方）")]
    [SerializeField] private Collider solidCollider;

    private void Start()
    {
        // 安全対策：もしセットし忘れていたら自動で探す
        if (solidCollider == null)
        {
            Collider[] cols = GetComponents<BoxCollider>();
            foreach (var c in cols)
            {
                if (!c.isTrigger) solidCollider = c;
            }
        }
    }

    //  トリガー（サイズ1.2の大きい方）にトカゲくんが触れた瞬間
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 床の上面のY座標
            float platformTopY = transform.position.y + (transform.localScale.y / 2f);
            // プレイヤーの足元の位置（中心から少し下を測る）
            float playerBottomY = other.transform.position.y - 0.5f;

            // 【ここが超重要】
            // プレイヤーが「床の上面よりも下」から触れてきたなら、
            // ジャンプの速度に関係なく、問答無用で固い床を「消去」する！
            if (playerBottomY < platformTopY - 0.1f)
            {
                solidCollider.enabled = false;
            }
        }
    }

    // トリガーの中にトカゲくんが「いる間」ずっと実行される
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            float platformTopY = transform.position.y + (transform.localScale.y / 2f);
            float playerBottomY = other.transform.position.y - 0.5f;

            // トカゲくんの足元が、床の上面を完全に超えたら（上に登りきったら）
            if (playerBottomY >= platformTopY - 0.05f)
            {
                Rigidbody rb = other.GetComponent<Rigidbody>();
                // しかも、下に向かって落ちてきている（または着地して止まった）なら、床を出現させる！
                if (rb != null && rb.linearVelocity.y <= 0.1f)
                {
                    solidCollider.enabled = true;
                }
            }
        }
    }

    // トリガーからトカゲくんが完全に離れたとき
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 安全のため、プレイヤーが離れたら確実に床を元に戻す
            solidCollider.enabled = true;
        }
    }
}