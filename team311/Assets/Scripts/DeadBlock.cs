using UnityEngine;
using UnityEngine.SceneManagement;

public class DeadBlock : MonoBehaviour
{
    // ゲームオーバーシーン名
    public string gameOverSceneName = "GameOverScene";

    private void OnCollisionEnter(Collision collision)
    {
        // ぶつかった相手がプレイヤーか確認
        if (collision.gameObject.CompareTag("Player"))
        {
            // ゲームオーバーシーンへ切り替える
            SceneManager.LoadScene(gameOverSceneName);
        }
    }
}
