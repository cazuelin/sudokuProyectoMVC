public class HiddenSingleTechnique : ISudokuTechnique//Esta técnica busca un Hidden Single, que significa:
    //Un número puede estar “oculto” entre varios candidatos, pero dentro de una fila, columna o caja solo aparece en una celda posible.
    //Ejemplo: en una fila, varias celdas tienen candidatos,
    //pero el número 7 solo aparece como candidato en una sola celda. Entonces esa celda obligatoriamente debe ser 7.
{
    public string Name => "Hidden Single";//Esta propiedad devuelve el nombre de la técnica.
    public bool TryApply(SudokuContext ctx, out SudokuHint hint)//esta es la funcion principal Revisar el tablero y buscar una celda vacía que tenga exactamente un candidato.
    //SudokuContext ctx : la técnica recibe el estado actual del tablero y los candidatos.
    //out SudokuHint hint : Esto permite devolver información de la jugada encontrada.
    {
        for (int r = 0; r < SudokuRules.Size; r++)//primero revisa las filas
        {
            if (TryInUnit(ctx, SudokuSolverUtils.GetRowUnit(r), out hint))//cada vuelta obtiene un fila completa
                //Ejemplo: GetRowUnit(0) devuelve [0, 1, 2, 3, 4, 5, 6, 7, 8] Eso representa la primera fila.
                return true;//Si encuentra algo, también retorna true.
        }
        for (int c = 0; c < SudokuRules.Size; c++)//despues revisa las columnas
        {
            if (TryInUnit(ctx, SudokuSolverUtils.GetColUnit(c), out hint))//cada vuelta obtine una columna completa
                //GetColUnit(c) devuelve los índices de una columna.
                //Ejemplo: GetColUnit(0) [0, 9, 18, 27, 36, 45, 54, 63, 72]
                return true;//Si encuentra algo, también retorna true.
        }
        for (int box = 0; box < SudokuRules.TotalBoxCount; box++)//despues revisa las cajas
        {
            if (TryInUnit(ctx, SudokuSolverUtils.GetBoxUnit(box), out hint))//cada vuelta obtiene los indexes de la caja
                //Las cajas se numeran así:
                //0 1 2
                //3 4 5
                //6 7 8
                //GetBoxUnit(box) devuelve las 9 celdas de esa caja.
                //Ejemplo: GetBoxUnit(0) devuelve [0, 1, 2, 9, 10, 11, 18, 19, 20]
                return true;//Si encuentra algo, también retorna true.
        }
        //Si termina de revisar todas las celdas y no encuentra ninguna con un solo candidato:
        hint = null;//Eso significa que no hay pista.
        return false;////Esta técnica no se puede aplicar en este tablero.
    }
    bool TryInUnit(SudokuContext ctx, int[] unit, out SudokuHint hint)//Esta es la función auxiliar más importante.
        //SudokuContext ctx: El contexto del Sudoku. Tiene el tablero y las notas
        //int[] unit : Esa unidad puede ser:una fila ,una columna, una caja 3x3
        //Ejemplo si es una fila: [0, 1, 2, 3, 4, 5, 6, 7, 8]
        //Ejemplo si es una columna: [0, 9, 18, 27, 36, 45, 54, 63, 72]
        //Y revisa si dentro de esa unidad algún número del 1 al 9 aparece como candidato en una sola celda.
        //out SudokuHint hint : Si encuentra una jugada, entrega un hint con las celdas destacadas y la acción de borrar notas.
    {
        for (int n = 1; n <= SudokuRules.MaxValue; n++)//Este ciclo prueba cada número del Sudoku.
            //Primero revisa si el 1 aparece una sola vez como candidato en la unidad. luego el 2 ,luego el 3 ,hasta el 9
        {
            int bit = 1 << (n - 1);//Aquí convierte el número n a su bit correspondiente.
            //ejemplo si n = 1
            //int bit = 1 << (n - 1)
            //1 << (1 - 1)
            //1 << 0
            //bit = 000000001
            //ejemplo si n = 5
            //int bit = 1 << (n - 1)
            //1 << (5 - 1)
            //1 << 4
            //bit = 000010000
            //Ese bit se usará para preguntar: ¿Esta celda tiene como candidato el número n?
            int count = 0;//count cuenta cuántas celdas de la unidad tienen ese número como candidato.
            int lastIndex = -1;//guarda la última celda donde apareció ese candidato.
            //Ejemplo: si estamos buscando el número 7 y aparece en la celda 22, entonces:
            //count = 1
            //lastIndex = 22
            //Si aparece en otra celda más, count sube a 2.
            for (int i = 0; i < unit.Length; i++)//Recorre las 9 celdas de la unidad.Como una fila, columna o caja tiene 9 celdas:unit.Length = 9
            {
                int index = unit[i];//Obtiene el índice lineal de la celda.
                //Ejemplo:index = 23 Ese índice representa una celda del tablero.
                int r = SudokuRules.GetRow(index);//Convierte el índice lineal a fila
                int c = SudokuRules.GetCol(index);//Convierte el índice lineal a columna.

                //entonces index 23 = board[2, 5]
                if (ctx.board[r, c] != 0)//Si la celda ya tiene número, la salta.Hidden Single solo analiza celdas vacías.
                    continue;
                int mask = ctx.notesMask[index];//Obtiene las notas/candidatos de esa celda. Por ejemplo, el mask puede representar: 2, 5, 7
                if ((mask & bit) == 0)//Aquí revisa si el candidato n está dentro del mask.
                    //Si el resultado es 0, significa:Esta celda no tiene el número n como candidato. Entonces la salta.
                    //ejemplo Si estamos buscando n = 5, bit representa el 5.
                    //Si la celda tiene candidatos: 2, 7, 9 ,entonces no contiene el 5, y hace continue.
                    continue;
                count++;//si contiene el candidato Aumenta el contador 
                lastIndex = index;//y guarda esa celda.
                //ejemplo 
                //count = 1
                //lastIndex = 23
                if (count > 1)//Si el número aparece en más de una celda de la unidad, ya no puede ser Hidden Single.
                    //Porque Hidden Single necesita que el número aparezca como candidato en una sola celda.
                    break;//Entonces corta el ciclo para ahorrar tiempo.
            }
            if (count == 1)//Si el candidato apareció exactamente una vez, encontró un Hidden Single.
                //ejemplo En una fila, el número 7 solo aparece como candidato en la celda 23. Entonces esa celda debe ser 7.
            {
                hint = new SudokuHint//crea el hint 
                {
                    technique = Name,//el nombre de la tecnica para guardar technique = "Hidden Single"
                    candidateMask = bit//En este caso, candidateMask representa solo el número encontrado.
                };
                hint.highlightCells.Add(lastIndex);//Agrega la celda encontrada para resaltarla visualmente.
                //Si lastIndex = 23, el hint dice: Resalta la celda 23.
                hint.actions.Add(new SudokuAction//Agrega la acción que debe hacerse.
                {
                    type = SudokuActionType.Place,//el tipo de accion que Significa colocar un número.
                    index = lastIndex,//La celda donde se coloca.
                    value = n,//El número que se coloca.
                    technique = Name//La técnica usada.
                    //ejemplo
                    //type = Place
                    //index = 23
                    //value = 7
                    //technique = "Hidden Single"
                });
                return true;//Devuelve true porque encontró una jugada válida.
            }
        }
        //Si prueba todos los números del 1 al 9 y ninguno aparece una sola vez:En esta unidad no hay Hidden Single.
        hint = null;//Eso significa que no hay pista.
        return false;//Esta técnica no se puede aplicar en este tablero.
    }
}
