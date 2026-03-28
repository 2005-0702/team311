using UnityEngine;

public class UpDownMover : MonoBehaviour
{
    [SerializeField] private float moveDistance = 2f; // 上下に動く距離
    [SerializeField] private float moveSpeed = 2f;    // 動く速さ

    private Vector3 startPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 最初の位置を記録
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // 0 〜 moveDistance を行ったり来たりする値
        float offset = Mathf.PingPong(Time.time * moveSpeed, moveDistance);

        // 上下方向（Y軸）にだけ動かす
        transform.position = startPos + new Vector3(0f, offset, 0f);
    }
}
