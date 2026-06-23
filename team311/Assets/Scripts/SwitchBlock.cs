using UnityEngine;

public class SwitchBlock : MonoBehaviour
{
    public enum BlockColor { Red, Blue }

    [Header("ブロックの色設定")]
    public BlockColor blockColor;

    private Renderer blockRenderer;
    private Collider blockCollider;
    private Material blockMaterial;
    private Color originalColor;

    void Awake()
    {
        blockRenderer = GetComponent<Renderer>();
        blockCollider = GetComponent<Collider>();

        // MAYAモデルの半透明マテリアルを取得
        blockMaterial = blockRenderer.material;
        originalColor = blockMaterial.color;
    }
           // マネージャーから「今は赤がアクティブだよ(isRedActive)」と教えてもらう関数
        public void RefreshState(bool isRedActive)
        {
            // ==========================================
            // 修正：あべこべを解消するための判定に書き換え
            // ==========================================
            bool shouldBeSolid = false;

            if (blockColor == BlockColor.Red)
            {
                // 自分が赤ブロックなら、マネージャーが「赤アクティブ」の時に実体化する！
                shouldBeSolid = isRedActive;
            }
            else if (blockColor == BlockColor.Blue)
            {
                // 自分が青ブロックなら、マネージャーが「赤アクティブではない（＝青アクティブ）」の時に実体化する！
                shouldBeSolid = !isRedActive;
            }

            // --- 以下はそのまま（半透明と不透明の切り替え） ---
            if (shouldBeSolid)
            {
                Color newColor = originalColor;
                newColor.a = 1.0f; // 不透明
                blockMaterial.color = newColor;
                blockCollider.enabled = true; // ぶつかる
                Debug.Log(gameObject.name + " を【不透明】にしました"); //原因を突き止めるためのログ
            }
            else
            {
                Color newColor = originalColor;
                newColor.a = 0.25f; // 半透明
                blockMaterial.color = newColor;
                blockCollider.enabled = false; // すり抜ける
                Debug.Log(gameObject.name + " を【半透明】にしました"); //原因を突き止めるためのログ
            }
        }
    }