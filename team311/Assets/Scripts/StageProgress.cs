using UnityEngine;

// ステージの「クリア済みか」「解放されているか」をシーンをまたいで管理するクラス。
// 既存のGoal.csが使っているPlayerPrefsキー "HighestClearedStage" をそのまま利用する。
// （＝クリア済みかどうかの判定は「そのステージ番号が、これまでの最高クリア番号以下か」で決まる）
public static class StageProgress
{
    private const string HighestClearedKey = "HighestClearedStage";

    // これまでにクリアした最高のステージ番号
    // デフォルトは0（＝「Stage Index 0（スタート地点）は最初からクリア済み」として扱う）。
    // これにより、Stage Index 1（実質的な最初の挑戦できるステージ）が
    // 「1つ前がクリアされていない」という理由で永久にロックされることを防ぐ。
    public static int GetHighestCleared()
    {
        return PlayerPrefs.GetInt(HighestClearedKey, 0);
    }

    // 指定したステージがクリア済みかどうか
    // （最高クリア番号以下なら「クリア済み」とみなす＝一直線の進行を想定）
    public static bool IsCleared(int stageIndex)
    {
        return stageIndex <= GetHighestCleared();
    }

    // 指定したステージをクリア済みとして記録する
    // Goal.cs側で既にこの処理と同じことをPlayerPrefsに直接書いているので、
    // Goal.csを使う場合はこちらを呼ばなくてもStageProgress.IsCleared/IsUnlockedは正しく動く。
    public static void ClearStage(int stageIndex)
    {
        int current = GetHighestCleared();
        int next = Mathf.Max(current, stageIndex);
        PlayerPrefs.SetInt(HighestClearedKey, next);
        PlayerPrefs.Save();
        Debug.Log($"StageProgress: HighestClearedStage を {next} に更新しました。");
    }

    // 指定したステージが「解放されていて挑戦できる状態」かどうか
    // ルール：最初のステージ(0)は常に解放。それ以外は「1つ前のステージがクリア済み」なら解放。
    public static bool IsUnlocked(int stageIndex)
    {
        if (stageIndex <= 0) return true;
        return IsCleared(stageIndex - 1);
    }

    // 進行状況を全部リセットする
    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(HighestClearedKey);
        PlayerPrefs.Save();
        Debug.Log("StageProgress: 進行状況をリセットしました。");
    }

#if UNITY_EDITOR
    // Unityエディタのメニューに「Tools > Reset Stage Progress」を追加する。
    // 再生していなくても、エディタ上でいつでも手動でリセットできる。
    [UnityEditor.MenuItem("Tools/Reset Stage Progress")]
    private static void ResetFromMenu()
    {
        ResetAll();
        Debug.Log("エディタメニューからステージ進行状況をリセットしました。");
    }
#endif
}