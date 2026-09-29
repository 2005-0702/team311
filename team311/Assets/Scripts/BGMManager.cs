using UnityEngine;
using UnityEngine.SceneManagement;

// シーンに応じてBGMを切り替えるクラス。
// 「タイトル」「チュートリアル」「ステージセレクト」「ステージプレイ中」の4種類を想定している。
//
// 【特徴】AudioManagerと同じく、シーンに何も置く必要がない。
// ゲーム開始時に自動でオブジェクトを1つ生成して常駐させるので、
// 「どのシーンから再生したか」によって鳴らなくなる、ということが起きない。
//
// 【音声ファイルの置き方】
// Resourcesフォルダの中に、次の構成でAudioClipを置く：
//   Resources/Audio/BGM/Title.wav
//   Resources/Audio/BGM/Tutorial.wav
//   Resources/Audio/BGM/StageSelect.wav
//   Resources/Audio/BGM/StagePlay.wav
// （拡張子はwavでもmp3でもOK。ファイル名は上の通りにする）
//
// 【シーン分類】
// 下のTitleSceneNames等に、実際のシーン名を書いてください。
// どのリストにも当てはまらないシーンは、全部「ステージプレイ中」として扱う。
//
// 同じカテゴリのままシーンが切り替わっても、曲は途切れずに流れ続ける。
public static class BGMManager
{
    private const string BgmResourcePath = "Audio/BGM/";

    // ここに実際のシーン名を入れてください
    private static readonly string[] TitleSceneNames = { "TitleScene" };
    private static readonly string[] TutorialSceneNames = { "TutorialScene" };
    private static readonly string[] StageSelectSceneNames = { "StageSelect" };
    // 上のどれにも当てはまらないシーンは、全部「ステージプレイ中」扱いになる

    public static float Volume = 0.5f;

    private enum BgmCategory { None, Title, Tutorial, StageSelect, StagePlay }
    private static BgmCategory currentCategory = BgmCategory.None;

    private static BgmRunner runner;
    private static bool sceneHooked = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (runner != null) return;

        var go = new GameObject("BGMManager (auto-generated)");
        runner = go.AddComponent<BgmRunner>();
        Object.DontDestroyOnLoad(go);
        Debug.Log("BGMManager: 起動しました。");

        if (!sceneHooked)
        {
            SceneManager.sceneLoaded += (scene, mode) => UpdateBgmForScene(scene.name);
            sceneHooked = true;
        }
        UpdateBgmForScene(SceneManager.GetActiveScene().name);
    }

    private static void UpdateBgmForScene(string sceneName)
    {
        BgmCategory targetCategory = GetCategoryForScene(sceneName);
        Debug.Log($"BGMManager: シーン'{sceneName}' → カテゴリ'{targetCategory}'と判定しました。");

        // カテゴリが変わっていなければ、曲はそのまま流し続ける（途切れさせない）
        if (targetCategory == currentCategory) return;
        currentCategory = targetCategory;

        AudioClip clip = LoadClipForCategory(targetCategory);
        if (clip == null)
        {
            Debug.LogWarning($"BGMManager: {targetCategory} 用のBGM(Resources/Audio/BGM/{targetCategory})が見つかりません。");
            runner.Stop();
            return;
        }

        runner.Play(clip, Volume);
        Debug.Log($"BGMManager: '{clip.name}' を再生します。");
    }

    private static BgmCategory GetCategoryForScene(string sceneName)
    {
        if (System.Array.IndexOf(TitleSceneNames, sceneName) >= 0) return BgmCategory.Title;
        if (System.Array.IndexOf(TutorialSceneNames, sceneName) >= 0) return BgmCategory.Tutorial;
        if (System.Array.IndexOf(StageSelectSceneNames, sceneName) >= 0) return BgmCategory.StageSelect;
        return BgmCategory.StagePlay;
    }

    private static AudioClip LoadClipForCategory(BgmCategory category)
    {
        if (category == BgmCategory.None) return null;
        return Resources.Load<AudioClip>(BgmResourcePath + category);
    }

    private class BgmRunner : MonoBehaviour
    {
        private AudioSource source;

        void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
        }

        public void Play(AudioClip clip, float volume)
        {
            source.clip = clip;
            source.volume = volume;
            source.Play();
        }

        public void Stop()
        {
            source.Stop();
        }
    }
}