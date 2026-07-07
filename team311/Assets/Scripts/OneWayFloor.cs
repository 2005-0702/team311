using UnityEngine;

public class OneWayFloor : MonoBehaviour
{
    private Collider floorCollider;

    void Start()
    {
        // 自身のコライダーを取得
        floorCollider = GetComponent<Collider>();

        // 最初は「空気（Trigger）」にしておくことで、下から確実に侵入できるようにする！
        if (floorCollider != null)
        {
            floorCollider.isTrigger = true;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 床のワールド座標での中心位置を取得
            Vector3 floorCenter = transform.position;
            // プレイヤーのワールド座標での中心位置を取得
            Vector3 playerPos = other.transform.position;

            // 判定をより確実に調整：プレイヤーの足元が床の中心より「上」に抜けたら
            // (playerPos.y - 0.2f でトカゲくんの足元の位置を大体計算しています)
            bool isPlayerAbove = (playerPos.y - 0.2f) > floorCenter.y;

            if (isPlayerAbove)
            {
                // プレイヤーが上に抜けたら、Triggerを「OFF」にしてガチッとした固い床にする！
                floorCollider.isTrigger = false;
            }
            else
            {
                // 下にいる間は、Triggerを「ON」にして空気のままにする
                floorCollider.isTrigger = true;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // プレイヤーが床から完全に離れたら、再び下から通れるようにトリガー（空気）に戻す
            floorCollider.isTrigger = true;
        }
    }
}