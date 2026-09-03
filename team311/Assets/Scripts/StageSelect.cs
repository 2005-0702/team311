using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class StageSelect : MonoBehaviour
{
    [Header("開始ステージ")]
    public StagePoint startStage;

    [Header("移動速度")]
    public float moveSpeed = 3f;

    [Header("フェード時間（秒）")]
    public float fadeDuration = 1.0f;

    [Header("フェード後の待機時間（秒）")]
    public float revealDelay = 2.0f;

    private StagePoint currentStage;
    private StagePoint targetStage;
    private bool isMoving;
    bool IsLoading = false;

    // フェード用
    private GameObject _fadeCanvasGO;
    private Image _fadeImage;
    private Material _fadeMaterial;
    private Vector2 _fadeCenter = new Vector2(0.5f, 0.5f);

    void Start()
    {
        if (startStage == null)
        {
            Debug.LogError("開始ステージ（startStage）が設定されていません！");
            return;
        }

        currentStage = startStage;
        transform.position = currentStage.transform.position;
    }

    void Update()
    {
        // 移動中は入力を受け付けない
        if (isMoving)
        {
            MovePlayer();
            return;
        }

        // 行き先を決定する（独立したif文にすることで入力をクリアに）
        if (Input.GetKeyDown(KeyCode.RightArrow)) MoveTo(currentStage.right, "右");
        if (Input.GetKeyDown(KeyCode.LeftArrow)) MoveTo(currentStage.left, "左");
        if (Input.GetKeyDown(KeyCode.UpArrow)) MoveTo(currentStage.up, "上");
        if (Input.GetKeyDown(KeyCode.DownArrow)) MoveTo(currentStage.down, "下");

        // ステージ決定
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!string.IsNullOrEmpty(currentStage.sceneName) && !IsLoading)
            {
                Debug.Log("Scene読み込み(フェード開始) : " + currentStage.sceneName);
                StartFadeAndLoad(currentStage.sceneName);
            }
            else
            {
                Debug.LogWarning($"{currentStage.name} にシーン名が設定されていません。");
            }
        }
    }

    // デバッグしやすくするために、どの方向へ進もうとしたか引数を追加
    void MoveTo(StagePoint next, string direction)
    {
        if (next == null)
        {
            // 動かない原因が「次のステージが未設定だから」だと分かるようにする
            Debug.LogWarning($"{currentStage.name} から見て 【{direction}】 にはステージが繋がっていません。");
            return;
        }

        targetStage = next;
        isMoving = true;
    }

    void MovePlayer()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetStage.transform.position,
            moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetStage.transform.position) < 5.0f)
        {
            transform.position = targetStage.transform.position;

            currentStage = targetStage;
            targetStage = null;
            isMoving = false;

            // 到着したステージの名前と、そこから移動できる方向をログに出す
            Debug.Log($"現在地更新：【{currentStage.name}】に到着しました。");
        }
    }

    private void StartFadeAndLoad(string sceneName)
    {
        if (IsLoading) return;
        IsLoading = true;

        CreateFadeCanvasIfNeeded();

        // フェード中心を画面中央に固定（ユーザーの要望）
        _fadeCenter = new Vector2(0.5f, 0.5f);
        if (_fadeMaterial != null)
        {
            _fadeMaterial.SetVector("_Center", new Vector4(_fadeCenter.x, _fadeCenter.y, 0f, 0f));
            _fadeImage.material = _fadeMaterial;
        }

        // フェード処理は FadeController に実行させる（StageSelect がシーン遷移で破棄されても継続するように）
        var controller = _fadeCanvasGO.GetComponent<FadeController>();
        if (controller == null) controller = _fadeCanvasGO.AddComponent<FadeController>();
        controller.BeginFade(sceneName, _fadeMaterial, _fadeImage, _fadeCenter, fadeDuration, revealDelay);
    }

    private void CreateFadeCanvasIfNeeded()
    {
        if (_fadeCanvasGO != null) return;

        _fadeCanvasGO = new GameObject("FadeCanvas");
        var canvas = _fadeCanvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        _fadeCanvasGO.AddComponent<CanvasGroup>();

        // 永続化してシーン切替後もフェードUIを保持する
        DontDestroyOnLoad(_fadeCanvasGO);

        var imgGO = new GameObject("FadeImage");
        imgGO.transform.SetParent(_fadeCanvasGO.transform, false);
        _fadeImage = imgGO.AddComponent<Image>();
        _fadeImage.color = Color.white;
        _fadeImage.raycastTarget = false;

        var rt = imgGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // カスタムシェーダーを探してマテリアルを作成
        Shader shader = Shader.Find("UI/CircularMask");
        if (shader != null)
        {
            _fadeMaterial = new Material(shader);
            // 初期値：穴が画面全体（見える）、オーバーレイ透明
            _fadeMaterial.SetFloat("_HoleRadius", 1f);
            _fadeMaterial.SetFloat("_Softness", 0.02f);
            _fadeMaterial.SetFloat("_OverlayAlpha", 0f);

            // フェード中心を画面中央に固定（プレイヤー中心を使わない）
            _fadeCenter = new Vector2(0.5f, 0.5f);
            _fadeMaterial.SetVector("_Center", new Vector4(_fadeCenter.x, _fadeCenter.y, 0f, 0f));
            _fadeImage.material = _fadeMaterial;
        }
        else
        {
            // フォールバック：全面黒フェード（従来通り）
            _fadeImage.color = new Color(0f, 0f, 0f, 0f);
            _fadeMaterial = null;
        }
    }

    // FadeController を同ファイルに定義：フェードのコルーチンはここで実行（DontDestroyOnLoad のオブジェクト上）
    public class FadeController : MonoBehaviour
    {
        private Material _material;
        private Image _image;
        private Vector2 _center;
        private float _fadeDuration;
        private float _revealDelay;

        public void BeginFade(string sceneName, Material material, Image image, Vector2 center, float fadeDuration, float revealDelay)
        {
            _material = material;
            _image = image;
            // 常に画面中央に固定する
            _center = new Vector2(0.5f, 0.5f);
            _fadeDuration = Mathf.Max(0.001f, fadeDuration);
            _revealDelay = Mathf.Max(0f, revealDelay);

            // このオブジェクト自体を破棄されないようにする
            DontDestroyOnLoad(this.gameObject);

            // 初期マテリアルセンター反映
            if (_material != null)
            {
                _material.SetVector("_Center", new Vector4(_center.x, _center.y, 0f, 0f));
            }

            StartCoroutine(RunFadeSequence(sceneName));
        }

        private IEnumerator RunFadeSequence(string sceneName)
        {
            // 非同期ロード開始（遷移は保留）
            var async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            async.allowSceneActivation = false;

            // 縮小フェーズ：丸い穴が縮んで画面が暗くなる（中心は画面中央）
            float t = 0f;
            while (t < _fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                float norm = Mathf.Clamp01(t / _fadeDuration);

                if (_material != null)
                {
                    float hole = Mathf.Lerp(1f, 0f, norm);
                    float overlay = Mathf.Lerp(0f, 1f, norm);
                    _material.SetFloat("_HoleRadius", hole);
                    _material.SetFloat("_OverlayAlpha", overlay);
                }
                else if (_image != null)
                {
                    _image.color = new Color(0f, 0f, 0f, norm);
                }

                yield return null;
            }

            if (_material != null)
            {
                _material.SetFloat("_HoleRadius", 0f);
                _material.SetFloat("_OverlayAlpha", 1f);
            }
            else if (_image != null)
            {
                _image.color = Color.black;
            }

            // 待機（リアルタイム）
            float waited = 0f;
            while (waited < _revealDelay)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            // シーンが読み込み 0.9 になるのを待つ
            while (async.progress < 0.9f)
            {
                yield return null;
            }

            // シーンをアクティブにする
            async.allowSceneActivation = true;
            while (!async.isDone)
            {
                yield return null;
            }

            // 新シーン表示後、拡大フェーズ（穴が拡大してフェードイン）
            // 中心は引き続き画面中央に固定
            if (_material != null)
            {
                _material.SetVector("_Center", new Vector4(_center.x, _center.y, 0f, 0f));
            }

            t = 0f;
            while (t < _fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                float norm = Mathf.Clamp01(t / _fadeDuration);

                if (_material != null)
                {
                    float hole = Mathf.Lerp(0f, 1f, norm);
                    float overlay = Mathf.Lerp(1f, 0f, norm);
                    _material.SetFloat("_HoleRadius", hole);
                    _material.SetFloat("_OverlayAlpha", overlay);
                }
                else if (_image != null)
                {
                    _image.color = new Color(0f, 0f, 0f, 1f - norm);
                }

                yield return null;
            }

            if (_material != null)
            {
                _material.SetFloat("_HoleRadius", 1f);
                _material.SetFloat("_OverlayAlpha", 0f);
            }
            else if (_image != null)
            {
                _image.color = Color.clear;
            }

            // フェード用オブジェクトを破棄
            if (this.gameObject != null)
            {
                Destroy(this.gameObject);
            }
        }
    }
}