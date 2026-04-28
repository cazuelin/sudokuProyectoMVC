using UnityEngine;
public class SudokuInputController : MonoBehaviour
{
    [SerializeField] SudokuHintSystem hintSystem;
    [SerializeField] SudokuBoardView boardView;
    [SerializeField] SudokuHighlightSystem highlightSystem;
    [SerializeField] SudokuBoardController boardController;
    [SerializeField] SudokuSaveManager saveManager;
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
        if (hintSystem.TryGetHint(out var hint))
        {
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
        return SudokuGameManager.Instance.gameState == SudokuGameState.Paused;
    }
    public void ClearSelection()
    {
        selectedCell = null;
    }
}