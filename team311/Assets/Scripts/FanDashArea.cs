using System;
using UnityEngine;

public class FanDashArea : MonoBehaviour
{
    public float dashForce = 20f; // ダッシュの強さ
    public Vector3 dashDirection = Vector3.forward; // 風の向き

    private void OnTriggerEnter(Collider other)
    {
        // Player に触れたら Player のダッシュ関数を呼ぶ
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            // プレイヤーが扇風機のどちら側にいるか判定
            float direction = Mathf.Sign(player.transform.position.x - transform.position.x);

            // direction が正 → プレイヤーは右側にいる → 右へダッシュ
            // direction が負 → プレイヤーは左側にいる → 左へダッシュ
            player.StartDash(direction);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
