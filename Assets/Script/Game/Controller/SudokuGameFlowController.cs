using UnityEngine;
public class SudokuGameFlowController : MonoBehaviour
{
    SudokuGeneratorPro generator = new SudokuGeneratorPro();
    [Header("Debug (temporal)")]
    [SerializeField] bool enableSudokuDebug;
    [SerializeField] bool logEachTechniqueStep;
    [SerializeField] SudokuBoardView boardView;
    [SerializeField] SudokuSaveManager saveManager;
    [SerializeField] SudokuBoardController boardController;
    [SerializeField] SudokuDifficultyUI difficultyUI;
    [SerializeField] SudokuMistakeSystem mistakeSystem;
    [SerializeField] SudokuTimer timer;
    [SerializeField] SessionContext sessionContext;
    void Start()
    {
        SudokuDebugMode.Enabled = enableSudokuDebug;
        SudokuDebugMode.LogEachTechniqueStep = logEachTechniqueStep;
        mistakeSystem.OnGameOver += OnGameOver;
    }
    public void Initialize()
    {
        if (sessionContext == null)
        {
            Debug.LogError("SessionContext no asignado en SudokuGameFlowController");
            return;
        }

        int slot = sessionContext.SelectedSlot;
        if (slot < 0)
        {
            Debug.LogError("No hay slot seleccionado");
            return;
        }
        if (sessionContext.LoadFromSave)
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
        EnsureBoardCreated();
        var data = boardController.GetBoardData();
        boardView.UpdateBoard(
            data.values,
            data.fixedCells,
            data.notesMask
        );
    }
    void GenerateGame()
    {
        EnsureBoardCreated();
        var difficulty = sessionContext.SelectedDifficulty;
        var data = generator.Generate(difficulty);
        difficultyUI.SetDifficulty(difficulty);
        boardController.SetBoardData(data);
        boardController.SetInitialState(data);
        boardView.UpdateBoard(data.values, data.fixedCells, data.notesMask);
    }
    void OnGameOver()
    {
        Debug.Log("Mostrar pantalla de derrota");
    }

    void EnsureBoardCreated()
    {
        var cells = boardView.GetCells();
        if (cells[0, 0] == null)
            boardView.CreateBoard();
    }
}