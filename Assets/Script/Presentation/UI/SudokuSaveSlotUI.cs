using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SudokuSaveSlotUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI slotLabel;
    [SerializeField] Button button;
    [SerializeField] SudokuSessionController sessionController;
    int slotIndex;
    SudokuSaveManager saveManager;
    public void Init(int index, SudokuSaveManager manager)
    {
        slotIndex = index;
        saveManager = manager;
        slotLabel.text = $"Slot {index + 1}";
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
        Refresh();
    }
    void OnClick()
    {
        Debug.Log($"Cargando slot {slotIndex}");
        if (sessionController != null)
            sessionController.OpenSlot(slotIndex, SudokuGameManager.Difficulty.Medium);
    }
    public void Refresh()
    {
        if (saveManager.HasSlot(slotIndex))
        {
            slotLabel.text = $"Slot {slotIndex + 1} (Guardado)";
        }
        else
        {
            slotLabel.text = $"Slot {slotIndex + 1} (Vacío)";
        }
    }
}