using UnityEngine;

/// <summary>
/// プレイヤーがこのオブジェクト（トリガー）に触れた時、
/// プレイヤーを上半身と下半身に切り離すスクリプト。
/// </summary>
public class Slicer : MonoBehaviour
{
    private bool isActive = true;

    public void SetActivate(bool state)
    {
        isActive = state;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        // 触れたオブジェクトが Player かどうかを確認
        Player player = other.GetComponent<Player>();
        
        // もし直属にない場合は親も探す（キャラクターの階層構造対策）
        if (player == null) player = other.GetComponentInParent<Player>();

        if (player != null)
        {
            Debug.Log("Player hit the Slicer! Splitting...");
            player.Split();
        }
    }
}
