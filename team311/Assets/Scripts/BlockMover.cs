using UnityEngine;

public class BlockMover : MonoBehaviour
{
    // スピード
    public float speed = 2f;
    // 距離
    public float distance = 3f;

    private Vector3 startPosition;
    private bool movingRight = true;

    void Start()
    {
        startPosition = transform.position;
    }

    // 更新
    void Update()
    {
        float step = speed * Time.deltaTime;
        if (movingRight)
        {
            transform.position += Vector3.right * step;
            if (transform.position.x >= startPosition.x + distance)
                movingRight = false;
        }
        else
        {
            transform.position -= Vector3.right * step;
            if (transform.position.x <= startPosition.x - distance)
                movingRight = true;
        }
    }
}