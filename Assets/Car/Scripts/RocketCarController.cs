using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketCarController : CarController
{
    [Header("Rocket Car Settings")]
    public float jumpForce = 10f;
    public float airRotationRate = 10f;

    protected override void OnUpdate()
    {
        base.OnUpdate();
        if (Grounded && Input.GetKeyDown(KeyCode.Space)) rigidbody.AddForce(transform.up * jumpForce);
    }

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        if (Grounded) return;
        
        float yAxisRotationInput = Input.GetAxisRaw("Horizontal");
        float xAxisRotationInput = Input.GetAxisRaw("Vertical");
        float zAxisRotationInput = Input.GetAxisRaw("Roll");
        float yAxisVelocity = Vector3.Dot(transform.up, rigidbody.angularVelocity);
        float xAxisVelocity = Vector3.Dot(transform.right, rigidbody.angularVelocity);
        float zAxisVelocity = Vector3.Dot(transform.forward, rigidbody.angularVelocity);
        
        if (yAxisRotationInput != 0f){
            rigidbody.AddTorque(transform.up * yAxisRotationInput * airRotationRate);
        }
        else{
            rigidbody.AddTorque(transform.up * -yAxisVelocity);
        }

        if (xAxisRotationInput != 0f){
            rigidbody.AddTorque(transform.right * xAxisRotationInput * airRotationRate);
        }
        else{
            rigidbody.AddTorque(transform.right * -xAxisVelocity);
        }

        if (zAxisRotationInput != 0f){
            rigidbody.AddTorque(transform.forward * zAxisRotationInput * airRotationRate);
        }
        else{
            rigidbody.AddTorque(transform.forward * -zAxisVelocity);
        }
    }
}
