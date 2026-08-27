public class SudokuContext//Este script es pequeño, pero es muy importante para las técnicas humanas del solver.
//Su función principal es guardar el contexto actual del Sudoku: el tablero y las notas/candidatos.
//SudokuContext es como una “caja de información” que se le pasa a otros scripts.
//ejemplo las tecnicsa puede recibir un SudokuContext para saber: ¿Qué números hay en el tablero?¿Qué candidatos/notas tiene cada celda?¿Se puede poner este número aquí?
{
    public int[,] board;//Este es el tablero del Sudoku.Es una matriz de dos dimensiones
    public int[] notesMask;//Este arreglo guarda las notas o candidatos de cada celda.notesMask usa un índice lineal de 0 a 80.
    //La conversión es:index = fila * 9 + columna;

    public bool CanPlace(int r, int c, int n)//Esta función revisa si se puede colocar un número n en la posición:
        //r = fila , c = columna , n = numero. CanPlace(2, 5, 7) ejemplo ¿Puedo poner el número 7 en la fila 2, columna 5?
        //si rompe alguna regla del Sudoku.
        //Las reglas son:
        //1 El número no puede repetirse en la misma fila.
        //2 El número no puede repetirse en la misma columna.
        //3 El número no puede repetirse en la misma caja.
    {
        for (int i = 0; i < SudokuRules.Size; i++)//Este for revisa fila y columna al mismo tiempo.
        {
            if (board[r, i] == n) return false;//Primero revisa la fila:Si encuentra el mismo número n, devuelve:return false;
            if (board[i, c] == n) return false;//Luego revisa la columna:Si encuentra el mismo número n, devuelve:return false;
        }
        //Después calcula dónde empieza la caja 3x3:
        //sr significa “start row”, fila inicial de la caja.
        //sc significa “start col”, columna inicial de la caja.
        //ejemplo
        //r = 5
        //c = 7
        int sr = (r / SudokuRules.BoxRows) * SudokuRules.BoxRows;
        int sc = (c / SudokuRules.BoxCols) * SudokuRules.BoxCols;
        for (int rr = 0; rr < SudokuRules.BoxRows; rr++)//primero recorre las filas de la caja
            for (int cc = 0; cc < SudokuRules.BoxCols; cc++)//luego recorre las columnas de la caja
                if (board[sr + rr, sc + cc] == n)
                    return false;//Si encuentra el número n dentro de esa caja, devuelve:return false;
        return true;//Si no encontró el número repetido ni en fila, ni en columna, ni en caja, entonces sí se puede colocar.
    }
}