using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SudokuSaveSlotUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI slotLabel;
    [SerializeField] Button button;
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
        SudokuGameSession.SelectedSlot = slotIndex;
        if (saveManager.HasSlot(slotIndex))
        {
            SudokuGameSession.LoadFromSave = true;
        }
        else
        {
            SudokuGameSession.LoadFromSave = false;
            SudokuGameSession.SelectedDifficulty = SudokuGameManager.Difficulty.Medium;
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
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