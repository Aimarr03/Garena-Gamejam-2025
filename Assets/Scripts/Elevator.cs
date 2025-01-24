using UnityEngine;

public class Elevator : MonoBehaviour
{
    private Rigidbody2D _rigidBody;
    private Collider2D _collider;

    private FloorBehaviour _currentLowerFloor;

    [SerializeField, Range(1, 15)] private float _maxUpwardVelocity = 10f;
    [SerializeField, Range(1,200)]private float forcePower = 2f;

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
    }
    private void OnDisable()
    {
        InputManager.buttonMashed -= InputManager_buttonMashed;
        InputManager.buttonUnloadElevator -= InputManager_buttonUnloadElevator;
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
        Debug.Log("Button Mashed");

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
        if(elevatorState == ElevatorState.Static)
        {
            Debug.Log("Mini Games");
            InputManager.buttonMashed += HandleDeloadingMashing;
            _currentLowerFloor = collision.gameObject.GetComponent<FloorBehaviour>();
            isGround = true;
            return;
        }
        Debug.Log($"Colliding {collision.gameObject}");
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("Hit Ground");
            Vector2 relativeVelocity = collision.relativeVelocity;

            Rigidbody2D otherRigidBody = collision.gameObject.GetComponent<Rigidbody2D>();
            float otherMass = otherRigidBody.mass;

            float thisMass = _rigidBody.mass;

            float CollisionPower = (thisMass + otherMass) * relativeVelocity.magnitude;
            Debug.Log($"Collision Power <b><color=red>{CollisionPower}</color></b>");
            isGround = true;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        isGround = false;
    }
    #region Floor Changing Behaviour
    private void OnTriggerExit2D(Collider2D collision)
    {
        //Debug.Log($"Triggering {collision.gameObject}");
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
            }
            else
            {
                if(_currentLowerFloor != null)
                {
                    FloorBehaviour lowerFloor = _currentLowerFloor.GetLowerFloor();
                    string groundLevel = lowerFloor == null ? "Ground Level" : lowerFloor.gameObject.ToString();
                    Debug.Log($"Renew Lower Floor: {groundLevel}");
                    _currentLowerFloor = _currentLowerFloor.GetLowerFloor();
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
                Debug.Log("Return to Moving Again");
                _currentLowerFloor.EnablePlatformEffector(false);
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
        Debug.Log("Stabilizing");
    }
    #endregion
}
public enum ElevatorState
{
    Static,
    Moving
}
