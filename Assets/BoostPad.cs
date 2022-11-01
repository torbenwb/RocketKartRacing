using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPad : MonoBehaviour
{
    public float forceAmount = 10f;
    private void OnTriggerEnter(Collider other)
    {
        CarController carController = other.GetComponentInParent<CarController>(); 
        if (carController){
            Rigidbody rigidbody = carController.transform.GetComponent<Rigidbody>();

            rigidbody.AddForce(rigidbody.transform.forward * forceAmount, ForceMode.Impulse);
        }      
    }
    
}
