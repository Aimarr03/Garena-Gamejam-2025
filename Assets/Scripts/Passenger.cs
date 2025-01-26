using System;
using UnityEngine;

public class Passenger : MonoBehaviour
{
    private Collider2D _collider;
    private Rigidbody2D _rigidBody;
    private Animator _animator;

    [Header("ID")] public string ID = "ID";

    public FloorManager currentFloor;
    public FloorManager targetFloor;

    [SerializeField, Range(0, 0.3f)] private float weight = 0.1f;
    [SerializeField, Range(2, 4)] private float speed = 2.5f;
    [SerializeField] private LayerMask npcMask;

    private PassengerState _currentState;
    public bool isChanging = false;
    public PassengerState currentState
    {
        get => _currentState;
        set
        {
            Debug.Log($"State changing from {_currentState} to {value}");
            _currentState = value;
        }
    }
    private Vector2 destination;
    private Vector2 movementDirection;

    private SpriteRenderer[] allSpriteRenderers;

    public static event EventHandler<PassengerState> OnFinishState;
    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        allSpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        if (_collider == null || _rigidBody == null)
        {
            Debug.LogError("Passenger is missing required components.");
        }
    }

    private void Update()
    {
        if (isChanging) return;
        HandleMovementLogic();
        HandleDetectingLogic();
        //Debug.Log(currentState);
    }

    private void HandleMovementLogic()
    {
        if (currentState == PassengerState.Idle || currentState == PassengerState.Arrived) return;

        if (Vector2.Distance(transform.position, destination) < 0.2f)
        {
            _rigidBody.linearVelocity = Vector2.zero;
            switch (currentState)
            {
                case PassengerState.GoingIn:
                    OnFinishState?.Invoke(this, currentState);
                    currentState = PassengerState.Idle;
                    isChanging = true;
                    break;
                case PassengerState.Elevator:
                    Debug.Log("Arrive at elevator");
                    currentState = PassengerState.Idle;
                    isChanging = true;
                    break;
                case PassengerState.GoingOut:
                    Debug.Log("Arrived at final destination");
                    currentState = PassengerState.Arrived;
                    OnFinishState?.Invoke(this, currentState);
                    isChanging = true;
                    break;
            }
            _animator.SetBool("IsMoving", false);
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
                    _rigidBody.linearVelocity = Vector2.zero;
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
            isChanging = false;
        }

    }

    public void SetDestination(Vector2 newDestination, PassengerState state)
    {
        destination = newDestination;
        destination.y = transform.position.y;
        movementDirection = (destination - (Vector2)transform.position).normalized;
        movementDirection.x = movementDirection.x > 0? 1 : -1;
        currentState = state;
        _rigidBody.linearVelocityX = speed * movementDirection.x;
        Debug.Log($"Destination set to {Vector2.Distance(transform.position, destination)} with state {currentState}.");
        isChanging = false;
        _animator.SetBool("IsMoving", true);
    }
    public void SetVisualOnLift(bool value)
    {
        int orders = value ? 30 : -30;
        foreach(SpriteRenderer sprite in allSpriteRenderers)
        {
            sprite.sortingOrder += orders;
        }
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
