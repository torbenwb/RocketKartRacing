using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketCar : Car
{
    [Header("Rocket Car Settings")]
    public float jumpForce = 10f;
    public float airRotationRate = 10f;

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        
        
        
        float yAxisRotationInput = Input.GetAxisRaw("Horizontal");
        float xAxisRotationInput = Input.GetAxisRaw("Vertical");
        float zAxisRotationInput = Input.GetAxisRaw("Roll");
        float yAxisVelocity = Vector3.Dot(transform.up, GetComponent<Rigidbody>().angularVelocity);
        float xAxisVelocity = Vector3.Dot(transform.right, GetComponent<Rigidbody>().angularVelocity);
        float zAxisVelocity = Vector3.Dot(transform.forward, GetComponent<Rigidbody>().angularVelocity);
        
        if (yAxisRotationInput != 0f){
            GetComponent<Rigidbody>().AddTorque(transform.up * yAxisRotationInput * airRotationRate);
        }
        else{
            GetComponent<Rigidbody>().AddTorque(transform.up * -yAxisVelocity);
        }

        if (xAxisRotationInput != 0f){
            GetComponent<Rigidbody>().AddTorque(transform.right * xAxisRotationInput * airRotationRate);
        }
        else{
            GetComponent<Rigidbody>().AddTorque(transform.right * -xAxisVelocity);
        }

        if (zAxisRotationInput != 0f){
            GetComponent<Rigidbody>().AddTorque(transform.forward * zAxisRotationInput * airRotationRate);
        }
        else{
            GetComponent<Rigidbody>().AddTorque(transform.forward * -zAxisVelocity);
        }
    }
}
