using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SudokuSaveSlotItem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI info;
    [SerializeField] Button mainButton;
    [SerializeField] Button createButton;
    [SerializeField] Button continueButton;
    [SerializeField] Button deleteButton;

    int slotIndex;
    SudokuSaveManager saveManager;
    SudokuGameFlowController flow;

    public void Init(int index, SudokuSaveManager manager, SudokuGameFlowController flowController)
    {
        slotIndex = index;
        saveManager = manager;
        flow = flowController;

        // limpiar listeners por seguridad
        createButton.onClick.RemoveAllListeners();
        continueButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();

        // asignar eventos
        createButton.onClick.AddListener(OnCreate);
        continueButton.onClick.AddListener(OnContinue);
        deleteButton.onClick.AddListener(OnDelete);

        Refresh();
    }

    public void Refresh()
    {
        var data = saveManager.GetSlotData(slotIndex);

        title.text = $"Slot {slotIndex + 1}";

        if (data == null)
        {
            info.text = "Nuevo juego";

            createButton.gameObject.SetActive(true);
            continueButton.gameObject.SetActive(false);
            deleteButton.gameObject.SetActive(false);

            return;
        }

        info.text = $"{((SudokuGameManager.Difficulty)data.difficulty)} • {FormatTime(data.time)}";

        createButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(true);
        deleteButton.gameObject.SetActive(true);
    }

    void OnClick()
    {
        if (saveManager.HasSlot(slotIndex))
        {
            saveManager.LoadGame(slotIndex);
            flow.LoadFromSave();
        }
        else
        {
            SudokuGameManager.Instance.difficulty = SudokuGameManager.Difficulty.Medium;
            flow.GenerateNewGame(); // lo creamos abajo
        }
    }

    string FormatTime(float t)
    {
        int min = Mathf.FloorToInt(t / 60);
        int sec = Mathf.FloorToInt(t % 60);
        return $"{min:00}:{sec:00}";
    }
    void OnCreate()
    {
        Debug.Log("Crear nuevo sudoku en slot " + slotIndex);

        SudokuGameManager.Instance.difficulty = SudokuGameManager.Difficulty.Medium;

        flow.GenerateNewGame();

        saveManager.SaveGame(slotIndex); // importante guardar en este slot
    }
    void OnContinue()
    {
        Debug.Log("Continuar slot " + slotIndex);

        saveManager.LoadGame(slotIndex);
        flow.LoadFromSave();
    }
    void OnDelete()
    {
        Debug.Log("Eliminar slot " + slotIndex);

        saveManager.DeleteSlot(slotIndex);
        Refresh();
    }

}