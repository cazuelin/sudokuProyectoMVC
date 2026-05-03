using UnityEngine;
using UnityEngine.UI;
public class SudokuHighlightSystem : MonoBehaviour
{
    SudokuCell[,] board;
    SudokuBoardController boardController;
    [SerializeField] Image notesButtonImage;
    [Header("Colors")]
    [SerializeField] Color baseColor = Color.white;
    [SerializeField] Color highlightColor = new Color(0.8f, 0.9f, 1f);
    [SerializeField] Color selectedColor = Color.yellow;
    [SerializeField] Color sameNumberColor = new Color(1f, 0.95f, 0.6f);
    [SerializeField] Color conflictColor = new Color(1f, 0.5f, 0.5f);
    [Header("Notes")]
    [SerializeField] Color notesOffColor = Color.white;
    [SerializeField] Color notesOnColor = new Color(1f, 0.9f, 0.3f);
    const int SIZE = 9;
    public void Init(SudokuCell[,] cells, SudokuBoardController controller)
    {
        board = cells;
        boardController = controller;
    }
    public void SelectCell(SudokuCell cell)
    {
        ClearHighlights();
        int r = cell.row;
        int c = cell.column;
        int index = r * 9 + c;
        int selectedValue = boardController.boardData.values[index];
        HighlightArea(r, c);
        if (selectedValue != 0)
        {
            HighlightSameNumbers(selectedValue);
            HighlightConflicts(r, c, selectedValue);
        }
        cell.SetHighlight(selectedColor);
    }
    void ClearHighlights()
    {
        ForEachCell((r, c) =>
        {
            board[r, c].SetHighlight(baseColor);
        });
    }
    void HighlightArea(int row, int col)
    {
        for (int i = 0; i < SIZE; i++)
        {
            board[row, i].SetHighlight(highlightColor);
            board[i, col].SetHighlight(highlightColor);
        }
        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
                board[startRow + r, startCol + c].SetHighlight(highlightColor);
    }
    void HighlightSameNumbers(int number)
    {
        var data = boardController.boardData;
        for (int i = 0; i < 81; i++)
        {
            if (data.values[i] == number)
            {
                int r = i / 9;
                int c = i % 9;
                board[r, c].SetHighlight(sameNumberColor);
            }
        }
    }
    void HighlightConflicts(int row, int col, int number)
    {
        var data = boardController.boardData;
        for (int c = 0; c < 9; c++)
        {
            if (c == col) continue;
            int index = row * 9 + c;
            if (data.values[index] == number)
                board[row, c].SetHighlight(conflictColor);
        }
        for (int r = 0; r < 9; r++)
        {
            if (r == row) continue;
            int index = r * 9 + col;
            if (data.values[index] == number)
                board[r, col].SetHighlight(conflictColor);
        }
        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
            {
                int rr = startRow + r;
                int cc = startCol + c;
                if (rr == row && cc == col) continue;
                int index = rr * 9 + cc;
                if (data.values[index] == number)
                    board[rr, cc].SetHighlight(conflictColor);
            }
    }
    void ForEachCell(System.Action<int, int> action)
    {
        for (int r = 0; r < SIZE; r++)
            for (int c = 0; c < SIZE; c++)
                action(r, c);
    }
    public void SetNotesMode(bool active)
    {
        if (notesButtonImage != null)
            notesButtonImage.color = active ? notesOnColor : notesOffColor;
    }
    public void HighlightHintCell(int row, int col)
    {
        ClearHighlights();
        board[row, col].SetHighlight(Color.green);
    }
    public void ShowHint(SudokuHint hint)
    {
        ClearHighlights();
        foreach (var index in hint.highlightCells)
        {
            HighlightCell(index, Color.yellow);
        }
        foreach (var index in hint.affectedCells)
        {
            HighlightCell(index, Color.red);
        }
        int number = GetNumberFromMask(hint.candidateMask);
        HighlightCandidate(number);
    }
    void HighlightCell(int index, Color color)
    {
        int r = index / 9;
        int c = index % 9;
        board[r, c].SetHighlight(color);
    }
    int GetNumberFromMask(int mask)
    {
        for (int i = 0; i < 9; i++)
            if ((mask & (1 << i)) != 0)
                return i + 1;

        return -1;
    }
    void HighlightCandidate(int number)
    {
    }
}