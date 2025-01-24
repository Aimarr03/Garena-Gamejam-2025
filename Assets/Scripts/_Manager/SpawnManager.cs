using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SpawnManager : MonoBehaviour
{
    public List<FloorManager> ListOfFloors = new List<FloorManager>();

    [SerializeField, Range(0,3f)] private float _baseInterval;
    [SerializeField, Range(10, 30)] private int maxPassenger;
    private float currentDuration = 0;

    [SerializeField] private Passenger passengerPrefab;
    
    private Queue<Passenger> queuePasengers = new Queue<Passenger>();
    private Transform poolContainer;
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
    private void Update()
    {
        currentDuration += Time.deltaTime;
        if(currentDuration > _baseInterval)
        {
            currentDuration = 0;
            FloorManager floorManager = ListOfFloors[Random.Range(0, ListOfFloors.Count)];
            Passenger passenger = DequeuePassenger();
            passenger.transform.position = floorManager.SpawnPoint;
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
        passenger.gameObject.SetActive(false);
    }
}
