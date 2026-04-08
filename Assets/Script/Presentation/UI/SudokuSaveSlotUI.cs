using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SudokuSaveSlotUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI slotLabel;
    [SerializeField] Button button;

    int slotIndex;
    SudokuSaveManager saveManager;
    SudokuGameFlowController flow;

    public void Init(int index, SudokuSaveManager manager, SudokuGameFlowController flowController)
    {
        slotIndex = index;
        saveManager = manager;
        flow = flowController;

        slotLabel.text = $"Slot {index + 1}";

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);

        Refresh();
    }

    void OnClick()
    {
        Debug.Log($"Cargando slot {slotIndex}");

        if (saveManager.LoadGame(slotIndex))
        {
            flow.Initialize(flow.GetComponent<SudokuBoardView>());
        }
        else
        {
            Debug.Log("Slot vacío → generar nuevo");
            flow.Initialize(flow.GetComponent<SudokuBoardView>());
        }
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