using System.Collections.Generic;
using UnityEngine;

public class SudokuSaveManager : MonoBehaviour
{
    SaveService saveService = new SaveService();

    [SerializeField] SudokuBoardController board;
    [SerializeField] SudokuTimer timer;
    [SerializeField] SudokuMistakeSystem mistakeSystem;

    int currentSlot = 0; // 👈 slot activo (0 = continue)

    // =========================
    // AUTO SAVE
    // =========================
    public void AutoSave()
    {
        SaveGame(currentSlot);
    }

    // =========================
    // SAVE MANUAL
    // =========================
    public void SaveGame(int slot)
    {
        var data = new SudokuSaveData
        {
            board = board.boardData,
            time = timer.GetTime(),
            difficulty = (int)SudokuGameManager.Instance.difficulty,
            undoStack = board.GetUndoStack() ?? new List<SudokuMove>(),
            redoStack = board.GetRedoStack() ?? new List<SudokuMove>(),
            mistakes = mistakeSystem.GetMistakes(),

            previewValues = (int[])board.boardData.values.Clone()
        };

        saveService.Save(data, slot);
    }

    // =========================
    // LOAD
    // =========================
    public bool LoadGame(int slot)
    {
        if (!saveService.Load(slot, out var data))
            return false;

        if (data == null || data.board == null)
        {
            Debug.LogError("SAVE DATA CORRUPTA");
            return false;
        }

        currentSlot = slot;

        board.SetBoardData(data.board);
        board.SetUndoRedo(data.undoStack, data.redoStack);

        timer.SetTime(data.time);

        SudokuGameManager.Instance.difficulty =
            (SudokuGameManager.Difficulty)data.difficulty;

        mistakeSystem.Init(data.mistakes);

        return true;
    }

    // =========================
    // CONTINUE
    // =========================
    public bool HasContinueGame()
    {
        return saveService.Exists(0);
    }

    public void DeleteSlot(int slot)
    {
        saveService.Delete(slot);
    }
    public bool HasSlot(int slot)
    {
        return saveService.Exists(slot);
    }
    public SudokuSaveData GetSlotData(int slot)
    {
        if (!saveService.Load(slot, out var data))
            return null;

        return data;
    }
}