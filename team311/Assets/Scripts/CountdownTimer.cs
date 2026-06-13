using TMPro;
using UnityEngine;

public class CountdownTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText; // TextMeshPro ‚ðƒAƒTƒCƒ“
    [SerializeField] private float startTime = 300f;        // 300•b = 5•ª

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

            // 0‚É‚È‚Á‚½uŠÔ‚É‰½‚©‚µ‚½‚¢ê‡‚Í‚±‚±‚Éˆ—‚ð‘‚­
            // OnTimeUp();
        }

        UpdateText(currentTime);
    }

    private void UpdateText(float time)
    {
        int seconds = Mathf.CeilToInt(time);
        // •b‚¾‚¯•\Ž¦
        countdownText.text = seconds.ToString();  
    }
}
