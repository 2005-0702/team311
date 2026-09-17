using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// ゲーム全体のBGM・SE(効果音)をまとめて管理するクラス。
//
// 【特徴】シーンに何も置く必要がない。
// ゲーム開始時に自動でオブジェクトを1つ生成して常駐させるので、
// チームでシーンファイルを触ることによるコンフリクトが起きない。
//
// 【音声ファイルの置き方】
// 次のフォルダを作って、その中にAudioClipを置く：
//   Assets/Resources/Audio/BGM/  ← BGM用（例：StageSelect.mp3, StagePlay.mp3）
//   Assets/Resources/Audio/SE/   ← 効果音用（例：Jump.wav, Clear.wav）
// ファイル名がそのまま呼び出す時の名前になる（拡張子は書かない）。
//
// 【呼び出し方】どのスクリプトからでもこう書くだけでいい：
//   AudioManager.PlaySE("Jump");
//   AudioManager.PlayBGM("StagePlay");
//
// 【シーンに応じた自動BGM切り替え】
// stageSelectSceneNamesに書いてあるシーン名なら自動でBGM("StageSelect")、
// それ以外のシーンでは自動でBGM("StagePlay")を流す。
// 手動でPlayBGMを呼んでいなくても、シーン切り替えだけで自動的に鳴る。
public static class AudioManager
{
    // ここに書いたシーン名は「ステージセレクト」として扱う。それ以外は全部「ステージプレイ中」扱い。
    private static readonly string[] StageSelectSceneNames = { "StageSelect" };

    private const string BgmResourcePath = "Audio/BGM/";
    private const string SeResourcePath = "Audio/SE/";

    private static AudioRunner runner;
    private static readonly Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();
    private static string currentBgmName = null;

    public static float BgmVolume = 0.5f;
    public static float SeVolume = 0.8f;

    // ゲーム開始時に自動で1回だけ呼ばれ、常駐用オブジェクトを作る
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (runner != null) return;

        var go = new GameObject("AudioManager (auto-generated)");
        runner = go.AddComponent<AudioRunner>();
        Object.DontDestroyOnLoad(go);

        SceneManager.sceneLoaded += (scene, mode) => UpdateBgmForScene(scene.name);
        UpdateBgmForScene(SceneManager.GetActiveScene().name);
    }

    // シーン名を見て、自動でBGMを切り替える（同じカテゴリのままなら何もしない＝途切れさせない）
    private static void UpdateBgmForScene(string sceneName)
    {
        bool isStageSelect = System.Array.IndexOf(StageSelectSceneNames, sceneName) >= 0;
        string targetBgm = isStageSelect ? "StageSelect" : "StagePlay";

        if (targetBgm == currentBgmName) return;

        PlayBGM(targetBgm);
    }

    // BGMを再生する（ループ再生。同じ曲がすでに流れていれば何もしない）
    public static void PlayBGM(string clipName)
    {
        if (runner == null) Bootstrap();

        AudioClip clip = LoadClip(BgmResourcePath, clipName);
        if (clip == null)
        {
            Debug.LogWarning($"AudioManager: BGM '{clipName}' が見つかりません（Resources/Audio/BGM/{clipName} を確認してください）。");
            return;
        }

        currentBgmName = clipName;
        runner.PlayBgm(clip, BgmVolume);
    }

    public static void StopBGM()
    {
        currentBgmName = null;
        if (runner != null) runner.StopBgm();
    }

    // 効果音を再生する（重なって鳴ってもOK）
    public static void PlaySE(string clipName)
    {
        if (runner == null) Bootstrap();

        AudioClip clip = LoadClip(SeResourcePath, clipName);
        if (clip == null)
        {
            Debug.LogWarning($"AudioManager: SE '{clipName}' が見つかりません（Resources/Audio/SE/{clipName} を確認してください）。");
            return;
        }

        runner.PlaySe(clip, SeVolume);
    }

    private static AudioClip LoadClip(string folder, string clipName)
    {
        string key = folder + clipName;
        if (clipCache.TryGetValue(key, out AudioClip cached))
        {
            return cached;
        }

        AudioClip loaded = Resources.Load<AudioClip>(key);
        clipCache[key] = loaded; // 見つからなくてもnullをキャッシュして、毎回検索し直さないようにする
        return loaded;
    }

    // 実際にAudioSourceを持って再生するための内部コンポーネント（外部からは触らない）
    private class AudioRunner : MonoBehaviour
    {
        private AudioSource bgmSource;
        private AudioSource seSource;

        void Awake()
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;

            seSource = gameObject.AddComponent<AudioSource>();
            seSource.loop = false;
            seSource.playOnAwake = false;
        }

        public void PlayBgm(AudioClip clip, float volume)
        {
            bgmSource.clip = clip;
            bgmSource.volume = volume;
            bgmSource.Play();
        }

        public void StopBgm()
        {
            bgmSource.Stop();
        }

        public void PlaySe(AudioClip clip, float volume)
        {
            seSource.PlayOneShot(clip, volume);
        }
    }
}
