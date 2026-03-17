using UnityEngine;
using System.Collections;

/// <summary>
/// プレイヤーが近くで E を押すと持ち上げられる箱のスクリプト。
/// セットアップ手順は下に記載。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Box : MonoBehaviour
{
    [Tooltip("プレイヤーの HoldPoint をアサイン。未設定なら Player の子オブジェクト 'HoldPoint' を探します。")]
    public Transform holdPoint;

    [Tooltip("プレイヤーが箱を持てる最大距離（トリガーがないセットアップ向けの保険）。")]
    public float maxPickupDistance = 2f;

    [Tooltip("拾ったときに前方向へ少し投げる力（ドロップ時の慣性）。")]
    public float dropForwardImpulse = 1.5f;

    Rigidbody rb;

    // プレイヤーが近くにいるか
    bool playerNearby = false;
    Transform nearbyPlayer;

    // 現在プレイヤーが持っているか
    bool isHeld = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 近くにプレイヤーがいて E 押下でトグル（持つ/置く）
        if (nearbyPlayer != null && Input.GetKeyDown(KeyCode.E))
        {
            if (!isHeld)
            {
                TryPickup(nearbyPlayer);
            }
            else
            {
                Drop(nearbyPlayer);
            }
        }

        // holdPoint が未指定かつ箱が保持されている場合、position を holdPoint に合わせる（安全策）
        if (isHeld && holdPoint != null)
        {
            transform.position = holdPoint.position;
            transform.rotation = holdPoint.rotation;
        }
    }

    void TryPickup(Transform player)
    {
        // 距離チェック（トリガーを使わない場合の保険）
        if (Vector3.Distance(transform.position, player.position) > maxPickupDistance)
            return;

        // HoldPoint を解決
        if (holdPoint == null)
        {
            var hp = player.Find("HoldPoint");
            if (hp != null) holdPoint = hp;
        }

        // 物理を停止して親子付け
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.SetParent(holdPoint != null ? holdPoint : player);
        // holdPoint があればそこに合わせる、なければプレイヤー前方にオフセット
        if (holdPoint != null)
        {
            transform.position = holdPoint.position;
            transform.rotation = holdPoint.rotation;
        }
        else
        {
            transform.localPosition = new Vector3(0f, 0.6f, 0.8f);
            transform.localRotation = Quaternion.identity;
        }

        isHeld = true;
    }

    void Drop(Transform player)
    {
        // 親子解除して物理復帰
        transform.SetParent(null);
        rb.isKinematic = false;

        // 少し前方に力を与えて自然に落ちるようにする（プレイヤーが動いている場合の慣性）
        var forward = player.forward;
        rb.AddForce(forward * dropForwardImpulse, ForceMode.Impulse);

        isHeld = false;
    }

    // --- 近接検出：推奨は「箱にトリガー用のコライダ（isTrigger=true）を追加」 ---
    // このコール群は、箱側にトリガーコライダ（例：SphereCollider isTrigger=true）を追加して、
    // プレイヤー（タグ "Player" を想定）で検出する想定です。

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            nearbyPlayer = other.transform;
            playerNearby = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isHeld)
            {
                nearbyPlayer = null;
                playerNearby = false;
            }
            // もしプレイヤーが去ったときに箱を持っていたら特別扱いはしない（意図次第で変更可）
        }
    }
}