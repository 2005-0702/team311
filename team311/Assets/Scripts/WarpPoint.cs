using UnityEngine;

public class WarpPoint : MonoBehaviour
{
    [Header("ワープ先")]
    public WarpPoint linkedWarp;   // ペアになるワープポイントをDrag&Drop

    [Header("出現位置の調整")]
    public Vector3 exitOffset = new Vector3(0, 0.5f, 1f); // 出口の少し前に出す

    [Header("クールダウン（連続ワープ防止）")]
    public float cooldownTime = 0.5f;
    private float lastWarpTime = -999f;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // クールダウン中なら無視（出た瞬間に戻りループするのを防ぐ）
        if (Time.time - lastWarpTime < cooldownTime) return;

        Warp(other.gameObject);
    }

    void Warp(GameObject player)
    {
        if (linkedWarp == null)
        {
            Debug.LogWarning("LinkedWarpが設定されていません: " + gameObject.name);
            return;
        }

        // プレイヤーを移動させる
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false; // CharacterController使用時は一旦切る

        player.transform.position = linkedWarp.transform.position + linkedWarp.exitOffset;

        if (cc != null) cc.enabled = true;

        // 両方のワープにクールダウンをかける（出た側にすぐ戻されないように）
        lastWarpTime = Time.time;
        linkedWarp.lastWarpTime = Time.time;
    }
}