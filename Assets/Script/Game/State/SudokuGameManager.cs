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
    //por ejemplo : colocar número , validar movimiento , hacer undo , manejar notas , guardar boardData , notificar cambios.
    //Es una de las referencias más importantes.
    [SerializeField] SudokuGameFlowController flowController;//Maneja el flujo de la partida.
    //por ejemplo : crear nueva partida , cargar partida , generar Sudoku , iniciar estado inicial
    [SerializeField] SudokuHighlightSystem highlightSystem;//Maneja el resaltado visual del tablero.
    //por ejemplo : resaltar celda seleccionada , resaltar iguales , resaltar fila/columna/caja
    public SudokuGameState gameState;//Guarda el estado actual del juego.
    //puedo ser algo como : Generating , Playing , Paused , Completed
    public enum Difficulty//Aquí defines las dificultades disponibles. Sirve para conectar la dificultad elegida con generación y técnicas.
    {
        Easy,
        Medium,
        Hard,
        Expert,
        Extreme
    }
    public Difficulty difficulty;//Guarda la dificultad actual de esta partida. 
    //En Start se carga desde : difficulty = sessionContext.SelectedDifficulty;
    void Start()
    {
        if (sessionContext == null)//Primero revisa que sessionContext esté asignado.
        {
            Debug.LogError("SessionContext no asignado en SudokuGameManager");//Si no está asignado, muestra error en consola
            return;//y detiene la función. Esto evita que el juego siga y falle con errores más confusos.
        }
        gameState = SudokuGameState.Generating;//Pone el juego en estado:Generating. El Sudoku se está preparando/generando.
        sessionContext.GameState = gameState;//Luego copia ese estado al sessionContext.
        //Así otros scripts que miran la sesión también saben que el juego está generando.
        boardView.CreateBoard();//Crea visualmente el tablero. Normalmente aquí se instancian o preparan las 81 celdas del Sudoku.
        var cells = boardView.GetCells();//Obtiene las celdas creadas por la vista.
        //var significa que C# deduce el tipo automáticamente.
        //cells es una colección de celdas visuales, como SudokuCell[] o List<SudokuCell>.
        highlightSystem.Init(cells, boardController);//Inicializa el sistema de resaltado.
        //le pasa cells que son Las celdas visuales que puede pintar/resaltar.
        //le pasa boardController que es La lógica del tablero, para saber datos actuales.
        difficulty = sessionContext.SelectedDifficulty;//Toma la dificultad elegida desde sessionContext.
        //por ejemplo : sessionContext.SelectedDifficulty = Hard 
        //entonces seria difficulty = Hard
        //eso significa Esta partida se jugará con la dificultad seleccionada antes de entrar a la escena.
        flowController.Initialize();//Inicializa el flujo de juego.
        //Aquí se decide si: crear partida nueva , cargar partida guardada , generar puzzle , preparar tablero inicial , iniciar timer
        //El GameManager no hace eso directamente. Se lo delega a SudokuGameFlowController.
        gameState = sessionContext.GameState;//Después de inicializar el flujo, vuelve a leer el estado desde sessionContext.
        //¿Por qué? Porque flowController.Initialize() puede haber cambiado el estado.
        //Por ejemplo, pudo pasar de: Generating a Playing. Entonces el GameManager sincroniza su variable local:gameState con la sesion
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
                data.notesMask//Las notas/candidatos de cada celda.
            );
        };
    }
    public void PauseGame()//Esta función pausa el juego.
    {
        gameState = SudokuGameState.Paused;//Cambia el estado local a pausado.
        sessionContext.GameState = gameState;//También actualiza el estado de la sesión.
        timer.StopTimer();//Detiene el timer.
    }
    public void ResumeGame()//Esta función reanuda el juego.
    {
        gameState = SudokuGameState.Playing;//Cambia el estado local a Playing.
        sessionContext.GameState = gameState;//También actualiza el estado de la sesión.
        timer.StartTimer();//Y finalmente vuelve a iniciar el timer:
    }
    public void SetGameState(SudokuGameState newState)//Esta función permite cambiar el estado del juego desde otro script.
    {
        gameState = newState;//Actualiza el estado local del GameManager.
        sessionContext.GameState = newState;//Actualiza también el estado guardado en la sesión.
        //No toca el timer. Solo cambia el estado.
    }
}
