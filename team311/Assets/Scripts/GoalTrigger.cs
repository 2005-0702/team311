using UnityEngine;
using UnityEngine.SceneManagement;

// 各レベルのゴール地点に置くスクリプト。
// プレイヤーが触れたら、このステージをクリア済みとして記録し、ステージセレクトに戻る。
public class GoalTrigger : MonoBehaviour
{
    [Tooltip("このレベルのステージ番号。対応するStagePointのstageIndexと必ず同じ値にしてください。")]
    public int stageIndex;

    [Tooltip("プレイヤーのタグ名")]
    public string playerTag = "Player";

    [Tooltip("クリア後に戻るシーン名（ステージセレクトのシーン名）")]
    public string stageSelectSceneName = "StageSelect";

    private bool _cleared = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_cleared) return;
        if (!other.CompareTag(playerTag)) return;

        _cleared = true;

        // ステージクリアを記録（これで次のステージが解放される）
        StageProgress.ClearStage(stageIndex);

        Debug.Log($"GoalTrigger: ステージ{stageIndex}クリア！ステージセレクトに戻ります。");

        // ステージセレクトシーンに戻る
        SceneManager.LoadScene(stageSelectSceneName);
    }
}
