using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct AxleInfo {
    [SerializeField] public WheelCollider leftWheel;
    [SerializeField] public WheelCollider rightWheel;
    [SerializeField] public bool motor;
    [SerializeField] public bool steering;
}
     
public class Car : MonoBehaviour {
    [SerializeField]  List<AxleInfo> axleInfos; 
    [SerializeField]  float maxMotorTorque;
    [SerializeField]  float maxSteeringAngle;
    [SerializeField]  float maxBrakeTorque;

    private float accelerationAxis = 0f;
    private float brakeAxis = 0f;
    private float turnAxis = 0f;

    // Public Interface

    /*  The Drive method allows a user to apply
        drive the car either forward or backward.

        directionAxis will be clamped between -1 and 1.

        A positive value for directionAxis results 
        forward acceleration.

        A negative value for directionAxis results 
        in backwards acceleration.
    */
    public void Drive(float accelerationAxis){
        this.accelerationAxis = Mathf.Clamp(accelerationAxis, -1f, 1f);
    }

    /*  The Brake method allows the user to apply
        braking force, opposing the current movement
        of the car.

        brakeAxis will be clamped between 0 and 1.

        Brake pressure will be proportional to brakeAxis
        input.
    */
    public void Brake(float brakeAxis){
        this.brakeAxis = Mathf.Clamp(brakeAxis, 0f, 1f);
    }

    /*  The Turn method allows the user to apply
        steering input to the front wheels of the car.

        A turnAxis value of less than one will rotate
        the car's front wheels to the left.

        A turnAxis value of greater than one will rotate
        the car's front wheels to the right.

        A turnAxis value of 0 will center the rotation
        of the car's front wheels.
    */
    public void Turn(float turnAxis){
        this.turnAxis = Mathf.Clamp(turnAxis, -1f, 1f);
    }
     
    // finds the corresponding visual wheel
    // correctly applies the transform
    private void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0) {
            return;
        }
     
        Transform visualWheel = collider.transform.GetChild(0);
     
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);
     
        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }
     
    private void FixedUpdate()
    {
        float motor = maxMotorTorque * accelerationAxis;
        float steering = maxSteeringAngle * turnAxis;
        float brake = brakeAxis * maxBrakeTorque;
     
        foreach (AxleInfo axleInfo in axleInfos) {
            if (axleInfo.steering) {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor) {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }

            axleInfo.leftWheel.brakeTorque = brake;
            axleInfo.rightWheel.brakeTorque = brake;

            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }
    }
}