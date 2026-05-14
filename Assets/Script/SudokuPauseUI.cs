using UnityEngine;
public class SudokuPauseUI : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] GameObject pauseButton;
    [SerializeField] SudokuBoardController boardController;
    [SerializeField] SudokuTimer timer;
    [SerializeField] SudokuInputController inputController;
    [SerializeField] SudokuBoardView boardView;
    [SerializeField] SudokuSessionController sessionController;
    [SerializeField] SudokuGameManager gameManager;
    [SerializeField] SudokuGameFlowController gameFlowController;

    public void OpenPause()
    {
        gameManager.PauseGame();
        sessionController?.SaveCurrentSlot();
        panel.SetActive(true);
        if (pauseButton != null)
            pauseButton.SetActive(false);
    }
    public void Resume()
    {
        panel.SetActive(false);
        if (pauseButton != null)
            pauseButton.SetActive(true);
        gameManager.ResumeGame();
    }
    public void RestartGame()
    {
        if (gameFlowController != null)
        {
            gameFlowController.RestartLevel();
        }
        else
        {
            boardController.ResetBoard();
            var data = boardController.GetBoardData();
            boardView.UpdateBoard(
                data.values,
                data.fixedCells,
                data.notesMask
            );
            inputController.ClearSelection();
            timer.ResetTime();
            timer.StartTimer();
            gameManager.SetGameState(SudokuGameState.Playing);
        }

        panel.SetActive(false);
        if (pauseButton != null)
            pauseButton.SetActive(true);
    }

    public void NewGame()
    {
        panel.SetActive(false);
        if (pauseButton != null)
            pauseButton.SetActive(true);
        gameFlowController?.OpenNewGamePanel();
    }

    public void GoToMenu()
    {
        if (sessionController != null)
            sessionController.SaveAndGoToMenu();
    }
}