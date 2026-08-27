using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
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
    [SerializeField] TMP_Text[] notes;//Es un conjunto base de textos pequeños. Se usa como plantilla para las notas.
    //notes[0] -> nota 1
    //notes[1] -> nota 2
    //...
    //La cantidad real se ajusta al tamaño del Sudoku.
    private int currentValue;//Guarda el valor actual de la celda.
    //Ejemplo:
    //0 = vacía
    //5 = tiene un 5
    private bool currentFixed;//Indica si la celda es fija.
    //true significa que era una pista inicial o quedó bloqueada.
    //false significa que el jugador puede modificarla.
    private bool currentHintLocked;//Indica si la celda fue colocada por una pista.
    private int currentNotesMask;//Guarda las notas actuales de la celda usando bits.
    //Ejemplo conceptual: 000010101
    //puede representar notas activas como: 1, 3 y 5
    RectTransform cellRect;
    RectTransform notesGridRect;
    GridLayoutGroup notesLayout;
    readonly List<TMP_Text> runtimeNotes = new List<TMP_Text>();
    bool notesLayoutDirty = true;
    public static Action<SudokuCell> OnCellClicked;//Este es un evento estático. Sirve para avisar a otros scripts: Esta celda fue presionada.
    //Action<SudokuCell> significa que el evento enviará una referencia a la celda clickeada.
    //ejemplo : OnCellClicked?.Invoke(this); seria como llama a esta misma celda.   Al ser static, todas las celdas comparten el mismo evento.
    void Awake()
    {
        CacheLayoutReferences();
        EnsureRuntimeNotes();
        notesLayoutDirty = true;
    }

    void OnEnable()
    {
        notesLayoutDirty = true;
    }

    void OnRectTransformDimensionsChange()
    {
        notesLayoutDirty = true;
    }

    public void Render(int value, bool isFixed, bool isHintLocked, int notesMask)//Esta función dibuja/actualiza la celda.
        //recibe un int value que es El número de la celda.
        //recibe un isFixed que Indica si la celda es fija.
        //recibe un bool isHintLocked que indica si la celda fue colocada por pista.
        //recibie un noesMask que son Las notas de la celda.
    {
        CacheLayoutReferences();
        EnsureRuntimeNotes();
        UpdateNotesLayoutIfNeeded();
        currentValue = value;//guarda el estado del valor actual de la celda
        currentFixed = isFixed;//guarda el estado de si la celda es fija o no con un true o false
        currentHintLocked = isHintLocked;
        currentNotesMask = notesMask;//guarda el estado de las notas con un mapa de bits
        SetError(false);//Limpia cualquier estado de error visual. Es decir, si antes estaba roja, intenta volverla a su estado normal.
        numberText.text = SudokuRules.ValueToLabel(value);//Si value == 0 devuelve "", si 1-9 muestra el número, si >=10 muestra la letra (A,B,C...)
        //ejemplo = value = 5 entonces seria numberText.text = "5"
        //ejemplo = value = 12 entonces seria numberText.text = "C"
        ApplyNumberColor();
        int requiredNotes = Mathf.Max(0, SudokuRules.MaxValue);
        for (int i = 0; i < runtimeNotes.Count; i++)//Recorre las notas visuales del tablero.
        {
            var note = runtimeNotes[i];
            if (note == null)
                continue;

            bool isVisible = i < requiredNotes;
            note.gameObject.SetActive(isVisible);
            if (!isVisible)
                continue;

            bool active = (notesMask & (1 << i)) != 0;//Pregunta si la nota está activa.
            //Ejemplo: Si i = 4, está revisando la nota 5:
            //1 << 4   Si ese bit está encendido en notesMask, active será true.
            note.text = active ? SudokuRules.ValueToLabel(i + 1) : "";//Si la nota está activa, muestra el número o la letra.
            //Si no está activa, deja el texto vacío.
            //ejemplo 
            //i = 4    -> nota 5 -> muestra "5"
            //i = 9    -> nota 10 -> muestra "A"
            //i = 11   -> nota 12 -> muestra "C"
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
            numberText.text = SudokuRules.ValueToLabel(number);//muestra el número equivocado (o letra si >9)
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
                numberText.text = SudokuRules.ValueToLabel(currentValue);//Vuelve a mostrar su número o letra real
            }
            ApplyNumberColor();//Luego restaura el color según si es fija, de pista o editable
            if (notesGrid != null)
                notesGrid.SetActive(currentValue == 0 && currentNotesMask != 0);//Finalmente restaura las notas.
                //Solo muestra notas si la celda está vacía y tiene notas
        }
    }

    void ApplyNumberColor()
    {
        if (currentHintLocked)
        {
            numberText.color = new Color(0.12f, 0.55f, 0.18f);//Verde para distinguir números puestos por pista.
            return;
        }

        numberText.color = currentFixed ? Color.black : Color.blue;//Negro para fijas, azul para editables.
    }

    void CacheLayoutReferences()
    {
        if (cellRect == null)
            cellRect = GetComponent<RectTransform>();
        if (notesGrid != null)
        {
            if (notesGridRect == null)
                notesGridRect = notesGrid.GetComponent<RectTransform>();
            if (notesLayout == null)
                notesLayout = notesGrid.GetComponent<GridLayoutGroup>();
        }
    }

    void EnsureRuntimeNotes()
    {
        if (notesGrid == null || notes == null || notes.Length == 0)
            return;

        int requiredNotes = Mathf.Max(1, SudokuRules.MaxValue);

        if (runtimeNotes.Count == 0)
        {
            foreach (var note in notes)
            {
                if (note != null)
                    runtimeNotes.Add(note);
            }
        }

        TMP_Text template = runtimeNotes.Count > 0 ? runtimeNotes[runtimeNotes.Count - 1] : null;
        if (template == null)
            template = notes[0];

        while (runtimeNotes.Count < requiredNotes && template != null)
        {
            var cloneObj = Instantiate(template.gameObject, notesGrid.transform);
            cloneObj.name = $"Notes{runtimeNotes.Count + 1}";
            var cloneText = cloneObj.GetComponent<TMP_Text>();
            runtimeNotes.Add(cloneText);
        }
    }

    void UpdateNotesLayoutIfNeeded()
    {
        if (!notesLayoutDirty)
            return;
        if (notesGrid == null || notesLayout == null || cellRect == null)
            return;
        if (SudokuRules.BoxRows <= 0 || SudokuRules.BoxCols <= 0)
            return;

        float width = cellRect.rect.width;
        float height = cellRect.rect.height;
        if (width <= 0f || height <= 0f)
            return;

        const int padding = 2;
        if (notesGridRect != null)
        {
            notesGridRect.anchorMin = Vector2.zero;
            notesGridRect.anchorMax = Vector2.one;
            notesGridRect.offsetMin = Vector2.zero;
            notesGridRect.offsetMax = Vector2.zero;
        }
        notesLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        notesLayout.constraintCount = SudokuRules.BoxCols;
        notesLayout.padding = new RectOffset(padding, padding, padding, padding);
        notesLayout.spacing = Vector2.zero;
        notesLayout.childAlignment = TextAnchor.MiddleCenter;

        float innerWidth = Mathf.Max(1f, width - padding * 2f);
        float innerHeight = Mathf.Max(1f, height - padding * 2f);
        float cellWidth = innerWidth / SudokuRules.BoxCols;
        float cellHeight = innerHeight / SudokuRules.BoxRows;
        notesLayout.cellSize = new Vector2(cellWidth, cellHeight);

        float fontSize = Mathf.Max(7f, Mathf.Min(cellWidth, cellHeight) * 0.9f);
        for (int i = 0; i < runtimeNotes.Count; i++)
        {
            var note = runtimeNotes[i];
            if (note == null)
                continue;

            note.enableAutoSizing = true;
            note.fontSize = fontSize;
            note.fontSizeMin = Mathf.Max(5f, fontSize * 0.72f);
            note.fontSizeMax = fontSize;
            note.fontStyle = FontStyles.Bold;
            note.alignment = TextAlignmentOptions.Center;
            note.margin = Vector4.zero;
        }

        notesLayoutDirty = false;
    }
}
