using UnityEngine;
public class SudokuBoardView : MonoBehaviour//Este script es la vista visual del tablero.
//No decide si un número es correcto, no genera el Sudoku y no maneja la lógica del juego. Su trabajo es:
//Crear las 81 celdas visuales y actualizarlas cuando cambian los datos del tablero.
{
    [SerializeField] GameObject cellPrefab;//Este es el prefab de una celda del Sudoku.
    //Ese prefab debe tener el script: SudokuCell
    //SudokuBoardView lo usa para crear cada una de las 81 celdas: Instantiate(cellPrefab, boxes[boxIndex]);
    [SerializeField] Transform[] boxes;//Este arreglo contiene las 9 cajas visuales del Sudoku.
    //Cada caja representa un bloque 3x3.
    //Debería tener 9 elementos:
    //boxes[0] boxes[1] boxes[2]
    //boxes[3] boxes[4] boxes[5]
    //boxes[6] boxes[7] boxes[8]
    //Cada celda se instancia dentro de su caja correspondiente.
    SudokuCell[,] cells = new SudokuCell[9, 9];//Esta matriz guarda las 81 celdas visuales creadas.
    //Se accede así: cells[fila, columna]
    //ejemplo cells[2, 5]  es la celda visual de fila 2, columna 5.
    const int SIZE = 9;//Constante del tamaño del tablero. const significa que no cambia.
    //Se usa para no escribir 9 manualmente en todos los ciclos.
    public void CreateBoard()//Esta función crea visualmente el tablero.
    {
        for (int r = 0; r < SIZE; r++)//Recorre filas
            for (int c = 0; c < SIZE; c++)//recorre columnas
                //Eso crea 81 posiciones: 9 filas * 9 columnas = 81 celdas
            {
                int boxIndex = (r / 3) * 3 + (c / 3);//Calcula en qué caja 3x3 debe ir esa celda.
                //ejemplo n° 1
                //r = 0
                //c = 0
                //boxIndex = (0 / 3) * 3 + (0 / 3)
                //boxIndex = 0
                //Celda en caja 0.
                //ejemplo n° 2
                //r = 4
                //c = 5
                //boxIndex = (4 / 3) * 3 + (5 / 3)
                //boxIndex = 1 * 3 + 1
                //boxIndex = 4
                //Celda en caja central.
                //Las cajas quedan así:
                //[0][1][2]
                //[3][4][5]
                //[6][7][8]
                var obj = Instantiate(cellPrefab, boxes[boxIndex]);//Crea una copia del prefab de celda.
                //La crea como hijo de: boxes[boxIndex] Así la celda queda ubicada dentro de su caja visual 3x3.
                var cell = obj.GetComponent<SudokuCell>();//Obtiene el componente SudokuCell del objeto creado.
                //Ese componente controla cómo se muestra una celda individual.
                cell.row = r;//Guarda en la celda su posición por la fila.
                cell.column = c;//Guarda en la celda su posición por la columna.
                //Esto es importante porque cuando haces clic en una celda ,
                //SudokuInputController puede saber: qué fila y columna fueron seleccionadas
                cells[r, c] = cell;//Guarda la celda creada dentro de la matriz cells.
                //Así después se puede actualizar usando: cells[r, c].Render(...)
            }
    }
    public void UpdateBoard(int[] values, bool[] fixedCells, int[] notesMask)
        //Esta función actualiza las 81 celdas visuales según los datos del tablero.
        //recibe int[] values : que son Los números del tablero.
        //recibe bool[] fixedCells : Qué celdas son fijas.
        //recibe int[] notesMask : que son Las notas/candidatos de cada celda.
    {
        for (int i = 0; i < 81; i++)//Recorre las 81 celdas usando índice lineal.
        {
            //Convierte índice lineal a fila y columna.
            //ejemplo:
            //i = 23
            int r = i / SIZE;//r = 23 / 9 = 2
            int c = i % SIZE;//c = 23 % 9 = 5
            //Entonces i = 23 corresponde a: cells[2, 5]
            cells[r, c].Render(//Le pide a esa celda que se dibuje.
                //le pasa los siguientes valores
                values[i],//El número que debe mostrar.
                fixedCells[i],//Si la celda es fija o editable.
                notesMask[i]//Las notas que debe mostrar.
            );
            //en simple UpdateBoard toma los datos del tablero y los manda a cada SudokuCell.
        }
    }
    public SudokuCell[,] GetCells() => cells;//Devuelve la matriz de celdas visuales.
    //Se usa, por ejemplo, para inicializar el sistema de resaltado: 
    //var cells = boardView.GetCells();
    //highlightSystem.Init(cells, boardController);

    public void SetCellError(int row, int col, bool active, int number = 0)
        //Esta función muestra o limpia error en una celda específica.
        //recibe int row : la posicion de la celda en la fila
        //recibe int col : la posicion de la celda en la columna
        //recibe bool active : Si el error se activa o se limpia.
        //recibe int number = 0 : Número opcional que se muestra cuando hay error.
    {
        if (cells[row, col] != null)//Verifica que esa celda exista.
            cells[row, col].SetError(active, number);//Llama a SetError en la celda individual.
        //ejemplo n° 1 : SetCellError(2, 5, true, 8); que es Muestra un 8 rojo en la celda [2,5].
        //ejemplo n° 2 : SetCellError(2, 5, false); que es Limpia el error visual.
    }

    public void ClearAllErrors()//Esta función limpia errores visuales en todas las celdas.
    {
        for (int r = 0; r < SIZE; r++)//recorre todas las filas
            for (int c = 0; c < SIZE; c++)//recorre todas las columnas
                cells[r, c]?.SetError(false);
        //El ?. significa: Si la celda no es null, llama SetError(false).
        //Eso restaura la celda a su estado normal.
    }
}