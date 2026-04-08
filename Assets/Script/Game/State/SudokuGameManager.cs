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
        Expert,
        Master,
        Extreme
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

        flowController.Initialize(boardView);

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
        hintSystem.Init(difficulty);
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
