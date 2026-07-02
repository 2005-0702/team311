using UnityEngine;
using UnityEngine.SceneManagement; // シーン切り替えに必要です！

public class GameOverManager : MonoBehaviour
{
    [Header("戻り先のタイトルシーンの名前を正確に書いてね")]
    [SerializeField] private string StageSelectName = "StageSelect";

    void Update()
    {
        // Enterキー（Returnキー）またはテンキーのEnterが押されたかチェック
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            // セレクトシーンを読み込む
            SceneManager.LoadScene(StageSelectName);
        }
    }
}