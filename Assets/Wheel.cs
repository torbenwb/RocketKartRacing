using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Wheel : MonoBehaviour
{
    Rigidbody rigidbody;
    public float wheelRadius = 0.5f;
    public float raycastAngle = 60f;
    public int increments = 6;
    public float maxOffset = 2f;
    public float springStrength = 100f;
    public float springDamping = 2f;
    public float wheelRotation = 0f;
    public float wheelRotationSpeed = 360f;
    [Range(0f, 1f)]
    public float friction = 0f;
    public float wheelTorque = 0f;
    public float maxTorque = 10f;

    private void Awake()
    {
        rigidbody = GetComponentInParent<Rigidbody>();
        
    }

    private void FixedUpdate()
    {
        float offset = WheelRaycasts(true);
        transform.GetChild(0).transform.localPosition = Vector3.up * offset;

        if (offset == 0f) return;
        float maxDelta = maxTorque * Time.fixedDeltaTime;
        wheelTorque = Mathf.MoveTowards(wheelTorque, maxTorque * Input.GetAxisRaw("Vertical"), maxDelta);
        rigidbody.AddForceAtPosition(Suspension(offset),transform.position);
        rigidbody.AddForceAtPosition(WheelForce(), transform.position);
        rigidbody.AddForceAtPosition(FrictionForce(), transform.position);

        SpinWheel();
    }

    private Vector3 FrictionForce(){
        Vector3 pointVelocity = rigidbody.GetPointVelocity(transform.position);
        float xAxisVelocity = Vector3.Dot(pointVelocity, transform.right);
        float zAxisVelocity = Vector3.Dot(pointVelocity, transform.forward);
        Vector3 frictionOutput = (transform.right * - xAxisVelocity * friction) + (transform.forward * -zAxisVelocity * friction);

        if (maxTorque == 0f){
            wheelTorque = zAxisVelocity;
        }

        return frictionOutput;
    }

    private Vector3 WheelForce(){
        Vector3 direction = transform.forward;
        return direction * wheelTorque * friction;
    }

    private void SpinWheel(){
        wheelRotation += wheelRotationSpeed * Time.fixedDeltaTime * wheelTorque;
        if (wheelRotation > 360f) wheelRotation -= 360f;
        if (wheelRotation < -360f) wheelRotation += 360f;
        Vector3 origin = transform.position;
        Vector3 direction = Quaternion.AngleAxis(wheelRotation, transform.right) * transform.up;
        Debug.DrawLine(origin, origin + direction.normalized * wheelRadius, Color.red);
        transform.GetChild(0).transform.localRotation = Quaternion.AngleAxis(wheelRotation, Vector3.right);
    }

    private Vector3 Suspension(float offset){
        // Wheels are not on the ground -> no suspension force
        if (offset == 0f) return Vector3.zero;

        float offsetRatio = offset / maxOffset;
        
        // World space direction to apply force in
        Vector3 springDirection = transform.up;
        // Velocity of parent's rigidbody at center of wheel
        Vector3 wheelWorldVelocity = rigidbody.GetPointVelocity(transform.position);
        // Magnitude velocity in spring direction
        float springVelocity = Vector3.Dot(springDirection, wheelWorldVelocity);
        // How much force to apply to rigidbody
        float springForce = springStrength * offsetRatio;
        springForce -= springVelocity * springDamping;
        return springDirection * springForce;
    }

    /*  Find wheel vertical offset from ground using a downward arc of raycasts.

        returns:
        - offset - The vertical offset from the wheel's transform position based on
        wheel radius and ground position. An offset of 0 means wheel is not touching
        the ground. An offset greater than 0 means wheel is touching the ground.
    */
    private float WheelRaycasts(bool drawDebug = false){
        float offset = 0f;
        // Origin point for all raycasts
        Vector3 origin = transform.position;

        // Starting raycast direction - rotated on the transform x axis
        // half the total raycast angles.
        Vector3 direction = Quaternion.AngleAxis((-raycastAngle / 2), transform.right) * -transform.up;
        
        // degrees to rotate direction each increment
        float angleIncrement = raycastAngle / increments;

        for (int i = 0; i <= increments; i++){
            // Where ray would end if no hit
            Vector3 end = origin + direction * wheelRadius;

            RaycastHit hit;
            if (Physics.Raycast(origin, direction, out hit, wheelRadius)){
                if (drawDebug) Debug.DrawLine(origin, hit.point, Color.red);

                float distance = (hit.point - end).magnitude;
                // If distance from end is greater than offset override offset
                if (distance > offset) offset = distance;
            }
            else{
                if (drawDebug) Debug.DrawLine(origin, origin + direction * wheelRadius, Color.green);
            }

            direction = Quaternion.AngleAxis(angleIncrement, transform.right) * direction;
        }

        if (offset != 0f && drawDebug) Debug.DrawLine(transform.position, transform.position + transform.up * offset, Color.blue);

        return offset;
    }   
}
