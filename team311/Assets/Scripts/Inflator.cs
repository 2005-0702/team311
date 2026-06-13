using UnityEngine;

public class Inflator : MonoBehaviour
{
    private bool playerInZone = false;
    private Player playerScript;

    void Update()
    {
        //プレイヤーが範囲内にいて、かつEキーが押されたら空気を入れる
        if (playerInZone && Input.GetKeyDown(KeyCode.E))
        {
            if (playerScript != null)
            {
                playerScript.Inflate();
            }
        }
    }

    // プレイヤーが空気入れの前に来たら検知
    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player == null) player = other.GetComponentInParent<Player>();

        if (player != null)
        {
            playerInZone = true;
            playerScript = player;
            Debug.Log("空気入れの前に入った：Eキーで膨らむ");
        }
    }

    // プレイヤーが空気入れの前から離れたらリセット
    private void OnTriggerExit(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player == null) player = other.GetComponentInParent<Player>();

        if (player != null)
        {
            playerInZone = false;
            playerScript = null;
            Debug.Log("空気入れの前から離れた");
        }
    }
}