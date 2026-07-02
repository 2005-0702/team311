using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CountdownTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText; // TextMeshPro をアサイン
    [SerializeField] private float startTime = 300f;        // 300秒 = 5分
    [SerializeField] private string nextSceneName = "GameOverScene";          // 切り替え先

    private float currentTime;
    private bool isRunning = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        currentTime = startTime;
        UpdateText(currentTime);
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning = false;

            // ゲームオーバシーンへ移動
            SceneManager.LoadScene(nextSceneName);
        }

        UpdateText(currentTime);
    }

    private void UpdateText(float time)
    {
        int seconds = Mathf.CeilToInt(time);
        // 秒だけ表示
        countdownText.text = seconds.ToString();  
    }
}
