using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FloorManager : MonoBehaviour
{
    [SerializeField] private Transform spawnPos;
    [SerializeField] private Transform endPos;
    [SerializeField] private Transform elevatorWaitingPos;

    public FloorManager lowerFloor; 

    private Collider2D _collider;
    public int floorNumber = 0;
    public Queue<Passenger> passengers = new Queue<Passenger>();
    public Vector3 SpawnPoint => new Vector3(spawnPos.position.x, _collider.bounds.max.y);
    public Vector3 EndPoint => new Vector3(endPos.position.x, _collider.bounds.max.y);
    public Vector3 ElevatorPoint => new Vector3(elevatorWaitingPos.position.x, _collider.bounds.max.y);
    
    Dictionary<string, FloorManager> Floors = new Dictionary<string, FloorManager>();
    List<FloorManager> ListOfFloors = new List<FloorManager>();
    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        
        List<FloorManager> FoundFloor =  FindObjectsByType<FloorManager>(FindObjectsSortMode.None).ToList();
        ListOfFloors = FoundFloor.OrderBy(floor => floor.floorNumber).ToList();
        ListOfFloors.Remove(this);
        foreach (var floor in FoundFloor)
        {
            if(floor != this)
            {
                Floors.Add(floor.name, floor);
            }
        }
    }
    public string[] GetFloorNames()
    {
        List<string> names = new List<string>();
        foreach (var item in Floors.Keys)
        {
            names.Add(item);
        }
        return names.ToArray();
    }
    public FloorManager GetFloorDestination(string floorName) => Floors[floorName];
}
