using UnityEngine;

public class FloatingUI : MonoBehaviour
{
    public float floatHeight = 0.2f;   // 上下する高さ
    public float floatSpeed = 2.0f;    // 上下する速さ

    // 最初の位置を保存する変数
    private Vector3 startPos;

    void Start()
    {
        // ゲーム開始時の位置を記録
        startPos = transform.position;
    }

    
    void Update()
    {
        // Sin関数を使って上下にふわふわ動かす
        // Time.time：ゲーム開始からの時間
        // Mathf.Sin：-1～1の値を繰り返す
        // Vector3.up：Y方向（上方向）に移動
        transform.position = startPos +
            Vector3.up * Mathf.Sin(Time.time * floatSpeed) * floatHeight;
    }
}
