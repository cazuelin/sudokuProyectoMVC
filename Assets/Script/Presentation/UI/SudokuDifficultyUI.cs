using TMPro;
using UnityEngine;
public class SudokuDifficultyUI : MonoBehaviour
{
    [SerializeField] TMP_Text difficultyText;//Esta variable guarda la referencia al texto donde se mostrará la dificultad.
    //Se asigna desde el Inspector de Unity.
    //Por ejemplo, puedes tener un texto en pantalla que diga: Easy o Hard
    public void SetDifficulty(SudokuGameManager.Difficulty diff)//Esta es la única función del script.
        //recibe un SudokuGameManager.Difficulty diff : Ese diff es la dificultad actual del juego.
    {
        difficultyText.text = diff.ToString();//Convierte el enum a texto.
        //ejemplo : diff = SudokuGameManager.Difficulty.Hard;
        //entonces diff.ToString()  devuelve "Hard"
        //Y el texto en pantalla queda: difficultyText.text = "Hard";
    }
}