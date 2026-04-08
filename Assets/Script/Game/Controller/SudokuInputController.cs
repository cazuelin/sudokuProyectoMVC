using System.Collections.Generic;
using UnityEngine;

public class SudokuInputController : MonoBehaviour
{
    [SerializeField] SudokuHintSystem hintSystem;
    [SerializeField] SudokuBoardView boardView;
    [SerializeField] SudokuHighlightSystem highlightSystem;
    [SerializeField] SudokuBoardController boardController;
    [SerializeField] SudokuSaveManager saveManager;
    //[SerializeField] SudokuHintUI hintUI;

    SudokuCell selectedCell;
    bool noteMode;

    [SerializeField] int maxHints = 3;

    int remainingHints;

    void Start()
    {
        remainingHints = maxHints;
    }
    public void SelectCell(SudokuCell cell)
    {
        selectedCell = cell;
        highlightSystem.SelectCell(cell);
    }

    public void ToggleNotesMode()
    {
        noteMode = !noteMode;
        highlightSystem.SetNotesMode(noteMode);
    }

    public void SetNumber(int number)
    {
        if (selectedCell == null) return;

        int index = selectedCell.row * 9 + selectedCell.column;
        var data = boardController.boardData;

        if (data.fixedCells[index]) return;

        if (noteMode)
        {
            boardController.ToggleNote(index, number);
        }
        else
        {
            var move = new SudokuMove
            {
                index = index,
                oldValue = data.values[index],
                newValue = number,
                oldNotes = data.notesMask[index],
                newNotes = 0
            };

            boardController.ApplyMove(move);
        }
    }

    void OnEnable()
    {
        NumberPanel.OnNumberPressed += SetNumber;
        SudokuCell.OnCellClicked += SelectCell;
    }

    void OnDisable()
    {
        NumberPanel.OnNumberPressed -= SetNumber;
        SudokuCell.OnCellClicked -= SelectCell;
    }
    public void UseHint()
    {
        var data = boardController.boardData;

        List<int> emptyCells = new List<int>();

        for (int i = 0; i < 81; i++)
        {
            if (data.values[i] == 0)
                emptyCells.Add(i);
        }

        if (emptyCells.Count == 0)
            return;

        int index = emptyCells[Random.Range(0, emptyCells.Count)];

        int r = index / 9;
        int c = index % 9;

        int correct = boardController.GetSolution(index);

        var move = new SudokuMove
        {
            index = index,
            oldValue = data.values[index],
            newValue = correct,
            oldNotes = data.notesMask[index],
            newNotes = 0
        };

        boardController.ApplyMove(move);

        remainingHints--;

        Debug.Log("Hints restantes: " + remainingHints);
    }
}