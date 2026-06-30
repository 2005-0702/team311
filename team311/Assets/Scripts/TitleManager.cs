using UnityEngine;
using UnityEngine.SceneManagement; // シーンを切り替えるためにこれが必要です！

public class TitleManager : MonoBehaviour
{
    [Header("移行先のゲームシーンの名前を正確に書いてね")]
    [SerializeField] private string gameSceneName = "SampleScene";

    // ボタンが押されたときに実行される関数
    public void OnStartButton()
    {
        // 指定した名前のシーンを読み込む
        SceneManager.LoadScene(gameSceneName);
    }
}