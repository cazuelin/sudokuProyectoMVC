using System;
using UnityEngine;
using UnityEngine.UI;
public class NumberPanel : MonoBehaviour
{
    public static event Action<int> OnNumberPressed;
    public static NumberPanel Instance { get; private set; }

    [SerializeField] Button[] numberButtons;

    void Awake()
    {
        Instance = this;
    }

    public void PressNumber(int number)
    {
        if (number != 0 && !IsNumberButtonInteractable(number))
            return;

        OnNumberPressed?.Invoke(number);
    }

    public void ClearNumber()
    {
        OnNumberPressed?.Invoke(0);
    }

    public void SetNumberButtonInteractable(int number, bool interactable)
    {
        var button = GetNumberButton(number);
        if (button != null)
            button.interactable = interactable;
    }

    public bool IsNumberButtonInteractable(int number)
    {
        var button = GetNumberButton(number);
        return button == null || button.interactable;
    }

    Button GetNumberButton(int number)
    {
        if (number < 1 || number > 9)
            return null;
        if (numberButtons == null || numberButtons.Length < 9)
            return null;
        return numberButtons[number - 1];
    }
}