using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudokuDefeatPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleText;//Referencia al texto del título del panel.
    //Se usa para mostrar: GAME OVER
    [SerializeField] Button restartButton;//Referencia al botón de reiniciar.
    //Cuando se presiona, llama: OnRestartClicked().  y ese método reinicia el nivel actual.
    [SerializeField] Button newGameButton;//Referencia al botón de nueva partida.
    //Cuando se presiona, llama: OnNewGameClicked().  y ese método abre el panel para elegir dificultad.
    [SerializeField] SudokuGameFlowController gameFlowController;//Referencia al controlador del flujo del juego.
    //Este script no reinicia ni crea partida por sí mismo. Le pide al gameFlowController que lo haga.
    //usa : gameFlowController.RestartLevel();   y    gameFlowController.OpenNewGamePanel();

    void OnEnable()//OnEnable es una función de Unity. Se ejecuta cada vez que el GameObject se activa.
        //Como el panel de derrota aparece con: defeatPanel.SetActive(true);
        //entonces Unity llama automáticamente a OnEnable.
    {
        if (titleText != null)//Si existe el texto, le pone: GAME OVER
            titleText.text = "GAME OVER";//Esto asegura que cada vez que el panel aparezca, tenga el título correcto.

        //Luego configura el botón de reinicio:
        if (restartButton != null)//Primero revisa que el botón exista.
        {
            restartButton.onClick.RemoveAllListeners();//Borra todos los eventos anteriores del botón.
            //Esto evita que el botón llame la misma función varias veces si el panel se activa más de una vez.
            restartButton.onClick.AddListener(() => OnRestartClicked());//Agrega una nueva acción al botón.
            //Cuando presiones este botón, ejecuta OnRestartClicked().
        }

        //Después configura el botón de nueva partida:
        if (newGameButton != null)////Primero revisa que el botón exista.
        {
            newGameButton.onClick.RemoveAllListeners();////Borra todos los eventos anteriores del botón.
            ////Esto evita que el botón llame la misma función varias veces si el panel se activa más de una vez.
            newGameButton.onClick.AddListener(() => OnNewGameClicked());////Agrega una nueva acción al botón.
            ////Cuando presiones este botón, ejecuta OnNewGameClicked().
        }
    }
    void OnRestartClicked()//Esta función se ejecuta cuando el jugador presiona el botón de reiniciar.
    {
        if (gameFlowController != null)//Si existe el controlador de flujo
            gameFlowController.RestartLevel();//RestartLevel reinicia el mismo Sudoku desde el estado inicial.
        //en simple Vuelve a empezar la misma partida.
    }
    void OnNewGameClicked()//Esta función se ejecuta cuando el jugador presiona el botón de nueva partida.
    {
        if (gameFlowController != null)//Primero revisa que exista gameFlowController.
            gameFlowController.OpenNewGamePanel();//Ese método abre el panel de selección de dificultad.
        //en simple No reinicia el mismo Sudoku; permite crear otro nuevo.
    }
}
