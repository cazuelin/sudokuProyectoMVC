using System.Collections.Generic;
using UnityEngine;
public class SudokuBoardController : MonoBehaviour
{
    public SudokuBoardData boardData { get; private set; }
    SudokuBitMask bitMask;
    Stack<SudokuMove> undoStack = new Stack<SudokuMove>();
    Stack<SudokuMove> redoStack = new Stack<SudokuMove>();
    public List<SudokuMove> GetUndoStack() => new List<SudokuMove>(undoStack);
    public List<SudokuMove> GetRedoStack() => new List<SudokuMove>(redoStack);
    const int SIZE = 9;
    public void SetUndoRedo(List<SudokuMove> undo, List<SudokuMove> redo)
    {
        undoStack = undo != null
            ? new Stack<SudokuMove>(undo)
            : new Stack<SudokuMove>();

        redoStack = redo != null
            ? new Stack<SudokuMove>(redo)
            : new Stack<SudokuMove>();
    }
    [SerializeField] SudokuSaveManager saveManager;
    SudokuBoardData initialData;
    public int Get(int r, int c)
    {
        return boardData.values[r * SIZE + c];
    }
    public void Init(SudokuBoardData BoardData)
    {
        boardData = BoardData;
    }
    public void SetValue(int index, int value)
    {
        if (boardData.fixedCells[index])
            return;
        boardData.values[index] = value;
        if (value != 0)
            boardData.notesMask[index] = 0;
        NotifyBoardChanged();
        saveManager.AutoSave();
    }
    public event System.Action OnBoardChanged;
    public void PlaceNumber(int index, int number)
    {
        if (boardData.fixedCells[index])
            return;
        int r = index / 9; int c = index % 9;
        int old = boardData.values[index];
        if (old != 0)
            bitMask.Remove(r, c, old);
        boardData.values[index] = number;
        if (number != 0)
        {
            bitMask.Place(r, c, number);
            RemoveNotesFromPeers(r, c, number);
            boardData.notesMask[index] = 0;
        }
        NotifyBoardChanged();
        saveManager.AutoSave();
    }
    public bool CanPlaceNumber(int r, int c, int number)
    {
        int index = r * SIZE + c;
        int current = boardData.values[index];
        if (current != 0)
            bitMask.Remove(r, c, current);
        bool canPlace = bitMask.CanPlace(r, c, number);
        if (current != 0)
            bitMask.Place(r, c, current);
        return canPlace;
    }
    public bool IsCorrect(int r, int c, int value)
    {
        return boardData.solution[r * SIZE + c] == value;
    }
    public bool CheckWin()
    {
        for (int i = 0; i < 81; i++)
            if (boardData.values[i] == 0)
                return false;
        return true;
    }
    public SudokuBoardData GetBoardData()
    {
        return boardData;
    }
    public void SetBoardData(SudokuBoardData data)
    {
        boardData = new SudokuBoardData();
        boardData.values = (int[])data.values.Clone();
        boardData.solution = (int[])data.solution.Clone();
        boardData.fixedCells = (bool[])data.fixedCells.Clone();
        boardData.notesMask = (int[])data.notesMask.Clone();
        InitBitMask();
    }
    public int[,] GetBoardMatrix()
    {
        int[,] grid = new int[9, 9];
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                grid[r, c] = Get(r, c);
        return grid;
    }
    public int GetSolution(int r, int c)
    {
        return boardData.solution[r * SIZE + c];
    }
    public void ToggleNote(int index, int number)
    {
        if (boardData.fixedCells[index]) return;
        NotesUtil.Toggle(ref boardData.notesMask[index], number);
        NotifyBoardChanged();
        saveManager.AutoSave();
    }
    public void AutoFillNotes()
    {
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                int index = r * 9 + c;
                if (boardData.values[index] != 0)
                    continue;
                boardData.notesMask[index] = 0;
                for (int n = 1; n <= 9; n++)
                {
                    if (CanPlaceNumber(r, c, n))
                    {
                        boardData.notesMask[index] |= (1 << (n - 1));
                    }
                }
            }
        }
        NotifyBoardChanged();
        saveManager.AutoSave();
    }
    void RemoveNotesFromPeers(int row, int col, int number)
    {
        int mask = ~(1 << (number - 1));
        for (int i = 0; i < 9; i++)
        {
            boardData.notesMask[row * 9 + i] &= mask;
            boardData.notesMask[i * 9 + col] &= mask;
        }
        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
            {
                int index = (startRow + r) * 9 + (startCol + c);
                boardData.notesMask[index] &= mask;
            }
    }
    public void ApplyMove(SudokuMove move)
    {
        if (boardData.fixedCells[move.index])
            return;
        int r = move.index / 9;
        int c = move.index % 9;
        int old = boardData.values[move.index];
        if (old != 0)
            bitMask.Remove(r, c, old);
        boardData.values[move.index] = move.newValue;
        boardData.notesMask[move.index] = move.newNotes;
        if (move.newValue != 0)
        {
            bitMask.Place(r, c, move.newValue);
            RemoveNotesFromPeers(r, c, move.newValue);
        }
        undoStack.Push(move);
        redoStack.Clear();
        NotifyBoardChanged();
        saveManager.AutoSave();
    }
    public void Undo()
    {
        if (undoStack.Count == 0)
            return;
        var move = undoStack.Pop();
        int r = move.index / 9;
        int c = move.index % 9;
        int current = boardData.values[move.index];
        if (current != 0)
            bitMask.Remove(r, c, current);
        boardData.values[move.index] = move.oldValue;
        boardData.notesMask[move.index] = move.oldNotes;
        if (move.oldValue != 0)
            bitMask.Place(r, c, move.oldValue);
        redoStack.Push(move);
        NotifyBoardChanged();
        saveManager.AutoSave();
    }
    public void Redo()
    {
        if (redoStack.Count == 0)
            return;
        var move = redoStack.Pop();
        ApplyMove(move);
    }
    public void NotifyBoardChanged()
    {
        OnBoardChanged?.Invoke();
    }
    public int GetSolution(int index)
    {
        return boardData.solution[index];
    }
    public void ResetBoard()
    {
        if (initialData == null) return;
        boardData.values = (int[])initialData.values.Clone();
        boardData.solution = (int[])initialData.solution.Clone();
        boardData.fixedCells = (bool[])initialData.fixedCells.Clone();
        boardData.notesMask = (int[])initialData.notesMask.Clone();
        undoStack.Clear();
        redoStack.Clear();
        NotifyBoardChanged();
    }
    public void SetInitialState(SudokuBoardData data)
    {
        initialData = new SudokuBoardData();
        initialData.values = (int[])data.values.Clone();
        initialData.solution = (int[])data.solution.Clone();
        initialData.fixedCells = (bool[])data.fixedCells.Clone();
        initialData.notesMask = (int[])data.notesMask.Clone();
    }
    public SudokuBoardData GetInitialData()
    {
        return initialData;
    }
    void InitBitMask()
    {
        bitMask.Clear();

        for (int i = 0; i < 81; i++)
        {
            int val = boardData.values[i];
            if (val != 0)
            {
                int r = i / 9;
                int c = i % 9;
                bitMask.Place(r, c, val);
            }
        }
    }
    public void ApplyActions(List<SudokuAction> actions)
    {
        foreach (var action in actions)
        {
            if (action.type == SudokuActionType.Place)
            {
                PlaceNumber(action.index, action.value);
            }
            else if (action.type == SudokuActionType.RemoveNotes)
            {
                boardData.notesMask[action.index] &= ~action.mask;
            }
        }

        NotifyBoardChanged();
    }
}