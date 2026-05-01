using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SudokuSaveSlotItem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI info;
    [SerializeField] Button createButton;
    [SerializeField] Button continueButton;
    [SerializeField] Button deleteButton;
    [Header("Dificulty")]
    [SerializeField] GameObject difficultyPanel;
    [SerializeField] Button easyBtn;
    [SerializeField] Button mediumBtn;
    [SerializeField] Button hardBtn;
    [SerializeField] Button expertBtn;
    [SerializeField] Button extremeBtn;
    int slotIndex;
    public event Action<int> OnContinueRequested;
    public event Action<int> OnDeleteRequested;
    public event Action<int, SudokuGameManager.Difficulty> OnCreateRequested;

    public void Init(int index)
    {
        slotIndex = index;
        createButton.onClick.RemoveAllListeners();
        continueButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();
        createButton.onClick.AddListener(OpenDifficultyPanel);
        continueButton.onClick.AddListener(OnContinue);
        deleteButton.onClick.AddListener(OnDelete);
        easyBtn.onClick.RemoveAllListeners();
        mediumBtn.onClick.RemoveAllListeners();
        hardBtn.onClick.RemoveAllListeners();
        expertBtn.onClick.RemoveAllListeners();
        if (extremeBtn != null)
            extremeBtn.onClick.RemoveAllListeners();
        easyBtn.onClick.AddListener(() => SelectDifficulty(SudokuGameManager.Difficulty.Easy));
        mediumBtn.onClick.AddListener(() => SelectDifficulty(SudokuGameManager.Difficulty.Medium));
        hardBtn.onClick.AddListener(() => SelectDifficulty(SudokuGameManager.Difficulty.Hard));
        expertBtn.onClick.AddListener(() => SelectDifficulty(SudokuGameManager.Difficulty.Expert));
        if (extremeBtn != null)
            extremeBtn.onClick.AddListener(() => SelectDifficulty(SudokuGameManager.Difficulty.Extreme));
        difficultyPanel.SetActive(false);
        title.text = $"Slot {slotIndex + 1}";
    }

    public void RenderEmpty()
    {
        info.text = "Nuevo juego";
        createButton.gameObject.SetActive(true);
        continueButton.gameObject.SetActive(false);
        deleteButton.gameObject.SetActive(false);
    }

    public void RenderSaved(SudokuGameManager.Difficulty difficulty, float time)
    {
        info.text = $"{difficulty} • {FormatTime(time)}";
        createButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(true);
        deleteButton.gameObject.SetActive(true);
    }
    string FormatTime(float t)
    {
        int min = Mathf.FloorToInt(t / 60);
        int sec = Mathf.FloorToInt(t % 60);
        return $"{min:00}:{sec:00}";
    }
void OnContinue()
{
    OnContinueRequested?.Invoke(slotIndex);
}
    void OnDelete()
    {
        OnDeleteRequested?.Invoke(slotIndex);
    }
    void OpenDifficultyPanel()
    {
        difficultyPanel.SetActive(true);
    }
    void SelectDifficulty(SudokuGameManager.Difficulty difficulty)
    {
        difficultyPanel.SetActive(false);
        OnCreateRequested?.Invoke(slotIndex, difficulty);
    }
    public void CloseDifficultyPanel()
    {
        difficultyPanel.SetActive(false);
    }
}