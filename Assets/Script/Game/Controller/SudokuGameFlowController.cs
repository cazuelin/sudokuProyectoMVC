using UnityEngine;

public class SudokuGameFlowController : MonoBehaviour
{
    SudokuGenerator generator = new SudokuGenerator();

    [SerializeField] SudokuBoardView boardView;
    [SerializeField] SudokuSaveManager saveManager;
    [SerializeField] SudokuBoardController boardController;
    [SerializeField] SudokuDifficultyUI difficultyUI;
    [SerializeField] SudokuMistakeSystem mistakeSystem;

    void Start()
    {
        mistakeSystem.OnGameOver += OnGameOver;
    }
    public void Initialize(SudokuBoardView view)
    {
        boardView = view;

        bool loaded = false;

        if (saveManager.HasContinueGame())
        {
            loaded = saveManager.LoadGame(0);
        }

        if (loaded)
        {

            var data = boardController.GetBoardData();

            boardView.UpdateBoard(
                data.values,
                data.fixedCells,
                data.notesMask
            );
        }
        else
        {
            GenerateGame();
        }
    }

    void GenerateGame()
    {
        var difficulty = SudokuGameManager.Instance.difficulty;

        SudokuTechniques solver = new SudokuTechniques(difficulty);
        SudokuDifficultyEvaluator evaluator = new SudokuDifficultyEvaluator();

        int removeAmount = GetRemoveAmount();

        int[,] solution = generator.GenerateSolution();
        int[,] puzzle = generator.CreatePuzzle(solution, removeAmount);

        int steps = CountSteps(puzzle, solver);

        var diff = evaluator.Evaluate(puzzle, steps);

        Debug.Log($"Real: {diff} | Selected: {difficulty} | Steps: {steps}");

        difficultyUI.SetDifficulty(SudokuGameManager.Instance.difficulty);

        var data = generator.ConvertToBoardData(solution, puzzle);

        boardController.SetBoardData(data);
        boardView.UpdateBoard(data.values, data.fixedCells, data.notesMask);
    }

    int GetRemoveAmount()
    {
        return SudokuGameManager.Instance.difficulty switch
        {
            SudokuGameManager.Difficulty.Easy => Random.Range(28, 32),
            SudokuGameManager.Difficulty.Medium => Random.Range(36, 42),
            SudokuGameManager.Difficulty.Hard => Random.Range(42, 50),
            SudokuGameManager.Difficulty.Expert => Random.Range(50, 56),
            SudokuGameManager.Difficulty.Master => Random.Range(56, 60),
            SudokuGameManager.Difficulty.Extreme => Random.Range(60, 64),
            _ => 40
        };
    }
    int CountSteps(int[,] puzzle, SudokuTechniques solver)
    {
        int[,] board = (int[,])puzzle.Clone();

        int steps = 0;

        while (solver.GetHint(board, out int r, out int c, out int v, out _))
        {
            board[r, c] = v;
            steps++;
        }

        return steps;
    }
    void OnGameOver()
    {
        Debug.Log("Mostrar pantalla de derrota");

        // bloquear input
        // mostrar popup
        // botón retry / continuar con ad
    }
    public void LoadFromSave()
    {
        var data = boardController.GetBoardData();

        boardView.UpdateBoard(
            data.values,
            data.fixedCells,
            data.notesMask
        );
    }
    public void GenerateNewGame()
    {
        GenerateGame();
    }

}