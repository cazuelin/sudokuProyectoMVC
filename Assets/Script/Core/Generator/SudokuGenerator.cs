using System.Collections.Generic;
using UnityEngine;


public class SudokuGenerator
{
    const int SIZE = 9;//Define que el Sudoku es 9x9.Se usa para evitar escribir 9 en todas partes.
    const int MAX_GENERATION_ATTEMPTS = 12;//Número máximo de intentos para generar un puzzle con la dificultad pedida.
    SudokuSolver solver = new SudokuSolver();//Crea un solver matemático.Este se usa para contar soluciones y asegurar que el puzzle tenga solución única.
    SudokuDifficultyEvaluatorPro evaluator = new SudokuDifficultyEvaluatorPro();//Crea el evaluador de dificultad.
    //Este revisa qué técnicas usó el solver humano y decide si el puzzle es:Easy, Medium, Hard, Expert, Extreme
    public SudokuBoardData Generate(SudokuGameManager.Difficulty targetDifficulty)//Recibe la dificultad que quiere el jugador.
    {
        var profile = GetProfile(targetDifficulty);//Busca cuántos números debería quitar según la dificultad.
        SudokuBoardData bestData = null;//Guarda el mejor tablero encontrado hasta ahora.
        //Porque puede que en 12 intentos no encuentre exactamente la dificultad pedida. Entonces devuelve el más cercano.
        int bestDistance = int.MaxValue;//Guarda qué tan lejos está la dificultad encontrada de la dificultad pedida.
        for (int attempt = 1; attempt <= MAX_GENERATION_ATTEMPTS; attempt++)//Intenta generar hasta 12 puzzles.
        {
            int targetRemoved = Random.Range(profile.minRemoved, profile.maxRemoved + 1);//Elige aleatoriamente cuántos números quitar dentro del rango de esa dificultad.
            int[,] solution = GenerateSolution();//Genera una solución completa válida.
            int[,] puzzle = BuildUniquePuzzle(solution, targetRemoved);//Quita números de esa solución, pero revisando que el puzzle siga teniendo una única solución.
            SudokuBoardData data = ConvertToBoardData(solution, puzzle);//Convierte la matriz del puzzle al formato que usa el juego.
            bool solved = TryEvaluateDifficulty(puzzle, targetDifficulty, out var realDiff);//Intenta resolver el puzzle con técnicas humanas.
            //solved: si pudo resolverlo usando técnicas -- realDiff: dificultad real detectada -- out var realDiff significa que la función va a llenar esa variable desde dentro.
            data.difficulty = (int)realDiff;//Guarda la dificultad real en el tablero.
            int distance = Mathf.Abs((int)realDiff - (int)targetDifficulty);//Calcula qué tan cerca está la dificultad real de la pedida.
            if (bestData == null || distance < bestDistance)//Si todavía no hay mejor Sudoku, o este está más cerca que el anterior, lo guarda.
            {
                bestData = data;//Actualiza el mejor Sudoku encontrado.
                bestDistance = distance;//Actualiza el mejor Sudoku encontrado.
            }
            if (solved && realDiff == targetDifficulty)//Si el puzzle:fue resuelto por el solver humano y tiene exactamente la dificultad pedida
                return data;//lo devuelve de inmediato.
        }
        return bestData;//Si no encontró una coincidencia exacta después de 12 intentos, devuelve el mejor intento.
    }
    int[,] GenerateSolution()//Esta función crea una solución completa válida.
    {
        int[,] grid = new int[SIZE, SIZE];//Crea una matriz 9x9.
        for (int r = 0; r < SIZE; r++)//Recorre todas las filas
            for (int c = 0; c < SIZE; c++)//Recorre todas las columnas
                grid[r, c] = (r * 3 + r / 3 + c) % SIZE + 1;//Esta es la fórmula base para crear un Sudoku válido.
        //la primera fila la genera del 1 al 9 pero la primera caja igual la ordena del 1 al 9 para poner los numeros siguientes al lado dependiendo de la caja
        //como la fila 2 la caja baja a 4 5 6 entonces la siguiente fila de la siguiente caja pone al 7 8 y 9
        //cada fila avanza 3 posiciones y cada bloque de 3 filas avanza 1 más
        //|1|2|3| |4|5|6| |7|8|9|
        //|4|5|6| |7|8|9| |1|2|3|
        //|7|8|9| |1|2|3| |4|5|6|
        //la caja 2 de la primera fila se le suma el +1 para que no vuelva a repetir la formula y asi empieza desde el 2 y no repite el numero de la columna desde arriba
        //|2|3|4| |5|6|7| |8|9|1|
        //|5|6|7| |8|9|1| |2|3|4|
        //|8|9|1| |2|3|4| |5|6|7|
        //entonces la caja 3 de la primera fila lo mismo ahora empieza del 3 para asi evitar que se repitan numeros y hara el numero entrelazado por la columna
        //|3|4|5| |6|7|8| |9|1|2|
        //|6|7|8| |9|1|2| |3|4|5|
        //|9|1|2| |3|4|5| |6|7|8|
        ShuffleNumbers(grid);//Mezcla el tablero para que no siempre sea igual.
        ShuffleRows(grid);//Mezcla el tablero para que no siempre sea igual por las filas.
        ShuffleColumns(grid); //Mezcla el tablero para que no siempre sea igual por las columnas.
        return grid;//Devuelve la solución completa.
    }

