using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "GoalScene";

    private void OnTriggerEnter(Collider other)
    {
        // 1. 何かが触れたら必ずログを出す
        Debug.Log("何かがゴールに触れました！名前: " + other.name);

        if (other.CompareTag("Player"))
        {
            // 2. プレイヤーだと判定されたらログを出す
            Debug.Log("プレイヤーだと認識しました。シーンを切り替えます。");
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
