using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudokuVictoryPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleText;//Referencia al texto del título del panel.
    //Se usa para mostrar : "¡VICTORIA!"
    [SerializeField] Button returnMenuButton;//Referencia al botón para volver al menú.
    //Se configura en OnEnable.

    void OnEnable()//OnEnable es una función de Unity.
        //Se ejecuta cada vez que el panel se activa con : SetActive(true)
    {
        if (titleText != null)//Si el texto existe
            titleText.text = "¡VICTORIA!";//cambia el título del panel.
        //La intención es mostrar: ¡VICTORIA!
        if (returnMenuButton != null)//Primero revisa si el botón existe.
        {
            returnMenuButton.onClick.RemoveAllListeners();//Elimina eventos anteriores del botón.
            //Esto evita que se acumulen llamadas si el panel se activa varias veces.
            returnMenuButton.onClick.AddListener(() => ReturnToMenu());//Agrega una nueva acción
            //Cuando se presione el botón, llama a ReturnToMenu.
        }
    }
    void ReturnToMenu()
    {
        Debug.Log("Volviendo al menu...");
    }
}
