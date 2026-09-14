using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class ImageTimer : MonoBehaviour
{
    // 0～9の数字画像
    public Sprite[] numberSprites;

    // 3つの数字を表示するImage
    public Image hundredsImage;
    public Image tensImage;
    public Image onesImage;

    // 最初の時間（300秒）
    public int startSeconds = 300;

    // 現在の残り時間
    private int remainingSeconds;

    // 1秒を測るための時間
    private float timer = 0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 残り時間を300秒にする
        remainingSeconds = startSeconds;

        // 最初の数字を表示する
        UpdateTimerImage();
    }

    // Update is called once per frame
    void Update()
    {
        // 0秒になったらゲームオーバーシーンへ移動
        if (remainingSeconds <= 0)
        {
            SceneManager.LoadScene("GameOver");
            return;
        }

        // 経過時間を加算
        timer += Time.deltaTime;

        // 1秒経過したら
        if (timer >= 1f)
        {
            timer -= 1f;

            // 残り時間を1秒減らす
            remainingSeconds--;

            // 数字画像を更新
            UpdateTimerImage();
        }

    }

    void UpdateTimerImage()
    {
        // 百の位を計算
        int hundreds = remainingSeconds / 100;

        // 十の位を計算
        int tens = (remainingSeconds / 10) % 10;

        // 一の位を計算
        int ones = remainingSeconds % 10;

        // 計算した数字の画像を表示
        hundredsImage.sprite = numberSprites[hundreds];
        tensImage.sprite = numberSprites[tens];
        onesImage.sprite = numberSprites[ones];
    }
}
