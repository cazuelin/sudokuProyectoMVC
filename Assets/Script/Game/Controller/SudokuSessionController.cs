using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SudokuSessionController : MonoBehaviour
{
    [SerializeField] string gameSceneName = "Game";
    [SerializeField] string menuSceneName = "MainMenu";
    [SerializeField] SudokuSaveManager saveManager;
    [SerializeField] SessionContext sessionContext;
    [SerializeField] Color loadingScreenColor = Color.black;

    GameObject loadingOverlay;

    public void StartNewGame(int slot, SudokuGameManager.Difficulty difficulty)
    {
        if (sessionContext == null)
            return;

        sessionContext.SelectedSlot = slot;
        sessionContext.LoadFromSave = false;
        sessionContext.SelectedDifficulty = difficulty;
        StartCoroutine(LoadGameSceneWithOverlay(gameSceneName));
    }

    public void ContinueGame(int slot)
    {
        if (sessionContext == null)
            return;

        sessionContext.SelectedSlot = slot;
        sessionContext.LoadFromSave = true;
        StartCoroutine(LoadGameSceneWithOverlay(gameSceneName));
    }

    public void OpenSlot(int slot, SudokuGameManager.Difficulty defaultDifficulty)
    {
        if (sessionContext == null)
            return;

        sessionContext.SelectedSlot = slot;
        sessionContext.LoadFromSave = saveManager != null && saveManager.HasSlot(slot);
        if (!sessionContext.LoadFromSave)
            sessionContext.SelectedDifficulty = defaultDifficulty;

        StartCoroutine(LoadGameSceneWithOverlay(gameSceneName));
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

    IEnumerator LoadGameSceneWithOverlay(string sceneName)
    {
        ShowLoadingOverlay();

        var asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
            yield return null;

        HideLoadingOverlay();
    }

    void ShowLoadingOverlay()
    {
        if (loadingOverlay != null)
            return;

        loadingOverlay = new GameObject("LoadingOverlay");
        var canvas = loadingOverlay.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        loadingOverlay.AddComponent<CanvasScaler>();
        loadingOverlay.AddComponent<GraphicRaycaster>();

        var imageObject = new GameObject("Background");
        imageObject.transform.SetParent(loadingOverlay.transform, false);

        var image = imageObject.AddComponent<Image>();
        image.color = loadingScreenColor;

        var rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        DontDestroyOnLoad(loadingOverlay);
    }

    void HideLoadingOverlay()
    {
        if (loadingOverlay == null)
            return;

        Destroy(loadingOverlay);
        loadingOverlay = null;
    }

    public void SaveAndGoToMenu()
    {
        SaveCurrentSlot();
        SceneManager.LoadScene(menuSceneName);
    }
}
