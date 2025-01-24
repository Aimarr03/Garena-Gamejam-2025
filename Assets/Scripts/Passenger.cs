using UnityEngine;

public class Passenger : MonoBehaviour
{
    [HideInInspector] public FloorManager _currentFloor;
    [HideInInspector] public FloorManager targetFloor;

    [SerializeField, Range(0, 0.3f)] private float weight;
    [SerializeField, Range(2, 4)] private float speed;
    private PassengerState currentState;
    private Vector2 Destinations;
    private void Update()
    {
        switch(currentState)
        {
            case PassengerState.Idle: 

                break;
            case PassengerState.Moving:
                Vector2.MoveTowards(transform.position, Destinations, Time.deltaTime * speed);
                if(Vector2.Distance(transform.position, Destinations) < 0.02f)
                {
                    currentState = PassengerState.Idle;
                }
                break;
        }
    }
    public void SetDestinations(Vector2 newDestination)
    {
        Destinations = newDestination;
        if(currentState == PassengerState.Idle)
        {
            currentState = PassengerState.Moving;
        }
    }
}
public enum PassengerState
{
    Idle, Moving
}
