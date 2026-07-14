using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public GameObject tutorialPanel;

    private bool shown = false;
    private bool tutorialOpen = false;

    void Start()
    {
        tutorialPanel.SetActive(false);
    }

    void Update()
    {
        // チュートリアル表示中にEnterキーで閉じる
        if (tutorialOpen && Input.GetKeyDown(KeyCode.Return))
        {
            CloseTutorial();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (shown) return;

        if (other.CompareTag("Player"))
        {
            shown = true;
            tutorialOpen = true;

            tutorialPanel.SetActive(true);

            // ゲーム停止
            Time.timeScale = 0f;
        }
    }

    void CloseTutorial()
    {
        tutorialPanel.SetActive(false);

        tutorialOpen = false;

        // ゲーム再開
        Time.timeScale = 1f;
    }
}