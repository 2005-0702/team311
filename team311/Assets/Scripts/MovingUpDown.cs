using UnityEngine;

public class MovingUpDown : MonoBehaviour
{
    public float moveDistance = 2f;
    public float moveSpeed = 2f;

    private Vector3 startPos;
    private Rigidbody rb;

    void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float y = Mathf.Sin(Time.time * moveSpeed) * moveDistance;

        Vector3 targetPos =
            startPos + new Vector3(0f, y, 0f);

        rb.MovePosition(targetPos);
    }
}