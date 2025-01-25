using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SpawnManager : MonoBehaviour
{
    public List<FloorManager> ListOfFloors = new List<FloorManager>();

    [SerializeField, Range(0,15f)] private float _baseInterval;
    [SerializeField, Range(1, 30)] private int maxPassenger;
    private float currentDuration = 0;
    private int currentPassengerCount = 0;

    [SerializeField] private Passenger passengerPrefab;
    
    private Queue<Passenger> queuePasengers = new Queue<Passenger>();
    [SerializeField] private Transform poolContainer;
    private void Awake()
    {
        List<FloorManager> FoundFloor = FindObjectsByType<FloorManager>(FindObjectsSortMode.None).ToList();
        ListOfFloors = FoundFloor.OrderBy(floor => floor.floorNumber).ToList();

        //Instantiate Pooling
        for(int index = 0; index < maxPassenger; index++)
        {
            Passenger _currentPasenger = Instantiate(passengerPrefab, poolContainer);
            _currentPasenger.gameObject.SetActive(false);
        }
    }
    private void Start()
    {
        Passenger.OnFinishState += Passenger_OnFinishState;
    }

    private void OnDisable()
    {
        Passenger.OnFinishState += Passenger_OnFinishState;
    }
    private void Update()
    {
        if (currentPassengerCount >= maxPassenger) return;
        currentDuration += Time.deltaTime;
        if(currentDuration > _baseInterval)
        {
            currentDuration = 0;
            Passenger passenger = DequeuePassenger();
            
            FloorManager floorManager = ListOfFloors[Random.Range(0, ListOfFloors.Count)];
            string[] FloorsConnected = floorManager.GetFloorNames();
            string nextFloorName = FloorsConnected[Random.Range(0, FloorsConnected.Length)];
            FloorManager destinationFloor = floorManager.GetFloorDestination(nextFloorName);

            passenger.gameObject.SetActive(true);
            passenger.transform.parent = null;
            
            passenger.SetCurrentFloor(floorManager);
            passenger.SetDestination(destinationFloor);
            
            passenger.transform.position = floorManager.SpawnPoint;
            passenger.SetDestination(floorManager.ElevatorPoint, PassengerState.GoingIn);
            currentPassengerCount++;
        }
    }
    private Passenger DequeuePassenger()
    {
        Passenger passenger;
        if(queuePasengers.Count <= 0)
        {
            passenger = Instantiate(passengerPrefab, poolContainer);
            passenger.transform.parent = null;
            return passenger;
        }
        else
        {
            return queuePasengers.Dequeue();
        }
    }
    public void EnqueuePassenger(Passenger passenger)
    {
        queuePasengers.Enqueue(passenger);
        passenger.transform.SetParent(poolContainer);
        passenger.gameObject.SetActive(false);
    }
    private void Passenger_OnFinishState(object passenger, PassengerState state)
    {
        if(state == PassengerState.Arrived)
        {
            EnqueuePassenger(passenger as Passenger);
        }
    }
}
