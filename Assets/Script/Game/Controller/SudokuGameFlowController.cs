using UnityEngine;
using UnityEngine.SceneManagement;
public class SudokuGameFlowController : MonoBehaviour
{
    SudokuGeneratorPro generator = new SudokuGeneratorPro();
    [Header("Debug (temporal)")]
    [SerializeField] bool enableSudokuDebug;
    [SerializeField] bool logEachTechniqueStep;
    [SerializeField] SudokuBoardView boardView;
    [SerializeField] SudokuSaveManager saveManager;
    [SerializeField] SudokuBoardController boardController;
    [SerializeField] SudokuInputController inputController;
    [SerializeField] SudokuDifficultyUI difficultyUI;
    [SerializeField] SudokuMistakeSystem mistakeSystem;
    [SerializeField] SudokuLivesUI livesUI;
    [SerializeField] SudokuHintsUI hintsUI;
    [SerializeField] SudokuTimer timer;
    [SerializeField] SessionContext sessionContext;
    [Header("End Game UI")]
    [SerializeField] GameObject victoryPanel;
    [SerializeField] GameObject defeatPanel;
    [SerializeField] GameObject defeatActionsPanel;
    [SerializeField] SudokuDifficultySelectionPanel difficultySelectionPanel;
    [SerializeField] GameObject victoryPanelButtons;
    [SerializeField] float victoryDelay = 2f;
    [SerializeField] float defeatDelay = 2f;
    [SerializeField] float defeatButtonsDelay = 0.5f;

