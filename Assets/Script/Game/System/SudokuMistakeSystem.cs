using System;
using UnityEngine;
public class SudokuMistakeSystem : MonoBehaviour
{
    public int maxMistakes = 3;
    int currentMistakes;
    public event Action<int> OnMistakeChanged;
    public event Action OnGameOver;
    public void Init(int savedMistakes = 0)
    {
        currentMistakes = savedMistakes;
        OnMistakeChanged?.Invoke(currentMistakes);
    }
    public void RegisterMistake()
    {
        currentMistakes++;
        OnMistakeChanged?.Invoke(currentMistakes);
        if (currentMistakes >= maxMistakes)
        {
            Debug.Log("GAME OVER");
            OnGameOver?.Invoke();
        }
    }
    public int GetMistakes() => currentMistakes;
}