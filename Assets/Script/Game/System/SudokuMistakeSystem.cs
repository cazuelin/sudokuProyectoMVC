using System;
using UnityEngine;
public class SudokuMistakeSystem : MonoBehaviour
{
    public int maxMistakes = 3;//Cantidad máxima de errores permitidos. Por defecto es: 3
    //Como es public, puedes verlo y modificarlo desde otros scripts o desde el Inspector.
    int currentMistakes;//Cantidad actual de errores cometidos por el jugador.
    //Es privada porque no tiene public. 
    //Ejemplo:
    //currentMistakes = 0
    //currentMistakes = 1
    //currentMistakes = 2
    //currentMistakes = 3
    //Cuando llega a maxMistakes, se dispara la derrota.
    public event Action<int> OnMistakeChanged;//Este evento avisa cuando cambia la cantidad de errores.
    //Envía un int, que es la cantidad actual de errores.
    //Ejemplo: OnMistakeChanged?.Invoke(currentMistakes);
    //Lo escucha SudokuGameFlowController, que actualiza la UI de vidas: livesUI?.UpdateLives(mistakes);
    public event Action OnGameOver;//Este evento avisa cuando el jugador perdió.
    //No envía datos, solo avisa: Se acabó el juego.
    //Lo escucha SudokuGameFlowController: mistakeSystem.OnGameOver += OnGameOver;  Y luego llama a derrota.
    public void Init(int savedMistakes = 0)//Inicializa el contador de errores.
        //Recibe un parámetro opcional: int savedMistakes = 0  Si no le pasas nada, usa 0.
        //Ejemplo: Init();  es igual que: Init(0);
        //Si cargas una partida guardada con 2 errores: Init(2);
    {
        currentMistakes = savedMistakes;//Guarda la cantidad de errores.
        OnMistakeChanged?.Invoke(currentMistakes);//Avisa a la UI que el contador cambió.
        //Así los corazones se actualizan.
    }
    public void RegisterMistake()//Esta función se llama cuando el jugador comete un error.
        //Por ejemplo, desde SudokuInputController : mistakeSystem?.RegisterMistake();
    {
        currentMistakes++;//Suma un error.
        OnMistakeChanged?.Invoke(currentMistakes);//Avisa que cambió el número de errores.
        //Por ejemplo, si ahora hay 2 errores, la UI puede mostrar 1 corazón lleno y 2 vacíos.
        if (currentMistakes >= maxMistakes)//Pregunta si llegó al límite de errores.
            //Ejemplo:
            //currentMistakes = 3
            //maxMistakes = 3
            //La condición es verdadera.
        {
            OnGameOver?.Invoke();//Dispara el evento de derrota
            //Esto avisa al SudokuGameFlowController, que llama a : HandleDefeat();
        }
    }
    public int GetMistakes() => currentMistakes;//Devuelve la cantidad actual de errores.
    //Se usa para guardar la partida: mistakes = mistakeSystem.GetMistakes()
    //y para actualizar UI cuando se carga: livesUI?.UpdateLives(mistakeSystem.GetMistakes());
}