public static class SudokuSolverUtils
{
    public static int CountBits(int mask)//Esta función cuenta cuántos bits encendidos tiene un mask.
        //En tu Sudoku, un mask se usa para representar candidatos.
    {
        int count = 0;//Empieza el contador en cero.
        while (mask != 0)//Mientras el mask todavía tenga algún bit encendido, sigue contando.
        {
            mask &= mask - 1;//Esta línea es la más rara, pero es muy útil.
            //Sirve para apagar el bit encendido más a la derecha.
            //ejemplo
            //mask =      0 0 1 0 1 1 0 0
            //mask - 1 =  0 0 1 0 1 0 1 1
            //resultado = 0 0 1 0 1 0 0 0
            //la regla es la siguiente
            // 1 & 1 = 1
            // 1 & 0 = 0
            // 0 & 1 = 0
            // 0 & 0 = 0
            //por eso cuando se ven lo numero del mask y del mask - 1 columna por columna se detallado como funciona la regla
            count++;//Entonces cada vuelta del while apaga un bit y suma:
        }
        return count;//Cuenta cuántos candidatos tiene una celda.Se usa, por ejemplo, en SudokuSolver para elegir la celda con menos candidatos.
    }

    public static int GetSingleValue(int mask)//Esta función recibe un mask y devuelve el primer número encontrado dentro de ese mask.
        //Por ejemplo, si el mask tiene activo solo el candidato 5: 000010000 entonces devuelve 5
    {
        for (int i = 0; i < SudokuRules.MaxValue; i++)//Recorre los posibles candidatos. Aquí i va desde 0, porque los bits empiezan en la posición 0.
            if ((mask & (1 << i)) != 0)//Esto revisa si el bit i está encendido.
                //Ejemplo con i = 4:  1 << 4   Eso crea este bit:  000010000.  Después lo compara con el mask usando &.
                //Si el resultado no es cero, significa: Este candidato existe en el mask.
                return i + 1;//¿Por qué i + 1?  Porque i empieza en 0, pero los números del Sudoku empiezan en 1.  Si i = 4, el número real es:4 + 1 = 5
        return -1;//Si no encuentra ningún bit activo: Ese -1 significa:No había ningún candidato.
    }

    public static int[] GetRowUnit(int row)//Esta función devuelve las 9 posiciones de una fila.Pero no devuelve row, col. Devuelve índices lineales.
        //En tu tablero puedes pensar las celdas así:
        //fila 0:  0  1  2  3  4  5  6  7  8
        //fila 1:  9 10 11 12 13 14 15 16 17
        //fila 2: 18 19 20 21 22 23 24 25 26
        //La fórmula para convertir fila/columna a índice es:
        //index = row * 9 + col;
    {
        int[] unit = new int[SudokuRules.Size];//Crea un arreglo de posiciones.
        for (int c = 0; c < SudokuRules.Size; c++)//Recorre las columnas de esa fila.
            unit[c] = row * SudokuRules.Size + c;//Guarda cada índice de esa fila.
        //ejemplo si llamas a GetRowUnit(1) = devuelve [9, 10, 11, 12, 13, 14, 15, 16, 17]
        return unit;//retorna todos los valores almacenados en el arreglo
    }

    public static int[] GetColUnit(int col)//Esta función devuelve las 9 posiciones de una columna.
    {
        int[] unit = new int[SudokuRules.Size];//Crea un arreglo de índices.
        for (int r = 0; r < SudokuRules.Size; r++)//Recorre las filas.
            unit[r] = r * SudokuRules.Size + col;//Convierte cada posición de esa columna a índice lineal.
        //ejemplo si llamas a GetColUnit(0) = devuelve [0, 9, 18, 27, 36, 45, 54, 63, 72] Eso representa la primera columna completa.
        return unit;//y retorna todos los valores almacenados en el arreglo
    }

    public static int[] GetBoxUnit(int box)
    {
        int[] unit = new int[SudokuRules.Size];
        int startRow = (box / SudokuRules.BoxRows) * SudokuRules.BoxRows;
        int startCol = (box % SudokuRules.BoxRows) * SudokuRules.BoxCols;
        int k = 0;
        for (int r = 0; r < SudokuRules.BoxRows; r++)
            for (int c = 0; c < SudokuRules.BoxCols; c++)
                unit[k++] = (startRow + r) * SudokuRules.Size + (startCol + c);
        return unit;
    }
}
