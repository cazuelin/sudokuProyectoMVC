using UnityEngine;
public class SudokuGameManager : MonoBehaviour//Este script es como el coordinador principal de la escena del Sudoku 9x9.
//No genera directamente el tablero, no maneja cada botón y no resuelve el Sudoku por sí mismo.
//Su trabajo es conectar sistemas importantes: sesión, vista del tablero, timer, controladores y estado del juego.
{
    [SerializeField] SessionContext sessionContext;//Guarda información de la partida actual.
    //Por ejemplo: dificultad seleccionada , estado del juego , slot de guardado , si se continúa una partida.
    //Es como la memoria compartida de la sesión.
    [SerializeField] SudokuBoardView boardView;//Es la vista del tablero.Se encarga de crear y actualizar las celdas visuales del Sudoku.
    [SerializeField] SudokuTimer timer;//Controla el tiempo de la partida.El GameManager lo usa cuando pausa o reanuda.
    [Header("Controllers")]
    [SerializeField] SudokuBoardController boardController;//Maneja la lógica del tablero.
    [SerializeField] SudokuRules.SudokuVariant variant = SudokuRules.SudokuVariant.Standard3x3;
    //por ejemplo : colocar número , validar movimiento , hacer undo , manejar notas , guardar boardData , notificar cambios.
    //Es una de las referencias más importantes.
    [SerializeField] SudokuGameFlowController flowController;//Maneja el flujo de la partida.
    //por ejemplo : crear nueva partida , cargar partida , generar Sudoku , iniciar estado inicial
    [SerializeField] SudokuHighlightSystem highlightSystem;//Maneja el resaltado visual del tablero.
    //por ejemplo : resaltar celda seleccionada , resaltar iguales , resaltar fila/columna/caja
    public enum Difficulty//Aquí defines las dificultades disponibles. Sirve para conectar la dificultad elegida con generación y técnicas.
    {
        Easy,
        Medium,
        Hard,
        Expert,
        Extreme
    }
    //NOTA: el estado del juego (SudokuGameState) ya NO se guarda en este script.
    //La ÚNICA fuente de verdad es SessionContext.GameState, para evitar dos estados que se desincronicen.
    void Start()
    {
        //Resuelve las referencias de forma robusta: si apuntan a un prefab (asset) o a un objeto
        //destruido, las busca automáticamente en la escena (se "acoplan" a las instancias reales).
        sessionContext = SudokuSceneRef.ResolveSession(sessionContext);
        boardView = SudokuSceneRef.Resolve(boardView);
        timer = SudokuSceneRef.Resolve(timer);
        boardController = SudokuSceneRef.Resolve(boardController);
        flowController = SudokuSceneRef.Resolve(flowController);
        highlightSystem = SudokuSceneRef.Resolve(highlightSystem);
        if (sessionContext == null)//Primero revisa que sessionContext esté asignado.
        {
            return;//y detiene la función. Esto evita que el juego siga y falle con errores más confusos.
        }
        if (sessionContext.SelectedSlot >= 0)
            variant = sessionContext.SelectedVariant;
        SudokuRules.SetVariant(variant);
        if (NumberPanel.Instance != null)
            NumberPanel.Instance.InitButtons();//Actualiza los botones del panel de números al tamaño del nuevo variante.
        sessionContext.GameState = SudokuGameState.Generating;//Pone el juego en estado:Generating. El Sudoku se está preparando/generando.
        //Así otros scripts que miran la sesión también saben que el juego está generando.
        boardView.CreateBoard();//Crea visualmente el tablero. Normalmente aquí se instancian o preparan las 81 celdas del Sudoku.
        var cells = boardView.GetCells();//Obtiene las celdas creadas por la vista.
        //var significa que C# deduce el tipo automáticamente.
        //cells es una colección de celdas visuales, como SudokuCell[] o List<SudokuCell>.
        highlightSystem.Init(cells, boardController);//Inicializa el sistema de resaltado.
        //le pasa cells que son Las celdas visuales que puede pintar/resaltar.
        //le pasa boardController que es La lógica del tablero, para saber datos actuales.
        flowController.Initialize();//Inicializa el flujo de juego.
        boardController.OnBoardChanged += () =>//Esta parte conecta la lógica del tablero con la vista.
        //OnBoardChanged es un evento. significa Cuando el tablero cambie, ejecuta este código.
        //boardController.OnBoardChanged += () => Esto registra una función anónima.
        //es que La función se ejecutará cada vez que boardController avise que cambió el tablero.
        {
            var data = boardController.boardData;//Obtiene los datos actuales del tablero.
            //boardData contiene cosas como: values , fixedCells , notesMask
            boardView.UpdateBoard(//Actualiza la vista usando los datos actuales. osea Si la lógica cambia, redibuja el tablero.
                data.values,//Los números del tablero.
                data.fixedCells,//Qué celdas son fijas y no se pueden modificar.
                data.notesMask,//Las notas/candidatos de cada celda.
                data.hintCells//Las celdas colocadas por pista.
            );
        };
    }
    public void PauseGame()//Esta función pausa el juego.
    {
        sessionContext.GameState = SudokuGameState.Paused;//El estado vive solo en SessionContext (fuente única de verdad).
        timer.StopTimer();//Detiene el timer.
    }
    public void ResumeGame()//Esta función reanuda el juego.
    {
        sessionContext.GameState = SudokuGameState.Playing;//El estado vive solo en SessionContext.
        timer.StartTimer();//Y finalmente vuelve a iniciar el timer:
    }
    public void SetGameState(SudokuGameState newState)//Esta función permite cambiar el estado del juego desde otro script.
    {
        sessionContext.GameState = newState;//Actualiza el estado en SessionContext (fuente única de verdad).
        //No toca el timer. Solo cambia el estado.
    }
}
