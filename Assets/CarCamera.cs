using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCamera : MonoBehaviour
{
   public CarController carController;
    public Transform carTransform;
    public Transform ballTransform;
    public bool followBall = true;
    public Vector3 carOffset = Vector3.up;
    public float targetArmLength = 5f;
    public float moveSpeed = 10f;
    public float minRotationDelta = 60f;
    public float maxDistance = 2f;

    private void FixedUpdate()
    {
        Quaternion targetRotation;
        Vector3 targetPosition;
        if (!followBall){
            targetRotation = Quaternion.LookRotation((carTransform.position + carOffset) - Camera.main.transform.position);
            Vector3 direction = (carTransform.position + carOffset) - Camera.main.transform.position;
            direction.Normalize();
            targetPosition = (carTransform.position + carOffset) + (-direction * targetArmLength);
            targetPosition = RaycastTestPosition(carTransform.position, targetPosition);
        }
        else {
            targetRotation = Quaternion.LookRotation(ballTransform.position - Camera.main.transform.position);
            targetPosition = GetTargetPosition();
        }
        
        
        
        Camera.main.transform.position = targetPosition;
        Camera.main.transform.rotation = targetRotation;
    }

    Vector3 RaycastTestPosition(Vector3 origin, Vector3 targetPosition){
        Vector3 direction = targetPosition - origin;
        float distance = (targetPosition - origin).magnitude;
        RaycastHit hit;
        if (Physics.Raycast(origin, direction.normalized, out hit, distance)) return hit.point;
        return targetPosition;
    }

    Vector3 GetTargetPosition(){
        Vector3 direction = ballTransform.position - (carTransform.position + carOffset);
        Vector3 targetPosition = (carTransform.position + carOffset) + (-direction.normalized * targetArmLength);
        return RaycastTestPosition(carTransform.position + carOffset, targetPosition);
    }
}