    int[,] BuildUniquePuzzle(int[,] solution, int targetRemoved)//Esta función transforma una solución completa en un puzzle con huecos.Pero solo acepta borrados que mantengan solución única.
    {
        int[,] puzzle = (int[,])solution.Clone();//Copia la solución.Así puede borrar números sin dañar la solución original.
        List<int> order = BuildShuffledPairSeeds();//Crea una lista de posiciones en orden aleatorio.Usa pares simétricos.
        int removed = 0;//Cantidad de celdas borradas hasta ahora.
        for (int i = 0; i < order.Count && removed < targetRemoved; i++)//Recorre la lista mientras:queden posiciones por probar y todavía no se hayan borrado suficientes celdas
        {
            int index = order[i];//Toma una celda candidata.
            int r = index / 9;//Convierte índice a fila.
            //ejemplo si el index es 23 entonces r = 23 / 9 = 2
            int c = index % 9;//Convierte índice a columna.
            //ejemplo si el index es 23 entonces r = 23 / 9 = 2 y el resto es si 9 * 2 = 18 entonces de 23 - 18 = 5
            int r2 = 8 - r;//Calcula la celda espejo. ejemplo si r2 = 8 - r entonces r2 = 8 - 2 = 6
            int c2 = 8 - c;//Calcula la celda espejo. ejemplo si r2 = 8 - c entonces r2 = 8 - 5 = 3
            //entonces la celda espejo del index 23 que es r=2 y c=5 es r=6 y c=3
            if (puzzle[r, c] == 0)//Si esa celda ya estaba borrada, la salta.
                continue;
            int oldA = puzzle[r, c];//Guarda los valores originales.Así puede restaurarlos si el puzzle queda inválido.
            int oldB = puzzle[r2, c2];//Guarda los valores originales.Así puede restaurarlos si el puzzle queda inválido.
            puzzle[r, c] = 0;//Borra la primera celda.
            int removedNow = 1;//Marca que por ahora borró una celda.
            if (r != r2 || c != c2)//Revisa si la celda espejo es distinta.En el centro del tablero, la celda espejo es la misma.
            {
                if (puzzle[r2, c2] == 0)//Si la celda espejo ya estaba vacía, restaura la primera y salta.
                {
                    puzzle[r, c] = oldA;
                    continue;
                }
                puzzle[r2, c2] = 0;//Borra también la celda espejo.
                removedNow = 2;//Ahora se borraron dos celdas.
            }
            if (solver.CountSolutions((int[,])puzzle.Clone()) == 1)//Revisa cuántas soluciones tiene el puzzle después de borrar.
            //Si tiene exactamente una solución, el borrado es válido.Usa una copia para que el solver no modifique el puzzle original.
            {
                removed += removedNow;//Acepta el borrado y aumenta el contador.
            }
            else
            {
                puzzle[r, c] = oldA;//Si el puzzle dejó de tener solución única, revierte el borrado.
                if (r != r2 || c != c2)
                    puzzle[r2, c2] = oldB;
            }
        }
        return puzzle;//Devuelve el puzzle final.
    }

