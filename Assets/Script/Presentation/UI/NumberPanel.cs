using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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
    [Header("Modo Prefab (recomendado)")]
    [SerializeField] GameObject numberButtonPrefab;//PREFAB de UN botón de número.
    //Si está asignado, el panel genera automáticamente un botón por cada valor del Sudoku actual
    //(6 para 2x3, 9 para 3x3, 12 para 3x4, 16 para 4x4) sin tocar las escenas.
    [SerializeField] Transform buttonsContainer;//Dónde se instancian los botones generados.
    //Si es null, se instancian como hijos de este mismo objeto (NumberPanel).
    [Header("Modo Manual (legacy)")]
    [SerializeField] Button[] numberButtons;//Este arreglo guarda los botones del 1 al 9. Debe tener 9 botones asignados desde el Inspector.
    //La relación es:
    //numberButtons[0] -> botón 1
    //numberButtons[1] -> botón 2
    //numberButtons[2] -> botón 3
    //.....
    //numberButtons[8] -> botón 9
    //Esto se usa para activar o desactivar botones.
    //Por ejemplo, si ya completaste todos los 5, puedes desactivar el botón 5.
    [Header("Modo dos filas (para 3x4 y 4x4)")]
    //Cuando el Sudoku tiene MÁS de 9 valores (12 en 3x4 y 16 en 4x4), los botones se dividen en
    //DOS filas: la mitad arriba y la otra mitad abajo, para que no queden amontonados sin espacio.
    [SerializeField] float twoRowPadding = 14f;//Margen exterior de la cuadrícula (top/bottom/left/right): evita que los botones se peguen a los bordes.
    [SerializeField] float twoRowSpacing = 12f;//Separación entre la fila superior y la fila inferior.
    readonly List<Button> generatedButtons = new List<Button>();//Botones creados desde el prefab.
    Transform rowContainerTop;//Contenedor de la fila superior (modo dos filas).
    Transform rowContainerBottom;//Contenedor de la fila inferior (modo dos filas).
    bool UsingPrefab => numberButtonPrefab != null;
    public GameObject NumberButtonPrefab { get => numberButtonPrefab; set => numberButtonPrefab = value; }
    public Transform ButtonsContainer { get => buttonsContainer; set => buttonsContainer = value; }
    public static void SetInstance(NumberPanel panel) => Instance = panel;//Permite al SudokuGameUI reasignar la instancia global.
    void Awake()//Awake es una función de Unity. Se ejecuta antes de Start, cuando el objeto se carga.
    {
        Instance = this;//Eso significa: La instancia global de NumberPanel será este objeto.
    }
    void Start()
    {
        InitButtons();
    }
    void Update()
    {
        HandleKeyboardInput();
    }
    public void InitButtons()
    {
        if (UsingPrefab)//Si hay prefab, genera los botones según el variante actual.
        {
            BuildFromPrefab();
            return;
        }
        if (numberButtons != null)
        {
            for (int i = 0; i < numberButtons.Length; i++)
            {
                var button = numberButtons[i];
                if (button == null)
                    continue;
                bool showButton = i < SudokuRules.MaxValue;
                button.gameObject.SetActive(showButton);
                button.interactable = showButton;
                if (showButton)
                {
                    //Actualiza la etiqueta visual del botón: 1-9 muestran su número, 10+=letra (A,B,C...)
                    var label = button.GetComponentInChildren<TMPro.TMP_Text>();
                    if (label != null)
                        label.text = SudokuRules.ValueToLabel(i + 1);
                }
            }
        }
    }
    void BuildFromPrefab()//Genera UN botón por cada valor del Sudoku actual usando numberButtonPrefab.
    //En 3x4 (12) y 4x4 (16) los botones se dividen en DOS filas: mitad arriba y mitad abajo,
    //con padding y separación, para que no se amontonen en una sola fila sin espacio.
    {
        ClearGenerated();//Borra los botones y filas generados anteriormente (por si cambió la variante).
        Transform parent = buttonsContainer != null ? buttonsContainer : transform;
        int maxValue = SudokuRules.MaxValue;

        bool twoRows = maxValue > 9;//12 (3x4) y 16 (4x4) usan dos filas; 6 (2x3) y 9 (3x3) una sola.
        if (twoRows)
            SetupTwoRowLayout(parent);
        else
            EnsureSingleRowLayout(parent);

        int half = (maxValue + 1) / 2;//Para 12 -> 6 y 6; para 16 -> 8 y 8.
        for (int number = 1; number <= maxValue; number++)
        {
            Transform slot = parent;
            if (twoRows)
                slot = number <= half ? rowContainerTop : rowContainerBottom;

            var obj = Instantiate(numberButtonPrefab, slot);//Copia el prefab.
            obj.name = $"NumberButton_{number}";//Le pone nombre para encontrarlo fácil.
            var button = obj.GetComponent<Button>();//Obtiene el botón (el prefab debería tener uno).
            if (button == null)
                button = obj.AddComponent<Button>();//Si no tiene, se crea uno.
            var numButton = obj.GetComponent<SudokuNumberButton>();//Obtiene el script opcional de botón de número.
            if (numButton == null)
                numButton = obj.AddComponent<SudokuNumberButton>();//Si no tiene, se agrega.
            var label = obj.GetComponentInChildren<TMP_Text>();//Busca el texto del botón.
            numButton.Configure(number, label);//Configura el número y la etiqueta del botón.
            generatedButtons.Add(button);//Lo guarda para poder activarlo/desactivarlo después.
        }
    }

    void SetupTwoRowLayout(Transform parent)
    //Convierte el panel en una cuadrícula vertical de DOS filas (la mitad de botones en cada una).
    {
        //Se quita el layout horizontal de una sola fila: la raíz pasa a apilar las filas en vertical.
        var oldLayout = parent.GetComponent<HorizontalLayoutGroup>();
        if (oldLayout != null)
            DestroyImmediate(oldLayout);
        var vlg = parent.GetComponent<VerticalLayoutGroup>();
        if (vlg == null)
            vlg = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        //Padding exterior: los botones NO se pegan a los bordes superior/inferior del panel.
        vlg.padding = new RectOffset((int)twoRowPadding, (int)twoRowPadding, (int)twoRowPadding, (int)twoRowPadding);
        vlg.spacing = twoRowSpacing;//Separación entre la fila de arriba y la de abajo.
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = true;

        //Cada fila reparte sus botones con un HorizontalLayoutGroup (todos del mismo ancho).
        rowContainerTop = CreateRowContainer("NumberRowTop", parent);
        rowContainerBottom = CreateRowContainer("NumberRowBottom", parent);
    }

    Transform CreateRowContainer(string name, Transform parent)//Crea una fila contenedora de botones.
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var hlg = go.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(0, 0, 0, 0);
        hlg.spacing = 0f;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;
        return go.transform;
    }

    void EnsureSingleRowLayout(Transform parent)
    //Modo normal (2x3 y 3x3): los botones van en UNA sola fila horizontal.
    {
        var vlg = parent.GetComponent<VerticalLayoutGroup>();
        if (vlg != null)
            DestroyImmediate(vlg);
        var hlg = parent.GetComponent<HorizontalLayoutGroup>();
        if (hlg == null)
        {
            hlg = parent.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
        }
    }

    void ClearGenerated()//Borra los botones (y las filas) generados del prefab.
    {
        for (int i = 0; i < generatedButtons.Count; i++)
        {
            if (generatedButtons[i] != null)
                Destroy(generatedButtons[i].gameObject);
        }
        generatedButtons.Clear();
        if (rowContainerTop != null)
        {
            Destroy(rowContainerTop.gameObject);
            rowContainerTop = null;
        }
        if (rowContainerBottom != null)
        {
            Destroy(rowContainerBottom.gameObject);
            rowContainerBottom = null;
        }
    }
    void HandleKeyboardInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.backspaceKey.wasPressedThisFrame || Keyboard.current.deleteKey.wasPressedThisFrame)
        {
            ClearNumber();
            return;
        }

        for (int number = 1; number <= SudokuRules.MaxValue; number++)
        {
            if (IsKeyPressedForNumber(number))
            {
                PressNumber(number);
                return;
            }
        }
    }

    bool IsKeyPressedForNumber(int number)
    {
        if (number >= 1 && number <= 9)
        {
            return number switch
            {
                1 => IsKeyPressed(Key.Digit1) || IsKeyPressed(Key.Numpad1),
                2 => IsKeyPressed(Key.Digit2) || IsKeyPressed(Key.Numpad2),
                3 => IsKeyPressed(Key.Digit3) || IsKeyPressed(Key.Numpad3),
                4 => IsKeyPressed(Key.Digit4) || IsKeyPressed(Key.Numpad4),
                5 => IsKeyPressed(Key.Digit5) || IsKeyPressed(Key.Numpad5),
                6 => IsKeyPressed(Key.Digit6) || IsKeyPressed(Key.Numpad6),
                7 => IsKeyPressed(Key.Digit7) || IsKeyPressed(Key.Numpad7),
                8 => IsKeyPressed(Key.Digit8) || IsKeyPressed(Key.Numpad8),
                9 => IsKeyPressed(Key.Digit9) || IsKeyPressed(Key.Numpad9),
                _ => false
            };
        }

        return number switch
        {
            10 => IsKeyPressed(Key.A),
            11 => IsKeyPressed(Key.B),
            12 => IsKeyPressed(Key.C),
            13 => IsKeyPressed(Key.D),
            14 => IsKeyPressed(Key.E),
            15 => IsKeyPressed(Key.F),
            16 => IsKeyPressed(Key.G),
            _ => false
        };
    }

    bool IsKeyPressed(Key key)
    {
        return Keyboard.current != null && Keyboard.current[key].wasPressedThisFrame;
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
        if (number < 1 || number > SudokuRules.MaxValue)//Si el número no está entre 1 y el máximo del tablero, .
            return null;//devuelve null
        if (UsingPrefab)//Modo prefab: busca en los botones generados.
        {
            if (generatedButtons == null || generatedButtons.Count < SudokuRules.MaxValue)
                return null;
            return generatedButtons[number - 1];
        }
        if (numberButtons == null || numberButtons.Length < SudokuRules.MaxValue)//Revisa que el arreglo exista y tenga al menos el máximo de botones.
            return null;//Si no está bien configurado en el Inspector, devuelve null.
        return numberButtons[number - 1];//Devuelve el botón correspondiente.
        //¿Por qué number - 1?
        //Porque los números del Sudoku van de 1 a 9, pero los arreglos empiezan en 0.
    }
}
