using UnityEngine;
using UnityEngine.SceneManagement; // シーンを切り替えるために必要

public class TitleManager : MonoBehaviour
{
    [Header("各ボタンの移動先シーン名")]
    [SerializeField] private string stageSelectSceneName = "StageSelect"; // ゲームスタート時の行き先
    [SerializeField] private string tutorialSceneName = "Tutorial";       // チュートリアル時の行き先

    /// <summary>
    /// 「ゲームスタート」ボタンが押されたときに実行
    /// </summary>
    public void OnStartButton()
    {
        SceneManager.LoadScene(stageSelectSceneName);
    }

    /// <summary>
    /// 「チュートリアル」ボタンが押されたときに実行
    /// </summary>
    public void OnTutorialButton()
    {
        SceneManager.LoadScene(tutorialSceneName);
    }
}