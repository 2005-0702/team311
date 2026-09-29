using UnityEngine;

/// <summary>
/// プレス機の下面に触れたプレイヤーを潰すスクリプト。
/// プレス機オブジェクトにアタッチしてください。
///
/// 挟まれている間（OnCollisionStay）は毎フレーム「潰れた状態を維持」するよう
/// Player側に伝え続けるので、途中で回復タイマーが動き出すことはない。
/// プレスから完全に離れた時（OnCollisionExit）に初めて、元に戻るまでの
/// カウントダウンが始まる。
/// </summary>
public class PressSquash : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"PressSquash: OnCollisionEnter with {collision.gameObject.name}");
        TrySquash(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        TrySquash(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log($"PressSquash: OnCollisionExit with {collision.gameObject.name}");

        Player player = GetPlayer(collision.gameObject);
        if (player != null)
        {
            // 離れたので、ここで初めて回復までのカウントダウンを開始する
            Debug.Log("PressSquash: ReleaseSquashを呼びます");
            player.ReleaseSquash();
        }
    }

    private void TrySquash(Collision collision)
    {
        if (collision.contacts.Length == 0) return;

        Player player = GetPlayer(collision.gameObject);
        if (player == null) return;

        // プレイヤーがこのオブジェクトの下面に当たっているか確認
        bool hitFromAbove = collision.contacts[0].point.y < transform.position.y;

        if (hitFromAbove)
        {
            player.Squash();
        }
    }

    private Player GetPlayer(GameObject obj)
    {
        Player player = obj.GetComponent<Player>();
        if (player == null) player = obj.GetComponentInParent<Player>();
        return player;
    }
}