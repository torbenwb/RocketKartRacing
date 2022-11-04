using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    protected Rigidbody rigidbody;

    [SerializeField] Transform centerOfMass;

    [Header("Wheels")]
    [SerializeField] Wheel[] frontWheels;
    [SerializeField] Wheel[] rearWheels;
    [SerializeField] float steeringAngle = 35f;
    [SerializeField] float steeringSpeed = 2f;

    private float steeringDirection = 0f;
    private bool grounded = false;

    public bool Grounded{get => grounded;}

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        if (centerOfMass) rigidbody.centerOfMass = centerOfMass.localPosition;
    }

    private void Update()
    {
        OnUpdate();
        
    }

    protected virtual void OnUpdate(){
        AddSteeringInput(Input.GetAxisRaw("Horizontal"));
    }

    private void FixedUpdate()
    {
        OnFixedUpdate();
    }

    protected virtual void OnFixedUpdate(){
        int groundedWheels = 0;

        foreach(Wheel wheel in frontWheels){
            Transform t = wheel.transform;
            Quaternion targetRotation = Quaternion.AngleAxis(steeringAngle * steeringDirection, Vector3.up);
            Quaternion newRotation = Quaternion.RotateTowards(t.localRotation, targetRotation, steeringSpeed * steeringAngle * Time.fixedDeltaTime);
            t.localRotation = newRotation;

            if (wheel.Grounded) groundedWheels++;
        }

        foreach(Wheel wheel in rearWheels){
            if (wheel.Grounded) groundedWheels++;
        }
        
        grounded = groundedWheels > 0f;
    }

    public void AddSteeringInput(float input){
        steeringDirection = input;
    }
}
