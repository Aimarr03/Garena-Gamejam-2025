using UnityEngine;

[RequireComponent (typeof(PlatformEffector2D))]
public class FloorBehaviour : MonoBehaviour
{
    private PlatformEffector2D _platformEffector;
    private Collider2D _collider;
    [SerializeField] private FloorBehaviour lowerFloor;
    private void Awake()
    {
        _platformEffector = GetComponent<PlatformEffector2D>();
        _collider = GetComponent<Collider2D>();
        _platformEffector.enabled = false;
        _collider.isTrigger = true;
        //_collider.enabled = false;
    }
    public void EnablePlatformEffector(bool value)
    {
        _platformEffector.enabled = value;
        //_collider.enabled = value;
    }
    public FloorBehaviour GetLowerFloor() => lowerFloor;
}
