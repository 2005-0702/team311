using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    public int damageAmount = 1; // 与えるダメージ量

    private void OnTriggerEnter(Collider other)
    {
        // 触れた相手が PlayerHealth スクリプトを持っているか確認
        PlayerHealth player = other.GetComponent<PlayerHealth>();

        if (player != null)
        {
            // プレイヤーの体力を減らす関数を呼び出す
            player.TakeDamage(damageAmount);
        }
    }
}
