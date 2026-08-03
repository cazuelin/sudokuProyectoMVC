using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class SudokuGameFlowController : MonoBehaviour
{
    SudokuGenerator generator = new SudokuGenerator();//Crea el generador de Sudoku.
    //Sirve para generar una partida nueva
    //No lleva [SerializeField] porque no se arrastra desde Unity. Es una clase lógica, no un componente de la escena.
    [Header("Debug (temporal)")]
    [SerializeField] SudokuBoardView boardView;//Referencia a la vista del tablero.
    //Sirve para crear y actualizar visualmente las celdas. En terminos simples boardView es el que muestra el tablero en pantalla.
    [SerializeField] SudokuSaveManager saveManager;//Referencia al sistema de guardado.
    //Sirve para guardar, cargar y borrar partidas. En palabras simples saveManager maneja los archivos/datos guardados de la partida.
    [SerializeField] SudokuBoardController boardController;//Referencia al controlador lógico del tablero.
    //Sirve para manejar los datos reales del Sudoku. boardController sabe qué números hay, qué celdas son fijas, qué notas hay y si ganaste.
    [SerializeField] SudokuInputController inputController;//Referencia al controlador de entrada del jugador.
    //Sirve para manejar cosas como pistas y posiblemente selección/colocación de números.
    //inputController controla la interacción del jugador y la cantidad de pistas disponibles.
    [SerializeField] SudokuDifficultyUI difficultyUI;//Referencia a la UI que muestra la dificultad.
    //difficultyUI actualiza en pantalla si la partida es Easy, Medium, Hard, etc.
    [SerializeField] SudokuMistakeSystem mistakeSystem;//Referencia al sistema de errores.
    //Sirve para controlar cuántos errores lleva el jugador y detectar derrota.
    [SerializeField] SudokuLivesUI livesUI;//Referencia a la UI de vidas/errores. livesUI muestra en pantalla cuántos errores o vidas quedan.
    [SerializeField] SudokuHintsUI hintsUI;//Referencia a la UI de pistas. hintsUI muestra en pantalla la cantidad de hints disponibles.
    [SerializeField] SudokuTimer timer;//Referencia al timer del juego. timer controla el tiempo de la partida.
    [SerializeField] SessionContext sessionContext;//Referencia al contexto de sesión.Este guarda datos importantes de la partida actual
    //se usa para saber : qué slot cargar , qué dificultad generar , si cargar desde guardado , si el juego está Playing, Victory o Defeat
    //en palabras simples sessionContext es la memoria de la sesión actual.
    [Header("End Game UI")]
    [SerializeField] GameObject victoryPanel;//Panel de victoria. Se activa cuando ganas
    [SerializeField] GameObject defeatPanel;//Panel de derrota. Se activa cuando pierde
    [SerializeField] GameObject defeatActionsPanel;//Panel con acciones después de perder.
    //Por ejemplo, botones como: reintentar , nueva partida , Menu. Se muestra después del panel de derrota
    [SerializeField] SudokuDifficultySelectionPanel difficultySelectionPanel;//Panel para seleccionar dificultad.
    //Se usa cuando el jugador quiere empezar una partida nueva después de perder o desde un panel.
    [SerializeField] GameObject victoryPanelButtons;//Botones del panel de victoria. Se muestran al ganar
    [SerializeField] float victoryDelay = 2f;//Tiempo de espera antes de mostrar o avanzar en la secuencia de victoria.
    [SerializeField] float defeatDelay = 2f;//Tiempo de espera antes de mostrar el panel de derrota.
    [SerializeField] float defeatButtonsDelay = 0.5f;//Tiempo de espera entre mostrar el panel de derrota y mostrar sus botones/acciones.

    void Start()
    {       
        if (difficultySelectionPanel == null)//Aquí pregunta:¿No tengo asignado el panel de selección de dificultad?
            //Si no está asignado en el Inspector, intenta buscarlo automáticamente en la escena.
        {
            difficultySelectionPanel = FindFirstObjectByType<SudokuDifficultySelectionPanel>();
            //Busca el primer objeto activo de tipo SudokuDifficultySelectionPanel.
            //Esto sirve como respaldo por si olvidaste arrastrarlo en Unity.
        }
        mistakeSystem.OnGameOver += OnGameOver;//Aquí se suscribe a un evento.
        //Cuando mistakeSystem dispare OnGameOver, ejecuta mi función OnGameOver.
        //en simple palabras Si el jugador pierde por errores, este script se entera.
        mistakeSystem.OnMistakeChanged += OnMistakeChanged;//También se suscribe al evento de cambios de errores.
        //eso significa Cuando cambie la cantidad de errores, llama OnMistakeChanged.
        //Eso permite actualizar la UI de vidas: livesUI?.UpdateLives(mistakes);
        if (boardController != null)//pregunta si el boardController no esta vacio
            boardController.OnBoardChanged += OnBoardChanged; //Si existe boardController, se suscribe al evento del tablero.
        //significa Cada vez que cambie el tablero, llama OnBoardChanged.
        //Ese método después revisa si el jugador ganó: boardController.CheckWin()
        if (inputController != null)//pregunta si el inputController no esta vacio
            inputController.OnHintsChanged += OnHintsChanged;//Si existe inputController, se suscribe al evento de cambio de pistas.
        //significa Cuando cambie la cantidad de hints disponibles, actualiza la UI.
        if (inputController != null)//pregunta si el inputController no esta vacio
            hintsUI?.UpdateHints(inputController.RemainingHints);//Aquí actualiza la UI de pistas al iniciar.
        //inputController.RemainingHints contiene cuántas pistas quedan.
        //El operador ?. significa: Si hintsUI no es null, llama UpdateHints.
        if (victoryPanel != null)//pregunta si el victoryPanel no esta vacio o activo
            victoryPanel.SetActive(false);//si esta activo Oculta el panel de victoria.
        if (defeatPanel != null)//pregunta si el defeatPanel no esta vacio o activo
            defeatPanel.SetActive(false);//si esta activo Oculta el panel de derrota.
        if (defeatActionsPanel != null)//pregunta si el defeatActionsPanel no esta vacio o activo
            defeatActionsPanel.SetActive(false);//si esta activo Oculta los botones/acciones de derrota.
        //Esto evita que aparezcan paneles viejos al entrar a la escena.
        if (difficultySelectionPanel != null)//pregunta si el difficultySelectionPanel no esta vacio o activo
        {
            difficultySelectionPanel.Close();//Si existe el panel de dificultad, lo cierra.
            difficultySelectionPanel.OnDifficultySelected += HandleDifficultySelected;//Luego se suscribe a su evento.
            //Significa:Cuando el jugador elija una dificultad, llama HandleDifficultySelected.
        }
        if (victoryPanelButtons != null)//pregunta si el victoryPanelButtons no esta vacio o activo
            victoryPanelButtons.SetActive(false);//si esta activo Oculta los botones/acciones de victoria.
    }
    void OnDestroy()//Se ejecuta cuando el GameObject se destruye o cuando la escena se cierra/cambia.Su objetivo es limpiar eventos.
    {
        if (mistakeSystem != null)//Aquí se desuscribe de los eventos del sistema de errores.
        {
            mistakeSystem.OnGameOver -= OnGameOver;
            mistakeSystem.OnMistakeChanged -= OnMistakeChanged;
            //Antes, en Start, hiciste: += OnGameOver y += OnMistakeChanged
            //Ahora haces: -= OnGameOver y -= OnMistakeChanged
            //Esto significa: Ya no quiero escuchar esos eventos.
            //Esto es importante porque si no te desuscribes, un objeto destruido podría seguir recibiendo llamadas, causando errores raros.
        }
        if (boardController != null)
            boardController.OnBoardChanged -= OnBoardChanged;//Deja de escuchar cambios del tablero.
        if (inputController != null)
            inputController.OnHintsChanged -= OnHintsChanged;//Deja de escuchar cambios de pistas.
        if (difficultySelectionPanel != null)
            difficultySelectionPanel.OnDifficultySelected -= HandleDifficultySelected;//Deja de escuchar selección de dificultad.
        //En resumen, OnDestroy hace lo contrario de Start:
        //Start      -> se conecta a eventos
        //OnDestroy  -> se desconecta de eventos
        //Esto mantiene el proyecto más limpio y evita llamadas duplicadas o referencias muertas.
    }
    public void Initialize()//Initialize es el inicio real de la partida.
        //A diferencia de Start, esta función no la llama Unity automáticamente. La llama SudokuGameManager desde su propio Start:
        //flowController.Initialize();
        //Esta función decide: ¿Cargo una partida guardada o genero una partida nueva?
    {
        if (sessionContext == null)//Si sessionContext no está asignado, no puede continuar.
            //¿Por qué? Porque necesita saber: SelectedSlot , SelectedDifficulty , LoadFromSave , GameState
            //Sin eso, no sabe qué partida cargar ni qué dificultad generar.
        {
            Debug.LogError("SessionContext no asignado en SudokuGameFlowController");
            return;//por lo tanto retorna si no hay sessionContext
        }
        //si hay una session disponible
        int slot = sessionContext.SelectedSlot;//Obtiene el slot seleccionado.
        //ejemplo slot = 0 , slot = 1 , slot = 2. Ese slot indica dónde guardar o desde dónde cargar.
        if (slot < 0)//Si el slot es menor que 0, significa que no hay slot válido.
        {
            Debug.LogError("No hay slot seleccionado");
            return;//Entonces muestra error y se detiene.
        }
        if (sessionContext.LoadFromSave)//Ahora revisa si debe cargar partida:
            //Si esto es true, significa: El jugador eligió continuar una partida guardada.
        {
            bool loaded = saveManager.LoadGame(slot);//Entonces intenta cargar la partida
            //LoadGame(slot) intenta cargar los datos guardados de ese slot.
            //si pudo cargar devuelve true si no pudo cargar devuelve false
            if (loaded)//Si la carga fue exitosa, entra aquí.
            {
                LoadBoardToView();//Carga los datos del tablero en la vista visual. O sea, hace que lo cargado aparezca en pantalla.
                sessionContext.GameState = SudokuGameState.Playing;//Pone el estado en jugando.
                var saved = saveManager.GetSlotData(slot);//Obtiene los datos guardados del slot.
                //Esto se usa especialmente para restaurar las pistas restantes.
                if (saved != null && inputController != null)
                //saved != null : si existen datos guardados
                //inputController != null : si existe inputController
                //el signo && es una condicion que requiere que los 2 elementos sean true si uno falla no pasa de aqui
                {
                    inputController.SetRemainingHints(saved.remainingHints);//si existen datos guardados restaura las pistas
                    //ejemplo saved.remainingHints = 2. Entonces el jugador vuelve con 2 pistas disponibles.
                }
                //Luego actualiza la UI de pistas:
                if (inputController != null)//Si hay inputController
                    hintsUI?.UpdateHints(inputController.RemainingHints);//entonces muestra sus pistas restantes
                else
                    hintsUI?.UpdateHints(0);//Si no hay, muestra 0.
                boardController?.NotifyBoardChanged();//Notifica que el tablero cambió.
                //Esto ayuda a que otros sistemas actualicen la vista o revisen el estado.
                //El ?. significa: Solo llama si boardController no es null.
                timer.StartTimer();//Inicia el timer.
                return;//Este return es muy importante.
                //Significa: Ya cargué la partida, no generes una nueva.
                //Si no estuviera este return, después de cargar podría seguir hacia abajo y generar otro Sudoku encima.
            }
        }
        //Si no se debe cargar, o si no se pudo cargar, llega a esta parte:
        //Esto crea una partida nueva.
        timer.ResetTime();//Reinicia el tiempo a cero.
        GenerateGame();//Genera un Sudoku nuevo con la dificultad seleccionada.
        timer.StartTimer();//Empieza a contar tiempo.
        saveManager.SaveGame(slot);//Guarda la partida recién creada en el slot. Esto permite continuar después.
        boardController?.NotifyBoardChanged();//Notifica que el tablero ya está listo/cambió.
    }
    void LoadBoardToView()//Esta función se usa cuando cargas una partida guardada.
        //Su objetivo es:Tomar los datos actuales del boardController y mostrarlos en pantalla.
    {
        EnsureBoardCreated();//Primero se asegura de que el tablero visual exista.
        //Si las celdas todavía no fueron creadas, esta función llama a: boardView.CreateBoard();
        var data = boardController.GetBoardData();//Obtiene los datos actuales del tablero desde el boardController.
        boardView.UpdateBoard(//Actualiza el tablero visual. Le pasa los datos a boardView.
            //En palabras simples : Dibuja los números, bloquea visualmente las celdas fijas y muestra las notas.
            data.values,//values son los números del tablero.
            data.fixedCells,//fixedCells indica qué celdas son fijas.
            data.notesMask//notesMask contiene las notas/candidatos.
        );
        ClearEndPanels();//Limpia los paneles de victoria/derrota.
        //Esto es útil porque al cargar una partida no quieres que aparezca un panel viejo en pantalla.
        livesUI?.UpdateLives(mistakeSystem.GetMistakes());//Actualiza la UI de vidas/errores.
        //obtiene cuántos errores tiene actualmente la partida cargada.
        //?. significa: Si livesUI no es null, actualiza las vidas.
        if (inputController != null)//Si existe inputController, actualiza la UI de pistas.
            hintsUI?.UpdateHints(inputController.RemainingHints);//indica cuántas pistas le quedan al jugador.
    }
    void GenerateGame()//Esta función se usa cuando empiezas una partida nueva.
        //su objetivo es Generar un Sudoku nuevo, cargarlo en la lógica, mostrarlo en pantalla y reiniciar los sistemas de partida.
    {
        EnsureBoardCreated();//Primero asegura que el tablero visual exista.
        var difficulty = sessionContext.SelectedDifficulty;//Obtiene la dificultad elegida. La dificultad viene desde sessionContext.
        var data = generator.Generate(difficulty);//Genera el Sudoku. generator.Generate(difficulty) devuelve un SudokuBoardData.
        //Ese data contiene:values , solution , fixedCells , notesMask
        //Depende de cómo esté definido tu SudokuBoardData. En palabras simples: Aquí nace el puzzle nuevo.
        difficultyUI.SetDifficulty(difficulty);//Actualiza la UI de dificultad. Por ejemplo, muestra en pantalla: Hard 
        boardController.SetBoardData(data);//Carga los datos generados en el controlador del tablero.
        //Desde este punto, boardController ya conoce el Sudoku actual.
        boardController.SetInitialState(data);//Guarda el estado inicial del tablero.
        //Esto es importante para poder reiniciar la partida con: boardController.ResetBoard();
        //Sin este estado inicial, no sabría a qué tablero volver.
        boardView.UpdateBoard(data.values, data.fixedCells, data.notesMask);//Actualiza la vista. Muestra en pantalla el Sudoku generado.
        mistakeSystem.Init(0);//Reinicia el sistema de errores en 0. es decir Nueva partida, cero errores.
        livesUI?.UpdateLives(0);//Actualiza la UI de vidas/errores para mostrar cero errores.
        inputController?.ResetHints();//Reinicia las pistas disponibles.
        //El ?. significa que solo lo llama si inputController existe.
        if (inputController != null)//si existe el inputController
            hintsUI?.UpdateHints(inputController.RemainingHints);//Después de reiniciar las pistas, actualiza la UI.
        sessionContext.GameState = SudokuGameState.Playing;//Cambia el estado del juego a: Playing
        //esto significa La partida ya está lista para jugar.
        ClearEndPanels();//Oculta paneles de victoria/derrota y limpia errores visuales.
    }
    void OnBoardChanged()//Esta función se ejecuta cuando el tablero cambia.
        //Se conecta en Start con: boardController.OnBoardChanged += OnBoardChanged;
        //Eso significa: Cada vez que el tablero avise que cambió, llama esta función.
    {
        if (sessionContext == null || sessionContext.GameState != SudokuGameState.Playing)//Esta línea protege la función.
            //sessionContext == null  ¿No existe el contexto de sesión?
            //sessionContext.GameState != SudokuGameState.Playing   ¿El juego no está en estado Playing?
            //Si cualquiera de esas condiciones es verdadera, hace: true
            //O sea, no revisa victoria. Esto evita revisar victoria mientras el juego está: Generating , Paused , Victory , Defeat
            return;

        if (boardController != null && boardController.CheckWin())
        //boardController != null    Existe el controlador del tablero.
        //boardController.CheckWin()    El jugador completó correctamente el Sudoku.
        //Si ambas son verdaderas, entra:   HandleVictory();
        {
            HandleVictory();//Llama a la función que maneja la victoria.
            //Esa función cambia el estado a Victory, detiene el timer y lanza la secuencia de victoria.
        }
    }
    void OnMistakeChanged(int mistakes)//Esta función se llama cuando cambia la cantidad de errores.
        //Se conecta en Start así:mistakeSystem.OnMistakeChanged += OnMistakeChanged;
        //Entonces cuando el jugador comete un error, mistakeSystem avisa : OnMistakeChanged(mistakes);
        //int mistakes : Recibe la cantidad actual de errores.
    {
        livesUI?.UpdateLives(mistakes);//Actualiza la UI de vidas/errores.
        //El ?. significa: Si livesUI no es null, llama UpdateLives.
        //En simple: Si el jugador tiene 2 errores, la UI muestra 2 errores.
    }
    void OnHintsChanged(int remainingHints)//Esta función se llama cuando cambia la cantidad de pistas disponibles.
        //Se conecta en Start así: inputController.OnHintsChanged += OnHintsChanged;
        //int remainingHints : Recibe cuántas pistas quedan.
    {
        hintsUI?.UpdateHints(remainingHints);//Actualiza la UI de hints.
        //En simple: Si usaste una pista y ahora quedan 2, la UI muestra 2.
    }
    void HandleVictory()//Esta función maneja qué pasa cuando ganas.
    {
        if (sessionContext == null || sessionContext.GameState != SudokuGameState.Playing)
            //sessionContext == null     Si no existe sessionContext, se detiene.
            //sessionContext.GameState != SudokuGameState.Playing     Si el juego no está en Playing, también se detiene.
            //Esto evita activar victoria dos veces o activarla cuando estás en derrota/pausa.
            return;//regresa si una o la otra no pasan de true
        sessionContext.GameState = SudokuGameState.Victory;//si pasa de la validacion entonces Cambia el estado del juego a Victory.
        //Desde este momento, otros sistemas pueden saber: La partida terminó en victoria.
        timer.StopTimer();//Detiene el timer. Ya no debe seguir contando tiempo después de ganar.
        StartCoroutine(VictorySequence());//Inicia una corrutina para mostrar la interfaz de victoria.
    }
    void HandleDefeat()//Esta función maneja qué pasa cuando pierdes.
    {
        if (sessionContext == null || sessionContext.GameState != SudokuGameState.Playing)
            //sessionContext == null     Si no existe sessionContext, se detiene.
            //sessionContext.GameState != SudokuGameState.Playing     Si el juego no está en Playing, también se detiene.
            //Solo puede entrar en derrota si el juego estaba jugando.
            return;
        sessionContext.GameState = SudokuGameState.Defeat;//si pasa de la validacion entonces Cambia el estado del juego a derrota.
        timer.StopTimer();//Detiene el timer. Ya no debe seguir contando tiempo después de perder.
        StartCoroutine(DefeatSequence());//Inicia una corrutina para mostrar la interfaz de derrota.
    }
    void OnGameOver()//Esta función se llama cuando el sistema de errores avisa que se acabó el juego.
        //Se conecta así: mistakeSystem.OnGameOver += OnGameOver;
        //Cuando el jugador llega al máximo de errores, mistakeSystem dispara OnGameOver.
    {
        HandleDefeat();//en simple El sistema de errores dice “perdiste”, y el flujo del juego ejecuta la derrota.
    }

    void ClearEndPanels()//Esta función limpia la UI final.  Se usa cuando empieza una partida nueva, se carga una partida o se reinicia.
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(false);//Oculta el panel de victoria.
        if (victoryPanelButtons != null)
            victoryPanelButtons.SetActive(false);//Oculta los botones del panel de victoria.
        if (defeatPanel != null)
            defeatPanel.SetActive(false);//Oculta el panel de derrota.
        if (defeatActionsPanel != null)
            defeatActionsPanel.SetActive(false);//Oculta los botones/acciones de derrota.
        if (difficultySelectionPanel != null)
            difficultySelectionPanel.Close();//Cierra el panel de selección de dificultad.
        boardView.ClearAllErrors();//Limpia errores visuales del tablero.
        //Por ejemplo, si alguna celda estaba marcada en rojo por error, aquí se limpia.
    }
    IEnumerator VictorySequence()//es la coroutina que permite esperar segundos antes y despues de generar la victoria
    {
        yield return new WaitForSeconds(victoryDelay);//Espera victoryDelay segundos.
        //Si tienes: victoryDelay = 2f; entonces espera 2 segundos.
        if (victoryPanel != null)
            victoryPanel.SetActive(true);//Muestra el panel de victoria.
        if (victoryPanelButtons != null)
            victoryPanelButtons.SetActive(true);//Muestra los botones del panel de victoria.
        saveManager?.DeleteSlot(sessionContext.SelectedSlot);//Borra la partida guardada del slot actual.
        //¿Por qué? Porque ya ganaste esa partida, entonces no tiene sentido dejarla como “continuar partida”.
        yield return new WaitForSeconds(victoryDelay);//Vuelve a esperar. Con victoryDelay = 2f, sería otra espera de 2 segundos.
        SceneManager.LoadScene("MainMenu");//Carga la escena del menú principal.
    }
    IEnumerator DefeatSequence()//es la coroutina que permite esperar segundos antes y despues de generar la derrota
    {
        yield return new WaitForSeconds(defeatDelay);//Espera victoryDelay segundos.
        //Si tienes: victoryDelay = 2f; entonces espera 2 segundos.
        if (defeatPanel != null)
            defeatPanel.SetActive(true);//Muestra el panel de derrota.
        yield return new WaitForSeconds(defeatButtonsDelay);//Espera un poco más antes de mostrar los botones/acciones.
        //defeatButtonsDelay = 0.5f; espera medio segundo.
        if (defeatActionsPanel != null)
            defeatActionsPanel.SetActive(true);//Activa el panel de acciones de derrota.
        //Ahí podrían estar botones como: Reintentar , Nueva partida , Volver al menú
    }
    public void RestartLevel()//Esta función reinicia la misma partida, no genera una nueva.
    {
        if (boardController == null || boardView == null || mistakeSystem == null)
            //Si falta una referencia esencial, se detiene.
            return;

        boardController.ResetBoard();//Restaura el tablero al estado inicial guardado.
        //Ese estado inicial se guardó antes con: boardController.SetInitialState(data);
        var data = boardController.GetBoardData();//Obtiene el tablero ya reiniciado.
        boardView.UpdateBoard(data.values, data.fixedCells, data.notesMask);//Luego actualiza la vista
        //Así el tablero visual vuelve a mostrarse como al inicio.
        mistakeSystem.Init(0);//Reinicia errores
        livesUI?.UpdateLives(0);//Actualiza la UI de vidas/errores
        inputController?.ResetHints();//Reinicia pistas
        if (inputController != null)
            hintsUI?.UpdateHints(inputController.RemainingHints);//Actualiza la UI de pistas
        sessionContext.GameState = SudokuGameState.Playing;//Pone el juego en estado jugando:
        timer.ResetTime();//Reinicia el timer
        timer.StartTimer();//arranca el timer
        ClearEndPanels();//limpia paneles finales
    }
    public void OpenNewGamePanel()//Esta función abre el panel para crear una nueva partida.
    {
        if (defeatPanel != null)
            defeatPanel.SetActive(false);//Primero oculta el panel de derrota
        if (defeatActionsPanel != null)
            defeatActionsPanel.SetActive(false);//Luego oculta las acciones de derrota
        if (difficultySelectionPanel != null)
            difficultySelectionPanel.Open(sessionContext.SelectedSlot);//Después abre el panel de dificultad
        //Le pasa el slot actual: sessionContext.SelectedSlot
        //eso significa La nueva partida se creará en este mismo slot.
        //Se usa normalmente después de perder, cuando el jugador quiere empezar otro Sudoku.
    }
    void StartNewGameWithDifficulty(int slotIndex, SudokuGameManager.Difficulty difficulty)
        //Esta función inicia una partida nueva con una dificultad específica.
    {
        if (sessionContext == null)//Sin sessionContext, no puede guardar slot ni dificultad.
            return;
        //si detecta sessionContext entonces guarda :
        sessionContext.SelectedSlot = slotIndex;//en qué slot se jugará
        sessionContext.SelectedDifficulty = difficulty;//qué dificultad tendrá
        timer.ResetTime();//Reinicia el tiempo
        GenerateGame();//Genera el nuevo Sudoku. Dentro de GenerateGame se crea el tablero, se reinician errores, pistas y estado.
        timer.StartTimer();//Arranca el timer
        saveManager?.SaveGame(sessionContext.SelectedSlot);//Guarda la nueva partida
        if (difficultySelectionPanel != null)
            difficultySelectionPanel.Close();//Y cierra el panel de dificultad
    }
    void HandleDifficultySelected(int slotIndex, SudokuGameManager.Difficulty difficulty)
    //Esta función responde al evento del panel de dificultad.
    //En Start, el script se suscribe así: difficultySelectionPanel.OnDifficultySelected += HandleDifficultySelected;
    //Eso significa: Cuando el jugador elija una dificultad, llama HandleDifficultySelected.
    {        
        StartNewGameWithDifficulty(slotIndex, difficulty);//Esta función simplemente pasa los datos
        //O sea, es un puente entre la UI y la lógica de crear partida nueva.
    }
    void EnsureBoardCreated()//Esta función asegura que el tablero visual exista antes de actualizarlo.
    {
        var cells = boardView.GetCells();//Obtiene la matriz/lista de celdas visuales del tablero.
        if (cells[0, 0] == null)//Revisa la primera celda. Si está en null, significa que el tablero todavía no fue creado.
            boardView.CreateBoard();//entonces. Crea las celdas visuales.
        //Se usa antes de cargar o generar tablero para evitar intentar actualizar celdas que aún no existen.
        //en simple Antes de pintar el Sudoku, asegúrate de que las 81 celdas visuales ya estén creadas.
    }
}
