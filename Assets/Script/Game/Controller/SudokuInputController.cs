using UnityEngine;
public class SudokuInputController : MonoBehaviour
{
    [SerializeField] SudokuHintSystem hintSystem;
    [SerializeField] SudokuBoardView boardView;
    [SerializeField] SudokuHighlightSystem highlightSystem;
    [SerializeField] SudokuBoardController boardController;
    [SerializeField] SudokuMistakeSystem mistakeSystem;
    [SerializeField] SessionContext sessionContext;
    SudokuCell selectedCell;
    bool noteMode;
    [SerializeField] int maxHints = 3;
    int remainingHints;

    public int RemainingHints => remainingHints;
    public event System.Action<int> OnHintsChanged;

    void Start()
    {
        ResetHints();
        UpdateNumberPanelButtons();
    }

    public void ResetHints()
    {
        remainingHints = maxHints;
        OnHintsChanged?.Invoke(remainingHints);
    }
    public void SelectCell(SudokuCell cell)
    {
        if (IsPaused()) return;
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
        if (IsPaused()) return;
        if (selectedCell == null) return;
        int index = selectedCell.row * 9 + selectedCell.column;
        var data = boardController.boardData;
        if (data.fixedCells[index]) return;

        if (noteMode)
        {
            boardController.ToggleNote(index, number);
            return;
        }

        if (number != 0 && !boardController.IsCorrect(selectedCell.row, selectedCell.column, number))
        {
            boardView.SetCellError(selectedCell.row, selectedCell.column, true, number);
            mistakeSystem?.RegisterMistake();
            return;
        }

        boardView.SetCellError(selectedCell.row, selectedCell.column, false);
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
    void OnEnable()
    {
        NumberPanel.OnNumberPressed += SetNumber;
        SudokuCell.OnCellClicked += SelectCell;
        if (boardController != null)
            boardController.OnBoardChanged += OnBoardChanged;
    }
    void OnDisable()
    {
        NumberPanel.OnNumberPressed -= SetNumber;
        SudokuCell.OnCellClicked -= SelectCell;
        if (boardController != null)
            boardController.OnBoardChanged -= OnBoardChanged;
    }

    void OnBoardChanged()
    {
        UpdateNumberPanelButtons();
    }

    public void UseHint()
    {
        if (IsPaused()) return;
        if (remainingHints <= 0) return;

        if (hintSystem.TryGetHint(out var hint))
        {
            remainingHints--;
            OnHintsChanged?.Invoke(remainingHints);
            highlightSystem.ShowHint(hint);

            Debug.Log($"Hint: {hint.technique}");

            // aplicar acción real (opcional)
            if (hint.actions.Count > 0)
            {
                boardController.ApplyActions(hint.actions);
            }
        }
    }
    bool IsPaused()
    {
        return sessionContext == null ||
               sessionContext.GameState != SudokuGameState.Playing;
    }

    void UpdateNumberPanelButtons()
    {
        if (NumberPanel.Instance == null || boardController == null)
            return;

        var data = boardController.boardData;
        if (data == null || data.solution == null)
            return;

        for (int number = 1; number <= 9; number++)
        {
            bool allCorrect = true;
            for (int i = 0; i < 81; i++)
            {
                if (data.solution[i] == number && data.values[i] != number)
                {
                    allCorrect = false;
                    break;
                }
            }

            if (allCorrect)
                boardController.SetNumberFixed(number);

            NumberPanel.Instance.SetNumberButtonInteractable(number, !allCorrect);
        }
    }

    public void ClearSelection()
    {
        selectedCell = null;
    }
}