using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudokuDifficultySelectionPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] Button easyButton;
    [SerializeField] Button mediumButton;
    [SerializeField] Button hardButton;
    [SerializeField] Button expertButton;
    [SerializeField] Button extremeButton;
    [SerializeField] Button cancelButton;

    int currentSlot = -1;
    public event Action<int, SudokuGameManager.Difficulty> OnDifficultySelected;

    void OnEnable()
    {
        if (titleText != null)
            titleText.text = "Selecciona dificultad";

        SetupButtons();
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(Close);
        }
    }

    public void Open(int slot)
    {
        currentSlot = slot;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        currentSlot = -1;
        gameObject.SetActive(false);
    }

    void SetupButtons()
    {
        if (easyButton != null)
        {
            easyButton.onClick.RemoveAllListeners();
            easyButton.onClick.AddListener(() => SelectDifficulty(SudokuGameManager.Difficulty.Easy));
        }
        if (mediumButton != null)
        {
            mediumButton.onClick.RemoveAllListeners();
            mediumButton.onClick.AddListener(() => SelectDifficulty(SudokuGameManager.Difficulty.Medium));
        }
        if (hardButton != null)
        {
            hardButton.onClick.RemoveAllListeners();
            hardButton.onClick.AddListener(() => SelectDifficulty(SudokuGameManager.Difficulty.Hard));
        }
        if (expertButton != null)
        {
            expertButton.onClick.RemoveAllListeners();
            expertButton.onClick.AddListener(() => SelectDifficulty(SudokuGameManager.Difficulty.Expert));
        }
        if (extremeButton != null)
        {
            extremeButton.onClick.RemoveAllListeners();
            extremeButton.onClick.AddListener(() => SelectDifficulty(SudokuGameManager.Difficulty.Extreme));
        }
    }

    void SelectDifficulty(SudokuGameManager.Difficulty difficulty)
    {
        if (currentSlot < 0)
            return;

        OnDifficultySelected?.Invoke(currentSlot, difficulty);
        Close();
    }
}
