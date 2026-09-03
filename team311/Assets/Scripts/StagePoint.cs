using UnityEngine;

public class StagePoint : MonoBehaviour
{
    [Header("Scene")]
    public string sceneName;

    [Header("ステージ順序 (0 から)")]
    public int stageIndex = 0;

    [Header("接続")]
    public StagePoint up;
    public StagePoint down;
    public StagePoint left;
    public StagePoint right;

    [Header("表示 (省略時はこのオブジェクトの SpriteRenderer を使用)")]
    public SpriteRenderer blockRenderer;

    [Header("色設定")]
    public Color lockedColor = new Color(0.25f, 0.25f, 0.25f, 1f);
    public Color clearedColor = Color.yellow;
    public Color normalColor = Color.white;

    // 状態 (外部から参照できるようにプロパティ風に公開)
    public bool IsLocked { get; private set; }
    public bool IsCleared { get; private set; }

    // Inspector 上で反映しやすいように OnValidate を用意
    void OnValidate()
    {
        if (blockRenderer == null)
        {
            blockRenderer = GetComponent<SpriteRenderer>();
        }
        // エディタで色を即時反映したい場合は有効化（実行中は Start 等から UpdateVisual を呼ぶ）
        if (blockRenderer != null)
        {
            var col = IsCleared ? clearedColor : (IsLocked ? lockedColor : normalColor);
            blockRenderer.color = col;
        }
    }

    // ロック状態 / クリア状態に応じて見た目を更新する
    public void UpdateVisual(bool locked, bool cleared)
    {
        IsLocked = locked;
        IsCleared = cleared;

        if (blockRenderer == null)
        {
            blockRenderer = GetComponent<SpriteRenderer>();
        }

        if (blockRenderer != null)
        {
            if (cleared)
            {
                blockRenderer.color = clearedColor;
            }
            else if (locked)
            {
                blockRenderer.color = lockedColor;
            }
            else
            {
                blockRenderer.color = normalColor;
            }
        }
    }
}