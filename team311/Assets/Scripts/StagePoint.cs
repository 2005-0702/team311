using UnityEngine;
using System.Collections;

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
    [Tooltip("未解放（挑戦できない）のときに表示するオブジェクト。例：赤い見た目のオブジェクト。自分自身を指定してもOK")]
    public GameObject lockedObject;
    [Tooltip("解放済み（挑戦できる）のときに表示するオブジェクト。例：青い見た目のオブジェクト。自分自身を指定してもOK")]
    public GameObject unlockedObject;
    [Tooltip("クリア済みのときに表示するオブジェクト。未設定ならunlockedObjectのまま表示。自分自身を指定してもOK")]
    public GameObject clearedObject;

    [Header("ステージ番号ラベル（PNG画像）")]
    [Tooltip("番号を表示するSpriteRenderer（このステージの子オブジェクトなどに置く）")]
    public SpriteRenderer labelRenderer;
    [Tooltip("表示したい番号のPNG画像（Texture TypeをSpriteにしてインポートしたもの）")]
    public Sprite labelSprite;
    [Tooltip("普段の大きさ（ローカルスケール倍率）")]
    public float labelNormalScale = 1f;
    [Tooltip("今いるステージのときの大きさ（ローカルスケール倍率）")]
    public float labelCurrentScale = 1.4f;
    [Tooltip("拡大・縮小にかかる時間（秒）")]
    public float labelAnimDuration = 0.25f;

    private Coroutine labelScaleCoroutine;

    // 状態 (外部から参照できるようにプロパティ風に公開)
    public bool IsLocked { get; private set; }
    public bool IsCleared { get; private set; }

    void Start()
    {
        if (labelRenderer != null)
        {
            if (labelSprite != null)
            {
                labelRenderer.sprite = labelSprite;
            }
            labelRenderer.transform.localScale = Vector3.one * labelNormalScale;
        }

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

        SetVisible(lockedObject, locked && !showCleared);
        SetVisible(unlockedObject, !locked && !showCleared);
        SetVisible(clearedObject, showCleared);
    }

    // 対象オブジェクトの表示/非表示を切り替える。
    // 対象が「自分自身（StagePointが付いているこのオブジェクト）」の場合は、
    // GameObjectを丸ごと無効化するとスクリプトごと止まってしまうため、
    // 代わりにRendererだけを消す（スクリプトの動作は止めない）。
    // 対象が別のオブジェクトなら、今まで通りSetActiveで切り替える。
    private void SetVisible(GameObject target, bool visible)
    {
        if (target == null) return;

        if (target == gameObject)
        {
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                r.enabled = visible;
            }
        }
        else
        {
            target.SetActive(visible);
        }
    }

    // 「今このステージにいるかどうか」を伝える。StageSelect側から、
    // プレイヤーが移動して到着した時・そこから離れた時に呼び出す。
    // ラベルの大きさを、パッと切り替わらないよう滑らかにアニメーションさせる。
    public void SetAsCurrent(bool isCurrent)
    {
        if (labelRenderer == null) return;

        float targetScale = isCurrent ? labelCurrentScale : labelNormalScale;

        if (labelScaleCoroutine != null)
        {
            StopCoroutine(labelScaleCoroutine);
        }
        labelScaleCoroutine = StartCoroutine(AnimateLabelScale(targetScale));
    }

    private IEnumerator AnimateLabelScale(float targetScale)
    {
        Transform labelTransform = labelRenderer.transform;
        Vector3 startScale = labelTransform.localScale;
        Vector3 endScale = Vector3.one * targetScale;

        float t = 0f;
        while (t < labelAnimDuration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / labelAnimDuration);

            // イーズアウト（だんだん減速しながら目的の大きさで自然に止まる）
            float eased = 1f - Mathf.Pow(1f - normalized, 3f);
            labelTransform.localScale = Vector3.Lerp(startScale, endScale, eased);

            yield return null;
        }

        labelTransform.localScale = endScale;
        labelScaleCoroutine = null;
    }
}