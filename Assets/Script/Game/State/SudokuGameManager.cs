using UnityEngine;
public class SudokuGameManager : MonoBehaviour
{
    [SerializeField] SessionContext sessionContext;
    [SerializeField] SudokuBoardView boardView;
    [SerializeField] SudokuTimer timer;
    [Header("Controllers")]
    [SerializeField] SudokuInputController inputController;
    [SerializeField] SudokuBoardController boardController;
    [SerializeField] SudokuGameFlowController flowController;
    [SerializeField] SudokuHighlightSystem highlightSystem;
    [SerializeField] SudokuHintSystem hintSystem;
    public SudokuGameState gameState;
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard,
        Expert,
        Extreme
    }
    public Difficulty difficulty;
    void Start()
    {
        if (sessionContext == null)
        {
            Debug.LogError("SessionContext no asignado en SudokuGameManager");
            return;
        }

        gameState = SudokuGameState.Generating;
        sessionContext.GameState = gameState;
        boardView.CreateBoard();
        var cells = boardView.GetCells();
        highlightSystem.Init(cells, boardController);
        difficulty = sessionContext.SelectedDifficulty;
        hintSystem.Init(difficulty);

        flowController.Initialize();
        gameState = SudokuGameState.Playing;
        sessionContext.GameState = gameState;
        timer.StartTimer();
        boardController.OnBoardChanged += () =>
        {
            var data = boardController.boardData;
            boardView.UpdateBoard(
                data.values,
                data.fixedCells,
                data.notesMask
            );
        };
    }
    public void PauseGame()
    {
        gameState = SudokuGameState.Paused;
        sessionContext.GameState = gameState;
        timer.StopTimer();
    }
    public void ResumeGame()
    {
        gameState = SudokuGameState.Playing;
        sessionContext.GameState = gameState;
        timer.StartTimer();
    }

    public void SetGameState(SudokuGameState newState)
    {
        gameState = newState;
        sessionContext.GameState = newState;
    }
}
