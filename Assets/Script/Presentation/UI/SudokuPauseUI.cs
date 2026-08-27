using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudokuPauseUI : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] GameObject pauseButton;
    [SerializeField] GameObject settingsPanel;//El panel de ajustes "PanelAjustes" creado DENTRO del prefab MenuPause.
    //SudokuGameUI lo toma a través de este script (SettingsPanel) para no duplicar referencias.
    [SerializeField] SudokuBoardController boardController;
    [SerializeField] SudokuTimer timer;
    [SerializeField] SudokuInputController inputController;
    [SerializeField] SudokuBoardView boardView;
    [SerializeField] SudokuSessionController sessionController;
    [SerializeField] SudokuGameManager gameManager;
    [SerializeField] SudokuGameFlowController gameFlowController;

    public GameObject SettingsPanel//Devuelve el PanelAjustes del menú de pausa (si existe).
    {
        get
        {
            if (settingsPanel != null && !SudokuSceneRef.IsSceneInstance(settingsPanel))
                settingsPanel = null;//Si apunta a un asset del prefab o a algo destruido, se limpia.
            return settingsPanel;
        }
    }

    void Awake()
    {
        ResolveReferences();
    }

    public void Setup(GameObject pausePanel, GameObject pauseButton)
    //Reasigna las referencias del panel y botón de pausa. Lo usa SudokuGameUI al construir la UI nueva.
    {
        panel = pausePanel;
        this.pauseButton = pauseButton;
    }

    void ResolveReferences()
    {
        boardController = SudokuSceneRef.Resolve(boardController);
        timer = SudokuSceneRef.Resolve(timer);
        inputController = SudokuSceneRef.Resolve(inputController);
        boardView = SudokuSceneRef.Resolve(boardView);
        sessionController = SudokuSceneRef.Resolve(sessionController);
        gameManager = SudokuSceneRef.Resolve(gameManager);
        gameFlowController = SudokuSceneRef.Resolve(gameFlowController);
        //Si panel/pauseButton/settingsPanel apuntan a un prefab o a un objeto destruido, se limpian:
        //SudokuGameUI se los reasigna con Setup() al construir la UI nueva.
        panel = SudokuSceneRef.IsSceneInstance(panel) ? panel : null;
        pauseButton = SudokuSceneRef.IsSceneInstance(pauseButton) ? pauseButton : null;
        settingsPanel = SudokuSceneRef.IsSceneInstance(settingsPanel) ? settingsPanel : null;
        //Resolución perezosa: si el panel/botón aún no existen (viven en un prefab que se
        //instancia en runtime), se buscan solos en la escena por nombre.
        if (panel == null)
        {
            var found = SudokuSceneRef.FindInScene<Transform>(t =>
            {
                string n = t.gameObject.name;
                return n == "MenuPause" || n == "PausePanel";
            });
            if (found != null)
                panel = found.gameObject;
        }
        if (pauseButton == null)
        {
            var found = SudokuSceneRef.FindInScene<Button>(b =>
            {
                var label = b.GetComponentInChildren<TMP_Text>(true);
                string n = (label != null && !string.IsNullOrEmpty(label.text)) ? label.text : b.name;
                n = n.Trim().ToLowerInvariant();
                return n.Contains("pausa") || n.Contains("pause");
            });
            if (found != null)
                pauseButton = found.gameObject;
        }
    }

    public void ClosePause()//Cierra el panel de pausa SIN cambiar el estado del juego.
    //Deja visible el botón de pausa ("Pause x") y oculta solo el panel superpuesto ("Panel").
    //La usa SudokuGameUI al inicializar, para que el botón de pausa quede visible desde el inicio.
    {
        if (panel != null)
            panel.SetActive(false);
        if (pauseButton != null)
            pauseButton.SetActive(true);
    }

    public void OpenSettings()//Abre el panel de ajustes (PanelAjustes) del menú de pausa.
    //Es VOID para que aparezca en el Inspector (onClick) y se pueda asignar al AjustesButton.
    {
        ResolveReferences();
        if (settingsPanel == null)
            return;
        //Se activa cubriendo la pantalla (SudokuGameUI se encarga de conectar sus botones).
        var rt = settingsPanel.transform as RectTransform;
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
        }
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()//Cierra el panel de ajustes (vuelve a la pausa).
    //VOID para asignarla desde el Inspector (por ejemplo en el botón "Cerrar" del PanelAjustes).
    {
        ResolveReferences();
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OpenPause()
    {
        ResolveReferences();
        gameManager?.PauseGame();
        sessionController?.SaveCurrentSlot();
        if (panel != null)
        {
            //Asegura que el panel cubra toda la pantalla (se posiciona encima) aunque el prefab
            //esté posicionado a un lado o sobresaliendo del mapa.
            var rt = panel.transform as RectTransform;
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;
            }
            panel.SetActive(true);
        }
        if (pauseButton != null)
            pauseButton.SetActive(false);
    }

    public void Resume()
    {
        if (panel != null)
            panel.SetActive(false);
        if (pauseButton != null)
            pauseButton.SetActive(true);
        gameManager?.ResumeGame();
    }

    public void RestartGame()
    {
        ResolveReferences();
        if (gameFlowController != null)
        {
            gameFlowController.RestartLevel();
        }
        else if (boardController != null && boardView != null && timer != null && inputController != null)
        {
            boardController.ResetBoard();
            var data = boardController.GetBoardData();
            boardView.UpdateBoard(data.values, data.fixedCells, data.notesMask, data.hintCells);
            inputController.ClearSelection();
            timer.ResetTime();
            timer.StartTimer();
            gameManager?.SetGameState(SudokuGameState.Playing);
        }

        if (panel != null)
            panel.SetActive(false);
        if (pauseButton != null)
            pauseButton.SetActive(true);
    }

    public void NewGame()
    {
        if (panel != null)
            panel.SetActive(false);
        if (pauseButton != null)
            pauseButton.SetActive(true);
        gameFlowController?.OpenNewGamePanel();
    }

    public void GoToMenu()
    {
        sessionController?.SaveAndGoToMenu();
    }
}
