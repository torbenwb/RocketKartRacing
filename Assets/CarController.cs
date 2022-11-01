using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour{
    // Car's Rigidbody, required, all forces act upon this rigidbody.
    Rigidbody rigidbody;
    enum DriveType{FWD, RWD, AWD};

    [Header("Center Of Mass")]
    [SerializeField] Transform socket;

    [Header("Drive Type")]
    [SerializeField] DriveType driveType;

    [Header("Wheels")]
    [SerializeField] Transform[] frontAxil;
    [SerializeField] Transform[] rearAxil;
    [SerializeField] float wheelRadius;
    [SerializeField] float wheelAngle;
    [SerializeField] float wheelMass = 0.1f;
    [SerializeField] int increments;
    public struct WheelForces{
        public Vector3 position, xAxis, yAxis, zAxis;
    }
    public WheelForces[] wheelForces = new WheelForces[4];
    
    [Header("Suspension")]
    [SerializeField] AnimationCurve springCurve;
    [SerializeField] float springStrength;
    [SerializeField] float springDamping;
    [SerializeField] [Range(0.1f, 3f)] float maxOffset;

    [Header("Acceleration")]
    [SerializeField] float currentSpeed;
    [SerializeField] float topSpeed;
    [SerializeField] float accelerationForce;
    [SerializeField] float brakePressure;
    [SerializeField] AnimationCurve accelerationCurve;
    [SerializeField] AnimationCurve frontAxilAccelerationCurve;
    [SerializeField] AnimationCurve rearAxilAccelerationCurve;
    public float SpeedRatio{get => currentSpeed / topSpeed;}

    [Header("Steering")]
    [SerializeField] float maxLocalXVelocity = 10f;
    [SerializeField] AnimationCurve frontAxilFrictionCurve;
    [SerializeField] AnimationCurve rearAxilFrictionCurve;

    [Header("Jump")]
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float airJumpForce = 5f;
    [SerializeField] bool grounded = false;
    [SerializeField] float airRotationForce = 20f;
    [SerializeField] bool airJump = false;
    [SerializeField] float flipRaycast = 2f;

    [Header("Downforce")]
    [SerializeField] float downForceRatio = 1.0f;
    

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        if (socket) rigidbody.centerOfMass = socket.localPosition;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)){
            if (grounded) rigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            else if (airJump){
                rigidbody.AddForce(transform.up * airJumpForce, ForceMode.Impulse);
                airJump = false;
            }
            else{
                if (Physics.Raycast(transform.position, transform.up, flipRaycast)){
                    rigidbody.AddForce(-transform.up * jumpForce, ForceMode.Impulse);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        int groundedWheels = 0;
        int i = 0;
        foreach(Transform wheel in frontAxil){
            // Turn front axil wheels based on Horizontal input.
            Quaternion frontWheelRotation = Quaternion.AngleAxis(30f * Input.GetAxisRaw("Horizontal"), Vector3.up);
            wheel.localRotation = Quaternion.RotateTowards(wheel.localRotation, frontWheelRotation, 60f * Time.fixedDeltaTime);
            //wheel.localRotation = Quaternion.AngleAxis(30f * Input.GetAxisRaw("Horizontal"), Vector3.up);
            // Get local Y axis offset from desired wheel position.
            // An offset 0 indicates no raycast hits so wheel is at desired position
            // and not colliding with the ground
            float offset = WheelRaycasts(wheel, wheelRadius, wheelAngle, increments);
            
            if (offset != 0f){
                groundedWheels++;
                wheelForces[i].position = wheel.position;
                // Determine wheel position and apply local Y force from suspension
                ParseWheelSuspension(ref wheelForces[i], rigidbody, wheel, offset, maxOffset, springStrength, springDamping, springCurve, wheel.GetChild(0));
                // Determine current grip factor and apply local X force to counteract slip
                float gripFactor = ParseWheelSteering(ref wheelForces[i], rigidbody, wheel, wheelMass, maxLocalXVelocity, frontAxilFrictionCurve);
                // AWD / FWD apply local Z force according to braking/acceleration input
                if (driveType != DriveType.RWD)
                    ParseWheelAcceleration(ref wheelForces[i], 
                    rigidbody, 
                    wheel, 
                    Input.GetAxisRaw("Vertical"),
                    accelerationForce, 
                    topSpeed, 
                    brakePressure, 
                    gripFactor, 
                    accelerationCurve,
                    frontAxilAccelerationCurve);
            }
            i++;
        }

        foreach(Transform wheel in rearAxil){
            float offset = WheelRaycasts(wheel, wheelRadius, wheelAngle, increments);
            
            if (offset != 0f){
                groundedWheels++;
                wheelForces[i].position = wheel.position;
                // Determine wheel position and apply local Y force from suspension
                ParseWheelSuspension(ref wheelForces[i], rigidbody, wheel, offset, maxOffset, springStrength, springDamping, springCurve, wheel.GetChild(0));
                // Determine current grip factor and apply local X force to counteract slip
                float gripFactor = ParseWheelSteering(ref wheelForces[i], rigidbody, wheel, wheelMass, maxLocalXVelocity, rearAxilFrictionCurve);
                // AWD / FWD apply local Z force according to braking/acceleration input
                if (driveType != DriveType.FWD)
                    ParseWheelAcceleration(ref wheelForces[i], 
                    rigidbody, 
                    wheel, 
                    Input.GetAxisRaw("Vertical"),
                    accelerationForce, 
                    topSpeed, 
                    brakePressure, 
                    gripFactor, 
                    accelerationCurve,
                    rearAxilAccelerationCurve);
            }
            i++;
        }

        grounded = groundedWheels > 0;

        if (grounded) airJump = true;
        currentSpeed = Vector3.Dot(rigidbody.transform.forward, rigidbody.velocity);
        AirRotation();
        Downforce();
    }

    private void Downforce(){
        if (!grounded) return;
        float forwardVelocity = Vector3.Dot(rigidbody.velocity, transform.forward);
        rigidbody.AddForce(-transform.up * forwardVelocity);
    }

    /*  Add torque on X, Y, and Z axes according to player input.
        If no player input - apply torque proportionally opposite to axis
        angular velocity to cancel out torque.
    */
    private void AirRotation(){
        if (grounded) return;

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        float rollInput = Input.GetAxisRaw("Roll");

        if (horizontalInput != 0f){
            rigidbody.AddTorque(transform.up * airRotationForce * Time.fixedDeltaTime * horizontalInput);
        }
        else{
            float yawVelocity = Vector3.Dot(rigidbody.angularVelocity, transform.up);
            rigidbody.AddTorque(transform.up * airRotationForce * Time.fixedDeltaTime * -yawVelocity);
        }

        if (verticalInput != 0f){
            rigidbody.AddTorque(transform.right * airRotationForce * Time.fixedDeltaTime * verticalInput);
        }
        else{
            float pitchVelocity = Vector3.Dot(rigidbody.angularVelocity, transform.right);
            rigidbody.AddTorque(transform.right * airRotationForce * Time.fixedDeltaTime * -pitchVelocity);
        }

        if (rollInput != 0f){
            rigidbody.AddTorque(transform.forward * airRotationForce * Time.fixedDeltaTime * rollInput);
        }
        else{
            float rollVelocity = Vector3.Dot(rigidbody.angularVelocity, transform.forward);
            rigidbody.AddTorque(transform.forward * airRotationForce * Time.fixedDeltaTime * -rollVelocity);
        }

    }

    /*  Takes wheel offset and turns it into suspension force at a given position on a Rigidbody

        Parameters:
        - rigidbody: Rigidbody component to apply force to
        - wheelTransform: Transform component of wheel GameObject
        - offset: Current vertical offset from wheel desired position
        - maxOffset: maximum vertical offset. Used to calculate offset ratio applied to spring strength
        - springStrength: strength of suspension spring.
        - springDamping: damping of suspension spring force.
        - springCurve: optional spring strength curve
        - tireTransform: optional tireTransform

    */
    private static void ParseWheelSuspension(
        ref WheelForces wheelForces,
        Rigidbody rigidbody, 
        Transform wheelTransform,
        float offset,
        float maxOffset,
        float springStrength,
        float springDamping,
        AnimationCurve springCurve = null,
        Transform tireTransform = null
        ){
        if (offset == 0f) return;
        float offsetRatio = offset / maxOffset;

        Vector3 springDirection = wheelTransform.up;
        Vector3 wheelWorldVelocity = rigidbody.GetPointVelocity(wheelTransform.position);

        float springVelocity = Vector3.Dot(springDirection, wheelWorldVelocity);
        float springForce;
        springForce = springStrength * ((springCurve != null) ? springCurve.Evaluate(offsetRatio) : offsetRatio);
        springForce -= (springVelocity * springDamping);
        

        rigidbody.AddForceAtPosition(springDirection * springForce, wheelTransform.position);
        
        wheelForces.yAxis = (springDirection * springForce);

        if (tireTransform) tireTransform.position = wheelTransform.position + wheelTransform.up * offset;
    }

    /*  Applies acceleration force to rigidbody at given wheelTransform. Assumes wheel is grounded

        Parameters:
        - rigidbody: Rigidbody component to apply force to.
        - wheelTransform: wheelTransform used for force calculation.
        - acclerationInput: user input - desired acceleration direction.
        - accelerationForce: force multiplier for acceleration input.
        - topSpeed: intended top speed of rigidbody. Used to determine force ratio.
        - brakePressure: brake pressure used to determine opposing force when acceleration input == 0.

    */
    private static void ParseWheelAcceleration(
        ref WheelForces wheelForces,
        Rigidbody rigidbody,
        Transform wheelTransform,
        float accelerationInput,
        float accelerationForce,
        float topSpeed,
        float brakePressure,
        float gripFactor,
        AnimationCurve accelerationCurve,
        AnimationCurve frictionCurve
    ){
        Vector3 accelerationDirection = wheelTransform.forward;
        float carSpeed = Vector3.Dot(rigidbody.transform.forward, rigidbody.velocity);
        gripFactor = frictionCurve.Evaluate(gripFactor);
        
        float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / topSpeed);
        if (accelerationInput != 0f){
            float force = accelerationInput * accelerationForce * gripFactor * accelerationCurve.Evaluate(normalizedSpeed);
            rigidbody.AddForceAtPosition(accelerationDirection * force, wheelTransform.position);
            wheelForces.zAxis = (accelerationDirection * force);
        }
        else{
            Debug.Log(gripFactor);
            if (gripFactor <= 0.2f){
                Debug.Log($"Allow Drift");
            }
            else{
                float force = -Mathf.Clamp(carSpeed,-1f,1f) * brakePressure * gripFactor;
                rigidbody.AddForceAtPosition(accelerationDirection * force, wheelTransform.position);
                wheelForces.zAxis = (accelerationDirection * force);
            }
            
        }
    }

    /*  Applies steering force in the opposite direction of wheel slip to prevent slipping. Assumes wheel is grounded.

        Parameters
        - rigidbody: rigidbody to receive force
        - wheelTransform: wheelTransform to calculate position of velocity and force
        - wheelMass: mass of wheel
        - maxXVelocity: maximum local x axis velocity used to evaluate friction curve
        - frictionCurve: used to determine how much stabilizing opposite force to apply
        at wheel position. Position on curve is determined by current local X velocity / 
        maxXVelocity. More friction means greater force in the opposite direction of 
        local X velocity.

        Returns
        - float: grip factor. Friction between wheel and ground evaluated on friction curve
    */
    private static float ParseWheelSteering(
        ref WheelForces wheelForces,
        Rigidbody rigidbody,
        Transform wheelTransform,
        float wheelMass,
        float maxXVelocity,
        AnimationCurve frictionCurve = null
    ){
        Vector3 steeringDirection = wheelTransform.right;
        Vector3 wheelWorldVelocity = rigidbody.GetPointVelocity(wheelTransform.position);
        float steeringVelocity = Vector3.Dot(steeringDirection, wheelWorldVelocity);
        
        float gripFactor = frictionCurve.Evaluate(Mathf.Abs(steeringVelocity / maxXVelocity));

        float desiredVelocityChange = -steeringVelocity * gripFactor;
        float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;

        rigidbody.AddForceAtPosition(steeringDirection * wheelMass * desiredAcceleration, wheelTransform.position);
        wheelForces.xAxis = steeringDirection * wheelMass * desiredAcceleration;

        return gripFactor;
    }

    /*  Casts a series of rays in a downward facing arc from wheelTransform position.

        Parameters:
        - wheelTransform: Wheel transform component. transform.up used for direction. transform.position
        used for Raycast origin.
        - wheelRadius: Desired radius of wheel used for max distance of all raycasts.
        - angle: The total angle of the raycast arc.
        - increments: Number of increments used to traverse arc angle

        Return:
        - float: The maximum offset - the maximum distance of any raycast from its intended destination
        to its hit point.
    */
    private static float WheelRaycasts(Transform wheelTransform, float wheelRadius, float angle, int increments){
        Vector3 origin = wheelTransform.position;
        Vector3 direction = Quaternion.AngleAxis(-(angle / 2), wheelTransform.right) * -wheelTransform.up;
        float newOffset = 0f, angleIncrement = angle / increments;

        for(int i = 0; i <= increments; i++){
            Vector3 end = origin + direction * wheelRadius;
            RaycastHit hit;
            if (Physics.Raycast(origin, direction, out hit, wheelRadius)) {
                Debug.DrawLine(origin, hit.point, Color.red, Time.fixedDeltaTime);
                float distance = (hit.point - end).magnitude;
                if (distance > newOffset) newOffset = distance;
            }
            else {
                Debug.DrawLine(origin, origin + direction * wheelRadius, Color.green, Time.fixedDeltaTime);
            }
            
            direction = Quaternion.AngleAxis(angleIncrement, wheelTransform.right) * direction;
        }
        return newOffset;
    }
}
