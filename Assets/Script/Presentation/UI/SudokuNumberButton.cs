using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudokuNumberButton : MonoBehaviour//Script del PREFAB de un botón de número del Sudoku.
//NumberPanel instancia este prefab una vez por cada valor del tablero (6, 9, 12 o 16)
//y llama Configure() para asignar el número y la etiqueta. Así no hace falta crear los botones a mano en cada escena.
{
    int number;//Qué número representa este botón (1..MaxValue).
    TMP_Text label;//Texto visual del botón.

    public void Configure(int number, TMP_Text label)//Prepara el botón para representar un número.
    {
        this.number = number;//Guarda el número.
        this.label = label;//Guarda el texto.
        if (label != null)
            label.text = SudokuRules.ValueToLabel(number);//Muestra el número o letra (10=A, 11=B...).
        var button = GetComponent<Button>();//Obtiene el botón del prefab.
        if (button != null)
        {
            button.onClick.RemoveAllListeners();//Limpia eventos anteriores (por si se reutiliza).
            button.onClick.AddListener(Press);//Al presionar, llama a Press.
        }
        gameObject.SetActive(true);//Asegura que el botón esté visible.
    }

    public void Press()//Se llama al presionar el botón. Avisa al NumberPanel global.
    {
        if (NumberPanel.Instance != null)
            NumberPanel.Instance.PressNumber(number);//El panel valida y dispara OnNumberPressed.
    }
}
