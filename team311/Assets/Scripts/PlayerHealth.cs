using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 1; // 最大体力
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // ダメージを受ける命令（トゲから呼ばれる）
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("痛い！ 残り体力: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("ゲームオーバー！ゲームを終了します。");

        // 1. Unityエディタ上での実行を停止する
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // 2. 実際にアプリとして書き出した（ビルドした）後にアプリを終了する
            Application.Quit();
#endif
    }
}
