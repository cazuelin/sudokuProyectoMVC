using UnityEngine;
using UnityEngine.SceneManagement;

public class SudokuSessionController : MonoBehaviour
{
    [SerializeField] string gameSceneName = "Game";
    [SerializeField] string menuSceneName = "MainMenu";
    [SerializeField] SudokuSaveManager saveManager;
    [SerializeField] SessionContext sessionContext;

    public void StartNewGame(int slot, SudokuGameManager.Difficulty difficulty)
    {
        if (sessionContext == null)
            return;

        sessionContext.SelectedSlot = slot;
        sessionContext.LoadFromSave = false;
        sessionContext.SelectedDifficulty = difficulty;
        SceneManager.LoadScene(gameSceneName);
    }

    public void ContinueGame(int slot)
    {
        if (sessionContext == null)
            return;

        sessionContext.SelectedSlot = slot;
        sessionContext.LoadFromSave = true;
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSlot(int slot, SudokuGameManager.Difficulty defaultDifficulty)
    {
        if (sessionContext == null)
            return;

        sessionContext.SelectedSlot = slot;
        sessionContext.LoadFromSave = saveManager != null && saveManager.HasSlot(slot);
        if (!sessionContext.LoadFromSave)
            sessionContext.SelectedDifficulty = defaultDifficulty;

        SceneManager.LoadScene(gameSceneName);
    }

    public void SaveCurrentSlot()
    {
        if (saveManager == null)
            return;
        if (sessionContext == null)
            return;

        int slot = sessionContext.SelectedSlot;
        if (slot >= 0)
            saveManager.SaveGame(slot);
    }

    public void SaveAndGoToMenu()
    {
        SaveCurrentSlot();
        SceneManager.LoadScene(menuSceneName);
    }
}
