using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSelect : MonoBehaviour
{
    public RectTransform[] buttons;

    private int current = 0;

    public Vector3 normalSize = new Vector3(2.272f, 1.033f, 1f);
    public Vector3 selectSize = new Vector3(2.6f, 1.2f, 1f);

    void Start()
    {
        UpdateSelect();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            current++;

            if (current >= buttons.Length)
                current = 0;

            UpdateSelect();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            current--;

            if (current < 0)
                current = buttons.Length - 1;

            UpdateSelect();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            // リトライ
            if (current == 0)
            {
                SceneManager.LoadScene(StageManager.CurrentStageName);
            }

            // ステージセレクト
            if (current == 1)
            {
                SceneManager.LoadScene("StageSelect");
            }
        }
    }

    void UpdateSelect()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].localScale = (i == current) ? selectSize : normalSize;
        }
    }
}