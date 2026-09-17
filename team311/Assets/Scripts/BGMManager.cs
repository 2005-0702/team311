using UnityEngine;
using UnityEngine.SceneManagement;

// シーンに応じてBGMを2種類に切り替えるスクリプト。
// 「ステージセレクト用」と「ステージプレイ中用」の2つを想定している。
//
// 使い方：
// 1. 最初に読み込まれるシーン（タイトルやステージセレクトなど）に、
//    空のGameObjectを1つ作り、名前を「BGMManager」などにする。
// 2. このスクリプトをAddComponentする。
// 3. インスペクタの「Stage Select Bgm」にステージセレクト用の曲、
//    「Stage Play Bgm」にステージプレイ中用の曲を設定する。
// 4. 「Stage Select Scene Names」に、ステージセレクトとして扱うシーン名を入れる
//    （デフォルトは "StageSelect" の1つ）。ここに書いていないシーン名は、
//    全部「ステージプレイ中」として扱われる。
//
// 同じカテゴリ（例：ステージ→次のステージ）のままシーンが切り替わっても、
// 曲は途切れずに流れ続ける。カテゴリが変わった時（例：ステージセレクット→ステージ）
// だけ、曲を止めて新しい方を再生し直す。
public class BGMManager : MonoBehaviour
{
    [Header("BGM設定")]
    [Tooltip("ステージセレクト画面で流す曲")]
    public AudioClip stageSelectBgm;
    [Tooltip("ステージをプレイしている間に流す曲")]
    public AudioClip stagePlayBgm;

    [Range(0f, 1f)]
    public float volume = 0.5f;

    [Header("シーン分類")]
    [Tooltip("ここに入れたシーン名は「ステージセレクト」として扱う。それ以外は全部「ステージプレイ中」扱いになる。")]
    public string[] stageSelectSceneNames = { "StageSelect" };

    private static BGMManager instance;
    private AudioSource audioSource;

    // 現在流しているカテゴリ（重複再生防止用）
    private enum BgmCategory { None, StageSelect, StagePlay }
    private BgmCategory currentCategory = BgmCategory.None;

    void Awake()
    {
        // すでに他のシーンから引き継がれたBGMManagerが存在するなら、
        // 自分（新しく読み込まれた方）は消して、重複再生を防ぐ
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        // 最初のシーン分もここで判定して再生する
        UpdateBgmForScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateBgmForScene(scene.name);
    }

    private void UpdateBgmForScene(string sceneName)
    {
        bool isStageSelect = System.Array.IndexOf(stageSelectSceneNames, sceneName) >= 0;
        BgmCategory targetCategory = isStageSelect ? BgmCategory.StageSelect : BgmCategory.StagePlay;

        // カテゴリが変わっていなければ、曲はそのまま流し続ける（途切れさせない）
        if (targetCategory == currentCategory) return;

        currentCategory = targetCategory;
        AudioClip clipToPlay = isStageSelect ? stageSelectBgm : stagePlayBgm;

        if (clipToPlay == null)
        {
            Debug.LogWarning($"BGMManager: {(isStageSelect ? "Stage Select Bgm" : "Stage Play Bgm")} が設定されていません。");
            audioSource.Stop();
            return;
        }

        audioSource.clip = clipToPlay;
        audioSource.Play();
    }
}
