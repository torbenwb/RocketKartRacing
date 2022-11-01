using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Rigidbody))]
public class ArcadeCarController : MonoBehaviour
{
    public Transform[] wheels;
    public Transform[] frontAxil;
    public Transform[] rearAxil;
    Rigidbody rigidbody;

    public Vector3 overrideCenterOfMass;
    public float suspensionDistance = 1f;
    public float springStrength = 10f;
    public float springDamping = 10f;
    public float carTopSpeed = 10f;
    public float tireMass = 0.1f;
    public float brakePressure = 0.8f;
    public float tireRadius = 0.4f;
    public float downForceCoefficient = 1f;
    public float maxXVelocity = 10f;
    public float rocketForce = 100f;
    public AnimationCurve powerCurve;
    public AnimationCurve frontAxilSteeringCurve;
    public AnimationCurve rearAxilSteeringCurve;
    public AnimationCurve frontAxilAccelerationCurve;
    public AnimationCurve rearAxilAccelerationCurve;

    public Vector3 flipForces;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.centerOfMass = overrideCenterOfMass;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)){
            rigidbody.AddForce(Vector3.up * flipForces.x);
            rigidbody.AddTorque(transform.forward * flipForces.y);
        }
    }

    void ParseWheelSuspension(Transform tireTransform, RaycastHit hit){
        Vector3 springDirection = tireTransform.up;
        Vector3 tireWorldVelocity = rigidbody.GetPointVelocity(tireTransform.position);
        float offset = suspensionDistance - hit.distance;
        float velocity = Vector3.Dot(springDirection, tireWorldVelocity);
        float force = (offset * springStrength) - (velocity * springDamping);
        rigidbody.AddForceAtPosition(springDirection * force, tireTransform.position);
        // Set wheel mesh position to ground
        tireTransform.GetChild(0).position = hit.point + tireTransform.up * tireRadius;
    }

    float EvaluateFrictionCurve(Transform tireTransform, AnimationCurve frictionCurve){
        Vector3 tireWorldVelocity = rigidbody.GetPointVelocity(tireTransform.position);
        float steeringVelocity = Vector3.Dot(tireTransform.right, tireWorldVelocity);
        float ratio = Mathf.Clamp01(Mathf.Abs(steeringVelocity) / maxXVelocity);
        Debug.Log($"Steering velocity ratio: {frictionCurve.Evaluate(ratio)}");
        return frictionCurve.Evaluate(ratio);
    }

    void ParseWheelSteering(Transform tireTransform, RaycastHit hit, AnimationCurve frictionCurve){
        Vector3 steeringDirection = tireTransform.right;
        Vector3 tireWorldVelocity = rigidbody.GetPointVelocity(tireTransform.position);

        float steeringVelocity = Vector3.Dot(steeringDirection, tireWorldVelocity);
        
        float tireGripFactor = EvaluateFrictionCurve(tireTransform, frictionCurve);

        float desiredVelocityChange = -steeringVelocity * tireGripFactor;

        float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;

        rigidbody.AddForceAtPosition(steeringDirection * tireMass * desiredAcceleration, tireTransform.position);
    }

    void ParseWheelAcceleration(Transform tireTransform, float accelerationInput, AnimationCurve frictionCurve = null){
        Vector3 accelerationDirection = tireTransform.forward;

        float carSpeed = Vector3.Dot(transform.forward, rigidbody.velocity);
        float tireGripFactor = EvaluateFrictionCurve(tireTransform, frictionCurve);

        if (accelerationInput != 0f){
            //float carSpeed = Vector3.Dot(transform.forward, rigidbody.velocity);

            float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / carTopSpeed);

            float availableTorque = powerCurve.Evaluate(normalizedSpeed) * accelerationInput;

            rigidbody.AddForceAtPosition(accelerationDirection * availableTorque * tireGripFactor, tireTransform.position);

            
        }
        else{
            // braking
            //float carSpeed = Vector3.Dot(transform.forward, rigidbody.velocity);
            
            rigidbody.AddForceAtPosition(accelerationDirection * -carSpeed * brakePressure * tireGripFactor, tireTransform.position);
        }
        
        // Downforce
        rigidbody.AddForceAtPosition(-tireTransform.up * carSpeed * downForceCoefficient, tireTransform.position);
    }

    private void FixedUpdate()
    {
        float frontAxilGrip = 0.3f;
        float rearAxilGrip = 0.3f;

        bool boost = Input.GetKey(KeyCode.LeftShift);

        if (boost) rigidbody.AddForce(transform.forward * rocketForce);

        foreach(Transform tireTransorm in frontAxil){
            tireTransorm.localRotation = Quaternion.AngleAxis(30f * Input.GetAxisRaw("Horizontal"), transform.up);

            RaycastHit hit;
            if (Physics.Raycast(tireTransorm.position, -tireTransorm.up, out hit, suspensionDistance)){
                ParseWheelSuspension(tireTransorm, hit);
                ParseWheelSteering(tireTransorm, hit, frontAxilSteeringCurve);
                ParseWheelAcceleration(tireTransorm, boost ? 1f : Input.GetAxisRaw("Vertical"), frontAxilAccelerationCurve);
            }
        }

        foreach(Transform tireTransorm in rearAxil){
            RaycastHit hit;
            if (Physics.Raycast(tireTransorm.position, -tireTransorm.up, out hit, suspensionDistance)){
                ParseWheelSuspension(tireTransorm, hit);
                ParseWheelSteering(tireTransorm, hit, rearAxilSteeringCurve);
                ParseWheelAcceleration(tireTransorm, boost ? 1f : Input.GetAxisRaw("Vertical"), rearAxilAccelerationCurve);
            }
        }     
    }
}
