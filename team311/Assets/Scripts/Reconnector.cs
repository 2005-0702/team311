using UnityEngine;

/// <summary>
/// 上半身がこのエリアに入ると、下半身を消去して元のプレイヤーの姿に戻すスクリプト。
/// </summary>
public class Reconnector : MonoBehaviour
{
    [Tooltip("復活させる元のプレイヤーのプレハブをアサインしてください")]
    public GameObject playerPrefab;

    private void OnTriggerEnter(Collider other)
    {
        // 上半身（UpperBodyControllerが付いているもの）が触れたかチェック
        UpperBodyController upperBody = other.GetComponent<UpperBodyController>();
        if (upperBody == null) upperBody = other.GetComponentInParent<UpperBodyController>();

        if (upperBody != null)
        {
            Reconnect(upperBody);
        }
    }

    void Reconnect(UpperBodyController upper)
    {
        Debug.Log("Reconnecting Player...");

        // 1. シーン内にある下半身を探して消去
        LowerBodyMove lowerBody = Object.FindAnyObjectByType<LowerBodyMove>();
        if (lowerBody != null)
        {
            Destroy(lowerBody.gameObject);
        }

        // 2. カメラを引き継ぐ準備
        Camera cam = upper.GetComponentInChildren<Camera>();

        // 3. 元のプレイヤーをこの位置に生成
        if (playerPrefab != null)
        {
            GameObject newPlayer = Instantiate(playerPrefab, transform.position, transform.rotation);
            
            // カメラを新しいプレイヤーに付け替える
            if (cam != null)
            {
                cam.transform.SetParent(newPlayer.transform);
                // カメラの位置をリセット
                cam.transform.localPosition = new Vector3(0, 2, -5);
                cam.transform.localRotation = Quaternion.Euler(15, 0, 0);
            }
        }

        // 4. 上半身を消去
        Destroy(upper.gameObject);
    }
}
