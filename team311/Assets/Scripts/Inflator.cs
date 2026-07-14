using UnityEngine;

public class Inflator : MonoBehaviour
{
    // EキーのUI（WorldCanvasやImage）を設定
    public GameObject eKeyUI;

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

                // EキーのUIを消す
                if (eKeyUI != null)
                {
                    eKeyUI.SetActive(false);
                }
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

            // まだUIが表示されていなければ表示する
            if (eKeyUI != null)
            {
                eKeyUI.SetActive(true);
            }

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

            // 範囲外に出たらUIを消す
            if (eKeyUI != null)
            {
                eKeyUI.SetActive(false);
            }

            Debug.Log("空気入れの前から離れた");
        }
    }
}