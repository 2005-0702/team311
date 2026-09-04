using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "GoalScene";

    [Header("このステージのインデックス (StagePoint.stageIndex と一致させる)")]
    [SerializeField] private int stageIndex = 0;

    [Header("ゴールの演出")]
    [SerializeField] private GameObject fadePanel;

    private bool isGoal = false;

    private void OnTriggerEnter(Collider other)
    {
        //親オブジェクトも含めて「Player」スクリプトがついているかチェック
        Player player = other.GetComponent<Player>();
        if (player == null) player = other.GetComponentInParent<Player>();

        // プレイヤーが存在する場合のみ処理
        if (player != null)
        {
            if (isGoal)
                return;

            //「鍵を持っているか」をプレイヤーに問い合わせる
            if (player.HasKey)
            {
                isGoal = true;

                Debug.Log("鍵を持っているので、ステージクリア！");

                // 進行度を更新（既存値より大きければ更新）
                int saved = PlayerPrefs.GetInt("HighestClearedStage", -1);
                int next = Mathf.Max(saved, stageIndex);
                PlayerPrefs.SetInt("HighestClearedStage", next);
                PlayerPrefs.Save();
                Debug.Log($"HighestClearedStage を {next} に更新しました。");

                StartCoroutine(GoalEffect());
            }
            else
            {
                // 鍵を持っていない場合の処理（お好みで音を鳴らしたりUIを出したり）
                Debug.Log("鍵がありません！ステージ内の鍵を探してください。");
            }
        }
    }

    private IEnumerator GoalEffect()
    {
        // 画面を暗くする
        if (fadePanel != null)
        {
            fadePanel.SetActive(true);
        }

        // 1秒待つ
        yield return new WaitForSeconds(1.0f);

        // ゴールシーンへ
        SceneManager.LoadScene(nextSceneName);
    }



}