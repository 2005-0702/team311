using UnityEngine;

public class BlockSwitch : MonoBehaviour
{
    public enum SwitchColor { Red, Blue }

    [Header("このスイッチが起動する色")]
    public SwitchColor targetColor;

    // プレイヤーが触れた瞬間（トリガー判定）
    void OnTriggerEnter(Collider other)
    {
        // 触れたオブジェクトがプレイヤーかどうか（タグで判定）
        if (other.CompareTag("Player"))
        {
            TriggerSwitch();
        }
    }

    // 2Dゲームの場合はこちらを使います
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerSwitch();
        }
    }

    private void TriggerSwitch()
    {
        bool activateRed = (targetColor == SwitchColor.Red);

        // マネージャーに色変更を要請
        BlockManager.Instance.ChangeColor(activateRed);

        Debug.Log(targetColor + " スイッチが押されました！");
    }
}