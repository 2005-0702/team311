using System.Collections.Generic;
using UnityEngine;

// 効果音(SE)をまとめて管理するクラス。
//
// 【特徴】シーンに何も置く必要がない。
// ゲーム開始時に自動でオブジェクトを1つ生成して常駐させるので、
// チームでシーンファイルを触ることによるコンフリクトが起きない。
//
// 【重要】BGMはこのクラスでは扱わない。BGMは BGMManager.cs 側が担当する。
// （以前はこのクラスでもBGMを再生していたが、BGMManager.csと二重に動いて
// 競合してしまうため、SE専用に整理した）
//
// 【音声ファイルの置き方】
// Resourcesフォルダの中に、次の構成でAudioClipを置く：
//   Resources/Audio/SE/   ← 効果音用（例：Jump.wav）
// ファイル名がそのまま呼び出す時の名前になる（拡張子は書かない）。
//
// 【呼び出し方】どのスクリプトからでもこう書くだけでいい：
//   AudioManager.PlaySE("Jump");
public static class AudioManager
{
    private const string SeResourcePath = "Audio/SE/";

    private static AudioRunner runner;
    private static readonly Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

    public static float SeVolume = 0.8f;

    // ゲーム開始時に自動で1回だけ呼ばれ、常駐用オブジェクトを作る
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (runner != null) return;

        var go = new GameObject("AudioManager (auto-generated, SE専用)");
        runner = go.AddComponent<AudioRunner>();
        Object.DontDestroyOnLoad(go);
        Debug.Log("AudioManager: 起動しました（SE専用。BGMはBGMManagerが担当）。");
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
        if (clipCache.TryGetValue(key, out AudioClip cached) && cached != null)
        {
            return cached;
        }

        AudioClip loaded = Resources.Load<AudioClip>(key);
        if (loaded != null)
        {
            clipCache[key] = loaded; // 見つかった時だけキャッシュする（見つからなかった時は次回また探す）
        }
        return loaded;
    }

    // 実際にAudioSourceを持って再生するための内部コンポーネント（外部からは触らない）
    private class AudioRunner : MonoBehaviour
    {
        private AudioSource seSource;

        void Awake()
        {
            seSource = gameObject.AddComponent<AudioSource>();
            seSource.loop = false;
            seSource.playOnAwake = false;
            seSource.spatialBlend = 0f; // 2D音声（距離で小さくならない）
        }

        public void PlaySe(AudioClip clip, float volume)
        {
            seSource.PlayOneShot(clip, volume);
        }
    }
}