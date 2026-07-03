using UnityEngine;

public class Key : MonoBehaviour
{
    // この鍵を取得すると消えるブロック
    [SerializeField] private GameObject lockedBlock;
    private void OnTriggerEnter(Collider other)
    {
        // 触れたのがプレイヤーかどうか確認
        Player player = other.GetComponent<Player>();
        if (player == null) player = other.GetComponentInParent<Player>();

        if (player != null)
        {
            // プレイヤーに鍵を渡す
            player.PickUpKey();

            // 鍵付きブロックを消す
            if (lockedBlock != null)
            {
                Destroy(lockedBlock);
            }

            // ?? ここで「チャキーン！」といったSE（効果音）を鳴らすと気持ちいいです

            // 拾われたので、鍵オブジェクト自体を画面から消す
            Destroy(gameObject);
        }
    }
}