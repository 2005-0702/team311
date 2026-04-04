using UnityEngine;

public class Dashable : MonoBehaviour
{
    Rigidbody rb;

    bool isDashing = false;
    float dashTime = 0.25f;
    float dashTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void StartDash(float direction)
    {
        rb.linearVelocity = new Vector3(direction * 40f, rb.linearVelocity.y, 0f);
        isDashing = true;
        dashTimer = dashTime;
    }
    // Update is called once per frame
    void Update()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
        }
    }
    public bool IsDashing()
    {
        return isDashing;
    }
}