    bool TryEvaluateDifficulty(int[,] puzzle,SudokuGameManager.Difficulty targetDifficulty,out SudokuGameManager.Difficulty realDiff)
        //Esta función intenta resolver el puzzle con técnicas humanas y calcula su dificultad real.
    {
        var ctx = new SudokuContext//Crea un contexto temporal.
        {
            board = (int[,])puzzle.Clone(),//board es una copia del puzzle.
            notesMask = new int[81]//notesMask empieza vacío.
            //Esto es para que el solver humano pueda trabajar sin modificar el puzzle original.
        };
        var humanSolver = new SudokuHumanSolver(targetDifficulty);//Crea un solver humano.Este solver usa técnicas permitidas según dificultad.
        bool solved = humanSolver.Solve(ctx);//Intenta resolver el puzzle.Mientras resuelve, guarda qué técnicas usó.
        realDiff = evaluator.Evaluate(humanSolver.techniquesUsed);//El evaluador mira la lista de técnicas usadas.
        //si usó Hidden Single => Medium
        //si usó Naked Pair => Hard
        //si usó X-Wing => Extreme
        return solved;//Devuelve si el solver humano pudo resolver el puzzle completo.
    }
    List<int> BuildShuffledPairSeeds()//Esta función crea una lista aleatoria de posiciones para intentar borrar celdas.Pero solo guarda una posición por cada par espejo.
    {
        List<int> indices = new List<int>(41);//Crea una lista con capacidad 41.
        //¿Por qué 41? -- Un Sudoku tiene 81 celdas. -- Si borras con simetría: -- 40 pares simétricos + 1 celda central = 41 semillas
        for (int i = 0; i < 81; i++)//Recorre todas las celdas.Para cada celda calcula su espejo:
        {
            int mirror = 80 - i;//ejemplo : 0 espejo 80,1 espejo 79,2 espejo 78 .......40 espejo 40
            if (i <= mirror)//Solo agrega i si: i <= mirror. Eso evita agregar dos veces el mismo par.
                //Ejemplo: agrega 0, pero no agrega 80,agrega 1, pero no agrega 79
                indices.Add(i);//lo agrega a la lista
        }
        for (int i = indices.Count - 1; i > 0; i--)//Mezcla la lista.Esto se llama Fisher-Yates shuffle.
        {
            int j = Random.Range(0, i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
            //Funciona así: empieza desde el final -- elige una posición aleatoria anterior -- intercambia -- repite.Así el orden de borrado es aleatorio.
        }
        return indices;//Devuelve las posiciones mezcladas.
    }
    void ShuffleNumbers(int[,] grid)//Esta función mezcla los números del Sudoku.
    {
        int[] map = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };//Crea un arreglo de números del 1 al 9.Este arreglo será el mapa de reemplazo.

        for (int i = 0; i < SIZE; i++)//Recorre el arreglo map.
        {
            int rand = Random.Range(i, SIZE);//Escoge una posición aleatoria desde i hasta SIZE - 1.Si SIZE = 9, entonces escoge entre:i y 8
            (map[i], map[rand]) = (map[rand], map[i]);//Intercambia dos posiciones del arreglo.ejemplo = map[i] cambia lugar con map[rand]
            //Después de ese for, el mapa podría quedar así: map = { 7, 2, 9, 1, 5, 8, 3, 6, 4 }
            //eso significa que: 1 será 7, 2 será 2, 3 será 9, 4 será 1, 5 será 5, 6 será 8, 7 será 3, 8 será 6, 9 será 4
        }
        for (int r = 0; r < SIZE; r++)//Recorre cada celda del tablero por su fila.
            for (int c = 0; c < SIZE; c++)//Recorre cada celda del tablero por su columna.
                grid[r, c] = map[grid[r, c] - 1];//Reemplaza el número de esa celda usando el mapa.
        //¿Por qué - 1? Porque los números del Sudoku van del 1 al 9, pero los índices del array van del 0 al 8.
        //Ejemplo: grid[r, c] = 3. Entonces:map[3 - 1]  = map[2]
    }

