using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
  [Header("Reference")]
  [SerializeField] Transform centerOfMass;
  [SerializeField] Wheel[] frontWheels;
  [SerializeField] Wheel[] backWheels;
  private List<Wheel> wheels;
  [HideInInspector] public Rigidbody rb;

  [Header("Steering")]
  [SerializeField] float maxSteeringAngle = 35f;
  [SerializeField] float steeringSpeed = 2f;
  Quaternion targetRotation = Quaternion.identity;

  [Header("Wheels")]
  public float raycastScanAngle = 60f;
  public int raycastAmount = 6;
  public float friction = 1f;
  private bool isGrounded = false;

  [Header("Suspension")]
  public float maxOffset = 2f;
  public float springStrength = 100f;
  public float springDamping = 2f;
  public float jumpForce = 20f;

  [Header("Torque")]
  public float maxTorque = 10f;
  private float acceleration = 0f;

  [Header("Braking")]
  public float brakeStrength = 1f;
  private float brake = 0f;

  [Header("Debugging")]
  public bool drawWheelGroundRaycasts = true;
  public bool drawFrictionVectors = true;
  public bool drawGroundedIndicator = true;

  void Awake()
  {
    rb = GetComponent<Rigidbody>();
    if (centerOfMass) rb.centerOfMass = centerOfMass.localPosition;
    wheels = new List<Wheel>();
    foreach (Wheel wheel in frontWheels)
    {
      wheels.Add(wheel);
    }
    foreach (Wheel wheel in backWheels)
    {
      wheels.Add(wheel);
    }
  }

  void FixedUpdate()
  {
    isGrounded = false;
    foreach (Wheel frontWheel in frontWheels)
    {
      // Apply steering
      Transform t = frontWheel.transform;
      t.localRotation = Quaternion.RotateTowards(t.localRotation, targetRotation, steeringSpeed * maxSteeringAngle * Time.fixedDeltaTime);
    }
    foreach (Wheel wheel in wheels)
    {
      // Apply wheel forces
      Vector3 forceVector = wheel.CalculateNetWheelForces(acceleration, brake);
      rb.AddForceAtPosition(forceVector, wheel.transform.position, ForceMode.Force);
      if (wheel.isGrounded) isGrounded = true;
    }
  }

  void Update()
  {
    foreach (Wheel wheel in wheels)
    {
      wheel.SpinMesh();
    }
  }

  public void SetSteeringTarget(float axis)
  {
    targetRotation = Quaternion.AngleAxis(maxSteeringAngle * axis, Vector3.up);
  }

  public void Accelerate(float magnitude)
  {
    acceleration = magnitude;
  }

  public void Brake(bool active)
  {
    brake = active ? brakeStrength : 0f;
  }

  public void Boost(bool active)
  {

  }

  public void Jump()
  {
    if (isGrounded)
    {
      rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
  }

  public void Tilt()
  {

  }
}
