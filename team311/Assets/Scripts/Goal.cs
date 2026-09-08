using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Goal : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "GoalScene";

    [Header("このステージのインデックス (StagePoint.stageIndex と一致させる)")]
    [SerializeField] private int stageIndex = 0;

    [Header("ゴールの演出")]
    [SerializeField] private GameObject fadePanel;

    [Header("暗転にかかる時間")]
    [SerializeField] private float fadeDuration = 1.5f;

    private bool isGoal = false;

    private void OnTriggerEnter(Collider other)
    {
        // 親オブジェクトも含めてPlayerを探す
        Player player = other.GetComponent<Player>();
        if (player == null)
            player = other.GetComponentInParent<Player>();

        if (player != null)
        {
            if (isGoal)
                return;
            // 鍵を持っているか確認
            if (player.HasKey)
            {
                isGoal = true;

                Debug.Log("鍵を持っているので、ステージクリア！");

                // 進行度を更新
                int saved = PlayerPrefs.GetInt("HighestClearedStage", -1);
                int next = Mathf.Max(saved, stageIndex);

                PlayerPrefs.SetInt("HighestClearedStage", next);
                PlayerPrefs.Save();

                Debug.Log($"HighestClearedStage を {next} に更新しました。");

                // ゴール演出開始
                StartCoroutine(GoalEffect());
            }
            else
            {
                Debug.Log("鍵がありません！ステージ内の鍵を探してください。");
            }
        }
    }

    private IEnumerator GoalEffect()
    {
        if (fadePanel == null)
        {
            Debug.LogError("FadePanelが設定されていません！");
            yield break;
        }

        // FadePanelのImageを取得
        Image fadeImage = fadePanel.GetComponent<Image>();
        if (fadeImage == null)
        {
            Debug.LogError("FadePanelにImageコンポーネントがありません！");
            yield break;
        }

        // 最初は透明
        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

        // 表示する
        fadePanel.SetActive(true);

        float time = 0f;

        // 徐々に暗くする
        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            float alpha = time / fadeDuration;

            color.a = alpha;
            fadeImage.color = color;

            yield return null;
        }

        // 完全に黒くする
        color.a = 1f;
        fadeImage.color = color;

        // 少し黒画面を見せる
        yield return new WaitForSeconds(0.3f);

        // ゴールシーンへ
        SceneManager.LoadScene(nextSceneName);
    }
}