    void ShuffleRows(int[,] grid)//Esta función mezcla filas, pero solo dentro de su mismo bloque de 3 filas.
    //En Sudoku, las filas se agrupan así:
    //Bloque 0: filas 0, 1, 2
    //Bloque 1: filas 3, 4, 5
    //Bloque 2: filas 6, 7, 8
    //En Sudoku, las filas se agrupan así ya que son vistas desde los lados:
        //                        bloque 1   bloque 2    bloque 3
        //                        columna    columna     columna
        //       bloque 0 - filas     0   ||     1    ||     2
        //       bloque 1 - filas     3   ||     4    ||     5
        //       bloque 2 - filas     6   ||     7    ||     8
    {
        for (int block = 0; block < 3; block++)//Recorre los tres bloques de filas.
            SwapRows(grid,block * 3 + Random.Range(0, 3),block * 3 + Random.Range(0, 3));//Calcula una fila aleatoria dentro de ese bloque.
            //Intercambia dos filas del mismo bloque. porque necesita escoger fila origen y fila destino
            //ejemplo SwapRows(grid,block * 3 + Random.Range(0, 3),block * 3 + Random.Range(0, 3));
            //quedaria SwapRows(grid,1(que es el bloque) * 3 + 2(el numero random),2(que es el bloque) * 3 + 1(el numero random));
            //quedaria SwapRows(grid,1 * 3 = 3 +2 =5 ,2 * 3 = 6 + 1 = 7));
            //entonces SwapRows(grid,5 ,7)); que remplazaria el numero 5 de la fila por el 7
    }
    void SwapRows(int[,] grid, int r1, int r2)//Esta función intercambia dos filas completas.recibe el grid => tablero el r1   => primera fila el r2 => segunda fila
    {
        for (int c = 0; c < SIZE; c++)//Recorre todas las columnas de esas filas.
            (grid[r1, c], grid[r2, c]) = (grid[r2, c], grid[r1, c]);//Intercambia los valores de ambas filas columna por columna.
        //ejemplo 
        //fila 0: 1 2 3 4 5 6 7 8 9
        //fila 2: 7 8 9 1 2 3 4 5 6
        //despues del SwapRows(grid, 0, 2):
        //fila 0: 7 8 9 1 2 3 4 5 6
        //fila 2: 1 2 3 4 5 6 7 8 9
        //cambia una fila completa por otra
    }
    void ShuffleColumns(int[,] grid)//Esta función mezcla columnas dentro de cada bloque de 3 columnas.
    {
        //En Sudoku, las columnas se agrupan así:
        //Bloque 0: columnas 0, 3, 6
        //Bloque 1: columnas 1, 4, 7
        //Bloque 2: columnas 2, 5, 8
        //En Sudoku, las columnas se agrupan así ya que son vistas desde arriba:
        //                        bloque 1   bloque 2    bloque 3
        //                        columna    columna     columna
        //       bloque 0 - filas     0   ||     1    ||     2
        //       bloque 1 - filas     3   ||     4    ||     5
        //       bloque 2 - filas     6   ||     7    ||     8
        for (int block = 0; block < 3; block++)//Recorre los tres bloques de columnas.
            SwapColumns(grid,block * 3 + Random.Range(0, 3),block * 3 + Random.Range(0, 3));//Calcula una columna aleatoria dentro del bloque.
            //Intercambia dos columnas del mismo bloque. porque necesita escoger columna origen y columna destino
            //ejemplo SwapColumns(grid,block * 3 + Random.Range(0, 3),block * 3 + Random.Range(0, 3));
            //quedaria SwapColumns(grid,1(que es el bloque) * 3 + 2(el numero random),2(que es el bloque) * 3 + 1(el numero random));
            //quedaria SwapColumns(grid,1 * 3 = 3 +2 =5 ,2 * 3 = 6 + 1 = 7));
            //entonces SwapColumns(grid,5 ,7)); que remplazaria el numero 5 de la columna por el 7
    }
    void SwapColumns(int[,] grid, int c1, int c2)//Esta función intercambia dos columnas completas.recibe el grid => tablero el r1   => primera columnas el r2 => segunda columnas
    {
        for (int r = 0; r < SIZE; r++)//Recorre todas las filas.
            (grid[r, c1], grid[r, c2]) = (grid[r, c2], grid[r, c1]);//Intercambia los valores de ambas columnas fila por fila.
        //ejemplo 
        //columna 0: 1 2 3 4 5 6 7 8 9
        //columna 2: 7 8 9 1 2 3 4 5 6
        //despues del SwapRows(grid, 0, 2):
        //columna 0: 7 8 9 1 2 3 4 5 6
        //columna 2: 1 2 3 4 5 6 7 8 9
        //cambia una columna completa por otra
    }
    SudokuBoardData ConvertToBoardData(int[,] solution, int[,] puzzle)//Esta función convierte el Sudoku desde matriz 9x9 al formato que usa tu juego.
    {
        SudokuBoardData data = new SudokuBoardData();//Crea un nuevo objeto de datos para el tablero.
        for (int r = 0; r < SIZE; r++)//Recorre todas las celdas de la matriz por las filas.
        {
            for (int c = 0; c < SIZE; c++)//Recorre todas las celdas de la matriz por las columnas.
            {
                int index = r * SIZE + c;//Convierte fila y columna a índice.
                //ejemplo si r = 5 y c = 7 y SIZE = 9
                //entonces si index = 5 * 9 + 7 = 5 * 9 = 45 + 7 = 52 que seria el index y hay que hacerlo con todas las celdas
                data.values[index] = puzzle[r, c];//Guarda el valor visible del puzzle.Si esa celda está vacía:puzzle[r, c] = 0 entonces sera data.values[index] = 0
                data.solution[index] = solution[r, c];//Guarda la respuesta correcta para esa celda.
                data.fixedCells[index] = puzzle[r, c] != 0;//Marca como fija toda celda que tiene número inicial.
                //ejemplo si puzzle[r, c] = 0 entonces fixedCells[index] = false
                //ejmeplo si puzzle[r, c] = 7 entonces fixedCells[index] = true
            }
        }
        return data;//Devuelve el tablero en el formato que usa el resto del juego.
    }

    (int minRemoved, int maxRemoved) GetProfile(SudokuGameManager.Difficulty difficulty)//Esta función devuelve el rango de números que se intentarán borrar según la dificultad.
    //(int minRemoved, int maxRemoved) Esto significa que la función devuelve una tupla.Una tupla es devolver más de un valor.En este caso devuelve:minRemoved y maxRemoved
    //ejemplo return (30, 36); significa minRemoved = 30 y maxRemoved = 36
    {
        return difficulty switch//Esto es un switch expression.Sirve para devolver un valor según el caso.
        {
            SudokuGameManager.Difficulty.Easy => (30, 36),
            SudokuGameManager.Difficulty.Medium => (37, 44),
            SudokuGameManager.Difficulty.Hard => (45, 52),
            SudokuGameManager.Difficulty.Expert => (53, 58),
            SudokuGameManager.Difficulty.Extreme => (59, 64),
            _ => (37, 44)//El _ significa: cualquier otro caso. Es el valor por defecto.Si por alguna razón llega una dificultad desconocida, usa rango Medium.
        };
    }
}
