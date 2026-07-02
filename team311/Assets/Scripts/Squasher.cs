using UnityEngine;

/// <summary>
/// 落下物やプレス機にアタッチするスクリプト。
/// MovingGimmickによる移動を検知してプレイヤーを押しつぶします。
/// </summary>
public class Squasher : MonoBehaviour
{
    [Tooltip("押しつぶすために必要な最低速度（0なら動いていなくても、触れるだけでOK）")]
    public float crushThreshold = 0.5f;

    private Vector3 lastPosition;
    private float currentVerticalSpeed = 0f;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        // 毎フレームの座標の変化から、実際の上下の移動速度を計算する
        if (Time.deltaTime > 0)
        {
            currentVerticalSpeed = (transform.position.y - lastPosition.y) / Time.deltaTime;
        }
        lastPosition = transform.position;
    }

    private void OnTriggerStay(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player == null) player = other.GetComponentInParent<Player>();

        if (player != null)
        {
            // 条件1：物理演算（Rigidbody）で下方向に移動しているか
            Rigidbody rb = GetComponent<Rigidbody>();
            bool isFalling = rb != null && rb.linearVelocity.y < -crushThreshold;

            // 条件2：transform（MovingGimmickなど）によって下方向に移動しているか
            // （currentVerticalSpeedがマイナス＝下方向に動いている）
            bool isMovingPosDown = currentVerticalSpeed < -crushThreshold;

            // 速度チェックが必要ない（0設定）か、何らかの方法で下移動中であること
            bool isCrushingSpeed = (crushThreshold <= 0) || isFalling || isMovingPosDown;

            if (isCrushingSpeed)
            {
                player.Squash();
                Debug.Log($"Squasher: プレイヤーを押しつぶしました！(下落速度: {currentVerticalSpeed})");
            }
        }
    }
}