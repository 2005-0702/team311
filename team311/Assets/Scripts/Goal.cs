using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "GoalScene";

    [Header("このステージのインデックス (StagePoint.stageIndex と一致させる)")]
    [SerializeField] private int stageIndex = 0;

    private void OnTriggerEnter(Collider other)
    {
        //親オブジェクトも含めて「Player」スクリプトがついているかチェック
        Player player = other.GetComponent<Player>();
        if (player == null) player = other.GetComponentInParent<Player>();

        // プレイヤーが存在する場合のみ処理
        if (player != null)
        {
            //「鍵を持っているか」をプレイヤーに問い合わせる
            if (player.HasKey)
            {
                Debug.Log("鍵を持っているので、ステージクリア！");

                // 進行度を更新（既存値より大きければ更新）
                int saved = PlayerPrefs.GetInt("HighestClearedStage", -1);
                int next = Mathf.Max(saved, stageIndex);
                PlayerPrefs.SetInt("HighestClearedStage", next);
                PlayerPrefs.Save();
                Debug.Log($"HighestClearedStage を {next} に更新しました。");

                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                // 鍵を持っていない場合の処理（お好みで音を鳴らしたりUIを出したり）
                Debug.Log("鍵がありません！ステージ内の鍵を探してください。");
            }
        }
    }
}