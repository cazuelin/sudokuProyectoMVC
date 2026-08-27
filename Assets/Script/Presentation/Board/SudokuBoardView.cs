using UnityEngine;
using UnityEngine.UI;

public class SudokuBoardView : MonoBehaviour
//Este script es la vista visual del tablero.
//No decide si un número es correcto, no genera el Sudoku y no maneja la lógica del juego. Su trabajo es:
//Crear las celdas visuales (y las cajas que las contienen) automáticamente y actualizarlas cuando cambian los datos del tablero.
//El prefab SudokuGrid solo aporta la raíz (imagen de fondo + GridLayoutGroup). Las cajas Box0..N y las
//celdas se GENERAN en runtime según la variante activa (SudokuRules), por lo que el mismo prefab sirve
//para 2x3, 3x3, 3x4 y 4x4: solo se le indica la cantidad de filas y columnas del Sudoku (BoxRows/BoxCols).
{
    [SerializeField] GameObject cellPrefab;//Este es el prefab de una celda del Sudoku.
    //Ese prefab debe tener el script: SudokuCell
    //SudokuBoardView lo usa para crear cada una de las celdas: Instantiate(cellPrefab, boxes[boxIndex]);

    [Header("Layout (generado en runtime)")]
    [SerializeField] float boxPadding = 6f;//Margen interno de cada caja (los 4 lados).
    [SerializeField] float cellSpacing = 2f;//Separación entre las celdas dentro de una caja.
    [SerializeField] float gridSpacing = 8f;//Separación entre cajas y margen de la cuadrícula.

    Transform[] boxes;//Este arreglo contiene las cajas del Sudoku generadas en runtime.
    //Cada caja representa un bloque (BoxRows x BoxCols).
    //La cantidad depende de la variante: 9 para 3x3, 6 para 2x3, 12 para 3x4, 16 para 4x4.
    //Se accede así: boxes[boxIndex] donde boxIndex = GetBoxIndex(r, c).
    SudokuCell[,] cells;//Esta matriz guarda las celdas visuales creadas.
    //Se accede así: cells[fila, columna]
    //ejemplo cells[2, 5]  es la celda visual de fila 2, columna 5.
    GridLayoutGroup gridLayout;//El layout de la raíz del prefab (coloca las cajas).
    RectTransform gridRect;//El rect de la raíz (para calcular tamaños).
    bool built;//Indica si el tablero ya fue construido (evita reconstruirlo varias veces).
    int builtSize;//El tamaño con el que se construyó el tablero. Si la variante cambia, se reconstruye.

    int SIZE => SudokuRules.Size;//Tamaño del tablero desde la configuración compartida.
    //Se usa para no escribir 9 manualmente en todos los ciclos.

    void Awake()//Al crear la instancia (desde el Canvas), la cuadrícula se autogenera sola.
    {
        gridRect = (RectTransform)transform;
        ClearBoard();//Limpia cajas/celdas que vinieran de la escena (todo se genera en runtime).
        BuildIfNeeded();
    }

    public void CreateBoard()//Esta función crea visualmente el tablero (cajas + celdas).
    //Es idempotente: si ya fue creado con la MISMA variante, no hace nada. Si la variante cambió
    //(por ejemplo al abrir la escena 2x3 directamente en el editor), destruye el tablero anterior
    //y lo reconstruye con las nuevas filas/columnas. La llaman otros scripts (SudokuGameManager,
    //SudokuGameFlowController) sin riesgo de duplicar las celdas.
    {
        if (built && builtSize == SIZE && cells != null && cells[0, 0] != null)
            return;
        ClearBoard();//Destruye cajas y celdas viejas (si la variante cambió o había restos de escena).
        built = false;
        BuildIfNeeded();
    }

    void BuildIfNeeded()
    {
        if (built)
            return;
        if (cellPrefab == null)
        {
            Debug.LogError("[SudokuBoardView] No hay cellPrefab asignado en el prefab SudokuGrid.");
            return;
        }
        if (gridLayout == null)
            gridLayout = GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
            gridLayout = gameObject.AddComponent<GridLayoutGroup>();

        BuildBoxes();//Genera las cajas según la variante actual (BoxRows x BoxCols cajas).
        BuildCells();//Genera las celdas dentro de cada caja.
        built = true;
        builtSize = SIZE;//Recuerda el tamaño usado para saber si la variante cambió.
        ApplySizes();//Calcula y aplica el tamaño de cajas y celdas según el rect disponible.
    }

    void ClearBoard()//Destruye las cajas y celdas actuales (si existen).
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var old = transform.GetChild(i);
            if (old != null)
                DestroyImmediate(old.gameObject);
        }
        boxes = null;
        cells = null;
    }

    void BuildBoxes()//Crea las cajas del tablero en runtime según la variante activa.
    {
        //Número total de cajas = BoxRows * BoxCols (para 3x3: 3*3 = 9 cajas).
        //Se usa SudokuRules.Size porque el tamaño SIEMPRE es igual a BoxRows * BoxCols.
        int boxCount = SudokuRules.Size;
        boxes = new Transform[boxCount];
        for (int i = 0; i < boxCount; i++)
        {
            //Cada caja es un GameObject con: RectTransform + Image (fondo) + GridLayoutGroup.
            var go = new GameObject($"Box{i}", typeof(RectTransform));
            go.transform.SetParent(transform, false);

            //Fondo de la caja: solo visual (no bloquea clics).
            var img = go.AddComponent<Image>();
            img.raycastTarget = false;
            img.color = new Color(1f, 1f, 1f, 0.35f);

            //Layout interno de la caja: coloca sus celdas en columnas de BoxCols.
            var boxLayout = go.AddComponent<GridLayoutGroup>();
            boxLayout.padding = new RectOffset((int)boxPadding, (int)boxPadding, (int)boxPadding, (int)boxPadding);
            boxLayout.spacing = new Vector2(cellSpacing, cellSpacing);
            boxLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            boxLayout.constraintCount = SudokuRules.BoxCols;
            boxes[i] = go.transform;
        }

        //Layout general de la raíz: coloca las cajas.
        //La cantidad de cajas por fila es BoxRows (para 3x3: 3 cajas por fila; para 2x3: 2).
        gridLayout.enabled = true;
        gridLayout.padding = new RectOffset((int)gridSpacing, (int)gridSpacing, (int)gridSpacing, (int)gridSpacing);
        gridLayout.spacing = new Vector2(gridSpacing, gridSpacing);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = SudokuRules.BoxRows;
    }

    void BuildCells()//Crea una celda por cada posición del tablero dentro de su caja correspondiente.
    {
        cells = new SudokuCell[SIZE, SIZE];
        for (int r = 0; r < SIZE; r++)//Recorre filas
            for (int c = 0; c < SIZE; c++)//recorre columnas
            {
                int boxIndex = SudokuRules.GetBoxIndex(r, c);//Calcula en qué caja debe ir esa celda.
                //Las cajas quedan así (para 3x3):
                //[0][1][2]
                //[3][4][5]
                //[6][7][8]
                var obj = Instantiate(cellPrefab, boxes[boxIndex]);//Crea una copia del prefab de celda.
                var cell = obj.GetComponent<SudokuCell>();//Obtiene el componente SudokuCell del objeto creado.
                cell.row = r;//Guarda en la celda su posición por la fila.
                cell.column = c;//Guarda en la celda su posición por la columna.
                cells[r, c] = cell;//Guarda la celda creada dentro de la matriz cells.
            }
    }

    void ApplySizes()//Calcula y aplica el tamaño de las cajas y de las celdas según el rect de la raíz.
    {
        if (gridRect == null || gridRect.rect.width <= 1f || gridRect.rect.height <= 1f)
            return;//Sin rect válido aún, se reintenta en OnRectTransformDimensionsChange.
        if (boxes == null || boxes.Length == 0)
            return;

        float width = gridRect.rect.width;
        float height = gridRect.rect.height;

        //Cajas por fila = BoxRows; filas de cajas = BoxCols.
        int boxColumns = SudokuRules.BoxRows;
        int boxRows = SudokuRules.BoxCols;

        float paddingX = gridLayout.padding.left + gridLayout.padding.right;
        float paddingY = gridLayout.padding.top + gridLayout.padding.bottom;
        float spacingX = gridLayout.spacing.x * (boxColumns - 1);
        float spacingY = gridLayout.spacing.y * (boxRows - 1);

        float boxWidth = (width - paddingX - spacingX) / boxColumns;
        float boxHeight = (height - paddingY - spacingY) / boxRows;
        if (boxWidth <= 1f || boxHeight <= 1f)
            return;

        gridLayout.cellSize = new Vector2(boxWidth, boxHeight);

        //Tamaño de cada celda dentro de su caja.
        float boxPad = boxPadding * 2f;
        float cellSpacingX = cellSpacing * (SudokuRules.BoxCols - 1);
        float cellSpacingY = cellSpacing * (SudokuRules.BoxRows - 1);
        float cellWidth = Mathf.Max(1f, (boxWidth - boxPad - cellSpacingX) / SudokuRules.BoxCols);
        float cellHeight = Mathf.Max(1f, (boxHeight - boxPad - cellSpacingY) / SudokuRules.BoxRows);

        for (int i = 0; i < boxes.Length; i++)
        {
            if (boxes[i] == null)
                continue;
            var boxLayout = boxes[i].GetComponent<GridLayoutGroup>();
            if (boxLayout != null)
                boxLayout.cellSize = new Vector2(cellWidth, cellHeight);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridRect);
    }

    void OnRectTransformDimensionsChange()
    //Si la pantalla cambia de tamaño (o el rect del Canvas se ajusta), recalcula el layout.
    {
        if (!built || gridRect == null)
            return;
        ApplySizes();
    }

    public void UpdateBoard(int[] values, bool[] fixedCells, int[] notesMask, bool[] hintCells = null)
        //Esta función actualiza las celdas visuales según los datos del tablero.
        //recibe int[] values : que son Los números del tablero.
        //recibe bool[] fixedCells : Qué celdas son fijas.
        //recibe int[] notesMask : que son Las notas/candidatos de cada celda.
        //recibe bool[] hintCells : qué celdas fueron colocadas por pista.
    {
        if (cells == null)
            return;
        for (int i = 0; i < SudokuRules.CellCount; i++)//Recorre las celdas usando índice lineal.
        {
            //Convierte índice lineal a fila y columna.
            int r = SudokuRules.GetRow(i);//r = i / SIZE
            int c = SudokuRules.GetCol(i);//c = i % SIZE
            cells[r, c].Render(//Le pide a esa celda que se dibuje.
                values[i],//El número que debe mostrar.
                fixedCells[i],//Si la celda es fija o editable.
                hintCells != null && i < hintCells.Length && hintCells[i],//Si esta celda fue colocada por una pista.
                notesMask[i]//Las notas que debe mostrar.
            );
        }
    }
    public SudokuCell[,] GetCells() => cells;//Devuelve la matriz de celdas visuales.
    //Se usa, por ejemplo, para inicializar el sistema de resaltado:
    //var cells = boardView.GetCells();
    //highlightSystem.Init(cells, boardController);

    public void SetCellError(int row, int col, bool active, int number = 0)
        //Esta función muestra o limpia error en una celda específica.
    {
        if (cells != null && cells[row, col] != null)
            cells[row, col].SetError(active, number);
    }

    public void ClearAllErrors()//Esta función limpia errores visuales en todas las celdas.
    {
        if (cells == null)
            return;
        for (int r = 0; r < SIZE; r++)//recorre todas las filas
            for (int c = 0; c < SIZE; c++)//recorre todas las columnas
                cells[r, c]?.SetError(false);
    }
}