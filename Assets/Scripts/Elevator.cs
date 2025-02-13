using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    private Rigidbody2D _rigidBody;
    private Collider2D _collider;
    private Animator _animator;

    private FloorBehaviour _currentLowerFloor;
    
    private FloorManager _currentFloorManager;
    private List<Passenger> passengers = new List<Passenger>();
    private int maxPassengers = 5;
    [SerializeField, UnityEngine.Range(1, 15)] private float _maxUpwardVelocity = 10f;
    [SerializeField, UnityEngine.Range(1,200)]private float forcePower = 2f;

    public static event Action<int, int> OnUpdatingPassengers;
    private bool isGround;
    private bool open = false;
    #region MONOBEHAVIOUR CALLBACKS
    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _animator = GetComponent<Animator>();
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
        if (!open)
        {
            _animator.SetBool("Open", true);
            open = true;
        }
        _currentFloorManager = collision.gameObject.GetComponent<FloorManager>();
        _currentLowerFloor = collision.gameObject.GetComponent<FloorBehaviour>();
        isGround = true;

        CalculateCollisionPower(collision);
        InputManager.buttonMashed -= InputManager_buttonMashed;
        elevatorState = ElevatorState.Static;
        InputManager.buttonMashed -= InputManager_buttonMashed;
        _currentLowerFloor = collision.gameObject.GetComponent<FloorBehaviour>();
        StartCoroutine(HandleTransitPassenger());
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        isGround = true;
        _currentFloorManager = collision.gameObject.GetComponent<FloorManager>();
        _currentLowerFloor = collision.gameObject.GetComponent<FloorBehaviour>();
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
    private IEnumerator HandleTransitPassenger()
    {
        yield return HandlesDeloadingPassengers();
        yield return HandlesLoadingPassengers();
    }
    private IEnumerator HandlesDeloadingPassengers()
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
                    OnUpdatingPassengers(currentPassenger.targetFloor.floorNumber, -1);
                    yield return new WaitForSeconds(0.2f);
                    currentPassenger.SetVisualOnLift(false);
                }
            }
            while(deportingPassengers.Count > 0)
            {
                Passenger deportedPassenger = deportingPassengers.Dequeue();
                passengers.Remove(deportedPassenger);
            }
        }
    }

    //========================================================================================//
    //========================================================================================//

    #region Passengers Logic
    private Dictionary<int, List<Passenger>> ListOfPassengersWaiting = new Dictionary<int, List<Passenger>>
    {
        { 0, new List<Passenger>() },
        { 1, new List<Passenger>() },
        { 2, new List<Passenger>() }
    };
    private void Passenger_OnFinishState(object obj, PassengerState state)
    {
        Passenger passenger = obj as Passenger;
        if(passenger.currentState == PassengerState.GoingIn)
        {
            HandlesPreLoadingPassengers(passenger);
        }
    }
    private void HandlesPreLoadingPassengers(Passenger passenger)
    {
        FloorManager passengerFloor = passenger.currentFloor;
        int floorNumber = passengerFloor.floorNumber;
        if (ListOfPassengersWaiting.ContainsKey(floorNumber))
        {
            ListOfPassengersWaiting[floorNumber].Add(passenger);
        }
        else
        {
            ListOfPassengersWaiting.Add(floorNumber, new List<Passenger>());
            ListOfPassengersWaiting[floorNumber].Add(passenger);
        }
        if(isGround && _currentFloorManager.floorNumber == floorNumber)
        {
            Debug.Log($"{passenger.name} is requesting going in");
            StartCoroutine(HandlesLoadingPassengers());
        }
    }
    [SerializeField] private Collider2D _elevatorStarterPoint;
    [SerializeField] private Collider2D _elevatorWaitingArea;
    private IEnumerator HandlesLoadingPassengers()
    {
        
        List<Passenger> passengerOnTheFloor = ListOfPassengersWaiting[_currentFloorManager.floorNumber];
        
        if (passengers.Count > maxPassengers || passengerOnTheFloor.Count == 0) yield break;
        
        Debug.Log("Current Floor passengers: " + passengerOnTheFloor.Count);
        Debug.Log("Handled Loading Passengers");
        Queue<Passenger> queue = new Queue<Passenger>(passengerOnTheFloor);
        if (queue.Count == 0) yield break;
        Bounds AreaLift = _elevatorWaitingArea.bounds;
        Vector2 destinations = new Vector2(UnityEngine.Random.Range(AreaLift.min.x, AreaLift.max.x), AreaLift.min.y);
        while(passengers.Count < maxPassengers)
        {
            Passenger passenger = queue.Dequeue();
            if(passenger.currentFloor.floorNumber != _currentFloorManager.floorNumber)
            {
                continue;
            }
            passenger.SetVisualOnLift(true);
            passengers.Add(passenger);
            passenger.transform.SetParent(transform);
            passenger.SetDestination(destinations, PassengerState.Elevator);
            passenger.transform.position = destinations;
            OnUpdatingPassengers(passenger.targetFloor.floorNumber, 1);
            if (queue.Count == 0) break;
            yield return new WaitForSeconds(0.5f);
        }
        yield break;
    }
    private void HandlesEnteringElevator()
    {
        Vector2 destinations = _collider.bounds.min;
    }
    #endregion

    #region Floor Changing Logic
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
                InputManager.buttonMashed -= HandleStabilizeLift;
                InputManager.buttonMashed += InputManager_buttonMashed;
                elevatorState = ElevatorState.Moving;
                if (open)
                {
                    open = false;
                    _animator.SetBool("Open", open);
                }
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
    private void HandleStabilizeLift()
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
