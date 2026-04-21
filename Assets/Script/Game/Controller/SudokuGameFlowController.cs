using UnityEngine;
public class SudokuGameFlowController : MonoBehaviour
{
    SudokuGenerator generator = new SudokuGenerator();
    [SerializeField] SudokuBoardView boardView;
    [SerializeField] SudokuSaveManager saveManager;
    [SerializeField] SudokuBoardController boardController;
    [SerializeField] SudokuDifficultyUI difficultyUI;
    [SerializeField] SudokuMistakeSystem mistakeSystem;
    [SerializeField] SudokuTimer timer;
    void Start()
    {
        mistakeSystem.OnGameOver += OnGameOver;
        Initialize();
    }
    public void Initialize()
    {
        int slot = SudokuGameSession.SelectedSlot;
        if (slot < 0)
        {
            Debug.LogError("No hay slot seleccionado");
            return;
        }
        if (SudokuGameSession.LoadFromSave)
        {
            bool loaded = saveManager.LoadGame(slot);
            if (loaded)
            {
                LoadBoardToView();
                timer.StartTimer(); 
                return;
            }
        }
        GenerateGame();
        saveManager.SaveGame(slot);
        timer.ResetTime();
        timer.StartTimer();
    }
    void LoadBoardToView()
    {
        var data = boardController.GetBoardData();
        boardView.UpdateBoard(
            data.values,
            data.fixedCells,
            data.notesMask
        );
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
        boardController.SetInitialState(data);
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
            _ => 40
        };
    }
    int CountSteps(int[,] puzzle, SudokuTechniques solver)
    {
        int[,] board = (int[,])puzzle.Clone();
        int[] notes = new int[81];

        SudokuContext ctx = new SudokuContext
        {
            board = board,
            notesMask = notes
        };

        int steps = 0;

        while (solver.GetHint(ctx, out var hint))
        {
            foreach (var a in hint.actions)
            {
                if (a.type == SudokuActionType.Place)
                {
                    int r = a.index / 9;
                    int c = a.index % 9;
                    board[r, c] = a.value;
                }
            }

            steps++;
        }

        return steps;
    }
    void OnGameOver()
    {
        Debug.Log("Mostrar pantalla de derrota");
    }
}