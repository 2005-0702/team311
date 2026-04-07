using UnityEngine;

public class MovingUpDown : MonoBehaviour
{
    public float moveDistance = 2f; // è„â∫Ç…ìÆÇ≠ãóó£
    public float moveSpeed = 2f;    // ë¨Ç≥

    private Vector3 startPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float y = Mathf.Sin(Time.time * moveSpeed) * moveDistance;
        transform.position = startPos + new Vector3(0, y, 0);
    }
}
