using UnityEngine;

public class Wheel : MonoBehaviour
{
  private bool grounded = false;
  private Transform wheelMesh;
  private float wheelRadius;
  private float wheelCircumference;
  private float wheelRotation = 0f;
  private float wheelTorque = 0f;
  private Vector3 lastHit, newHit;
  public bool isGrounded { get => grounded; }
  private Car car;

  private void Awake()
  {
    car = GetComponentInParent<Car>();
    wheelMesh = transform.GetChild(0).transform;
    wheelRadius = wheelMesh.GetComponent<MeshFilter>().mesh.bounds.extents.y;
    wheelCircumference = 3.14f * wheelRadius * 2f;
  }

  public Vector3 CalculateNetWheelForces(float acceleration, float brake)
  {
    float wheelToGroundDistance = CalculateWheelToGroundDistance();
    float maxRotationChange = car.maxTorque * Time.fixedDeltaTime;
    wheelMesh.localPosition = Vector3.up * wheelToGroundDistance;
    wheelTorque = Mathf.MoveTowards(wheelTorque, car.maxTorque * acceleration, maxRotationChange);
    if (!grounded) return Vector3.zero;
    return (
      CalculateSuspensionForce(wheelToGroundDistance) +
      CalculateForceFromWheelTorque(brake) +
      CalculateFrictionForce()
    );
  }

  /*  Find wheel vertical offset from ground using a downward arc of raycasts.
     returns:
     - wheelToGroundDistance - The distance between the wheel's transform position based on
     wheel radius and the position of the ground. An offset of 0 means wheel is not touching
     the ground. An offset greater than 0 means wheel is touching the ground.
 */
  private float CalculateWheelToGroundDistance()
  {
    float wheelToGroundDistance = 0f;
    float angleIncrement = car.raycastScanAngle / car.raycastAmount;
    grounded = false;

    // Prepare to emit a series of raycasts from the center of the wheel. 
    Vector3 scanOrigin = transform.position;
    Vector3 scanDirection = Quaternion.AngleAxis((-car.raycastScanAngle / 2), transform.right) * -transform.up;

    // Loop through each raycast and use the one with the greatest hitDistance.
    for (int i = 0; i <= car.raycastAmount; i++)
    {
      Vector3 end = scanOrigin + scanDirection * wheelRadius;
      RaycastHit hit;
      bool didHit = Physics.Raycast(scanOrigin, scanDirection, out hit, wheelRadius);
      if (didHit)
      {
        float hitDistance = (hit.point - end).magnitude;
        if (hitDistance > wheelToGroundDistance) wheelToGroundDistance = hitDistance;
        if (i == car.raycastAmount / 2) newHit = hit.point;
        grounded = true;
      }
      if (car.drawWheelGroundRaycasts) Debug.DrawLine(scanOrigin, didHit ? hit.point : end, didHit ? Color.red : Color.green);
      scanDirection = Quaternion.AngleAxis(angleIncrement, transform.right) * scanDirection;
    }
    return wheelToGroundDistance;
  }

  private Vector3 CalculateFrictionForce()
  {
    Vector3 wheelPointVelocity = car.rb.GetPointVelocity(transform.position);

    // If braking apply opposite force proportional to z axis velocity
    float zSpeed = Vector3.Dot(transform.forward, wheelPointVelocity);
    Vector3 zAxisFriction = transform.forward * -zSpeed * car.friction;
    if (car.drawFrictionVectors) Debug.DrawLine(transform.position, transform.position + zAxisFriction, Color.blue);

    // Apply oppositional x axis force, proportional x axis velocity, and friction
    float xSpeed = Vector3.Dot(transform.right, wheelPointVelocity);
    Vector3 xAxisFriction = transform.right * -xSpeed * car.friction;
    if (car.drawFrictionVectors) Debug.DrawLine(transform.position, transform.position + xAxisFriction, Color.red);

    // Combine friction vectors
    Vector3 netFriction = xAxisFriction + zAxisFriction;

    return netFriction;
  }

  private Vector3 CalculateForceFromWheelTorque(float brake)
  {
    return transform.forward * (wheelTorque - car.brakeStrength * brake);
  }

  private Vector3 CalculateSuspensionForce(float wheelToGroundDistance)
  {
    float offsetRatio = wheelToGroundDistance / car.maxOffset;

    // World space direction to apply force in
    Vector3 springDirection = transform.up;

    // Get velocity of parent's rigidbody at center of wheel
    Vector3 wheelWorldVelocity = car.rb.GetPointVelocity(transform.position);

    // Speed in spring direction
    float springSpeed = Vector3.Dot(transform.up, wheelWorldVelocity);

    // Force magnitude to apply to rigidbody (strength) - (damping)
    float springMagnitude = (car.springStrength * offsetRatio) - (springSpeed * car.springDamping);

    return springDirection * springMagnitude;
  }



  public void SpinMesh()
  {
    // TODO: Wheels should retain spin when in mid-air.
    float distanceTraveled = Vector3.Dot(newHit - lastHit, transform.forward);
    wheelRotation += (distanceTraveled / wheelCircumference) * 360f;
    if (Mathf.Abs(wheelRotation) >= 360f) wheelRotation = Mathf.RoundToInt(wheelRotation) % 90;
    wheelMesh.localRotation = Quaternion.AngleAxis(wheelRotation, Vector3.right);
    lastHit = newHit;
  }
}
