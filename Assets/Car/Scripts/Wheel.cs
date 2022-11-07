using UnityEngine;


public class Wheel : MonoBehaviour
{
    Rigidbody rigidbody;
    
    private bool grounded = false;
    public bool Grounded{get => grounded;}

    [Header("Wheel Raycast Settings")]
    public float wheelRadius = 0.4f;
    public float raycastAngle = 60f;
    public int increments = 6;

    [Header("Suspension")]
    public float maxOffset = 2f;
    public float springStrength = 100f;
    public float springDamping = 2f;

    [Header("Wheel Rotation")]
    private float wheelRotation = 0f;
    private float wheelRotationVelocity = 0f;
    Vector3 lastHit, newHit;

    [Header("Wheel Torque")]
    public float maxTorque = 10f;
    private float wheelTorque = 0f;
    public float topSpeed = 20f;

    [Header("Braking")]
    public float brakePressure = 1f;

    [Header("Friction")]
    public float friction = 1f;
    public float xAxisVelocity;
    public float zAxisVelocity;
    public Vector3 frictionOutput;

    private void Awake()
    {
        rigidbody = GetComponentInParent<Rigidbody>();
        
    }

    private void FixedUpdate()
    {
        float offset = WheelRaycasts(true);
        transform.GetChild(0).transform.localPosition = Vector3.up * offset;

        if (brakePressure == 0f) SpinWheel(offset > 0f);

        if (offset == 0f) return;
        float maxDelta = maxTorque * Time.fixedDeltaTime;
        wheelTorque = Mathf.MoveTowards(wheelTorque, maxTorque * Input.GetAxisRaw("Vertical"), maxDelta);
        brakePressure = (Input.GetAxisRaw("Vertical") == 0f) ? 1f : 0f;
        
        rigidbody.AddForceAtPosition(Suspension(offset),transform.position);
        rigidbody.AddForceAtPosition(WheelForce(), transform.position);
        rigidbody.AddForceAtPosition(FrictionForce(), transform.position);
        
        
    }

    private Vector3 FrictionForce(){

        Vector3 pointVelocity = rigidbody.GetPointVelocity(transform.position);
        float forwardVelocity = Vector3.Dot(transform.forward, pointVelocity);
        float rightVelocity = Vector3.Dot(transform.right, pointVelocity);
        // If braking apply opposite force proportional to z axis velocity
        Vector3 zAxisFriction = transform.forward * -forwardVelocity * friction * brakePressure;
        // Apply oppositional x axis force proportional x axis velocity and friction
        Vector3 xAxisFriction = transform.right * -rightVelocity * friction;

        Vector3 frictionOutput = xAxisFriction + zAxisFriction;

        Debug.DrawLine(transform.position, transform.position + zAxisFriction, Color.blue);
        Debug.DrawLine(transform.position, transform.position + xAxisFriction, Color.red);

        return (frictionOutput);
    }

    private Vector3 WheelForce(){
        float carForwardSpeed = Vector3.Dot(rigidbody.transform.forward, rigidbody.velocity);
        float carSpeedRatio = 1f - (Mathf.Abs(carForwardSpeed) / topSpeed);
        return transform.forward * carSpeedRatio * wheelTorque * friction;
    }

    private void SpinWheel(bool tireGrounded){
        if (tireGrounded){
            Vector3 hitDistance = newHit - lastHit;
            float zAxisDistance = Vector3.Dot(hitDistance, transform.forward);
            float distanceTraveled = zAxisDistance;

            if (distanceTraveled == 0f) return;

            float wheelCircumference = 3.14f * wheelRadius * 2f;
            float a = Mathf.Abs(distanceTraveled) / wheelCircumference;

            float circumferenceRatio = a;

            if (zAxisDistance > 0f) wheelRotationVelocity = circumferenceRatio * 360f;
            else wheelRotationVelocity = -circumferenceRatio * 360f;
        }
    
        wheelRotation += wheelRotationVelocity;
        
        if (wheelRotation > 360f) wheelRotation -= 360f;
        if (wheelRotation < -360f) wheelRotation += 360f;
        transform.GetChild(0).transform.localRotation = Quaternion.AngleAxis(wheelRotation, Vector3.right);

        lastHit = newHit;
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
                
                if (direction == -transform.up) newHit = hit.point;

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

        grounded = offset != 0f;
        return offset;
    }   
}
