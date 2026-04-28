using UnityEngine;

/// <summary>
/// スイッチでON/OFF可能な汎用移動ギミック。
/// 上下だけでなく、moveAxisを設定することでどの方向にも動かせます。
/// </summary>
public class MovingGimmick : MonoBehaviour
{
    public Vector3 moveAxis = Vector3.up; // 動かしたい方向
    public float moveDistance = 3f;       // 動く距離
    public float speed = 2f;              // 動く速さ
    public bool isActiveAtStart = true;   // 最初から動くか

    private Vector3 startPos;
    private bool isActive = true;
    private float timeOffset = 0f;
    private float pauseStartTime = 0f;

    void Start()
    {
        startPos = transform.position;
        isActive = isActiveAtStart;
        if (!isActive) pauseStartTime = Time.time;
    }

    void Update()
    {
        if (!isActive) return;

        // 停止時間を考慮した経過時間の計算
        float adjustedTime = Time.time - timeOffset;
        float offset = Mathf.PingPong(adjustedTime * speed, moveDistance);
        
        // 開始位置から指定方向に移動
        transform.position = startPos + (moveAxis.normalized * offset);
    }

    // スイッチから呼ばれる命令
    public void SetActivate(bool state)
    {
        if (isActive == state) return;

        if (state)
        {
            // 再開時：停止していた時間をオフセットに加算
            timeOffset += Time.time - pauseStartTime;
        }
        else
        {
            // 停止時：現在の時間を記録
            pauseStartTime = Time.time;
        }

        isActive = state;
    }
}
