using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour
{
    [Header("開く動き")]
    public Vector3 openOffset = new Vector3(0, 3f, 0); // 上にスライドして開く
    public float openSpeed = 2f;

    [Header("演出")]
    public GameObject lockedIcon; // 鍵がないときに表示するアイコン（任意）

    private bool isOpen = false;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Collider blockCollider; // 物理的にふさぐ用

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + openOffset;
        blockCollider = GetComponent<Collider>();
    }

    // ドアの手前にある「トリガー判定用」の子オブジェクトに付けるか、
    // ドア本体のColliderをトリガーにして使う
    private void OnTriggerEnter(Collider other)
    {
        if (isOpen) return;

        Player player = other.GetComponent<Player>();
        if (player == null) player = other.GetComponentInParent<Player>();

        if (player == null) return;

        if (player.HasKey)
        {
            player.UseKey(); // 鍵を消費
            StartCoroutine(OpenDoor());
        }
        else
        {
            // 鍵がないときの反応
            if (lockedIcon != null) lockedIcon.SetActive(true);
            Debug.Log("鍵が必要です！");
        }
    }

    IEnumerator OpenDoor()
    {
        isOpen = true;
        if (lockedIcon != null) lockedIcon.SetActive(false);
        if (blockCollider != null) blockCollider.enabled = false; // 通れるようにする

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            transform.position = Vector3.Lerp(closedPosition, openPosition, t);
            yield return null;
        }
    }
}