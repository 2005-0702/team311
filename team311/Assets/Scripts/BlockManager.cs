using UnityEngine;

public class BlockManager : MonoBehaviour
{
    // 外部から簡単にこのマネージャーにアクセスできるようにする設定（シングルトン）
    public static BlockManager Instance { get; private set; }

    // 現在「赤」がアクティブかどうか（falseなら青がアクティブ）
    public bool isRedActive = true;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // ゲーム開始時の状態を全ブロックに通知
        UpdateAllBlocks();
    }

    // 赤と青の状態を切り替える関数
    public void ChangeColor(bool toRed)
    {
        // すでにその色なら何もしない
        if (isRedActive == toRed) return;

        isRedActive = toRed;
        UpdateAllBlocks();
    }

    // ステージ上のすべてのブロックに「切り替わったよ！」と伝える
    private void UpdateAllBlocks()
    {
        // 画面内のすべてのSwitchBlockスクリプトを探して実行
        SwitchBlock[] blocks = FindObjectsByType<SwitchBlock>(FindObjectsSortMode.None);
        foreach (SwitchBlock block in blocks)

            block.RefreshState(isRedActive);

        SwitchingSpikeTrap[] spikes = FindObjectsByType<SwitchingSpikeTrap>(FindObjectsSortMode.None);
        foreach (SwitchingSpikeTrap spike in spikes)
            spike.RefreshState(isRedActive);
    }
}