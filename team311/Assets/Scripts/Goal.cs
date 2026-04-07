using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "GoalScene";

    private void OnTriggerEnter(Collider other)
    {
        // Tag判定ではなく、名前に「Player」という文字が含まれているかチェックする
        if (other.name.Contains("Player"))
        {
            Debug.Log("プレイヤー（クローン含む）を検知しました！");
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
