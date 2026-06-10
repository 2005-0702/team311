using UnityEngine;

public class SwitchBlock : MonoBehaviour
{
    public enum BlockColor { Red, Blue }

    [Header("このブロックの色設定")]
    public BlockColor myColor;

    private Collider2D myCollider2D; // 2Dゲームの場合
    private Collider myCollider3D;     // 3Dゲームの場合
    private MeshRenderer myRenderer;

    void Awake()
    {
        // コンポーネントを自動取得
        myCollider2D = GetComponent<Collider2D>();
        myCollider3D = GetComponent<Collider>();
        myRenderer = GetComponent<MeshRenderer>();
    }

    // マネージャーから呼び出されて、表示・非表示（判定ON/OFF）を切り替える
    public void RefreshState(bool isRedActive)
    {
        // 自分自身が表示されるべき条件を計算
        // (自分が赤でマネージャーも赤、または、自分が青でマネージャーも青)
        bool shouldBeActive = (myColor == BlockColor.Red && isRedActive) ||
                              (myColor == BlockColor.Blue && !isRedActive);

        // 見た目の透明度や表示を切り替える
        if (myRenderer != null)
        {
            // 分かりやすくするために、非アクティブ時は半透明にするか、完全に消す
            // 今回はシンプルに、非アクティブ時は完全に非表示（または半透明）にします
            Color color = myRenderer.material.color;
            color.a = shouldBeActive ? 1.0f : 0.2f; // ONならくっきり、OFFなら薄く
            myRenderer.material.color = color;
        }

        // 当たり判定の切り替え（これがないとすり抜けない）
        if (myCollider2D != null) myCollider2D.enabled = shouldBeActive;
        if (myCollider3D != null) myCollider3D.enabled = shouldBeActive;
    }
}