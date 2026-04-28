using System.Collections.Generic;
using UnityEngine;
public class SudokuSaveManager : MonoBehaviour
{
    SaveService saveService = new SaveService();
    [SerializeField] SudokuBoardController board;
    [SerializeField] SudokuTimer timer;
    [SerializeField] SudokuMistakeSystem mistakeSystem;
    int currentSlot = 0;
    public void AutoSave()
    {
        SaveGame(currentSlot);
    }
    public void SaveGame(int slot)
    {
        if (board == null || timer == null || mistakeSystem == null)
        {
            Debug.LogWarning("SaveManager en modo MENU (solo lectura)");
            return;
        }
        var data = new SudokuSaveData
        {
            board = board.boardData,
            initialBoard = board.GetInitialData(),
            time = timer.GetTime(),
            difficulty = (int)SudokuGameManager.Instance.difficulty,
            undoStack = board.GetUndoStack() ?? new List<SudokuMove>(),
            redoStack = board.GetRedoStack() ?? new List<SudokuMove>(),
            mistakes = mistakeSystem.GetMistakes(),
            previewValues = (int[])board.boardData.values.Clone()
        };
        saveService.Save(data, slot);
    }
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
        board.AutoFillNotes();
        Debug.Log("Notas celda 0: " + data.board.notesMask[0]);
        if (data.initialBoard != null)
        {
            board.SetInitialState(data.initialBoard);
        }
        timer.SetTime(data.time);
        SudokuGameManager.Instance.difficulty =
            (SudokuGameManager.Difficulty)data.difficulty;
        mistakeSystem.Init(data.mistakes);
        return true;
    }
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