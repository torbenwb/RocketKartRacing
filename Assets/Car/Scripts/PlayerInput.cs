using UnityEngine;


public class PlayerInput : MonoBehaviour
{
  Car car;
  private void Awake()
  {
    car = GameObject.FindWithTag("Car").GetComponent<Car>();
  }

  private void Update()
  {
    // Car Controls
    car.SetSteeringTarget(Input.GetAxis("Horizontal"));
    car.Accelerate(Input.GetAxis("Vertical"));
    car.Brake(Input.GetKey(KeyCode.LeftShift));
    car.Boost(Input.GetMouseButton(0));
    if (Input.GetKeyDown(KeyCode.Space)) car.Jump();

    // Camera Controls
  }
}
