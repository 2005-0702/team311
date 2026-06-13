using UnityEngine;

public class FloatingItem : MonoBehaviour
{
    [Header("ふわふわ（上下移動）の設定")]
    public float floatDistance = 0.2f; // 上下に動く幅（鍵なのでトゲより小さめがおすすめ）
    public float floatSpeed = 3.0f;    // ふわふわする速度

    [Header("回転の設定")]
    public Vector3 rotateSpeed = new Vector3(0, 100, 0); // 1秒間に回転する角度（初期値はY軸回転）

    private Vector3 startPosition;

    void Start()
    {
        // ゲーム開始時のアイテムの位置を記憶
        startPosition = transform.position;
    }

    void Update()
    {
        // 1. ふわふわ動かす処理（MovingSpikeの応用）
        float wave = Mathf.Sin(Time.time * floatSpeed);
        transform.position = startPosition + (Vector3.up * wave * floatDistance);

        // 2. クルクル回転させる処理（新規追加！）
        // Time.deltaTimeを掛けることで、パソコンの重さに関わらず一定の速度で回ります
        transform.Rotate(rotateSpeed * Time.deltaTime);
    }
}