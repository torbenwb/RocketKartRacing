using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    Rigidbody rigidbody;
    public Transform centerOfMass;
    public Transform[] frontWheels;
    public Transform[] rearWheels;
    public TextMeshProUGUI speed;
    public float downForce = 1f;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        if (centerOfMass) rigidbody.centerOfMass = centerOfMass.localPosition;
    }

    private void FixedUpdate()
    {
        foreach(Transform t in frontWheels){
            t.localRotation = Quaternion.AngleAxis(30f * Input.GetAxisRaw("Horizontal"), t.up);
        }

        float forwardVelocity = Mathf.Abs(Vector3.Dot(transform.forward, rigidbody.velocity));
        //rigidbody.AddForce(transform.up * -forwardVelocity * downForce);
    }
}
