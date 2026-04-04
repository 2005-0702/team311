using System;
using UnityEngine;

public class FanDashArea : MonoBehaviour
{
    public float dashForce = 2.0f; // ƒ_ƒbƒVƒ…‚Ì‹­‚³
    public Vector3 dashDirection = Vector3.forward; // •—‚ÌŒü‚«

    private void OnTriggerEnter(Collider other)
    {
        Dashable dash = other.GetComponent<Dashable>();
        if (dash != null)
        {
            float direction = Mathf.Sign(other.transform.position.x - transform.position.x);
            dash.StartDash(direction);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
