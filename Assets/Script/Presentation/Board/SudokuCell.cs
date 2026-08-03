using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
public class SudokuCell : MonoBehaviour//Este script representa una celda visual del Sudoku.
//Cada celda del tablero tiene este componente para mostrar número, notas, color, errores y detectar clics.
{
    [Header("Data")]
    public int row;//Guarda la fila de esta celda.
    public int column;//Guarda la columna de esta celda.
    //ejemplo row = 2 y column = 5. significa fila 2, columna 5. Esto sirve para saber qué celda fue presionada.
    [Header("UI")]
    [SerializeField] TMP_Text numberText;//Es el texto principal donde se muestra el número grande de la celda.
    [SerializeField] Image image;//Es la imagen/fondo de la celda. Se usa para cambiar el color de fondo con:image.color = color;
    [SerializeField] GameObject notesGrid;//Es el contenedor visual de las notas pequeñas.
    //Si la celda está vacía y tiene notas, se activa. Si la celda tiene número, se oculta. 
    [SerializeField] TMP_Text[] notes;//Es un arreglo de 9 textos pequeños. Cada posición representa una nota
    //notes[0] -> nota 1
    //notes[1] -> nota 2
    //....
    //notes[8] -> nota 9
    private int currentValue;//Guarda el valor actual de la celda.
    //Ejemplo:
    //0 = vacía
    //5 = tiene un 5
    private bool currentFixed;//Indica si la celda es fija.
    //true significa que era una pista inicial o quedó bloqueada.
    //false significa que el jugador puede modificarla.
    private int currentNotesMask;//Guarda las notas actuales de la celda usando bits.
    //Ejemplo conceptual: 000010101
    //puede representar notas activas como: 1, 3 y 5
    public static Action<SudokuCell> OnCellClicked;//Este es un evento estático. Sirve para avisar a otros scripts: Esta celda fue presionada.
    //Action<SudokuCell> significa que el evento enviará una referencia a la celda clickeada.
    //ejemplo : OnCellClicked?.Invoke(this); seria como llama a esta misma celda.   Al ser static, todas las celdas comparten el mismo evento.
    public void Render(int value, bool isFixed, int notesMask)//Esta función dibuja/actualiza la celda.
        //recibe un int value que es El número de la celda.
        //recibe un isFixed que Indica si la celda es fija.
        //recibie un noesMask que son Las notas de la celda.
    {
        currentValue = value;//guarda el estado del valor actual de la celda
        currentFixed = isFixed;//guarda el estado de si la celda es fija o no con un true o false
        currentNotesMask = notesMask;//guarda el estado de las notas con un mapa de bits
        SetError(false);//Limpia cualquier estado de error visual. Es decir, si antes estaba roja, intenta volverla a su estado normal.
        numberText.text = value == 0 ? "" : value.ToString();//Esto usa un operador ternario: condición ? valorSiTrue : valorSiFalse
        //Si value == 0, muestra texto vacío: ""
        //Si no, muestra el número: value.ToString()
        //ejemplo = value = 5 entonces seria numberText.text = "5"
        numberText.color = isFixed ? Color.black : Color.blue;//Esto usa un operador ternario: condición ? valorSiTrue : valorSiFalse
        //Si la celda es fija, el número será negro.
        //Si no es fija, será azul.
        for (int i = 0; i < 9; i++)//Recorre las 9 notas.
        {
            bool active = (notesMask & (1 << i)) != 0;//Pregunta si la nota está activa.
            //Ejemplo: Si i = 4, está revisando la nota 5:
            //1 << 4   Si ese bit está encendido en notesMask, active será true.
            notes[i].text = active ? (i + 1).ToString() : "";//Si la nota está activa, muestra el número.
            //Si no está activa, deja el texto vacío.
            //ejemplo 
            //i = 4
            //active = true 
            //notes[4].text = "5"
        }
        notesGrid.SetActive(value == 0 && notesMask != 0);//Activa o desactiva el contenedor de notas.
        //Se activa solo si: value == 0   La celda está vacía.
        //y notesMask != 0  Tiene al menos una nota.
        //Si la celda tiene número, no muestra notas.
    }
    public void SetHighlight(Color color)//Esta función cambia el color de fondo de la celda. Recibe un color
    {
        image.color = color;//aplica el color a la image
        //Se usa para resaltar celdas seleccionadas, relacionadas, errores, etc.
    }
    public void OnClick()//Esta función se llama cuando el jugador presiona la celda.
        //Normalmente se conecta al botón de la celda desde Unity.
    {
        OnCellClicked?.Invoke(this);//significa Si hay alguien escuchando este evento, avísale que esta celda fue clickeada.
        //El this significa: esta misma celda. Entonces otro script puede recibir: SudokuCell cell y leer cell.row y cell.column
    }
    public void SetError(bool active, int number = 0)//Esta función activa o desactiva el estado visual de error.
        //recibe un bool active si es true muestra error y si es false restaura la celda normal 
        //recibe un int number = 0 
        //Ese = 0 significa que es opcional.
        //puede llamar a SetError(false); o SetError(true, 8);
    {
        if (active)//Si hay error 
        {
            numberText.text = number.ToString();//muestra el número equivocado
            numberText.color = Color.red;//Lo pinta rojo
            if (notesGrid != null)
                notesGrid.SetActive(false);//Y oculta las notas
        }
        else// Si active es false, restaura la celda.
        {
            if (currentValue == 0)//Primero revisa si la celda estaba vacía
            {
                numberText.text = string.Empty;//Si estaba vacía, deja el texto vacío
            }
            else//Si tenía número
            {
                numberText.text = currentValue.ToString();//Vuelve a mostrar su número real
            }
            numberText.color = currentFixed ? Color.black : Color.blue;//Luego restaura el color. Negro si es fija, azul si es editable
            if (notesGrid != null)
                notesGrid.SetActive(currentValue == 0 && currentNotesMask != 0);//Finalmente restaura las notas.
                //Solo muestra notas si la celda está vacía y tiene notas
        }
    }
}
