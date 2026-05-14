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
    int slotIndex;
    public event Action<int> OnContinueRequested;
    public event Action<int> OnDeleteRequested;
    public event Action<int> OnCreateRequested;

    public void Init(int index)
    {
        slotIndex = index;
        createButton.onClick.RemoveAllListeners();
        continueButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();
        createButton.onClick.AddListener(OnCreate);
        continueButton.onClick.AddListener(OnContinue);
        deleteButton.onClick.AddListener(OnDelete);
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

    void OnCreate()
    {
        Debug.Log($"OnCreate llamado para slot {slotIndex}");
        OnCreateRequested?.Invoke(slotIndex);
    }
}