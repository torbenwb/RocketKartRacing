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
    car.Drive(Input.GetAxisRaw("Vertical"));
    car.Turn(Input.GetAxis("Horizontal"));
    car.Brake(Input.GetKey(KeyCode.LeftShift) ? 1f : 0f);
  }
  
}
