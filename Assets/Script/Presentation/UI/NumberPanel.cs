using System;
using UnityEngine;
public class NumberPanel : MonoBehaviour
{
    public static event Action<int> OnNumberPressed;
    public void PressNumber(int number)
    {
        OnNumberPressed?.Invoke(number);
    }
    public void ClearNumber()
    {
        OnNumberPressed?.Invoke(0);
    }
}