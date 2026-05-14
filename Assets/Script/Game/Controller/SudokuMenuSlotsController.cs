using System.Collections.Generic;
using UnityEngine;

public class SudokuMenuSlotsController : MonoBehaviour
{
    [SerializeField] SudokuSaveManager saveManager;
    [SerializeField] SudokuSessionController sessionController;
    [SerializeField] SudokuSaveSlotItem prefab;
    [SerializeField] Transform container;
    [SerializeField] int slotCount = 3;
    [SerializeField]SudokuDifficultySelectionPanel difficultySelectionPanel;

    readonly List<SudokuSaveSlotItem> slotViews = new List<SudokuSaveSlotItem>();

    void Start()
    {
        // Si no hay panel asignado, busca en la escena
        if (difficultySelectionPanel == null)
        {
            difficultySelectionPanel = FindFirstObjectByType<SudokuDifficultySelectionPanel>();
            Debug.Log($"Encontrado panel de dificultad: {difficultySelectionPanel != null}");
        }

        if (difficultySelectionPanel != null)
            difficultySelectionPanel.OnDifficultySelected += HandleDifficultySelected;
        else
            Debug.LogWarning("No se encontró panel de dificultad en SudokuMenuSlotsController");

        if (prefab == null || container == null)
        {
            Debug.LogWarning($"SudokuMenuSlotsController en '{name}' no tiene prefab o container asignado. No se generarán slots.");
            return;
        }

        BuildView();
        RefreshAll();
    }

    void OnDestroy()
    {
        if (difficultySelectionPanel != null)
            difficultySelectionPanel.OnDifficultySelected -= HandleDifficultySelected;

        foreach (var slot in slotViews)
        {
            if (slot == null)
                continue;

            slot.OnContinueRequested -= HandleContinueRequested;
            slot.OnDeleteRequested -= HandleDeleteRequested;
            slot.OnCreateRequested -= HandleCreateRequested;
        }
    }

    void BuildView()
    {
        foreach (Transform child in container)
            Destroy(child.gameObject);

        slotViews.Clear();
        for (int i = 0; i < slotCount; i++)
        {
            var slot = Instantiate(prefab, container);
            slot.Init(i);
            slot.OnContinueRequested += HandleContinueRequested;
            slot.OnDeleteRequested += HandleDeleteRequested;
            slot.OnCreateRequested += HandleCreateRequested;
            slotViews.Add(slot);
        }
    }

    void RefreshAll()
    {
        for (int i = 0; i < slotViews.Count; i++)
            RefreshSlot(i);
    }

    void RefreshSlot(int slotIndex)
    {
        var view = slotViews[slotIndex];
        var data = saveManager.GetSlotData(slotIndex);
        if (data == null)
        {
            view.RenderEmpty();
            return;
        }

        view.RenderSaved((SudokuGameManager.Difficulty)data.difficulty, data.time);
    }

    void HandleContinueRequested(int slotIndex)
    {
        if (!saveManager.HasSlot(slotIndex))
        {
            Debug.LogWarning($"No existe guardado para slot {slotIndex}");
            RefreshSlot(slotIndex);
            return;
        }

        sessionController.ContinueGame(slotIndex);
    }

    void HandleDeleteRequested(int slotIndex)
    {
        saveManager.DeleteSlot(slotIndex);
        RefreshSlot(slotIndex);
    }

    void HandleCreateRequested(int slotIndex)
    {
        Debug.Log($"HandleCreateRequested llamado para slot {slotIndex}, panel: {difficultySelectionPanel}");
        if (difficultySelectionPanel != null)
            difficultySelectionPanel.Open(slotIndex);
        else
            Debug.LogWarning("Difficulty selection panel no asignado en SudokuMenuSlotsController");
    }

    void HandleDifficultySelected(int slotIndex, SudokuGameManager.Difficulty difficulty)
    {
        sessionController.StartNewGame(slotIndex, difficulty);
    }
}
