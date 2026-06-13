using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    // 💡 インスペクターから「足場用」のコライダーを直接指定できるようにする
    [Header("実体化させる足場用のコライダーをドラッグしてね")]
    [SerializeField] private Collider solidCollider;

    private Transform playerTransform;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // 💡 もしインスペクターでの設定を忘れていた場合の保険
        if (solidCollider == null)
        {
            Debug.LogError($"{gameObject.name} の OneWayPlatform スクリプトに、足場用のコライダーがセットされていません！");
        }
    }

    void Update()
    {
        if (playerTransform == null || solidCollider == null) return;

        // 床（このオブジェクト全体）の上面のY座標
        float platformTopY = transform.position.y + (transform.localScale.y / 2f);
        float playerBottomY = playerTransform.position.y;

        // 💡「足場用コライダー」のisTriggerだけをコントロールする
        if (playerBottomY < platformTopY + 0.2f)
        {
            // プレイヤーが下にいる間は、足場用コライダーもすり抜けにする
            solidCollider.isTrigger = true;
        }
        else
        {
            // プレイヤーが上に抜けたら、足場用コライダーの実体を有効にする
            solidCollider.isTrigger = false;
        }
    }
}