    void Start()
    {
        SudokuDebugMode.Enabled = enableSudokuDebug;
        SudokuDebugMode.LogEachTechniqueStep = logEachTechniqueStep;
        
        // Busca el panel de dificultad en la escena
        if (difficultySelectionPanel == null)
        {
            difficultySelectionPanel = FindFirstObjectByType<SudokuDifficultySelectionPanel>();
        }
        
        mistakeSystem.OnGameOver += OnGameOver;
        mistakeSystem.OnMistakeChanged += OnMistakeChanged;
        if (boardController != null)
            boardController.OnBoardChanged += OnBoardChanged;
        if (inputController != null)
            inputController.OnHintsChanged += OnHintsChanged;

        if (victoryPanel != null)
            victoryPanel.SetActive(false);
        if (defeatPanel != null)
            defeatPanel.SetActive(false);
        if (defeatActionsPanel != null)
            defeatActionsPanel.SetActive(false);
        if (difficultySelectionPanel != null)
        {
            difficultySelectionPanel.Close();
            difficultySelectionPanel.OnDifficultySelected += HandleDifficultySelected;
        }
        if (victoryPanelButtons != null)
            victoryPanelButtons.SetActive(false);
    }
    void OnDestroy()
    {
        if (mistakeSystem != null)
        {
            mistakeSystem.OnGameOver -= OnGameOver;
            mistakeSystem.OnMistakeChanged -= OnMistakeChanged;
        }
        if (boardController != null)
            boardController.OnBoardChanged -= OnBoardChanged;
        if (inputController != null)
            inputController.OnHintsChanged -= OnHintsChanged;
        if (difficultySelectionPanel != null)
            difficultySelectionPanel.OnDifficultySelected -= HandleDifficultySelected;
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
                sessionContext.GameState = SudokuGameState.Playing;
                inputController?.ResetHints();
                boardController?.NotifyBoardChanged();
                timer.StartTimer();
                return;
            }
        }
        GenerateGame();
        saveManager.SaveGame(slot);
        inputController?.ResetHints();
        boardController?.NotifyBoardChanged();
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
        ClearEndPanels();
        livesUI?.UpdateLives(mistakeSystem.GetMistakes());
        if (inputController != null)
            hintsUI?.UpdateHints(inputController.RemainingHints);
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
        mistakeSystem.Init(0);
        livesUI?.UpdateLives(0);
        inputController?.ResetHints();
        if (inputController != null)
            hintsUI?.UpdateHints(inputController.RemainingHints);
        sessionContext.GameState = SudokuGameState.Playing;
        ClearEndPanels();
    }
    void OnBoardChanged()
    {
        if (sessionContext == null || sessionContext.GameState != SudokuGameState.Playing)
            return;

        if (boardController != null && boardController.CheckWin())
        {
            HandleVictory();
        }
    }
    void OnMistakeChanged(int mistakes)
    {
        livesUI?.UpdateLives(mistakes);
    }

    void OnHintsChanged(int remainingHints)
    {
        hintsUI?.UpdateHints(remainingHints);
    }
    void HandleVictory()
    {
        if (sessionContext == null || sessionContext.GameState != SudokuGameState.Playing)
            return;

        sessionContext.GameState = SudokuGameState.Victory;
        timer.StopTimer();
        StartCoroutine(VictorySequence());
    }
    void HandleDefeat()
    {
        if (sessionContext == null || sessionContext.GameState != SudokuGameState.Playing)
            return;

        sessionContext.GameState = SudokuGameState.Defeat;
        timer.StopTimer();
        StartCoroutine(DefeatSequence());
    }
    void OnGameOver()
    {
        HandleDefeat();
    }

    void ClearEndPanels()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
        if (victoryPanelButtons != null)
            victoryPanelButtons.SetActive(false);
        if (defeatPanel != null)
            defeatPanel.SetActive(false);
        if (defeatActionsPanel != null)
            defeatActionsPanel.SetActive(false);
        if (difficultySelectionPanel != null)
            difficultySelectionPanel.Close();
        boardView.ClearAllErrors();
    }

    System.Collections.IEnumerator VictorySequence()
    {
        yield return new WaitForSeconds(victoryDelay);
        if (victoryPanel != null)
            victoryPanel.SetActive(true);
        if (victoryPanelButtons != null)
            victoryPanelButtons.SetActive(true);
        saveManager?.DeleteSlot(sessionContext.SelectedSlot);
        Debug.Log("Victory");
        yield return new WaitForSeconds(victoryDelay);
        SceneManager.LoadScene("MainMenu");
    }

    System.Collections.IEnumerator DefeatSequence()
    {
        yield return new WaitForSeconds(defeatDelay);
        if (defeatPanel != null)
            defeatPanel.SetActive(true);
        yield return new WaitForSeconds(defeatButtonsDelay);
        if (defeatActionsPanel != null)
            defeatActionsPanel.SetActive(true);
        Debug.Log("Game Over");
    }

    public void RestartLevel()
    {
        if (boardController == null || boardView == null || mistakeSystem == null)
            return;

        boardController.ResetBoard();
        var data = boardController.GetBoardData();
        boardView.UpdateBoard(data.values, data.fixedCells, data.notesMask);
        mistakeSystem.Init(0);
        livesUI?.UpdateLives(0);
        inputController?.ResetHints();
        if (inputController != null)
            hintsUI?.UpdateHints(inputController.RemainingHints);
        sessionContext.GameState = SudokuGameState.Playing;
        timer.ResetTime();
        timer.StartTimer();
        ClearEndPanels();
    }

    public void OpenNewGamePanel()
    {
        if (defeatPanel != null)
            defeatPanel.SetActive(false);
        if (defeatActionsPanel != null)
            defeatActionsPanel.SetActive(false);
        if (difficultySelectionPanel != null)
            difficultySelectionPanel.Open(sessionContext.SelectedSlot);
    }

    public void StartNewGameWithDifficulty(int difficultyIndex)
    {
        if (sessionContext == null)
            return;

        StartNewGameWithDifficulty(sessionContext.SelectedSlot, (SudokuGameManager.Difficulty)difficultyIndex);
    }

    public void StartNewGameWithDifficulty(int slotIndex, SudokuGameManager.Difficulty difficulty)
    {
        if (sessionContext == null)
            return;

        sessionContext.SelectedSlot = slotIndex;
        sessionContext.SelectedDifficulty = difficulty;
        GenerateGame();
        saveManager?.SaveGame(sessionContext.SelectedSlot);
        if (difficultySelectionPanel != null)
            difficultySelectionPanel.Close();
    }

    void HandleDifficultySelected(int slotIndex, SudokuGameManager.Difficulty difficulty)
    {
        StartNewGameWithDifficulty(slotIndex, difficulty);
    }

    void EnsureBoardCreated()
    {
        var cells = boardView.GetCells();
        if (cells[0, 0] == null)
            boardView.CreateBoard();
    }
}