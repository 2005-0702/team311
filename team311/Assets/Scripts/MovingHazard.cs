using UnityEngine;

/// <summary>
/// オブジェクトを指定した軸方向に一定距離、往復移動させるスクリプト。
/// リジッドボディがある場合は Is Kinematic をオンにすることを推奨します。
/// </summary>
public class MovingHazard : MonoBehaviour
{
    public Vector3 moveDirection = Vector3.up; // 移動する方向
    public float distance = 3f;                // 移動する距離
    public float speed = 2f;                   // 移動速度
    public float pauseTime = 0.5f;             // 端での待ち時間

    private Vector3 startPos;
    private Rigidbody rb;
    private bool isActive = true;
    private float timeOffset = 0f;
    private float pauseStartTime = 0f;

    void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!isActive) return;

        // Mathf.PingPong を使って 0〜1 の間のウェイト値を計算
        float adjustedTime = Time.time - timeOffset;
        float t = Mathf.PingPong(adjustedTime * speed / distance, 1f);
        
        // イージング（滑らかな動き）を入れたい場合はここを調整
        // t = Mathf.SmoothStep(0, 1, t);

        Vector3 targetPos = startPos + (moveDirection.normalized * distance * t);

        if (rb != null && rb.isKinematic)
        {
            rb.MovePosition(targetPos);
        }
        else
        {
            transform.position = targetPos;
        }
    }

    // Squasherスクリプトが「今は下に動いているか」を判定するために使用
    public bool IsMovingDown()
    {
        if (!isActive) return false;

        // 簡易的に：前回の位置と比較するか、PingPongの計算式から進行方向を割り出す
        float cycle = (Time.time - timeOffset) * speed / distance;
        return (Mathf.FloorToInt(cycle) % 2 == 1); // PingPongの後半サイクル（戻り）
    }

    // スイッチから呼ばれる命令
    public void SetActivate(bool state)
    {
        if (isActive == state) return;

        if (state)
        {
            // 再開時：停止していた時間をオフセットに加算して、位置が飛ばないようにする
            timeOffset += Time.time - pauseStartTime;
        }
        else
        {
            // 停止時：停止した瞬間の時間を記録
            pauseStartTime = Time.time;
        }

        isActive = state;
    }
}
