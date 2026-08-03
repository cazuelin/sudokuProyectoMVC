using System;
using UnityEngine;
using UnityEngine.UI;
public class NumberPanel : MonoBehaviour//Este script controla el panel de números del Sudoku,
//o sea los botones del 1 al 9 y la acción de borrar/limpiar. No coloca directamente el número en el tablero; solo avisa: El jugador presionó este número.
{
    public static event Action<int> OnNumberPressed;//Este es un evento estático. Sirve para avisar a otros scripts: Se presionó un número.
    //El int representa el número presionado. 
    //Ejemplo: OnNumberPressed?.Invoke(5);  significa: El jugador presionó el número 5.
    //Si manda 0, significa: El jugador quiere borrar/limpiar la celda.
    //Al ser static, otros scripts pueden escuchar este evento sin tener una referencia directa al objeto NumberPanel.
    public static NumberPanel Instance { get; private set; }//Esto crea una instancia global del NumberPanel. 
    //Permite acceder al panel desde otros scripts así: NumberPanel.Instance
    //get público significa que otros scripts pueden leerlo.
    //private set significa que solo NumberPanel puede asignarlo.
    [SerializeField] Button[] numberButtons;//Este arreglo guarda los botones del 1 al 9. Debe tener 9 botones asignados desde el Inspector.
    //La relación es:
    //numberButtons[0] -> botón 1
    //numberButtons[1] -> botón 2
    //numberButtons[2] -> botón 3
    //.....
    //numberButtons[8] -> botón 9
    //Esto se usa para activar o desactivar botones.
    //Por ejemplo, si ya completaste todos los 5, puedes desactivar el botón 5.
    void Awake()//Awake es una función de Unity. Se ejecuta antes de Start, cuando el objeto se carga.
    {
        Instance = this;//Eso significa: La instancia global de NumberPanel será este objeto.
    }
    public void PressNumber(int number)//Esta función se llama cuando el jugador presiona un botón de número.
    {
        if (number != 0 && !IsNumberButtonInteractable(number))
            //number != 0  valida primero Si el número no es 0
            //number != 0 está porque el 0 se usa para borrar, no para un botón normal del Sudoku.
            //!IsNumberButtonInteractable(number) valida luego su botón no está interactuable
            return;////si ambas no se cumplen no hagas nada y return

        //Si el botón sí se puede usar
        OnNumberPressed?.Invoke(number);//Dispara el evento.
        //El ?. significa: Si hay algún script escuchando este evento, avísale.
        //Ejemplo: PressNumber(7);    termina llamando: OnNumberPressed?.Invoke(7);     En palabras simples: Se presionó el 7.
    }
    public void ClearNumber()//Esta función representa borrar/limpiar número.
    {
        OnNumberPressed?.Invoke(0);//No manda 1 a 9. Manda: 0
        //En el proyecto, 0 suele significar:celda vacía o borrar valor
    }
    public void SetNumberButtonInteractable(int number, bool interactable)//Esta función activa o desactiva un botón de número.
        //Recibe int number que es El número del botón.
        //recibe bool interactable Si es true, el botón se puede presionar. Si es false, el botón queda desactivado.
    {
        var button = GetNumberButton(number);//Busca el botón correspondiente.
        //ejemplo : number = 5 entonces seria numberButtons[4]
        if (button != null)//Si encontró el botón
            button.interactable = interactable;//cambia su estado
        //ejemplo : SetNumberButtonInteractable(5, false);  entonces desactiva el botón 5.
    }
    public bool IsNumberButtonInteractable(int number)//Esta función revisa si un botón se puede usar.
    {
        var button = GetNumberButton(number);//Primero busca el botón
        return button == null || button.interactable;
        // Si no encontró botón, devuelve true.
        //Si encontró botón, devuelve si está interactuable.
        //¿Por qué si button == null devuelve true?
        //Porque así evita bloquear el flujo si el arreglo no está configurado. Es como decir: Si no puedo verificar el botón, no impido la acción.
    }
    Button GetNumberButton(int number)//Esta función busca el botón que corresponde a un número.
    {
        if (number < 1 || number > 9)//Si el número no está entre 1 y 9, .
            return null;//devuelve null
        if (numberButtons == null || numberButtons.Length < 9)//Revisa que el arreglo exista y tenga al menos 9 botones.
            return null;//Si no está bien configurado en el Inspector, devuelve null.
        return numberButtons[number - 1];//Devuelve el botón correspondiente.
        //¿Por qué number - 1?
        //Porque los números del Sudoku van de 1 a 9, pero los arreglos empiezan en 0.
    }
}