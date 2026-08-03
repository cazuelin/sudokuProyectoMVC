using UnityEngine;
using UnityEngine.UI;
public class SudokuHighlightSystem : MonoBehaviour
{
    SudokuCell[,] board;//Esta variable guarda todas las celdas visuales del tablero.
    //Es una matriz 2D: board[fila, columna]
    //Ejemplo: board[2, 5] representa la celda visual de la fila 2, columna 5.
    SudokuBoardController boardController;//Referencia al controlador lógico del tablero.
    //boardController le dice al sistema de resaltado qué números hay en el tablero.
    [SerializeField] Image notesButtonImage;//Referencia a la imagen del botón de notas.
    //Se usa en: SetNotesMode(bool active) para cambiar el color del botón cuando el modo notas está activo o apagado.
    //notesButtonImage permite cambiar visualmente el botón de notas.
    [Header("Colors")]
    [SerializeField] Color baseColor = Color.white;//Color base de las celdas. Por defecto es blanco. Se usa cuando se limpian los resaltados
    //baseColor es el color normal de una celda sin resaltado.
    [SerializeField] Color highlightColor = new Color(0.8f, 0.9f, 1f);//Color para resaltar la zona relacionada con la celda seleccionada.
    //Se usa para pintar: fila , columna , caja 3x3. Es un color celeste claro.
    //highlightColor pinta el área relacionada con la celda seleccionada.
    [SerializeField] Color selectedColor = Color.yellow;//Color de la celda seleccionada. Por defecto es amarillo.
    //selectedColor marca la celda exacta que tocaste.
    [SerializeField] Color sameNumberColor = new Color(1f, 0.95f, 0.6f);//Color para resaltar los números iguales al seleccionado.
    //Por ejemplo, si seleccionas una celda que tiene un 5, todas las celdas con 5 se pintan con este color.
    //sameNumberColor muestra todos los números iguales al seleccionado.
    [SerializeField] Color conflictColor = new Color(1f, 0.5f, 0.5f);//Color para mostrar conflictos.
    //Por ejemplo, si hay dos números iguales en la misma fila, columna o caja, se pintan con este color. Es un rojo claro.
    //conflictColor marca números repetidos que chocan con la regla del Sudoku.
    [Header("Notes")]
    [SerializeField] Color notesOffColor = Color.white;//Color del botón de notas cuando el modo notas está apagado.
    //notesOffColor es el color del botón cuando no estás escribiendo notas.
    [SerializeField] Color notesOnColor = new Color(1f, 0.9f, 0.3f);//Color del botón de notas cuando el modo notas está activo. 
    //Es parecido a amarillo/naranja claro.
    //notesOnColor indica visualmente que estás en modo notas.
    const int SIZE = 9;//Constante que representa el tamaño del Sudoku.
    public void Init(SudokuCell[,] cells, SudokuBoardController controller)//Esta función inicializa el sistema de resaltado.
        //recibe SudokuCell[,] cells que es La matriz de celdas visuales del tablero.
        //recibe SudokuBoardController controller El controlador lógico del tablero.
        //Init le entrega a SudokuHighlightSystem las celdas que puede pintar y los datos que debe leer.
    {
        board = cells;//Guarda las celdas visuales dentro de la variable interna board.
        boardController = controller;//Guarda la referencia al controlador del tablero.
    }
    public void SelectCell(SudokuCell cell)//Esta función se llama cuando el jugador selecciona una celda
        //recibe SudokuCell cell que es La celda que fue presionada.
    {
        ClearHighlights();//Primero limpia todos los colores anteriores. Esto evita que queden resaltados viejos de otra celda.
        int r = cell.row;//Obtiene la fila de la celda seleccionada.
        int c = cell.column;//Obtiene la columna de la celda seleccionada.
        int index = r * 9 + c;//Convierte fila/columna a índice lineal.
        //Ejemplo:
        //r = 2
        //c = 5
        //index = 2 * 9 + 5
        //index = 23
        int selectedValue = boardController.boardData.values[index];//Obtiene el número que tiene esa celda.
        HighlightArea(r, c);//Resalta la zona relacionada con esa celda: fila , columna , caja 3x3
        if (selectedValue != 0)//Pregunta si la celda tiene número.
            //Si está vacía (0), no resalta números iguales ni conflictos.
        {
            //Si tiene número, entra.
            HighlightSameNumbers(selectedValue);//Resalta todas las celdas que tienen el mismo número.
            //ejemplo Si seleccionaste un 7, pinta todos los 7 del tablero.
            HighlightConflicts(r, c, selectedValue);//Busca conflictos con ese número en fila, columna y caja.
            //ejemplo Si seleccionaste un 7 y hay otro 7 en la misma fila, lo marca con color de conflicto.
        }
        cell.SetHighlight(selectedColor);//Al final pinta la celda seleccionada con el color de selección.
        //Esto se hace al final para que el color seleccionado quede por encima de otros resaltados.
    }
    void ClearHighlights()//Esta función limpia todos los resaltados del tablero.
    {
        ForEachCell((r, c) =>//Aquí usa una función auxiliar llamada ForEachCell.
        //Le pasa una acción: Para cada celda del tablero, ponle el color base.
        {
            board[r, c].SetHighlight(baseColor);//Deja todas las celdas como normales.
        });
    }
    void HighlightArea(int row, int col)//Esta función resalta la fila, columna y caja 3x3 de una celda.
        //recibe int row que es la fila seleccionada
        //recibe int col que es la columna seleccionada
    {
        for (int i = 0; i < SIZE; i++)//recorre las filas y columnas
        {
            board[row, i].SetHighlight(highlightColor);//pinta toda la fila.
            //Ejemplo si row = 2:
            //board[2,0]
            //board[2,1]
            //.....
            //board[2,8]
            board[i, col].SetHighlight(highlightColor);//pinta toda la columna.
            //Ejemplo si col = 5:
            //board[0,5]
            //board[1,5]
            //.....
            //board[8,5]
        }
        //Luego calcula dónde empieza la caja 3x3:
        //ejemplo
        //row = 5
        //col = 7
        int startRow = (row / 3) * 3;//startRow = (5 / 3) * 3 = 1 * 3 = 3
        int startCol = (col / 3) * 3;//startCol = (7 / 3) * 3 = 2 * 3 = 6
        //la caja empieza en fila 3, columna 6
        for (int r = 0; r < 3; r++)//Recorre las 3 filas internas de la caja.
            for (int c = 0; c < 3; c++)//Recorre las 3 columnas internas de la caja.
                board[startRow + r, startCol + c].SetHighlight(highlightColor);//pinta toda la caja 3x3
        //Ejemplo con startRow = 3, startCol = 6:
        //board[3,6] board[3,7] board[3,8]
        //board[4,6] board[4,7] board[4,8]
        //board[5,6] board[5,7] board[5,8]
        //en simple HighlightArea pinta la zona lógica relacionada con la celda seleccionada.
    }
    void HighlightSameNumbers(int number)//Esta función resalta todos los números iguales al seleccionado.
        //recibe un int number que es El número que se quiere buscar.
    {
        var data = boardController.boardData;//Obtiene los datos actuales del tablero.
        for (int i = 0; i < 81; i++)//Recorre las 81 celdas del Sudoku.
        {
            if (data.values[i] == number)//pregunta ¿Esta celda tiene el número que estoy buscando?
            {
                //sí, entra aqui entonces lo encontro
                //Convierte índice lineal a fila y columna.
                //si i = 23
                int r = i / 9;//r = 23 / 9 = 2
                int c = i % 9;//c = 23 % 9 = 5
                //pinta la celda ubicada en la fila 2 y columna 5
                board[r, c].SetHighlight(sameNumberColor);//Pinta esa celda con el color de números iguales.
            }
        }
    }
    void HighlightConflicts(int row, int col, int number)//Esta función busca si el number seleccionado está repetido en su fila, columna o caja 3x3.
        //recibe int row : Fila de la celda seleccionada.
        //recibe int col : Columna de la celda seleccionada.
        //recibe int number : Número que se quiere revisar.
    {
        var data = boardController.boardData;//Obtiene los datos actuales del tablero.
        //primero revisa la fila
        for (int c = 0; c < 9; c++)//Recorre todas las columnas de esa misma fila.
        {
            if (c == col) continue;//Salta la celda seleccionada, porque no quieres compararla consigo misma.
            //Convierte fila/columna a índice lineal.
            //ejemplo
            ////row = 5
            ////col = 7
            int index = row * 9 + c;//int index = 5 * 9 + 7 = (5 * 9) + 7 = 45 + 7 = 52
            if (data.values[index] == number)//Si encuentra el mismo número en esa fila
                board[row, c].SetHighlight(conflictColor);//lo marca como conflico y lo resalta 
        }
        //Después revisa la columna
        for (int r = 0; r < 9; r++)//Aquí recorre todas las filas de la misma columna
        {
            if (r == row) continue;////Salta la celda seleccionada, porque no quieres compararla consigo misma.
            //Convierte fila/columna a índice lineal.
            //ejemplo
            ////row = 5
            ////col = 7
            int index = r * 9 + col;//int index = 5 * 9 + 7 = (5 * 9) + 7 = 45 + 7 = 52
            if (data.values[index] == number)//Si encuentra el mismo número en esa columna
                board[r, col].SetHighlight(conflictColor);//lo marca como conflico y lo resalta 
        }
        //Luego revisa la caja 3x3
        //primero Calcula dónde empieza la caja
        //ejemplo
        //row = 5
        //col = 7
        int startRow = (row / 3) * 3;//int startRow = (5 / 3) * 3 = 1 * 3 = 3
        int startCol = (col / 3) * 3;//int startCol = (7 / 3) * 3 = 2 * 3 = 6
        //entonces la caja empieza en la fila 3 columna 6

        //luego Recorre las 9 celdas de esa caja
        for (int r = 0; r < 3; r++)//revisa las 3 filas de la caja 3x3
            for (int c = 0; c < 3; c++)//revisa las 3 columnas de la caja 3x3
            {
                //Convierte posición interna de caja a fila/columna real
                //ejemplo
                //row = 5
                //col = 7
                //startRow = 3
                //startCol = 6
                int rr = startRow + r;
                //int rr = 3 + 0 = 3
                //int rr = 3 + 1 = 4
                //int rr = 3 + 2 = 5
                int cc = startCol + c;
                //int cc = 6 + 0 = 6
                //int cc = 6 + 1 = 7
                //int cc = 6 + 2 = 8
                if (rr == row && cc == col) continue;//Salta la celda seleccionada para no compararla consigo misma
                //Convierte fila/columna a índice lineal
                int index = rr * 9 + cc;
                //int index = 3 * 9 + 6 = (3 * 9) + 6 = 24 + 6 = 30
                //int index = 3 * 9 + 7 = (3 * 9) + 7 = 24 + 6 = 31
                //int index = 3 * 9 + 8 = (3 * 9) + 8 = 24 + 6 = 32

                //int index = 4 * 9 + 6 = (4 * 9) + 6 = 24 + 6 = 42
                //int index = 4 * 9 + 7 = (4 * 9) + 7 = 24 + 6 = 43
                //int index = 4 * 9 + 8 = (4 * 9) + 8 = 24 + 6 = 44

                //int index = 5 * 9 + 6 = (5 * 9) + 6 = 24 + 6 = 51
                //int index = 5 * 9 + 7 = (5 * 9) + 7 = 24 + 6 = 52
                //int index = 5 * 9 + 8 = (5 * 9) + 8 = 24 + 6 = 53
                if (data.values[index] == number)//Si encuentra el mismo número dentro de la caja
                    board[rr, cc].SetHighlight(conflictColor);//lo pinta como conflicto
            }
    }
    void ForEachCell(System.Action<int, int> action)//Esta función es una utilidad para recorrer todas las celdas del tablero.
        //recibe System.Action<int, int> action : que es Una función que recibe dos enteros: fila y columna.
    {
        for (int r = 0; r < SIZE; r++)//recorre las filas 
            for (int c = 0; c < SIZE; c++)//recorre las columnas
                action(r, c);//Ejecuta la acción recibida usando esa fila y columna.
    }
    public void SetNotesMode(bool active)//Esta función cambia el color del botón de notas.
        //recibe un bool active 
        //Si active es true, el modo notas está encendido.
        //Si es false, está apagado.
    {
        if (notesButtonImage != null)//Si la imagen del botón existe, puede cambiarle color.
            notesButtonImage.color = active ? notesOnColor : notesOffColor;//Esto usa operador ternario: condicion ? valorSiTrue : valorSiFalse
        //Si active == true, usa: notesOnColor
        //Si active == false, usa: notesOffColor
        //en simple Si activas notas, el botón cambia a color encendido. Si desactivas notas, vuelve a color normal.
    }
    public void ShowHint(SudokuHint hint)//Esta función muestra visualmente una pista.
        //recibe SudokuHint hint Ese hint contiene listas como: highlightCells , affectedCells
    {
        ClearHighlights();//Limpia los colores anteriores.
        foreach (var index in hint.highlightCells)//Recorre las celdas principales de la pista.
            //Por ejemplo, en una técnica podrían ser las celdas que explican el patrón.
        {
            HighlightCell(index, Color.yellow);//Las pinta en amarillo.
        }
        foreach (var index in hint.affectedCells)//Recorre las celdas afectadas.
            //Por ejemplo, celdas donde se eliminarán candidatos.
        {
            HighlightCell(index, Color.red);//Las pinta en rojo.
            //en simple ShowHint limpia el tablero y pinta en amarillo lo importante de la pista y en rojo lo afectado.
        }
    }
    void HighlightCell(int index, Color color)//Esta función pinta una sola celda usando un índice lineal.
        //recibe un int index : Índice de celda de 0 a 80.
        //recibe Color color : Color que se quiere aplicar.
    {
        //si index = 23
        //Primero convierte el índice a fila
        int r = index / 9;//r = 23 / 9 = 2
        //Luego convierte índice a columna
        int c = index % 9;//c = 23 % 9 = 5
        board[r, c].SetHighlight(color);//entonces pista seria board[2, 5].SetHighlight(color);
        //en simple HighlightCell recibe una celda como número del 0 al 80, calcula su fila/columna y la pinta.
    }
}
