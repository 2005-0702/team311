using UnityEngine;

// 一方通行の床：下からはすり抜けられるが、上に乗ると固い床になる。
//
// 【重要】isTriggerを「プレイヤーが重なった状態のまま」切り替えると、
// 物理エンジンがめり込みを解消しようとして押し出し、プレイヤーが
// ちょっと跳ねる（ポップする）原因になる。
// これを避けるため、実際に乗るためのメインのコライダーは常にソリッド
// （isTrigger = false）のままにし、プレイヤーの検知だけを別の
// トリガー用コライダーで行う。衝突のON/OFFはisTriggerではなく
// Physics.IgnoreCollisionで切り替える（重なったまま切り替えても
// isTrigger切り替えほど激しいポップが起きにくい）。
[RequireComponent(typeof(Collider))]
public class OneWayFloor : MonoBehaviour
{
    [Tooltip("プレイヤーのタグ名")]
    public string playerTag = "Player";

    [Tooltip("プレイヤーの足元とみなすオフセット（プレイヤーのpivotからどれくらい下が足元か）")]
    public float footOffset = 0.2f;

    [Tooltip("プレイヤー検知用トリガーを、本体のコライダーより上下にどれだけ広げるか")]
    public float detectionPadding = 0.6f;

    // 実際にプレイヤーが乗るための、常にソリッドなコライダー（isTriggerは切り替えない）
    private Collider solidCollider;

    // プレイヤーを検知するためだけの、自動生成したトリガー用コライダー
    private BoxCollider detectionCollider;

    void Awake()
    {
        solidCollider = GetComponent<Collider>();
        solidCollider.isTrigger = false; // ここは常にfalseのまま触らない

        SetupDetectionCollider();
    }

    // プレイヤー検知専用の、少し大きめのトリガーコライダーを自動で追加する
    private void SetupDetectionCollider()
    {
        detectionCollider = gameObject.AddComponent<BoxCollider>();
        detectionCollider.isTrigger = true;

        // メインのコライダーがBoxColliderならサイズ・中心を参考にする。
        // それ以外の形状の場合は、ワールドのbounds(の大きさ)を目安に代用する。
        if (solidCollider is BoxCollider mainBox)
        {
            detectionCollider.center = mainBox.center;
            Vector3 size = mainBox.size;
            size.y += detectionPadding;
            detectionCollider.size = size;
        }
        else
        {
            Vector3 worldSize = solidCollider.bounds.size;
            Vector3 localSize = new Vector3(
                worldSize.x / Mathf.Max(transform.lossyScale.x, 0.0001f),
                worldSize.y / Mathf.Max(transform.lossyScale.y, 0.0001f) + detectionPadding,
                worldSize.z / Mathf.Max(transform.lossyScale.z, 0.0001f)
            );
            detectionCollider.size = localSize;
            detectionCollider.center = Vector3.zero;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        // 床のワールド座標での中心位置
        Vector3 floorCenter = transform.position;
        // プレイヤーのワールド座標での中心位置
        Vector3 playerPos = other.transform.position;

        // プレイヤーの足元が床の中心より上に抜けたら「上に乗っている」とみなす
        bool isPlayerAbove = (playerPos.y - footOffset) > floorCenter.y;

        // isTriggerは一切変えず、衝突を無視するかどうかだけを切り替える。
        // 上にいる(isPlayerAbove)なら衝突させる(ignore=false)、
        // 下にいるなら衝突を無視してすり抜けさせる(ignore=true)。
        Physics.IgnoreCollision(solidCollider, other, !isPlayerAbove);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        // 完全に離れたら、次に下から近づいた時にまた通れるよう衝突無視に戻しておく
        Physics.IgnoreCollision(solidCollider, other, true);
    }
}