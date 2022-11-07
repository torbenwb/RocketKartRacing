using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class math : MonoBehaviour
{
    Rigidbody rigidbody;
    public float forwardForce = 10f;
    Vector3 lastPosition;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.AddForce(transform.forward * forwardForce);
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float distanceFromLastPosition = (transform.position - lastPosition).magnitude;
        Debug.Log($"Distance from last position = {distanceFromLastPosition}");
        float yAxisVelocity = (transform.position - lastPosition).y;
        Debug.Log($"Y axis velocity: {yAxisVelocity / Time.fixedDeltaTime}");
        float yAxisDotProduct = Vector3.Dot(Vector3.up, rigidbody.velocity);
        Debug.Log($"Y Axis Dot Product: {yAxisDotProduct}");
        
        lastPosition = transform.position;
    }
}
