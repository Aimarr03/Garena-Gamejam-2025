using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    private Rigidbody2D _rigidBody;
    private Collider2D _collider;

    private FloorBehaviour _currentLowerFloor;
    
    private FloorManager _currentFloorManager;
    private List<Passenger> passengers = new List<Passenger>();
    private int maxPassengers = 5;

    [SerializeField, UnityEngine.Range(1, 15)] private float _maxUpwardVelocity = 10f;
    [SerializeField, UnityEngine.Range(1,200)]private float forcePower = 2f;

    private bool isGround;
    #region MONOBEHAVIOUR CALLBACKS
    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
    }
    private void Start()
    {
        InputManager.buttonMashed += InputManager_buttonMashed;
        InputManager.buttonUnloadElevator += InputManager_buttonUnloadElevator;
        Passenger.OnFinishState += Passenger_OnFinishState;
    }
    private void OnDisable()
    {
        InputManager.buttonMashed -= InputManager_buttonMashed;
        InputManager.buttonUnloadElevator -= InputManager_buttonUnloadElevator;
        Passenger.OnFinishState -= Passenger_OnFinishState;
    }
    private void FixedUpdate()
    {
        if (_rigidBody.linearVelocity.y > 0)
        {
            _rigidBody.linearVelocity += Vector2.down * (forcePower * 0.007f) * Time.fixedDeltaTime;
        }
    }
    #endregion
    private void InputManager_buttonMashed()
    {
        //Debug.Log("Button Mashed");

        Vector2 currentVelocity = _rigidBody.linearVelocity;

        currentVelocity.y += Mathf.Lerp(currentVelocity.y, _maxUpwardVelocity, Time.fixedDeltaTime * forcePower);

        if (currentVelocity.y > _maxUpwardVelocity)
        {
            currentVelocity.y = _maxUpwardVelocity;
        }
        _rigidBody.linearVelocity = currentVelocity;
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Colliding {collision.gameObject}");
        _currentFloorManager ??= collision.gameObject.GetComponent<FloorManager>();
        _currentLowerFloor ??= collision.gameObject.GetComponent<FloorBehaviour>();
        isGround = true;

        CalculateCollisionPower(collision);

        if (elevatorState == ElevatorState.Static)
        {
            InputManager.buttonMashed += HandleDeloadingMashing;
            _currentLowerFloor = collision.gameObject.GetComponent<FloorBehaviour>();

            HandlesDeloadingPassengers();
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        Debug.Log("Exiting from " + collision.gameObject.name);
        isGround = false;
    }
    /*private void OnCollisionStay(Collision collision)
    {
        HandlesLoadingPassengers();
    }*/
    private void CalculateCollisionPower(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Floor"))
        {
            Debug.Log("Hit Ground");
            Vector2 relativeVelocity = collision.relativeVelocity;

            Rigidbody2D otherRigidBody = collision.gameObject.GetComponent<Rigidbody2D>();
            float otherMass = otherRigidBody.mass;

            float thisMass = _rigidBody.mass;

            float CollisionPower = (thisMass + otherMass) * relativeVelocity.magnitude;
            Debug.Log($"Collision Power <b><color=red>{CollisionPower}</color></b>");
        }
    }
    private void HandlesDeloadingPassengers()
    {
        if (passengers.Count > 0)
        {
            Debug.Log("Deloading Passengers");
            Queue<Passenger> deportingPassengers = new Queue<Passenger>();

            foreach (Passenger currentPassenger in passengers)
            {
                Debug.Log($"{currentPassenger.name} target Floors: {currentPassenger.targetFloor.name} " +
                    $"current Floors: {_currentFloorManager.name}");
                
                bool testament = currentPassenger.targetFloor == _currentFloorManager;
                if (testament)
                {
                    currentPassenger.currentState = PassengerState.GoingOut;
                    currentPassenger.SetCurrentFloor(_currentFloorManager);
                    currentPassenger.transform.SetParent(null);
                    deportingPassengers.Enqueue(currentPassenger);
                    
                    currentPassenger.SetDestination(_currentFloorManager.EndPoint, PassengerState.GoingOut);
                }
            }
            Debug.Log(deportingPassengers.Count);
            while(deportingPassengers.Count > 0)
            {
                Passenger deportedPassenger = deportingPassengers.Dequeue();
                passengers.Remove(deportedPassenger);
            }
        }
    }
    private void Passenger_OnFinishState(object obj, PassengerState state)
    {
        Passenger passenger = obj as Passenger;
        if(passenger.currentState == PassengerState.GoingIn)
        {
            HandlesLoadingPassengers(passenger);
        }
    }
    private void HandlesLoadingPassengers(Passenger passenger)
    {
        if (passengers.Count > maxPassengers) return;
        Debug.Log("Current Floor passengers: " + _currentFloorManager.passengers.Count);
        Debug.Log("Handled Loading Passengers");
        if (passenger.currentState == PassengerState.GoingOut) return;

        Vector2 destinations = _collider.bounds.min;
        passenger.SetDestination(destinations, PassengerState.Elevator);
        passengers.Add(passenger);
        passenger.transform.SetParent(transform);
    }
    
    #region Floor Changing Behaviour
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (elevatorState == ElevatorState.Static) return;
        
        Debug.Log($"Triggering Exit {collision.gameObject}");
        if (collision.gameObject.CompareTag("Floor"))
        {
            float bottomElevatorPosition = _collider.bounds.min.y;
            float upperFloorPosition = collision.bounds.max.y;
            float calculateDistance = bottomElevatorPosition - upperFloorPosition;
            //Debug.Log($"Calculate Distance = {calculateDistance}");
            if(calculateDistance > 0)
            {
                Debug.Log($"Renew Floor: {collision.gameObject}");
                _currentLowerFloor = collision.gameObject.GetComponent<FloorBehaviour>();
                _currentFloorManager = collision.gameObject.GetComponent<FloorManager>();
            }
            else
            {
                if(_currentLowerFloor != null)
                {
                    FloorBehaviour lowerFloor = _currentLowerFloor.GetLowerFloor();
                    string groundLevel = lowerFloor == null ? "Ground Level" : lowerFloor.gameObject.ToString();
                    Debug.Log($"Renew Lower Floor: {groundLevel}");
                    _currentLowerFloor = lowerFloor;
                    _currentFloorManager = _currentFloorManager.lowerFloor;
                }
            }
        }
    }
    #endregion
    #region Unload Passengers Logic
    private ElevatorState elevatorState = ElevatorState.Moving;
    private void InputManager_buttonUnloadElevator()
    {
        if (elevatorState == ElevatorState.Static)
        {
            if (isGround)
            {
                foreach (Passenger passenger in passengers)
                {
                    if (passenger.transform.parent == null) return;
                }
                Debug.Log("Return to Moving Again");
                _currentLowerFloor?.EnablePlatformEffector(false);
                InputManager.buttonMashed -= HandleDeloadingMashing;
                InputManager.buttonMashed += InputManager_buttonMashed;
                elevatorState = ElevatorState.Moving;
            }
            return;
        }
        elevatorState = ElevatorState.Static;
        InputManager.buttonMashed -= InputManager_buttonMashed;
        if(_currentLowerFloor != null)
        {
            _currentLowerFloor.EnablePlatformEffector(true);
        }
    }
    private void HandleDeloadingMashing()
    {
        //Debug.Log("Stabilizing");
    }
    #endregion
}
public enum ElevatorState
{
    Static,
    Moving
}
