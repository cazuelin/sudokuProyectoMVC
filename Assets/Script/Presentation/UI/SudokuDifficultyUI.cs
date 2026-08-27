using TMPro;
using UnityEngine;
public class SudokuDifficultyUI : MonoBehaviour
{
    [SerializeField] TMP_Text difficultyText;//Esta variable guarda la referencia al texto donde se mostrará la dificultad.
    //Se asigna desde el Inspector de Unity o se busca solo en los prefabs instanciados.
    //Por ejemplo, puedes tener un texto en pantalla que diga: Easy o Hard
    public void Setup(TMP_Text text)//Reasigna el texto de la dificultad. Lo usa SudokuGameUI.
    {
        difficultyText = text;
    }
    public void SetDifficulty(SudokuGameManager.Difficulty diff)//Esta es la única función del script.
        //recibe un SudokuGameManager.Difficulty diff : Ese diff es la dificultad actual del juego.
    {
        if (difficultyText == null)
            EnsureDifficultyText();//Intenta encontrarlo solo en los prefabs instanciados (DatosNivel).
        if (difficultyText == null)//Si todavía no existe, no hace nada.
            return;
        difficultyText.text = diff.ToString();//Convierte el enum a texto.
        //ejemplo : diff = SudokuGameManager.Difficulty.Hard;
        //entonces diff.ToString()  devuelve "Hard"
        //Y el texto en pantalla queda: difficultyText.text = "Hard";
    }
    void EnsureDifficultyText()//Busca el texto de dificultad en los prefabs instanciados (por nombre).
    {
        difficultyText = SudokuSceneRef.FindInScene<TMP_Text>(t =>
        {
            string n = t.gameObject.name.ToLowerInvariant();
            return n.Contains("difficulty") || n.Contains("dificultad");
        });
    }
}
