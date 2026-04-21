using UnityEngine;
public class SudokuGameManager : MonoBehaviour
{
    public static SudokuGameManager Instance;
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
        Expert
    }
    public Difficulty difficulty;
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        gameState = SudokuGameState.Generating;
        boardView.CreateBoard();
        var cells = boardView.GetCells();
        highlightSystem.Init(cells, boardController);
        if (!SudokuGameSession.LoadFromSave)
        {
            difficulty = SudokuGameSession.SelectedDifficulty;
        }
        flowController.Initialize();
        gameState = SudokuGameState.Playing;
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
        timer.StopTimer();
    }
    public void ResumeGame()
    {
        gameState = SudokuGameState.Playing;
        timer.StartTimer();
    }
}
