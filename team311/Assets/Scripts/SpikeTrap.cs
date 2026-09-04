using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SpikeTrap : MonoBehaviour
{
    // ゲームオーバーシーン名
    public string gameOverSceneName = "GameOverScene";

    [Header("フェード")]
    [SerializeField] private GameObject fadePanel;

    [Header("暗転にかかる時間")]
    [SerializeField] private float fadeDuration = 1.5f;

    private bool isDead = false;
    private void OnTriggerEnter(Collider other)
    {
        if (isDead)
            return;

        if (other.CompareTag("Player"))
        {
            isDead = true;

            // =========================
            // プレイヤーの操作を禁止
            // =========================

            Player player = other.GetComponent<Player>();

            if (player == null)
            {
                player = other.GetComponentInParent<Player>();
            }

            if (player != null)
            {
                player.SetMoveEnabled(false);
            }

            // フェード開始
            StartCoroutine(FadeAndGameOver());
        }
    }
    private IEnumerator FadeAndGameOver()
    {
        if (fadePanel == null)
        {
            Debug.LogError("FadePanelが設定されていません！");
            yield break;
        }

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

        fadePanel.SetActive(true);

        // 徐々に暗くする
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            float alpha = time / fadeDuration;

            color.a = alpha;
            fadeImage.color = color;
            yield return null;
        }

        // 完全に黒
        color.a = 1f;
        fadeImage.color = color;

        // 少し待つ
        yield return new WaitForSeconds(0.3f);

        // ゲームオーバーシーンへ
        SceneManager.LoadScene(gameOverSceneName);
    }
}