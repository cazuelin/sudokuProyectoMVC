public class SudokuSolver
{
    SudokuBitMask bitMask = new SudokuBitMask();//Aquí crea una instancia de SudokuBitMask.
    //SudokuBitMask es como una memoria rápida que sabe qué números ya están usados en cada fila, columna y caja 3x3.
    //En vez de revisar todo el tablero cada vez, el solver pregunta: 
    //bitMask.CanPlace(row, col, num) Eso hace que resolver y contar soluciones sea mucho más rápido.
    public int CountSolutions(int[,] board)//Esta función recibe un tablero:
    //Ese tablero es una matriz 9x9. Los números del 1 al 9 son celdas llenas, y el 0 significa celda vacía.
    {
        bitMask.Init(board);//Eso carga el estado actual del tablero dentro del SudokuBitMask.
        //Por ejemplo, si en la fila 0 ya existe un 5, el bitmask recuerda:En esta fila ya está usado el número 5.
        int count = 0;//Crea un contador de soluciones encontradas.
        SolveCount(board, ref count, 2);//Aquí empieza la búsqueda real.El 2 significa:Deja de buscar cuando encuentres 2 soluciones.
        //¿Por qué 2? Porque para saber si un Sudoku es válido solo importa distinguir entre:
        //0 soluciones = puzzle imposible
        //1 solución = puzzle correcto
        //2 o más soluciones = puzzle ambiguo
        return count;//Devuelve cuántas soluciones encontró, normalmente 0, 1 o 2.
    }
    bool SolveCount(int[,] board, ref int count, int maxSolutions)//Es una función recursiva. Eso significa que se llama a sí misma para ir probando números hasta resolver el tablero.
    //El contador de soluciones. Tiene ref porque la función necesita modificar el valor original, no una copia.
    {
        if (!FindBestEmptyCell(board, out int row, out int col, out int candidateMask))//Aquí busca la mejor celda vacía para intentar resolver.
            //FindBestEmptyCell devuelve varias cosas usando out:
            //row = La fila encontrada.
            //col = La columna encontrada.
            //candidateMask = Los números posibles para esa celda, guardados como bits.
        {
            if (row != -1)//Esto significa que hubo un problema, normalmente un camino muerto.
                return false;// Por ejemplo, una celda vacía no tiene ningún número posible. Entonces este intento no sirve.
            //Si row == -1, significa que no quedan celdas vacías. O sea, el tablero está resuelto.
            count++;//Entonces aumenta el contador de soluciones.
            return count >= maxSolutions;//Si ya encontró suficientes soluciones, devuelve true para cortar la búsqueda.
        }
        int num = 1;//num empieza en 1.
        int mask = candidateMask;//mask guarda los candidatos posibles de la celda.
        while (mask != 0)//Mientras todavía queden candidatos por revisar, sigue.
        {
            if ((mask & 1) != 0)//Esta línea revisa si el bit actual está encendido.¿El número actual es candidato válido?
                //ejemplo Si num = 3, esta condición está revisando si el número 3 puede ir en esa celda.
                //El operador & compara bits. Si el último bit del mask es 1, significa que el número actual está permitido.
            {
                if (bitMask.CanPlace(row, col, num))//Aunque el número venía del candidateMask, vuelve a confirmar si se puede poner.
                    //Esto es una validación extra usando el estado actual del bitMask.
                    //¿Puedo colocar num en esta fila y columna sin romper fila, columna o caja 3x3?
                {
                    board[row, col] = num;//coloca el numero en el tablero real.
                    bitMask.Place(row, col, num);//Actualiza el bitmask para recordar que ese número ahora está usado en esa fila, columna y caja.
                    //Esto es muy importante. Si solo modificaras el tablero pero no el bitmask, CanPlace seguiría trabajando con información vieja.
                    if (SolveCount(board, ref count, maxSolutions))//Después de colocar un número, intenta resolver el resto del tablero.
                        //es como decir Ya puse este número. Ahora intenta completar todo lo demás.
                        //Si esa llamada devuelve true, significa que ya encontró suficientes soluciones.
                    {
                        board[row, col] = 0;//Primero deshace el número colocado:
                        bitMask.Remove(row, col, num);//Y también lo borra del bitmask:
                        return true;//Luego devuelve true para avisar:Ya encontré el límite de soluciones, no sigas buscando.
                    }
                    //Si la recursión no terminó la búsqueda, igual tiene que deshacer el movimiento:
                    board[row, col] = 0;//Primero deshace el número colocado:
                    bitMask.Remove(row, col, num);//Y también lo borra del bitmask:
                }
            }
            ////Esta parte avanza al siguiente candidato.
            mask >>= 1;//Mueve los bits una posición a la derecha.Si antes estaba revisando el candidato del número 1, ahora pasa al número 2.
            num++;//Aumenta el número actual.
            //ejemplo
            //num = 1
            //mask revisa bit del 1
            //num = 2
            //mask revisa bit del 2
            //num = 3
            //mask revisa bit del 3
        }
        return false;//Si probó todos los candidatos y ninguno llevó a una solución suficiente, devuelve false.
    }

    bool FindBestEmptyCell(int[,] board, out int bestRow, out int bestCol, out int candidateMask)//Esta función busca una celda vacía para resolver.
        //Pero no escoge cualquier celda. Escoge la mejor: la que tenga menos candidatos posibles.Eso hace que el solver sea más rápido.
        //ejemplo Si una celda tiene candidatos:1, 2, 3, 4, 5 y otra celda tiene solo el 7. Conviene resolver primero la que solo tiene 7.
    {
        //Valores iniciales.
        bestRow = -1;//Todavía no encontré ninguna celda vacía.
        bestCol = -1;//Todavía no encontré ninguna celda vacía.
        candidateMask = 0;//Todavía no hay candidatos.
        int bestCount = 10;//Como en Sudoku solo puede haber máximo 9 candidatos, pone 10 para que cualquier celda encontrada sea mejor.
        for (int row = 0; row < 9; row++)//Recorre todas las celdas del tablero, fila por fila.
        {
            for (int col = 0; col < 9; col++)//Recorre todas las celdas del tablero columna por columna
            {
                if (board[row, col] != 0)//Si la celda no está vacía, la salta.
                    continue;
                int mask = BuildCandidateMask(row, col);//Construye la lista de números posibles para esa celda.Pero en vez de una lista normal, usa bits.
                int count = SudokuSolverUtils.CountBits(mask);//Cuenta cuántos candidatos tiene esa celda.
                //ejemplo Si el mask representa:2, 5, 9 entonces count sera de 3
                if (count == 0)//Si una celda vacía no tiene candidatos, este camino es imposible.
                    //Una celda vacía no puede usar ningún número del 1 al 9 porque todos rompen fila, columna o caja.
                    //Este intento de solución está muerto.
                {
                    bestRow = -2; //Ese -2 es una señal especial para SolveCount.No significa una fila real. Es una marca interna que dice:Dead-end, camino imposible.
                    return false;//y retorna false
                }
                if (count < bestCount)//Si esta celda tiene menos candidatos que la mejor encontrada hasta ahora, la guarda.
                {
                    //Actualiza la mejor celda.Esta es la celda que conviene resolver ahora.
                    bestCount = count;
                    bestRow = row;//guarda la mejor fila
                    bestCol = col;//guarda la mejor columna
                    candidateMask = mask;//guarda los mejores candidatos

                    if (bestCount == 1)//Si encuentra una celda con un solo candidato, no necesita seguir buscando.Es la mejor opción posible.
                        //Una celda no puede tener menos de 1 candidato válido, porque si tuviera 0 ya habría entrado en el caso de error
                        return true;//Entonces retorna inmediatamente.
                }
            }
        }
        // No hay celdas vacías: tablero resuelto.
        return bestRow != -1;//Al final, si encontró alguna celda vacía, devuelve true.
        //Si no encontró ninguna celda vacía, bestRow sigue siendo -1.El tablero ya está completo.
        //Por eso devuelve false, y SolveCount interpreta ese caso como una solución encontrada.
    }

    int BuildCandidateMask(int row, int col)//Esta función crea una máscara de bits con todos los números posibles para una celda.
    {
        int mask = 0;//Eso significa que todavía no hay ningún candidato.
        for (int num = 1; num <= 9; num++)//Luego revisa números del 1 al 9:
        {
            if (bitMask.CanPlace(row, col, num))//Para cada número pregunta:Si ese número se puede colocar, lo agrega al mask:
                mask |= 1 << (num - 1);
            //Ejemplo con num = 1:
            //1 << (1 - 1)
            //1 << 0
            //Eso en bits sería: 000000001
            //Ejemplo con num = 5:
            //1 << (5 - 1)
            //1 << 4
            //Eso en bits sería: 000010000
            // El operador: |=  significa:Agrega este bit al mask sin borrar los anteriores.
        }
        return mask;//Devuelve todos los candidatos posibles.
    }

}
