using System;
using UnityEngine;

public class Passenger : MonoBehaviour
{
    private Collider2D _collider;
    private Rigidbody2D _rigidBody;

    public FloorManager currentFloor;
    public FloorManager targetFloor;

    [SerializeField, Range(0, 0.3f)] private float weight = 0.1f;
    [SerializeField, Range(2, 4)] private float speed = 2.5f;
    [SerializeField] private LayerMask npcMask;

    public PassengerState currentState = PassengerState.Idle;
    private Vector2 destination;
    private Vector2 movementDirection;
    private float direction;

    public delegate void OnArrive();

    public static event EventHandler<PassengerState> OnFinishState;
    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _rigidBody = GetComponent<Rigidbody2D>();

        if (_collider == null || _rigidBody == null)
        {
            Debug.LogError("Passenger is missing required components.");
        }
    }

    private void Update()
    {
        HandleMovementLogic();
        HandleDetectingLogic();
    }

    private void HandleMovementLogic()
    {
        if (currentState == PassengerState.Idle || currentState == PassengerState.Arrived) return;

        
        if (Vector2.Distance(transform.position, destination) < 0.1f)
        {
            _rigidBody.linearVelocity = Vector2.zero;
            switch (currentState)
            {
                case PassengerState.GoingIn:
                    OnFinishState?.Invoke(this, currentState);
                    currentState = PassengerState.Idle;
                    break;
                case PassengerState.Elevator:
                    currentState = PassengerState.Idle;
                    break;
                case PassengerState.GoingOut:
                    currentState = PassengerState.Arrived;
                    OnFinishState?.Invoke(this, currentState);
                    break;
            }
        }
    }

    private void HandleDetectingLogic()
    {
        if (currentState == PassengerState.GoingIn)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(_collider.bounds.center, movementDirection, 0.5f, npcMask);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.collider.gameObject != gameObject)
                {
                    Debug.Log("Passenger detected another object and stopped.");
                    currentState = PassengerState.Idle;
                    return;
                }
            }
        }

        if (targetFloor != null && Vector2.Distance(transform.position, targetFloor.EndPoint) < 0.01f)
        {
            Debug.Log("Passenger arrived at the target floor.");
            currentState = PassengerState.Arrived;
        }
    }

    public void SetCurrentFloor(FloorManager floorManager)
    {
        currentFloor = floorManager;
        if (currentState == PassengerState.GoingOut) return;
        currentFloor.passengers.Enqueue(this);
    }

    public void SetDestination(FloorManager targetFloor)
    {
        this.targetFloor = targetFloor;
        _rigidBody.linearVelocityX = speed * movementDirection.x;
        if (currentState == PassengerState.Idle)
        {
            currentState = PassengerState.GoingIn;
        }

    }

    public void SetDestination(Vector2 newDestination, PassengerState state)
    {
        destination = newDestination;
        movementDirection = (destination - (Vector2)transform.position).normalized;
        movementDirection.x = movementDirection.x > 0? 1 : -1;
        currentState = state;
        _rigidBody.linearVelocityX = speed * movementDirection.x;
        Debug.Log($"Destination set to {destination} with state {state}.");
    }
}

public enum PassengerState
{
    Idle,
    GoingIn,
    Elevator,
    GoingOut,
    Arrived,
}
