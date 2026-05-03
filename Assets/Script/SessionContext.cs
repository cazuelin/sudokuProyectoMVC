using UnityEngine;

[CreateAssetMenu(fileName = "SessionContext", menuName = "Sudoku/Session Context")]
public class SessionContext : ScriptableObject
{
    [SerializeField] int selectedSlot = -1;
    [SerializeField] bool loadFromSave;
    [SerializeField] SudokuGameManager.Difficulty selectedDifficulty = SudokuGameManager.Difficulty.Medium;
    [SerializeField] SudokuGameState gameState = SudokuGameState.Generating;

    public int SelectedSlot
    {
        get => selectedSlot;
        set => selectedSlot = value;
    }

    public bool LoadFromSave
    {
        get => loadFromSave;
        set => loadFromSave = value;
    }

    public SudokuGameManager.Difficulty SelectedDifficulty
    {
        get => selectedDifficulty;
        set => selectedDifficulty = value;
    }

    public SudokuGameState GameState
    {
        get => gameState;
        set => gameState = value;
    }
}
