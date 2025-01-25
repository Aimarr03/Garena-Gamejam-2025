using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

public class SpawnManager : MonoBehaviour
{
    public List<FloorManager> ListOfFloors = new List<FloorManager>();
    
    public LevelData _levelData;
    private Queue<WaveData> queueWaves = new Queue<WaveData>();
    private Queue<int> queueSpawns = new Queue<int>();
    [SerializeField, Range(0,15f)] private float _baseInterval;
    [SerializeField, Range(1, 30)] private int maxPassenger;
    
    private float currentDuration = 0;
    
    private int currentPassengerCount = 0;
    private WaveData currentWave;

    private int currentPassengerCollected = 0;
    private bool wavePhase = false;

    [SerializeField] private Passenger[] passengerPrefabs;
    
    private Queue<Passenger> queuePasengers = new Queue<Passenger>();
    [SerializeField] private Transform poolContainer;
    private void Awake()
    {
        HandlesData();

        List<FloorManager> FoundFloor = FindObjectsByType<FloorManager>(FindObjectsSortMode.None).ToList();
        ListOfFloors = FoundFloor.OrderBy(floor => floor.floorNumber).ToList();
        
        
        for(int index = 0; index < maxPassenger; index++)
        {
            Passenger _currentPasenger = Instantiate(passengerPrefabs[Random.Range(0, passengerPrefabs.Length)], poolContainer);
            _currentPasenger.gameObject.SetActive(false);
        }
    }
    private void HandlesData()
    {
        TextAsset textLevelData = Resources.Load("Data/LevelData/Level1") as TextAsset;
        _levelData = JsonUtility.FromJson<LevelData>(textLevelData.text);
        queueWaves = new Queue<WaveData>(_levelData.waves);
        queueSpawns = new Queue<int>(_levelData.spawns);
        if(queueWaves.Count > 0)
        {
            currentWave = queueWaves.Dequeue();
        }
    }
    private void Start()
    {
        Passenger.OnFinishState += Passenger_OnFinishState;
    }

    private void OnDisable()
    {
        Passenger.OnFinishState -= Passenger_OnFinishState;
    }
    private void Update()
    {
        if (GameManager.instance.paused) return;
        if (!GameManager.instance.CheckGameState(GameState.Gameplay)) return;
        if (currentPassengerCount > 0) return;
        currentDuration += Time.deltaTime;
        if(currentDuration > _baseInterval)
        {
            currentDuration = 0;
            currentPassengerCount = queueSpawns.Dequeue();
            StartCoroutine(HandleSpawns(currentPassengerCount));
        }
    }
    
    private IEnumerator HandleSpawns(int quantity)
    {
        while(quantity > 0)
        {
            yield return new WaitForSeconds(Random.Range(_levelData.minInterval, _levelData.maxInterval));
            SpawnPassenger();
            quantity--;
            yield return null;
        }
        yield return null;
    }
    private void SpawnPassenger()
    {
        Passenger passenger = DequeuePassenger();

        //FloorManager floorManager = ListOfFloors[Random.Range(0, ListOfFloors.Count)];
        FloorManager floorManager = ListOfFloors[0];
        
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
    #region Object Pooling
    private Passenger DequeuePassenger()
    {
        Passenger passenger;
        if(queuePasengers.Count <= 0)
        {
            passenger = Instantiate(passengerPrefabs[Random.Range(0, passengerPrefabs.Length)], poolContainer);
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
            currentPassengerCount--;
            currentPassengerCollected++;
        }
        if(currentPassengerCount == 0)
        {
            if(currentPassengerCollected >= currentWave.requirementToSpawn)
            {
                wavePhase = true;
            }
        }
    }
    #endregion
}
