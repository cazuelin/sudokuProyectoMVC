using UnityEngine;
using UnityEngine.SceneManagement;
public class SudokuPauseUI : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] SudokuBoardController boardController;
    [SerializeField] SudokuTimer timer;
    [SerializeField] SudokuInputController inputController;
    [SerializeField] SudokuBoardView boardView;
    public void OpenPause()
    {
        SudokuGameManager.Instance.PauseGame();
        int slot = SudokuGameSession.SelectedSlot;
        if (slot >= 0)
        {
            FindFirstObjectByType<SudokuSaveManager>().SaveGame(slot);
        }
        panel.SetActive(true);
    }
    public void Resume()
    {
        panel.SetActive(false);
        SudokuGameManager.Instance.ResumeGame();
    }
    public void RestartGame()
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
        SudokuGameManager.Instance.gameState = SudokuGameState.Playing;
    }
    public void GoToMenu()
    {
        int slot = SudokuGameSession.SelectedSlot;
        if (slot >= 0)
        {
            FindFirstObjectByType<SudokuSaveManager>().SaveGame(slot);
        }
        SceneManager.LoadScene("MainMenu");
    }
}