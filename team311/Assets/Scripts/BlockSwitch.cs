using UnityEngine;

public class BlockSwitch : MonoBehaviour
{
    public enum SwitchColor { Red, Blue }

    [Header("ーーー 赤青切り替えの設定 ーーー")]
    [Tooltip("このスイッチが起動する色")]
    public SwitchColor targetColor;

    [Header("ーーー 沈む動きの設定 ーーー")]
    [Tooltip("スイッチがどれくらい下に沈むか（通常は 0.2 や 0.3 くらい）")]
    [SerializeField] private float sinkDepth = 0.25f;
    [Tooltip("スイッチが動くスピード")]
    [SerializeField] private float moveSpeed = 10f;

    private Vector3 upPosition;   // 最初（上）の位置
    private Vector3 downPosition; // 沈んだ（下）の位置
    private Vector3 targetPosition;

    // スイッチの上に乗っているオブジェクトの数を数えるカウンター
    private int occupantCount = 0;

    void Start()
    {
        // 最初の位置を記憶
        upPosition = transform.localPosition;

        // 沈む位置を計算
        downPosition = upPosition + new Vector3(0f, -sinkDepth, 0f);

        // 最初は上の位置を目指す
        targetPosition = upPosition;
    }

    void Update()
    {
        // ターゲットの位置（上か下か）に向かって、毎フレームスムーズに移動させる
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * moveSpeed);
    }

    // プレイヤーや箱が触れた瞬間
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<Box>() != null || other.GetComponentInParent<Box>() != null)
        {
            occupantCount++;

            // 誰かが乗った最初の1個目の瞬間「だけ」色を切り替え、スイッチを沈める
            if (occupantCount == 1)
            {
                TriggerSwitch();       // 元からあった色切り替えを確実に実行！
                targetPosition = downPosition; // 下に下がる
            }
        }
    }

    // プレイヤーや箱が離れた瞬間
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<Box>() != null || other.GetComponentInParent<Box>() != null)
        {
            occupantCount--;

            // スイッチの上に誰もいなくなったら見た目を元に戻す（※ここでは色は変えない！）
            if (occupantCount <= 0)
            {
                occupantCount = 0;
                targetPosition = upPosition; // 上に戻る
                Debug.Log(targetColor + " スイッチの見た目が戻りました（色は維持）。");
            }
        }
    }

    // 2Dゲーム用のトリガー
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerSwitch();
        }
    }

    private void TriggerSwitch()
    {
        bool activateRed = (targetColor == SwitchColor.Red);

        // マネージャーに色変更を要請
        if (BlockManager.Instance != null)
        {
            BlockManager.Instance.ChangeColor(activateRed);
        }

        Debug.Log(targetColor + " スイッチが押されました！");
    }
}