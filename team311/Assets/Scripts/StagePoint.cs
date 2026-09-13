using UnityEngine;

public class StagePoint : MonoBehaviour
{
    [Header("Scene")]
    public string sceneName;

    [Header("ステージ順序 (0 から)")]
    [Tooltip("StageProgressで解放判定に使う番号。左から順に 0,1,2,3... と振ってください。")]
    public int stageIndex = 0;

    [Header("接続")]
    public StagePoint up;
    public StagePoint down;
    public StagePoint left;
    public StagePoint right;

    [Header("見た目の切り替え")]
    [Tooltip("未解放（挑戦できない）のときに表示するオブジェクト。例：赤い見た目のオブジェクト")]
    public GameObject lockedObject;
    [Tooltip("解放済み（挑戦できる）のときに表示するオブジェクト。例：青い見た目のオブジェクト")]
    public GameObject unlockedObject;
    [Tooltip("クリア済みのときに表示するオブジェクト。未設定ならunlockedObjectのまま表示")]
    public GameObject clearedObject;

    // 状態 (外部から参照できるようにプロパティ風に公開)
    public bool IsLocked { get; private set; }
    public bool IsCleared { get; private set; }

    void Start()
    {
        // StageProgressの記録から、今の状態（クリア済みか／解放されているか）を反映する
        RefreshFromProgress();
    }

    // StageProgressの記録を読み直して見た目を更新する
    // （ステージセレクトに戻ってきた時などに呼び出す）
    public void RefreshFromProgress()
    {
        bool cleared = StageProgress.IsCleared(stageIndex);
        bool locked = !StageProgress.IsUnlocked(stageIndex);
        UpdateVisual(locked, cleared);
    }

    // ロック状態 / クリア状態に応じて見た目を更新する
    public void UpdateVisual(bool locked, bool cleared)
    {
        IsLocked = locked;
        IsCleared = cleared;

        bool showCleared = cleared && clearedObject != null;

        if (lockedObject != null) lockedObject.SetActive(locked && !showCleared);
        if (unlockedObject != null) unlockedObject.SetActive(!locked && !showCleared);
        if (clearedObject != null) clearedObject.SetActive(showCleared);
    }
}