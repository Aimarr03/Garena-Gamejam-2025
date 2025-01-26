using System.Collections.Generic;
using UnityEngine;

public class UI_TargetFloorManager : MonoBehaviour
{
    [SerializeField] private List<UI_TargetFloorChild> _children;
    private void Start()
    {
        Elevator.OnUpdatingPassengers += Elevator_OnUpdatingPassengers;
    }
    private void OnDisable()
    {
        Elevator.OnUpdatingPassengers -= Elevator_OnUpdatingPassengers;
    }

    private void Elevator_OnUpdatingPassengers(int index, int value)
    {
        _children[index].HandlesUpdating(value);
    }
}
