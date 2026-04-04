using UnityEngine;

/// <summary>
/// プレス機の下面に触れたプレイヤーを潰すスクリプト。
/// プレス機オブジェクトにアタッチしてください。
/// </summary>
public class PressSquash: MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Player player = collision.gameObject.GetComponent<Player>();
        if (player == null) player = collision.gameObject.GetComponentInParent<Player>();

        if (player != null)
        {
            // プレイヤーがこのオブジェクトの下面に当たったか確認
            bool hitFromAbove = collision.contacts[0].point.y < transform.position.y;

            if (hitFromAbove)
            {
                player.Squash();
            }
        }
    }
}