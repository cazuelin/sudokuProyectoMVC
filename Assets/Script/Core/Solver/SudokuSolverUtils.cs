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
        for (int i = 0; i < 9; i++)//Recorre los 9 posibles candidatos. Pero aquí i va de 0 a 8, porque los bits empiezan en posición 0.
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
        int[] unit = new int[9];//Crea un arreglo de 9 posiciones.
        for (int c = 0; c < 9; c++)//Recorre las columnas de esa fila.
            unit[c] = row * 9 + c;//Guarda cada índice de esa fila.
        //ejemplo si llamas a GetRowUnit(1) = devuelve [9, 10, 11, 12, 13, 14, 15, 16, 17]
        return unit;//retorna todos los valores almacenados en el arreglo
    }

    public static int[] GetColUnit(int col)//Esta función devuelve las 9 posiciones de una columna.
    {
        int[] unit = new int[9];//Crea un arreglo de 9 índices.
        for (int r = 0; r < 9; r++)//Recorre las filas.
            unit[r] = r * 9 + col;//Convierte cada posición de esa columna a índice lineal.
        //ejemplo si llamas a GetColUnit(0) = devuelve [0, 9, 18, 27, 36, 45, 54, 63, 72] Eso representa la primera columna completa.
        return unit;//y retorna todos los valores almacenados en el arreglo
    }

    public static int[] GetBoxUnit(int box)//Esta función devuelve las 9 posiciones de una caja 3x3.
        //Las cajas del Sudoku se pueden numerar así:
        // 0 1 2
        // 3 4 5
        // 6 7 8
    {
        int[] unit = new int[9];//Crea un arreglo para guardar las 9 celdas de la caja.
        int startRow = (box / 3) * 3;//Calcula en qué fila empieza esa caja.
        //ejemplo si box = 4
        // (box / 3) * 3
        // (4 / 3) * 3
        // 1 * 3 = 3
        //entonces la caja 4 empieza en la fila 3
        int startCol = (box % 3) * 3;//Calcula en qué columna empieza. % es módulo, devuelve el resto de una división.
        //ejemplo si box = 4
        //(box % 3) * 3
        //(4 / 3) * 3
        //si 3 cae 1 vez en 4 entonces seria 1 pero es el resto de la division entonces seria 4 - 3 = 1
        //1 * 3 = 3
        //Entonces la caja 4 empieza en la columna 3.
        //La caja 4 es la del centro:
        //filas     3, 4, 5
        //columnas  3, 4, 5
        int k = 0;//k es el índice donde se irá guardando dentro del arreglo unit.
        for (int r = 0; r < 3; r++)//Recorre las 3 filas internas de esa caja.
            for (int c = 0; c < 3; c++)//Recorre las 3 columnas internas de esa caja.
                unit[k++] = (startRow + r) * 9 + (startCol + c);//Esta línea guarda el índice lineal de cada celda.
        //k++ significa:Usa el valor actual de k, y después súmale 1.
        //ejemplo con caja 0 
        //startRow = 0
        //startCol = 0
        //[0, 1, 2, 9, 10, 11, 18, 19, 20]
        //unit[k++] = (startRow + r) * 9 + (startCol + c);
        //1 = (0 + 0) * 9 + (0 + 0); = 0 * 9 + 0 = 0 * 9 = 0 + 0 = 0
        //2 = (0 + 0) * 9 + (0 + 1); = 0 * 9 + 1 = 0 * 9 = 0 + 1 = 1
        //3 = (0 + 0) * 9 + (0 + 2); = 0 * 9 + 2 = 0 * 9 = 0 + 2 = 2

        //4 = (0 + 1) * 9 + (0 + 0); = 1 * 9 + 0 = 1 * 9 = 9 + 0 = 9
        //5 = (0 + 1) * 9 + (0 + 1); = 1 * 9 + 1 = 1 * 9 = 9 + 1 = 10
        //6 = (0 + 1) * 9 + (0 + 2); = 1 * 9 + 1 = 1 * 9 = 9 + 2 = 11

        //7 = (0 + 2) * 9 + (0 + 0); = 2 * 9 + 0 = 2 * 9 = 18 + 0 = 18
        //8 = (0 + 2) * 9 + (0 + 1); = 2 * 9 + 1 = 2 * 9 = 18 + 1 = 19
        //9 = (0 + 2) * 9 + (0 + 2); = 2 * 9 + 2 = 2 * 9 = 18 + 2 = 20
        return unit;//Devuelve las 9 posiciones de esa caja.
    }
}
