using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FloorManager : MonoBehaviour
{
    [SerializeField] private Transform spawnPos;
    [SerializeField] private Transform endPos;
    private Collider2D _collider;
    public int floorNumber = 0;
    public Vector3 SpawnPoint => new Vector3(spawnPos.position.x, _collider.bounds.max.y);
    public Vector3 EndPoint => new Vector3(endPos.position.x, _collider.bounds.max.y);
    
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
    public void GetFloorDestination()
    {

    }
}
