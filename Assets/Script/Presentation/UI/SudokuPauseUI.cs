using UnityEngine;
public class SudokuPauseUI : MonoBehaviour
{
    [SerializeField] GameObject panel;//Es el panel visual de pausa.
    //Se activa cuando pausas: panel.SetActive(true);
    //Y se oculta cuando reanudas: panel.SetActive(false);
    [SerializeField] GameObject pauseButton;//Es el botón visible para abrir la pausa.
    //Cuando el panel de pausa está abierto, este botón se oculta: pauseButton.SetActive(false);
    //Cuando sales de pausa, vuelve a mostrarse.

    //Estas referencias se usan principalmente en el reinicio manual de respaldo dentro de RestartGame.
    [SerializeField] SudokuBoardController boardController;//Referencia al controlador lógico del tablero.
    //boardController le dice al sistema de resaltado qué números hay en el tablero.
    [SerializeField] SudokuTimer timer;//Referencia al timer del juego. timer controla el tiempo de la partida.
    [SerializeField] SudokuInputController inputController;//Referencia al controlador de entrada del jugador.
    //Sirve para manejar cosas como pistas y posiblemente selección/colocación de números.
    [SerializeField] SudokuBoardView boardView;//Referencia a la vista visual del tablero.
    //boardView se encarga de mostrar visualmente errores o limpiar errores en las celdas.
    [SerializeField] SudokuSessionController sessionController;//Controla guardado y cambio de escena.
    [SerializeField] SudokuGameManager gameManager;//Controla el estado general del juego.
    [SerializeField] SudokuGameFlowController gameFlowController;//Controla flujo de partida.
    ////por ejemplo : crear nueva partida , cargar partida , generar Sudoku , iniciar estado inicial
    public void OpenPause()//Esta función abre el menú de pausa.
    {
        gameManager.PauseGame();//Pone el juego en estado Paused y detiene el timer.
        sessionController?.SaveCurrentSlot();//Guarda la partida actual.
        //El ?. significa: Si sessionController no es null, llama SaveCurrentSlot.
        panel.SetActive(true);//Muestra el panel de pausa.
        if (pauseButton != null)//si existe el boton de pausa
            pauseButton.SetActive(false);//Oculta el botón de pausa para que no quede visible encima del panel.
    }
    public void Resume()//Esta función reanuda la partida.
    {
        panel.SetActive(false);//Primero oculta el panel
        if (pauseButton != null)//si existe el boton de pausa
            pauseButton.SetActive(true);//muestra nuevamente el botón de pausa
        gameManager.ResumeGame();//Cambia el estado a Playing y vuelve a iniciar el timer.
    }
    public void RestartGame()//Esta función reinicia la partida actual.
    {
        if (gameFlowController != null)//Si gameFlowController existe
        {
            gameFlowController.RestartLevel();
            //Esa es la forma preferida, porque SudokuGameFlowController reinicia tablero, errores, pistas, timer y paneles finales.
        }
        else
        {
            //Si gameFlowController no existe, usa un método de respaldo:
            boardController.ResetBoard();//Restaura el tablero inicial.
            var data = boardController.GetBoardData();//Obtiene los datos reiniciados.
            boardView.UpdateBoard(//Actualiza visualmente el tablero.
                data.values,//actualiza los valores
                data.fixedCells,//actualiza las celdas fijas
                data.notesMask//actualiza las notas
            );
            inputController.ClearSelection();//Limpia la celda seleccionada.
            timer.ResetTime();//Reinicia el tiempo
            timer.StartTimer();//empieza denuevo el tiempo
            gameManager.SetGameState(SudokuGameState.Playing);//Vuelve el juego al estado Playing.
        }
        //Después de reiniciar:
        panel.SetActive(false);//Cierra el panel de pausa.
        if (pauseButton != null)//si existe el boton de pausa
            pauseButton.SetActive(true);//Y muestra el botón de pausa
    }
    public void NewGame()//Esta función inicia el flujo para crear una partida nueva.
    {
        panel.SetActive(false);//Primero cierra el panel de pausa.
        if (pauseButton != null)//si existe el boton de pausa
            pauseButton.SetActive(true);//muestra el botón de pausa
        gameFlowController?.OpenNewGamePanel();//Abre el panel de selección de dificultad.
        //No genera directamente el Sudoku. Solo abre la UI para elegir nueva dificultad.
    }
    public void GoToMenu()//Esta función vuelve al menú principal.
    {
        if (sessionController != null)//Primero revisa que exista sessionController.
            sessionController.SaveAndGoToMenu();//Eso guarda la partida actual y carga la escena del menú.
    }
}