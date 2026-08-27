using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]//Este script se ejecuta ANTES que los demás (Awake/Start primero).
//Así, cuando SudokuGameManager.Start o SudokuGameFlowController.Start se ejecuten,
//la UI nueva ya está construida y las referencias reasignadas.
public class SudokuGameUI : MonoBehaviour//CONSTRUCTOR CENTRAL DE LA UI.
//Este script se pone UNA vez en el Canvas de cada escena de Sudoku y construye toda la interfaz
//(timer, vidas, botones de notas/undo/borrar/auto-notas/pista/pausa, panel de números, paneles de
//victoria/derrota/dificultad/pausa) en runtime.
//Beneficio: ya NO hay que configurar la UI manualmente en las 4 escenas. Todo se edita aquí.
//Si quieres un look personalizado, crea tus propios prefabs y asígnalos en los campos "Prefabs opcionales".
{
    [Header("Prefabs opcionales (si se dejan vacíos se construye UI por código)")]
    [SerializeField] GameObject numberButtonPrefab;//Prefab de UN botón de número (el resto se genera automáticamente).
    [SerializeField] GameObject numberPanelPrefab;//Prefab del CONTENEDOR del panel de números (imagen de fondo + altura deseada). Los botones se generan DENTRO de él.
    [SerializeField] GameObject victoryPanelPrefab;//Prefab del panel de victoria.
    [SerializeField] GameObject defeatPanelPrefab;//Prefab del panel de derrota.
    [SerializeField] GameObject difficultyPanelPrefab;//Prefab del panel de selección de dificultad.
    [SerializeField] GameObject topBarPrefab;//Prefab de la barra superior (DatosNivel: dificultad + timer + pistas).
    [SerializeField] GameObject actionRowPrefab;//Prefab de la fila de ajustes (PanelAjustes: notas, undo, borrar, auto-notas, pista, pausa).
    [SerializeField] GameObject pausePanelPrefab;//Prefab del menú de pausa (MenuPause).
    [SerializeField] GameObject sudokuGridPrefab;//Prefab de la cuadrícula del tablero (SudokuGrid). Se instancia desde el Canvas con la variante activa y se AUTO-GENERA: las cajas y celdas se crean en runtime según las filas/columnas del tipo de Sudoku (2x3, 3x3, 3x4, 4x4).

    [Header("Tablero (rect que ocupa la cuadrícula dentro del Canvas)")]
    [SerializeField] float boardBottomOffset = 155f;//Espacio que queda libre en la parte inferior (fila de ajustes + panel de números).
    [SerializeField] float boardTopOffset = 435f;//Espacio que queda libre en la parte superior (barra DatosNivel + vidas).

    [Header("Estilo (un solo lugar para cambiar la UI de todas las escenas)")]
    [SerializeField] Color overlayColor = new Color(0f, 0f, 0f, 0.82f);//Fondo de los paneles.
    [SerializeField] Color buttonColor = new Color(0.92f, 0.92f, 0.92f, 1f);//Color de los botones.
    [SerializeField] Color buttonTextColor = Color.black;//Color del texto de los botones.
    [SerializeField] Color titleColor = Color.white;//Color de los títulos.
    [SerializeField] Color accentColor = new Color(0.25f, 0.45f, 1f, 1f);//Color de resaltado de botones.
    [SerializeField] float fontSize = 28f;//Tamaño de texto de botones.
    [SerializeField] float titleFontSize = 48f;//Tamaño de texto de títulos.
    [SerializeField] float topBarHeight = 70f;//Alto de la barra superior (dificultad + timer).
    [SerializeField] float heartRowHeight = 46f;//Alto de la fila de vidas.
    [SerializeField] float numberButtonSize = 62f;//Tamaño de los botones del panel de números.
    [SerializeField] float numberPanelBottomOffset = 12f;//Distancia del panel de números al borde inferior.
    [SerializeField] float buttonWidth = 140f;//Ancho de los botones de acción.
    [SerializeField] float buttonHeight = 55f;//Alto de los botones de acción.
    [SerializeField] float actionRowOffset = 165f;//Altura de la fila de botones de acción sobre el borde inferior.
    [SerializeField] float spacing = 10f;//Separación entre elementos.

    [Header("Vidas (sprites opcionales)")]
    [SerializeField] Sprite heartFullSprite;//Sprite del corazón lleno. Si no se asigna (ni se hereda de la UI vieja), la fila de vidas no se crea.
    [SerializeField] Sprite heartEmptySprite;//Sprite del corazón vacío. Si no se asigna, el corazón vacío se atenúa.

    [Header("Panel de números")]
    [SerializeField] float numberPanelPadding = 10f;//Margen interno (padding) de los 4 lados del panel de números.
    [SerializeField] float numberSpacing = 10f;//Separación entre botones del panel de números.

    //Referencias resueltas automáticamente (FindFirstObjectByType).
    SudokuGameFlowController flowController;
    SudokuBoardController boardController;
    SudokuInputController inputController;
    SudokuTimer timer;
    SudokuLivesUI livesUI;
    SudokuHintsUI hintsUI;
    SudokuDifficultyUI difficultyUI;
    SudokuPauseUI pauseUI;
    SudokuHighlightSystem highlightSystem;
    SudokuMistakeSystem mistakeSystem;
    SudokuSessionController sessionController;
    SessionContext sessionContext;
    NumberPanel numberPanel;

    //Elementos construidos.
    TMP_Text difficultyText;
    TMP_Text timerText;
    TMP_Text hintText;
    Image[] hearts;
    Image notesButtonImage;
    Button hintButton;
    Button pauseButton;
    GameObject pausePanel;
    GameObject victoryPanel;
    GameObject defeatPanel;
    GameObject difficultyPanel;
    SudokuDifficultySelectionPanel difficultyPanelComp;
    //Raíces de los elementos del HUD (para cambiarles el color desde los ajustes).
    GameObject topBarRoot;
    GameObject actionRowRoot;
    GameObject gridRoot;
    //Panel de ajustes (PanelAjustes) que se abre desde el botón AjustesButton del menú de pausa.
    //Es UN GameObject para poder hacer referencia al elemento creado en el propio prefab MenuPause.
    //Si se deja vacío, SudokuGameUI lo busca dentro del menú de pausa por nombre, y si no existe,
    //lo construye por código.
    [SerializeField] GameObject settingsPanelComp;

    //Ajustes del jugador (volumen, vidas y color del HUD) guardados en PlayerPrefs.
    const string VolumePrefKey = "Sudoku_Volume";//Clave del volumen (0..1).
    const string LivesPrefKey = "Sudoku_LivesEnabled";//Clave de vidas activadas/desactivadas.
    const string ThemePrefKey = "Sudoku_ThemeIndex";//Clave del color del HUD.
    static readonly Color[] hudThemeColors = new Color[]
    {
        new Color(0.25f, 0.45f, 1f, 1f),   // Azul (por defecto)
        new Color(0.15f, 0.6f, 0.35f, 1f), // Verde
        new Color(0.85f, 0.3f, 0.3f, 1f),  // Rojo
        new Color(0.6f, 0.4f, 0.9f, 1f),   // Morado
        new Color(0.95f, 0.6f, 0.2f, 1f),  // Naranja
    };
    //Controles del panel de ajustes construidos por código (solo en el modo fallback).
    TMP_Text settingsVolumeLabel;
    TMP_Text settingsLivesLabel;
    Image[] settingsSwatches;
    //Etiquetas de los botones del PanelAjustes del usuario (se actualizan ON/OFF).
    TMP_Text settingsVolumeToggleLabel;
    GameObject colorPickerPanel;//Selector de colores del HUD (gama de colores).
    bool settingsWired;//Evita reconectar los botones del elemento del usuario más de una vez.

    //Visuales heredados de la UI vieja (para no perder el estilo al reemplazarla).
    TMP_FontAsset cachedFont;
    Sprite cachedButtonSprite;
    Sprite cachedHeartSprite;
    Sprite resolvedHeartSprite;//Sprite de corazón detectado (del prefab DatosNivel, del Inspector o heredado).

    void Awake()
    {
        CacheOldVisuals();//Guarda fuente y sprites de la UI anterior antes de borrarla.
        ClearOldUI();//Borra la UI vieja de la escena (botones, textos, paneles repetidos).
        ResolveControllers();//Busca todos los controladores del juego.
        ConfigureVariant();//Activa la variante (filas y columnas) según el tipo de Sudoku elegido: TODO el tablero se genera a partir de ella.
        //Cada fase se ejecuta de forma independiente: si una falla (por ejemplo un prefab roto),
        //las demás se ejecutan igual y el juego no se rompe.
        SafeRun(BuildUI);
        SafeRun(WireActions);
        SafeRun(SetupComponents);
    }

    void ConfigureVariant()
    //Activa en SudokuRules la variante guardada en SessionContext (2x3, 3x3, 3x4 o 4x4).
    //Debe ejecutarse ANTES de instanciar el SudokuGrid: las cajas y celdas se autogeneran
    //según BoxRows/BoxCols (filas y columnas del tipo de Sudoku actual).
    {
        var variant = sessionContext != null
            ? sessionContext.SelectedVariant
            : SudokuRules.SudokuVariant.Standard3x3;
        SudokuRules.SetVariant(variant);
    }
    void SafeRun(System.Action action)
    //Ejecuta una fase atrapando excepciones para que el resto del juego siga funcionando.
    {
        try
        {
            action();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SudokuGameUI] {action.Method.Name} falló: {e}");
        }
    }

    void Start()
    {
        //Si el editor dejó varias escenas cargadas (por ejemplo el menú abierto junto al juego),
        //se descargan para que no quede la imagen del menú de fondo detrás de la escena del Sudoku.
        //Se hace en Start porque en Awake puede que las demás escenas aún no hayan terminado de cargar.
        UnloadExtraScenes();
        //Seguridad: si algún script viejo pisó la instancia global, la restauramos.
        if (NumberPanel.Instance == null && numberPanel != null)
            NumberPanel.SetInstance(numberPanel);
        //Defensa: asegura que todos los paneles inicien cerrados (por si un prefab viene activo).
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
        if (defeatPanel != null)
            defeatPanel.SetActive(false);
        if (difficultyPanel != null)
            difficultyPanel.SetActive(false);
        //Defensa: asegura que el panel de pausa inicie CERRADO pero su botón "Pause x" visible
        //(el prefab MenuPause separa el botón del panel superpuesto).
        var pauseComp = pausePanel != null ? pausePanel.GetComponent<SudokuPauseUI>() : null;
        if (pauseComp != null)
            pauseComp.ClosePause();
        else if (pausePanel != null)
            pausePanel.SetActive(false);
        //Aplica los ajustes guardados del jugador: volumen y color del HUD.
        AudioListener.volume = SettingsVolume;
        ApplyHudTheme(SettingsThemeIndex);
    }

    void UnloadExtraScenes()
    //Descarga las escenas cargadas que no son la actual (por ejemplo, el MainMenu abierto en el
    //editor junto a la escena del Sudoku). Unity NO permite descargar la escena ACTIVA: si la
    //escena extra es la activa, primero se activa la nuestra y luego se descarga.
    {
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded || scene == gameObject.scene)
                continue;
            if (SceneManager.GetActiveScene() == scene)
                SceneManager.SetActiveScene(gameObject.scene);
            var operation = SceneManager.UnloadSceneAsync(scene);
            if (operation == null)
                Debug.LogWarning($"[SudokuGameUI] No se pudo descargar la escena extra: {scene.name}");
        }
    }

    void CacheOldVisuals()//Guarda fuente de texto y sprites de la UI actual antes de destruirla.
    {
        foreach (var text in GetComponentsInChildren<TMP_Text>(true))
        {
            if (text.font != null)
            {
                cachedFont = text.font;
                break;
            }
        }
        foreach (var img in GetComponentsInChildren<Image>(true))
        {
            if (img.sprite == null)
                continue;
            if (img.gameObject.name.StartsWith("Live") && cachedHeartSprite == null)
                cachedHeartSprite = img.sprite;
            else if (img.gameObject.name.StartsWith("Button") && cachedButtonSprite == null)
                cachedButtonSprite = img.sprite;
        }
    }

    void ClearOldUI()//Destruye la UI vieja de la escena (la nueva se genera por código).
    {
        //Borra cualquier NumberPanel viejo de la escena (puede estar fuera del Canvas).
        var oldPanels = FindObjectsByType<NumberPanel>(FindObjectsSortMode.None);
        for (int i = 0; i < oldPanels.Length; i++)
        {
            if (oldPanels[i] != null)
                DestroyImmediate(oldPanels[i].gameObject);
        }
        //Borra los componentes de UI sueltos en la escena (DifficultySystem, etc.) para que NO
        //haya dos instancias (escena + prefab) y las referencias viejas no rompan el juego.
        //SudokuGameUI los vuelve a crear (EnsureUIComponent) o los toma de los prefabs.
        DestroyAllSceneComponents<SudokuDifficultyUI>();
        DestroyAllSceneComponents<SudokuLivesUI>();
        DestroyAllSceneComponents<SudokuHintsUI>();
        DestroyAllSceneComponents<SudokuPauseUI>();
        //Borra los hijos del Canvas EXCEPTO la lógica de pausa (Sus visuales se reconstruyen).
        //IMPORTANTE: el tablero (SudokuGrid) YA NO se conserva de la escena: ahora es un prefab que
        //SudokuGameUI instancia y que se autogenera según la variante activa (BuildGrid).
        //Cualquier cuadrícula vieja de la escena se destruye aquí y se reemplaza por la nueva.
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            bool isPauseLogic = child.GetComponentInChildren<SudokuPauseUI>(true) != null;
            if (isPauseLogic)
            {
                //La lógica de pausa se conserva, pero se borran sus visuales viejos (Setup reasigna).
                for (int j = child.childCount - 1; j >= 0; j--)
                    DestroyImmediate(child.GetChild(j).gameObject);
                continue;
            }
            DestroyImmediate(child.gameObject);
        }
    }

    void DestroyAllSceneComponents<T>() where T : Component
    //Destruye todas las instancias de un componente de UI que existan en la escena
    //(no las de los prefabs, que se crean después). Así no hay duplicados escena+prefab.
    {
        foreach (var candidate in FindObjectsByType<T>(FindObjectsSortMode.None))
        {
            if (candidate != null)
                DestroyImmediate(candidate.gameObject);
        }
    }

    void ResolveControllers()//Busca automáticamente los controladores del juego en la escena.
    {
        flowController = FindFirstObjectByType<SudokuGameFlowController>();
        boardController = FindFirstObjectByType<SudokuBoardController>();
        inputController = FindFirstObjectByType<SudokuInputController>();
        timer = FindFirstObjectByType<SudokuTimer>();
        livesUI = FindFirstObjectByType<SudokuLivesUI>();
        hintsUI = FindFirstObjectByType<SudokuHintsUI>();
        difficultyUI = FindFirstObjectByType<SudokuDifficultyUI>();
        pauseUI = FindFirstObjectByType<SudokuPauseUI>();
        highlightSystem = FindFirstObjectByType<SudokuHighlightSystem>();
        mistakeSystem = FindFirstObjectByType<SudokuMistakeSystem>();
        sessionController = FindFirstObjectByType<SudokuSessionController>();
        var contexts = Resources.FindObjectsOfTypeAll<SessionContext>();
        if (contexts != null && contexts.Length > 0)
            sessionContext = contexts[0];
    }

    void BuildUI()//Construye toda la interfaz: tablero (SudokuGrid), barra superior (DatosNivel), botones, panel de números y paneles.
    {
        BuildGrid();//Instancia el prefab SudokuGrid con la variante activa (cajas + celdas autogeneradas).
        BuildTopBar();//Prefab DatosNivel: vidas + tiempo + dificultad (o versión por código).
        BuildActionRow();//Notas, Undo, Borrar, AutoNotas, Pista, Pausa.
        BuildNumberPanel();//Botones de números (generados desde prefab o código).
        BuildPausePanel();//Menú de pausa (con su botón AjustesButton).
        BuildSettingsPanel();//Panel de ajustes (PanelAjustes): volumen, vidas y color del HUD.
        BuildVictoryPanel();//Panel de victoria.
        BuildDefeatPanel();//Panel de derrota.
        BuildDifficultyPanel();//Panel de selección de dificultad.
    }

    void BuildGrid()//Instancia el prefab SudokuGrid como hijo del Canvas y le da el rect del tablero.
    //El prefab NO trae cajas ni celdas: al instanciarse se autogeneran según SudokuRules
    //(cantidad de filas y columnas del tipo de Sudoku elegido en SessionContext).
    {
        if (sudokuGridPrefab == null)
        {
            Debug.LogError("[SudokuGameUI] Falta el prefab SudokuGrid asignado en el Canvas. No se podrá jugar sin el tablero.");
            return;
        }
        var go = Instantiate(sudokuGridPrefab, transform);//Se activa desde el Canvas.
        go.name = "SudokuGrid";
        gridRoot = go;//Guarda la raíz para poder cambiarle el color desde los ajustes.
        var rt = go.transform as RectTransform;
        if (rt != null)
        {
            //La cuadrícula ocupa el área entre la barra superior (DatosNivel + vidas) y
            //la fila de ajustes + panel de números.
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.anchoredPosition = new Vector2(0f, boardBottomOffset);
            rt.sizeDelta = new Vector2(0f, -(boardBottomOffset + boardTopOffset));
            rt.localScale = Vector3.one;
        }
    }

    void BuildTopBar()//Crea la zona superior: vidas, tiempo y dificultad.
    {
        //El prefab DatosNivel es el encargado de la barra superior: dificultad, timer y vidas.
        //El contador de pistas NO vive en DatosNivel: está en el prefab PanelAjustes (HintPanel,
        //con su propio script SudokuHintsUI y su texto "Hint TMP").
        //Se instancia TAL CUAL (no se crea una TopBar ni una HeartsRow por código).
        if (topBarPrefab != null)
        {
            var go = Instantiate(topBarPrefab, transform);
            topBarRoot = go;//Guarda la raíz para poder cambiarle el color desde los ajustes.
            WireTopBarFromPrefab(go.transform);//Conecta dificultad y timer por nombre.
            WireHeartsFromPrefab(go.transform);//Conecta las vidas (imágenes "Live*") del prefab.
            //Solo se exigen los textos que SIEMPRE trae el prefab (dificultad y timer). Si faltan,
            //se borra y se construye la versión por código para que el juego nunca se quede sin UI.
            if (difficultyText == null || timerText == null)
            {
                Destroy(go);
                BuildTopBarByCode();
                BuildHeartsByCode();
            }
            return;
        }
        BuildTopBarByCode();
        BuildHeartsByCode();
    }
    void WireHeartsFromPrefab(Transform root)//Busca las imágenes de vidas ("Live*", heart, corazon) dentro del prefab DatosNivel.
    {
        var images = root.GetComponentsInChildren<Image>(true);
        var found = new System.Collections.Generic.List<Image>();
        Sprite full = heartFullSprite;
        for (int i = 0; i < images.Length; i++)
        {
            Image img = images[i];
            string n = img.gameObject.name.ToLowerInvariant();
            if (n.StartsWith("live") || n.Contains("heart") || n.Contains("corazon"))
            {
                found.Add(img);
                if (full == null && img.sprite != null)
                    full = img.sprite;//Usa el sprite del propio prefab si no se asignó otro.
            }
        }
        if (found.Count == 0)
            return;
        hearts = found.ToArray();
        resolvedHeartSprite = full;
    }
    void BuildTopBarByCode()//Construye la barra superior por código (fallback).
    {
        RectTransform topBar = CreateUI("TopBar", transform);
        topBarRoot = topBar.gameObject;//Guarda la raíz para poder cambiarle el color desde los ajustes.
        topBar.anchorMin = new Vector2(0f, 1f);
        topBar.anchorMax = new Vector2(1f, 1f);
        topBar.pivot = new Vector2(0.5f, 1f);
        topBar.anchoredPosition = new Vector2(0f, -10f);
        topBar.sizeDelta = new Vector2(0f, topBarHeight);

        difficultyText = CreateText("DifficultyText", "Dificultad", topBar, fontSize, titleColor, TextAlignmentOptions.Left, "left");
        hintText = CreateText("HintText", "Pistas: 0", topBar, fontSize, titleColor, TextAlignmentOptions.Center, "center");
        timerText = CreateText("TimerText", "00:00", topBar, fontSize, titleColor, TextAlignmentOptions.Right, "right");
    }
    void WireTopBarFromPrefab(Transform root)//Busca y conecta los textos del prefab de la barra superior por su nombre.
    {
        foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
        {
            string n = text.gameObject.name.ToLowerInvariant();
            if (difficultyText == null && (n.Contains("difficulty") || n.Contains("dificultad")))
                difficultyText = text;
            else if (timerText == null && n.Contains("timer"))
                timerText = text;
            else if (hintText == null && (n.Contains("hint") || n.Contains("pistas")))
                hintText = text;
        }
    }

    void BuildHeartsByCode()//Crea la fila de vidas por código (solo si NO hay prefab DatosNivel).
    {
        //Si no hay sprite de corazón (ni asignado en el Inspector ni heredado de la UI vieja),
        //no se crea la fila de vidas: evita que salgan imágenes vacías.
        Sprite full = heartFullSprite != null ? heartFullSprite : cachedHeartSprite;
        if (full == null)
            return;
        resolvedHeartSprite = full;
        int lives = sessionContext != null ? sessionContext.MaxMistakes : 3;
        RectTransform row = CreateUI("HeartsRow", transform);
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.anchoredPosition = new Vector2(0f, -topBarHeight - 20f);
        row.sizeDelta = new Vector2(0f, heartRowHeight);
        var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        hearts = new Image[lives];
        for (int i = 0; i < lives; i++)
        {
            var img = CreateImage($"Live{i + 1}", row, Color.white, full);
            img.rectTransform.sizeDelta = new Vector2(heartRowHeight, heartRowHeight);
            hearts[i] = img;
        }
    }

    void BuildActionRow()//Crea la fila de botones de acción: Notas, Undo, Borrar, AutoNotas, Pista, Pausa.
    {
        //Si hay prefab asignado (PanelAjustes), se instancia y se conectan sus botones por etiqueta/nombre.
        if (actionRowPrefab != null)
        {
            var go = Instantiate(actionRowPrefab, transform);
            actionRowRoot = go;//Guarda la raíz para poder cambiarle el color desde los ajustes.
            WireActionRowFromPrefab(go.transform);
            return;
        }
        //Ancho explícito: ocupa todo el ancho del Canvas con margen de 20px a cada lado.
        float rowWidth = ((RectTransform)transform).rect.width - 40f;
        RectTransform row = CreateUI("ActionRow", transform);
        actionRowRoot = row.gameObject;//Guarda la raíz para poder cambiarle el color desde los ajustes.
        row.anchorMin = new Vector2(0.5f, 0f);
        row.anchorMax = new Vector2(0.5f, 0f);
        row.pivot = new Vector2(0.5f, 0f);
        row.anchoredPosition = new Vector2(0f, actionRowOffset);
        row.sizeDelta = new Vector2(rowWidth, buttonHeight + 16f);
        var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        //Los botones se expanden y se adaptan al ancho máximo del contenedor (margen izq/der).
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        CreateButton("Notas", row, () => inputController?.ToggleNotesMode());
        CreateButton("Undo", row, () => boardController?.Undo());
        CreateButton("Borrar", row, () => numberPanel?.ClearNumber());
        CreateButton("AutoNotas", row, () => boardController?.AutoFillNotes());
        hintButton = CreateButton("Pista", row, () => inputController?.UseHint());
        pauseButton = CreateButton("Pausa", row, () => pauseUI?.OpenPause());
    }
    void WireActionRowFromPrefab(Transform root)//Conecta los botones del prefab de ajustes según su etiqueta o nombre.
    {
        foreach (var btn in root.GetComponentsInChildren<Button>(true))
        {
            var label = btn.GetComponentInChildren<TMP_Text>(true);
            string match = (label != null && !string.IsNullOrEmpty(label.text)) ? label.text : btn.name;
            string n = match.Trim().ToLowerInvariant();
            if (n.Contains("auto"))//AutoNotas (revisar antes que "notas" porque "auto notas" también la contiene).
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => boardController?.AutoFillNotes());
            }
            else if (n.Contains("notas"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => inputController?.ToggleNotesMode());
                notesButtonImage = btn.GetComponent<Image>();//Imagen del botón de notas para el resaltado.
            }
            else if (n.Contains("undo"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => boardController?.Undo());
            }
            else if (n.Contains("borrar") || n.Contains("clear"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => numberPanel?.ClearNumber());
            }
            else if (n.Contains("pista") || n.Contains("hint"))
            {
                hintButton = btn;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => inputController?.UseHint());
            }
            else if (n.Contains("pausa") || n.Contains("pause"))
            {
                pauseButton = btn;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => pauseUI?.OpenPause());
            }
        }
    }

    void BuildNumberPanel()//Crea el panel de números. Genera un botón por valor del Sudoku actual.
    {
        RectTransform panel;
        if (numberPanelPrefab != null)
        {
            //Se instancia el prefab del CONTENEDOR: aporta la imagen de fondo y la altura deseada.
            //(El prefab se coloca y dimensiona en el Canvas al crearlo; los botones se generan dentro).
            panel = (RectTransform)Instantiate(numberPanelPrefab, transform).transform;
            panel.localScale = Vector3.one;
        }
        else
        {
            //Versión por código: ocupa todo el ancho del Canvas (con margen) y la altura que queda
            //debajo de la fila de ajustes (PanelAjustes), como el panel de números viejo.
            float canvasWidth = ((RectTransform)transform).rect.width;
            float panelWidth = canvasWidth - 40f;
            float actionRowBottom = actionRowOffset - (buttonHeight + 16f) * 0.5f - 10f;
            float panelHeight = Mathf.Max(numberButtonSize + 20f, actionRowBottom - numberPanelBottomOffset);
            panel = CreateUI("NumberPanel", transform);
            panel.anchorMin = new Vector2(0.5f, 0f);
            panel.anchorMax = new Vector2(0.5f, 0f);
            panel.pivot = new Vector2(0.5f, 0f);
            panel.anchoredPosition = new Vector2(0f, numberPanelBottomOffset);
            panel.sizeDelta = new Vector2(panelWidth, panelHeight);
        }

        //Asegura el layout horizontal (si el prefab no trae uno, se agrega) y lo configura.
        var layout = panel.GetComponent<HorizontalLayoutGroup>();
        if (layout == null)
            layout = panel.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = numberSpacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        //Los botones se adaptan al tamaño máximo del contenedor (todos iguales).
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        //Margen interno (padding) de los 4 lados para que los números no se salgan de los bordes.
        layout.padding.left = (int)numberPanelPadding;
        layout.padding.right = (int)numberPanelPadding;
        layout.padding.top = (int)numberPanelPadding;
        layout.padding.bottom = (int)numberPanelPadding;

        //Si no hay prefab de botón asignado, se crea un botón por defecto en código.
        if (numberButtonPrefab == null)
            numberButtonPrefab = BuildDefaultNumberButton();

        //Si el prefab del contenedor ya trae el componente NumberPanel, se reutiliza
        //(AddComponent duplicado lanzaría un error y rompería la construcción de la UI).
        numberPanel = panel.GetComponent<NumberPanel>();
        if (numberPanel == null)
            numberPanel = panel.gameObject.AddComponent<NumberPanel>();
        numberPanel.ButtonsContainer = panel;
        numberPanel.NumberButtonPrefab = numberButtonPrefab;
        NumberPanel.SetInstance(numberPanel);
        numberPanel.InitButtons();//Genera los botones según la variante actual.
    }

    void BuildPausePanel()//Crea el panel de pausa (inactivo al inicio) o instancia su prefab.
    {
        if (pausePanelPrefab != null)
        {
            pausePanel = Instantiate(pausePanelPrefab, transform);
            SetFullScreenRect(pausePanel.transform as RectTransform);//Se posiciona encima (cubre la pantalla).
            WirePausePanelFromPrefab(pausePanel.transform);//Conecta sus botones por etiqueta.
            //El prefab MenuPause trae su botón de pausa "Pause x" VISIBLE y el panel superpuesto
            //("Panel") OCULTO. NO se apaga la raíz entera: eso escondería el botón que abre la pausa.
            var pauseComp = pausePanel.GetComponent<SudokuPauseUI>();
            if (pauseComp != null)
                pauseComp.ClosePause();//Deja el botón visible y el panel cerrado.
            else
                pausePanel.SetActive(false);//Fallback: si el prefab no trae SudokuPauseUI, se oculta.
            //AddPauseSettingsButton(pausePanel.transform);//Botón "AjustesButton" que abre el PanelAjustes.
            return;
        }
        pausePanel = CreateOverlay("PausePanel", out var title);
        title.text = "PAUSA";
        CreateButton("Reanudar", pausePanel.transform, () => pauseUI?.Resume());
        CreateButton("Reiniciar", pausePanel.transform, () => pauseUI?.RestartGame());
        CreateButton("Nueva partida", pausePanel.transform, () => pauseUI?.NewGame());
        //Botón "AjustesButton" que abre el PanelAjustes (se renombra para coincidir con el del prefab).
        var settingsBtn = CreateButton("Ajustes", pausePanel.transform, () => OpenSettingsPanel());
        if (settingsBtn != null)
            settingsBtn.gameObject.name = "AjustesButton";
        CreateButton("Menú", pausePanel.transform, () => pauseUI?.GoToMenu());
    }    void WirePausePanelFromPrefab(Transform root)//Conecta los botones del prefab de pausa según su etiqueta o nombre.
    {
        foreach (var btn in root.GetComponentsInChildren<Button>(true))
        {
            var label = btn.GetComponentInChildren<TMP_Text>(true);
            string match = (label != null && !string.IsNullOrEmpty(label.text)) ? label.text : btn.name;
            string n = match.Trim().ToLowerInvariant();
            if (n.Contains("reanudar") || n.Contains("resume") || n.Contains("continuar"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => pauseUI?.Resume());
            }
            else if (n.Contains("reiniciar") || n.Contains("restart"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => pauseUI?.RestartGame());
            }
            else if (n.Contains("nueva") || n.Contains("nuevo") || n.Contains("new game"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => pauseUI?.NewGame());
            }
            else if (n.Contains("menú") || n.Contains("menu"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => pauseUI?.GoToMenu());
            }
        }
    }

    void BuildSettingsPanel()//Crea el panel de ajustes "PanelAjustes" (se abre desde el botón AjustesButton del menú de pausa).
    //Contiene: subir/bajar volumen, activar/desactivar vidas y elegir el color del HUD.
    {
        //La referencia se toma del script SudokuPauseUI del prefab MenuPause (campo settingsPanel):
        //ahí se asigna el elemento "PanelAjustes" que se creó dentro del propio menú de pausa.
        var pauseComp = pausePanel != null ? pausePanel.GetComponent<SudokuPauseUI>() : null;
        if (settingsPanelComp == null && pauseComp != null && pauseComp.SettingsPanel != null)
            settingsPanelComp = pauseComp.SettingsPanel;

        //Si no vino por SudokuPauseUI, se busca por nombre dentro del menú de pausa.
        if (settingsPanelComp == null && pausePanel != null)
        {
            var panelChild = pausePanel.transform.Find("Panel");
            var found = pausePanel.transform.Find("PanelAjustes");
            if (found == null && panelChild != null)
                found = panelChild.Find("PanelAjustes");
            if (found != null)
                settingsPanelComp = found.gameObject;
        }
        if (settingsPanelComp != null)
        {
            //Elemento existente (creado por el usuario en MenuPause): se conectan sus botones
            //por etiqueta (volumen, vidas, cerrar) para que funcionen sin componentes extra.
            WireSettingsElement(settingsPanelComp);
            return;
        }

        //Fallback: si no existe ningún panel de ajustes, se construye por código.
        settingsPanelComp = CreateOverlay("PanelAjustes", out var title);
        title.text = "AJUSTES";

        //Fila de VOLUMEN: etiqueta + botón bajar + valor + botón subir.
        var volumeRow = CreateSettingsRow("VolumeRow", settingsPanelComp.transform);
        var volumeCaption = CreateSettingsText("VolumeCaption", "Volumen", volumeRow, 260f);
        var volDown = CreateSettingsButton("VolumeDown", " - ", volumeRow, 130f, () => ChangeSettingsVolume(-0.1f));
        settingsVolumeLabel = CreateSettingsText("VolumeValue", "", volumeRow, 320f);
        var volUp = CreateSettingsButton("VolumeUp", " + ", volumeRow, 130f, () => ChangeSettingsVolume(0.1f));

        //Fila de VIDAS: un botón que activa/desactiva las vidas (su texto cambia solo).
        var livesRow = CreateSettingsRow("LivesRow", settingsPanelComp.transform);
        var livesButton = CreateSettingsButton("LivesToggle", "", livesRow, 900f, () => ToggleSettingsLives());
        settingsLivesLabel = livesButton.GetComponentInChildren<TMP_Text>(true);

        //Fila de COLOR del HUD: etiqueta + una muestra de color por tema.
        var themeRow = CreateSettingsRow("ThemeRow", settingsPanelComp.transform);
        var themeCaption = CreateSettingsText("ThemeCaption", "Color del HUD", themeRow, 300f);
        settingsSwatches = new Image[hudThemeColors.Length];
        for (int i = 0; i < settingsSwatches.Length; i++)
        {
            var swatch = CreateUI($"Swatch{i}", themeRow);
            swatch.anchorMin = new Vector2(0.5f, 0.5f);
            swatch.anchorMax = new Vector2(0.5f, 0.5f);
            swatch.sizeDelta = new Vector2(90f, 90f);
            var img = swatch.gameObject.AddComponent<Image>();
            img.color = hudThemeColors[i];//La muestra muestra el color del tema.
            settingsSwatches[i] = img;
            var swatchButton = swatch.gameObject.AddComponent<Button>();
            swatchButton.targetGraphic = img;
            int index = i;//Copia local para la lambda.
            swatchButton.onClick.AddListener(() => SelectSettingsTheme(index));
        }

        //Botón CERRAR: vuelve al menú de pausa.
        CreateButton("Cerrar", settingsPanelComp.transform, () => CloseSettingsPanel());
        RefreshSettingsUI();
    }

    void WireSettingsElement(GameObject element)
    //Conecta los botones de un panel de ajustes existente (creado en el prefab MenuPause) por
    //su etiqueta o nombre: volumen (ON/OFF o -/+), vidas (ON/OFF), colores y volver atrás.
    //Así funciona sin componentes extra.
    {
        if (element == null || settingsWired)
            return;
        settingsWired = true;
        foreach (var btn in element.GetComponentsInChildren<Button>(true))
        {
            var label = btn.GetComponentInChildren<TMP_Text>(true);
            string match = (label != null && !string.IsNullOrEmpty(label.text)) ? label.text : btn.name;
            string n = match.Trim().ToLowerInvariant();
            if (n.Contains("volumen"))
            {
                //Botón ON/OFF del volumen: apaga/revive el sonido y cambia su texto.
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ToggleSettingsVolume());
                settingsVolumeToggleLabel = label;
            }
            else if (n.Contains("bajar") || n.Contains(" - ") || n == "-" || n.EndsWith("-"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ChangeSettingsVolume(-0.1f));
            }
            else if (n.Contains("subir") || n.Contains(" + ") || n == "+" || n.EndsWith("+"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ChangeSettingsVolume(0.1f));
            }
            else if (n.Contains("vida"))
            {
                //Botón ON/OFF de vidas: oculta/muestra los corazones y cambia su texto.
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ToggleSettingsLives());
                settingsLivesLabel = label;
            }
            else if (n.Contains("color") || n.Contains("colores") || n.Contains("tema"))
            {
                //Botón de colores: despliega la gama de colores del HUD.
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ToggleColorPicker());
            }
            else if (n.Contains("volver") || n.Contains("atras") || n.Contains("atrás")
                     || n.Contains("cerrar") || n.Contains("close") || n.Contains("salir") || n.Contains("back"))
            {
                //Botón "volver atrás": solo cierra el PanelAjustes, NO el menú de pausa.
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => CloseSettingsPanel());
            }
        }
        RefreshSettingsUI();
    }

    void OpenSettingsPanel()//Abre el panel de ajustes: a través del script SudokuPauseUI si lo trae.
    {
        //Primero se intenta abrir con el SudokuPauseUI del menú de pausa (referencia del PanelAjustes).
        var pauseComp = pausePanel != null ? pausePanel.GetComponent<SudokuPauseUI>() : null;
        if (pauseComp != null && pauseComp.SettingsPanel != null)
        {
            pauseComp.OpenSettings();
            WireSettingsElement(pauseComp.SettingsPanel);
            return;
        }
        if (settingsPanelComp == null)
            return;
        var rt = settingsPanelComp.transform as RectTransform;
        if (rt != null)
            SetFullScreenRect(rt);
        WireSettingsElement(settingsPanelComp);
        settingsPanelComp.SetActive(true);
        RefreshSettingsUI();
    }

    void CloseSettingsPanel()//Cierra el panel de ajustes (vuelve a la pausa).
    {
        if (settingsPanelComp != null)
            settingsPanelComp.SetActive(false);
    }

    void ChangeSettingsVolume(float delta)//Sube o baja el volumen (10% por clic).
    {
        SettingsVolume += delta;
        AudioListener.volume = SettingsVolume;//Aplica el volumen al instante.
        RefreshSettingsUI();
    }

    void ToggleSettingsVolume()//Apaga/reactiva el volumen (recuerda el volumen anterior).
    {
        if (SettingsVolume > 0.001f)//Estaba encendido: se guarda el volumen y se apaga.
        {
            PlayerPrefs.SetFloat("Sudoku_LastVolume", SettingsVolume);
            SettingsVolume = 0f;
        }
        else//Estaba apagado: se restaura el último volumen usado (o el máximo).
            SettingsVolume = PlayerPrefs.GetFloat("Sudoku_LastVolume", 1f);
        AudioListener.volume = SettingsVolume;//Aplica el volumen al instante.
        RefreshSettingsUI();
    }

    void ToggleSettingsLives()//Activa/desactiva las vidas y muestra/oculta los corazones del HUD.
    {
        SettingsLivesEnabled = !SettingsLivesEnabled;
        bool showLives = SettingsLivesEnabled;

        //Muestra u oculta la interfaz de corazones (DatosNivel) al instante, en todos los Sudokus.
        if (hearts != null)
        {
            for (int i = 0; i < hearts.Length; i++)
            {
                if (hearts[i] != null)
                    hearts[i].enabled = showLives;
            }
        }
        //Si se reactivan las vidas, se refresca el estado actual de los corazones.
        if (showLives && livesUI != null)
            livesUI.UpdateLives(mistakeSystem != null ? mistakeSystem.GetMistakes() : 0);
        RefreshSettingsUI();
    }

    void ToggleColorPicker()//Muestra/oculta la gama de colores del HUD.
    {
        if (colorPickerPanel == null)
            BuildColorPickerPanel();
        if (colorPickerPanel != null)
            colorPickerPanel.SetActive(!colorPickerPanel.activeSelf);
    }

    void BuildColorPickerPanel()//Crea el selector de colores: una gama para elegir el color del HUD.
    //Al elegir un color, cambia toda la interfaz (NumberPanel, PanelAjustes, DatosNivel, cuadrícula)
    //y el color queda guardado para la próxima vez que se cargue el juego.
    {
        colorPickerPanel = CreateOverlay("ColorPickerPanel", out var title);
        title.text = "COLOR DEL HUD";
        var swatchRow = CreateSettingsRow("SwatchesRow", colorPickerPanel.transform);
        for (int i = 0; i < hudThemeColors.Length; i++)
        {
            var swatch = CreateUI($"ColorSwatch{i}", swatchRow);
            swatch.anchorMin = new Vector2(0.5f, 0.5f);
            swatch.anchorMax = new Vector2(0.5f, 0.5f);
            swatch.sizeDelta = new Vector2(110f, 110f);
            var img = swatch.gameObject.AddComponent<Image>();
            img.color = hudThemeColors[i];//Cada muestra es un color de la gama.
            var swatchButton = swatch.gameObject.AddComponent<Button>();
            swatchButton.targetGraphic = img;
            int index = i;//Copia local para la lambda.
            swatchButton.onClick.AddListener(() => SelectSettingsTheme(index));
        }
        CreateButton("Cerrar", colorPickerPanel.transform, () => ToggleColorPicker());
    }

    void SelectSettingsTheme(int index)//Elige un color de HUD y lo aplica al instante.
    {
        SettingsThemeIndex = index;
        ApplyHudTheme(index);
        RefreshSettingsUI();
    }

    void RefreshSettingsUI()//Actualiza las etiquetas y el resaltado de las muestras de color.
    {
        bool volumeOn = SettingsVolume > 0.001f;
        //Etiqueta del botón ON/OFF del volumen (elemento del usuario en MenuPause).
        if (settingsVolumeToggleLabel != null)
            settingsVolumeToggleLabel.text = volumeOn ? "Volumen: ON" : "Volumen: OFF";
        if (settingsVolumeLabel != null)
            settingsVolumeLabel.text = $"Volumen: {Mathf.RoundToInt(SettingsVolume * 100f)}%";
        //Etiqueta del botón ON/OFF de vidas (elemento del usuario en MenuPause).
        if (settingsLivesLabel != null)
            settingsLivesLabel.text = SettingsLivesEnabled ? "Vidas: ON" : "Vidas: OFF";
        for (int i = 0; i < (settingsSwatches != null ? settingsSwatches.Length : 0); i++)
        {
            var swatch = settingsSwatches[i];
            if (swatch == null)
                continue;
            bool selected = i == SettingsThemeIndex;
            //La muestra seleccionada se ve a color completo y más grande; las demás atenuadas.
            swatch.color = selected ? hudThemeColors[i] : hudThemeColors[i] * 0.55f;
            swatch.transform.localScale = selected ? Vector3.one * 1.25f : Vector3.one;
        }
    }

    //Propiedades de los ajustes (guardados en PlayerPrefs).
    float SettingsVolume
    {
        get => Mathf.Clamp01(PlayerPrefs.GetFloat(VolumePrefKey, 1f));
        set
        {
            PlayerPrefs.SetFloat(VolumePrefKey, Mathf.Clamp01(value));
            PlayerPrefs.Save();
        }
    }
    bool SettingsLivesEnabled
    {
        get => PlayerPrefs.GetInt(LivesPrefKey, 1) == 1;
        set
        {
            PlayerPrefs.SetInt(LivesPrefKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    int SettingsThemeIndex
    {
        get => Mathf.Clamp(PlayerPrefs.GetInt(ThemePrefKey, 0), 0, hudThemeColors.Length - 1);
        set
        {
            PlayerPrefs.SetInt(ThemePrefKey, Mathf.Clamp(value, 0, hudThemeColors.Length - 1));
            PlayerPrefs.Save();
        }
    }

    Transform CreateSettingsRow(string name, Transform parent)
    //Crea una fila (layout horizontal) para el panel de ajustes, con tamaño fijo.
    {
        var rt = CreateUI(name, parent);
        rt.sizeDelta = new Vector2(900f, 90f);
        var le = rt.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = 900f;
        le.preferredHeight = 90f;
        var hlg = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(16, 16, 0, 0);
        hlg.spacing = 12f;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        return rt;
    }

    TMP_Text CreateSettingsText(string name, string content, Transform parent, float width)
    //Crea un texto para una fila de ajustes (ancho fijo).
    {
        var rt = CreateUI(name, parent);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(width, 70f);
        var text = rt.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = cachedFont != null ? cachedFont : TMP_Settings.defaultFontAsset;
        text.fontSize = fontSize;
        text.color = titleColor;
        text.text = content;
        text.alignment = TextAlignmentOptions.Left;
        return text;
    }

    Button CreateSettingsButton(string name, string label, Transform parent, float width, System.Action onClick)
    //Crea un botón simple para una fila de ajustes (ancho fijo).
    {
        var rt = CreateUI(name, parent);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(width, 70f);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = buttonColor;
        img.sprite = cachedButtonSprite;
        if (cachedButtonSprite != null)
            img.type = Image.Type.Sliced;
        var btn = rt.gameObject.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = accentColor;
        colors.pressedColor = accentColor;
        btn.colors = colors;
        var text = CreateText("Label", label, rt, fontSize, buttonTextColor, TextAlignmentOptions.Center, "stretch");
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;
        if (onClick != null)
            btn.onClick.AddListener(() => onClick());
        return btn;
    }

    void ApplyHudTheme(int themeIndex)//Aplica el color elegido del HUD a los paneles del juego.
    {
        var color = hudThemeColors[Mathf.Clamp(themeIndex, 0, hudThemeColors.Length - 1)];
        ApplyPanelColor(topBarRoot, color);//DatosNivel / TopBar
        ApplyPanelColor(actionRowRoot, color);//PanelAjustes (fila de acciones)
        if (numberPanel != null)
            ApplyPanelColor(numberPanel.gameObject, color);//NumberPanel
        if (gridRoot != null)
        {
            ApplyPanelColor(gridRoot, color);//SudokuGrid (fondo)
            //Las cajas de la cuadrícula también se repintan.
            for (int i = 0; i < gridRoot.transform.childCount; i++)
                ApplyPanelColor(gridRoot.transform.GetChild(i).gameObject, color);
        }
    }
    void ApplyPanelColor(GameObject target, Color color)
    //Pinta la imagen de fondo de un panel conservando su transparencia original.
    {
        if (target == null)
            return;
        var img = target.GetComponent<Image>();
        if (img == null)
            return;
        img.color = new Color(color.r, color.g, color.b, img.color.a);
    }

    void BuildVictoryPanel()//Crea el panel de victoria (o instancia el prefab si está asignado).
    {
        if (victoryPanelPrefab != null)
        {
            victoryPanel = Instantiate(victoryPanelPrefab, transform);
            SetFullScreenRect(victoryPanel.transform as RectTransform);//Cubre la pantalla.
            victoryPanel.SetActive(false);
            return;
        }
        victoryPanel = CreateOverlay("VictoryPanel", out var title);
        title.text = "VICTORIA";
        CreateButton("Menú", victoryPanel.transform, () => sessionController?.SaveAndGoToMenu());
    }

    void BuildDefeatPanel()//Crea el panel de derrota (o instancia el prefab si está asignado).
    {
        if (defeatPanelPrefab != null)
        {
            defeatPanel = Instantiate(defeatPanelPrefab, transform);
            SetFullScreenRect(defeatPanel.transform as RectTransform);//Cubre la pantalla.
            defeatPanel.SetActive(false);
            return;
        }
        defeatPanel = CreateOverlay("DefeatPanel", out var title);
        title.text = "GAME OVER";
        CreateButton("Reintentar", defeatPanel.transform, () => flowController?.RestartLevel());
        CreateButton("Nueva partida", defeatPanel.transform, () => flowController?.OpenNewGamePanel());
    }

    void BuildDifficultyPanel()//Crea el panel de selección de dificultad (o instancia el prefab si está asignado).
    {
        if (difficultyPanelPrefab != null)
        {
            difficultyPanel = Instantiate(difficultyPanelPrefab, transform);
            SetFullScreenRect(difficultyPanel.transform as RectTransform);//Cubre la pantalla.
            difficultyPanel.SetActive(false);
            difficultyPanelComp = difficultyPanel.GetComponent<SudokuDifficultySelectionPanel>();
            return;
        }
        difficultyPanel = CreateOverlay("DifficultyPanel", out var title);
        title.text = "Selecciona dificultad";
        difficultyPanelComp = difficultyPanel.AddComponent<SudokuDifficultySelectionPanel>();
        var easy = CreateButton("Fácil", difficultyPanel.transform, null);
        var medium = CreateButton("Medio", difficultyPanel.transform, null);
        var hard = CreateButton("Difícil", difficultyPanel.transform, null);
        var expert = CreateButton("Experto", difficultyPanel.transform, null);
        var extreme = CreateButton("Extremo", difficultyPanel.transform, null);
        var cancel = CreateButton("Cancelar", difficultyPanel.transform, () => difficultyPanelComp.Close(true));
        difficultyPanelComp.Setup(title, new[] { easy, medium, hard, expert, extreme }, cancel);
    }

    void WireActions()//Conecta los botones de acción con los controladores.
    {
        //Notas, Undo, Borrar, AutoNotas y Pista ya se conectaron en BuildActionRow.
        //Aquí solo conectamos lo que necesita el resaltado del botón de notas.
        if (highlightSystem != null && notesButtonImage != null)
            highlightSystem.SetNotesButtonImage(notesButtonImage);
    }

    void SetupComponents()//Reasigna las referencias de los scripts existentes para que apunten a la UI nueva.
    {
        //Los prefabs instanciados traen SUS PROPIOS scripts de UI DENTRO (SudokuDifficultyUI y
        //SudokuLivesUI en DatosNivel, SudokuHintsUI en "HintPanel" de PanelAjustes, SudokuPauseUI
        //en MenuPause). Como ResolveControllers corrió ANTES de construirlos, se re-resuelven aquí
        //para acoplarse a las instancias reales y NO crear duplicados.
        difficultyUI = SudokuSceneRef.Resolve(difficultyUI);
        livesUI = SudokuSceneRef.Resolve(livesUI);
        hintsUI = SudokuSceneRef.Resolve(hintsUI);
        pauseUI = SudokuSceneRef.Resolve(pauseUI);

        //Solo si algún script sigue faltando (prefab sin el componente), se crea en runtime
        //un controlador de respaldo para que pausa, vidas y pistas no se pierdan.
        EnsureUIComponent(ref pauseUI, "PauseController");
        EnsureUIComponent(ref livesUI, "LivesController");
        EnsureUIComponent(ref hintsUI, "HintsController");

        timer?.SetTimerText(timerText);
        if (livesUI != null && hearts != null)
            livesUI.Setup(hearts, resolvedHeartSprite, heartEmptySprite);
        if (difficultyUI != null && difficultyText != null)
            difficultyUI.Setup(difficultyText);
        //hintsUI: solo se reasigna si el texto/botón fueron creados por código; si viene del
        //prefab PanelAjustes ya trae sus referencias serializadas (Hint TMP + HintButton).
        if (hintsUI != null && hintText != null && hintButton != null)
            hintsUI.Setup(hintText, hintButton);
        //pauseUI: solo se reasigna si existe el botón de pausa (fila de ajustes por código);
        //si viene del prefab MenuPause, su botón propio ("Pause x") ya está serializado.
        if (pauseUI != null && pausePanel != null && pauseButton != null)
            pauseUI.Setup(pausePanel, pauseButton.gameObject);
        flowController?.ConfigureEndGamePanels(
            victoryPanel, victoryPanel,
            defeatPanel, defeatPanel,
            difficultyPanelComp);
    }
    T EnsureUIComponent<T>(ref T component, string objectName) where T : Component
    //Si el controlador no existe en la escena (por ejemplo SudokuPauseUI), lo crea en runtime.
    {
        if (component == null)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(transform, false);
            component = go.AddComponent<T>();
        }
        return component;
    }

    //================ HELPERS DE CREACIÓN DE UI ================

    RectTransform CreateUI(string name, Transform parent)//Crea un objeto UI con RectTransform.
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    Image CreateImage(string name, Transform parent, Color color, Sprite sprite)//Crea una imagen de fondo.
    {
        var rt = CreateUI(name, parent);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        img.sprite = sprite;
        if (sprite != null)
            img.type = Image.Type.Sliced;
        return img;
    }

    TextMeshProUGUI CreateText(string name, string content, Transform parent, float size, Color color, TextAlignmentOptions align, string anchor)
    //Crea un texto TMP con una posición predefinida: left, right, center o stretch (rellena el padre).
    {
        var rt = CreateUI(name, parent);
        var text = rt.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = cachedFont != null ? cachedFont : TMP_Settings.defaultFontAsset;
        text.fontSize = size;
        text.color = color;
        text.text = content;
        text.alignment = align;
        switch (anchor)
        {
            case "left":
                rt.anchorMin = new Vector2(0f, 0.5f);
                rt.anchorMax = new Vector2(0f, 0.5f);
                rt.pivot = new Vector2(0f, 0.5f);
                rt.anchoredPosition = new Vector2(20f, 0f);
                rt.sizeDelta = new Vector2(300f, 60f);
                break;
            case "right":
                rt.anchorMin = new Vector2(1f, 0.5f);
                rt.anchorMax = new Vector2(1f, 0.5f);
                rt.pivot = new Vector2(1f, 0.5f);
                rt.anchoredPosition = new Vector2(-20f, 0f);
                rt.sizeDelta = new Vector2(300f, 60f);
                break;
            case "center":
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(300f, 60f);
                break;
            default://stretch: rellena todo el padre.
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                break;
        }
        return text;
    }

    Button CreateButton(string label, Transform parent, Action onClick)//Crea un botón con texto.
    {
        var rt = CreateUI("Btn_" + label, parent);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = buttonColor;
        img.sprite = cachedButtonSprite;
        if (cachedButtonSprite != null)
            img.type = Image.Type.Sliced;
        var btn = rt.gameObject.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = accentColor;
        colors.pressedColor = accentColor;
        btn.colors = colors;
        var le = rt.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = buttonWidth;
        le.preferredHeight = buttonHeight;
        var labelText = CreateText("Label", label, rt, fontSize, buttonTextColor, TextAlignmentOptions.Center, "stretch");
        labelText.rectTransform.offsetMin = Vector2.zero;
        labelText.rectTransform.offsetMax = Vector2.zero;
        if (onClick != null)
            btn.onClick.AddListener(() => onClick());
        if (label == "Notas")
            notesButtonImage = img;//Guarda la imagen del botón de notas para el resaltado.
        return btn;
    }

    GameObject CreateOverlay(string name, out TextMeshProUGUI title)//Crea un panel de pantalla completa (inactivo).
    {
        var rt = CreateUI(name, transform);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var img = rt.gameObject.AddComponent<Image>();
        img.color = overlayColor;
        var layout = rt.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = spacing;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        title = CreateText("Title", "", rt, titleFontSize, titleColor, TextAlignmentOptions.Center, "stretch");
        var titleLayout = title.gameObject.AddComponent<LayoutElement>();
        titleLayout.preferredWidth = 600f;
        titleLayout.preferredHeight = 80f;
        rt.gameObject.SetActive(false);
        return rt.gameObject;
    }

    static void SetFullScreenRect(RectTransform rt)
    //Ajusta el rect de un panel para que cubra toda la pantalla (se posiciona encima del mapa).
    //Se usa con los paneles de prefab, por si el prefab quedó posicionado a un lado o sobresaliendo.
    {
        if (rt == null)
            return;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    GameObject BuildDefaultNumberButton()//Crea un botón de número por defecto si no hay prefab asignado.
    {
        var rt = CreateUI("DefaultNumberButton", transform);
        rt.sizeDelta = new Vector2(numberButtonSize, numberButtonSize);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = buttonColor;
        img.sprite = cachedButtonSprite;
        if (cachedButtonSprite != null)
            img.type = Image.Type.Sliced;
        rt.gameObject.AddComponent<Button>();
        var le = rt.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = numberButtonSize;
        le.preferredHeight = numberButtonSize;
        var label = CreateText("Label", "1", rt, fontSize, buttonTextColor, TextAlignmentOptions.Center, "stretch");
        label.rectTransform.offsetMin = Vector2.zero;
        label.rectTransform.offsetMax = Vector2.zero;
        rt.gameObject.SetActive(false);//Se activa cuando NumberPanel lo configure.
        return rt.gameObject;
    }
}
