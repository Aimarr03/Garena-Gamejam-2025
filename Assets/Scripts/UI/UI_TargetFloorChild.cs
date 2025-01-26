using TMPro;
using UnityEngine;

public class UI_TargetFloorChild : MonoBehaviour
{
    public int value = 0;
    public TextMeshProUGUI displayValue;

    private void Start()
    {
        displayValue.text = value.ToString();
    }
    public void HandlesUpdating(int AdditionValue)
    {
        value = Mathf.Clamp(value + AdditionValue, 0, int.MaxValue);
        displayValue.text = value.ToString();
    }
}
