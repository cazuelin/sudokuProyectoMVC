using System.Collections.Generic;
public class XWingTechnique : ISudokuTechnique//XWingTechnique busca la técnica XWing.
//Si un candidato aparece exactamente en las mismas 2 columnas dentro de 2 filas diferentes,
//entonces ese candidato queda “encerrado” en esas 4 esquinas. Por eso se puede eliminar ese candidato de las demás celdas de esas columnas.
//Ejemplo visual con candidato 5:
//fila 1: . . 5 . . . 5 . .
//fila 6: . . 5 . . . 5 . .
//El 5 en esas dos filas solo puede estar en columna 2 o columna 6.
//Entonces se forma un rectángulo:
//[fila 1, col 2]    [fila 1, col 6]
//[fila 6, col 2]    [fila 6, col 6]
//Por eso puedes borrar el candidato 5 de las demás celdas de las columnas 2 y 6.
{
    public string Name => "X-Wing";//Devuelve el nombre de la técnica.
    public bool TryApply(SudokuContext ctx, out SudokuHint hint)//Esta es la función que llama el SudokuSolverEngine.
        //su funcion es Intentar aplicar X-Wing para algún número del 1 al 9.
        //Recibe: SudokuContext ctx El estado actual del Sudoku
        //Devuelve por out: SudokuHint hint Si encuentra un X-Wing, aquí coloca la pista.
    {
        for (int num = 1; num <= 9; num++)//Recorre los números del Sudoku 1, 2, 3, 4, 5, 6, 7, 8, 9
            //X-Wing se busca candidato por candidato.
            //Primero pregunta:
            //¿Hay X-Wing con el número 1?
            //luego 
            //¿Hay X-Wing con el número 2?
            //Y así hasta el 9.
        {
            int mask = 1 << (num - 1);//Convierte el número actual a una máscara de bits.
            //num = 1
            //mask = 1 << 0
            //Representa el candidato 1.
            //num = 5
            //mask = 1 << 4
            //Representa el candidato 5.
            if (TryRowBased(ctx, mask, out hint))//Primero intenta buscar X-Wing basado en filas.
                //Esto significa: Busca dos filas donde el candidato aparezca exactamente en las mismas dos columnas.
                return true;//Si TryRowBased encuentra algo, devuelve true.
            //Entonces TryApply también devuelve true inmediatamente.
            if (TryColBased(ctx, mask, out hint))//Si no encontró X-Wing por filas, intenta por columnas.
                //Esto significa: Busca dos columnas donde el candidato aparezca exactamente en las mismas dos filas.
                return true;//Si TryColBased encuentra algo, devuelve true.
            //Entonces TryApply también devuelve true inmediatamente.
        }
        //Si termina de revisar todas las celdas y no encuentra ninguna con un solo candidato:
        hint = null;//Eso significa que no hay pista.
        return false;////Esta técnica no se puede aplicar en este tablero.
    }
    bool TryRowBased(SudokuContext ctx, int mask, out SudokuHint hint)//Esta función busca X-Wing por filas.
        //SudokuContext ctx : que es El tablero y las notas.
        //int mask : que es El candidato que se está buscando.
        //Ejemplo: si mask representa el 5, esta función busca un X-Wing del número 5.
        //out SudokuHint hint : La pista que devuelve si encuentra algo.
    {
        var rowCols = new List<(int row, int c1, int c2)>();//Aquí crea una lista de tuplas.
        //Cada elemento guarda: row , c1 , c2
        //O sea : En esta fila, el candidato aparece exactamente en estas dos columnas.
        //ejemplo (row: 1, c1: 2, c2: 6)
        //Significa: En la fila 1, este candidato aparece solo en las columnas 2 y 6.
        for (int row = 0; row < 9; row++)//Luego recorre todas las filas
        {
            var cols = GetCandidateColsInRow(ctx, row, mask);//Esta función auxiliar devuelve las columnas de esa fila donde aparece el candidato.
            //Ejemplo : Si está buscando candidato 5, y en la fila 3 el 5 aparece como nota en columnas 1 y 7: cols = [1, 7]
            if (cols.Count == 2)//Solo le interesan filas donde el candidato aparece exactamente en 2 columnas.
                //¿Por qué? Porque X-Wing necesita dos filas donde el candidato esté limitado a las mismas dos columnas.
                //Si aparece en 1 columna, sería otro tipo de técnica.
                //Si aparece en 3 o más columnas, no sirve para X-Wing.
                rowCols.Add((row, cols[0], cols[1]));//Cuando encuentra una fila válida, guarda
            //(row, cols[0], cols[1])
            //ejemplo rowCols.Add((3, 1, 7));
            //significa En la fila 3, el candidato está solo en columnas 1 y 7.
            //Hasta aquí, la primera parte recolecta filas candidatas para X-Wing.
            //Ejemplo final de rowCols:
            //[
            //(row: 1, c1: 2, c2: 6),
            //(row: 4, c1: 0, c2: 8),
            //(row: 7, c1: 2, c2: 6)
            //]
            //La fila 1 y la fila 7 tienen el mismo patrón: c1 = 2 y c2 = 6
        }
        //Estos dos for comparan cada fila candidata con otra fila candidata.
        for (int i = 0; i < rowCols.Count; i++)//i toma una fila.
        {
            for (int j = i + 1; j < rowCols.Count; j++)//j toma otra fila después de i.
                //ejemplo 
                //i = 0
                //j = 1
                //Compara elemento 0 con elemento 1.
                //luego 
                //i = 0
                //j = 2
                //Compara elemento 0 con elemento 2.
                //Esto evita comparar una fila consigo misma y evita repetir comparaciones.
            {
                var a = rowCols[i];//Guarda las dos filas que se están comparando.
                var b = rowCols[j];//Guarda las dos filas que se están comparando.
                //ejemplo
                //a = (row: 1, c1: 2, c2: 6)
                //b = (row: 7, c1: 2, c2: 6)
                if (a.c1 != b.c1 || a.c2 != b.c2)//Esta condición revisa si las dos filas tienen exactamente las mismas columnas.
                    //Para X-Wing basado en filas necesitas esto:
                    //fila A: candidato en columnas 2 y 6
                    //fila B: candidato en columnas 2 y 6
                    //Si las columnas son diferentes, no sirve.
                    //Ejemplo que no sirve:
                    //a = (row: 1, c1: 2, c2: 6)
                    //b = (row: 7, c1: 3, c2: 6)
                    //Como c1 es diferente, salta con : continue
                    continue;

                List<int> affected = new();//Crea una lista para guardar celdas donde se podrá eliminar ese candidato.
                for (int r = 0; r < 9; r++)//Recorre todas las filas del tablero.
                    //¿Por qué? Porque si X-Wing está en dos columnas, se eliminan candidatos en esas columnas, pero en otras filas.
                {
                    if (r == a.row || r == b.row)//Salta las dos filas que forman el X-Wing.
                        //No se eliminan candidatos de las 4 esquinas del patrón.
                        //Solo se eliminan de otras filas en las mismas columnas.
                        continue;

                    int idx1 = r * 9 + a.c1;//Calcula las dos celdas de esa fila en las columnas del X-Wing.
                    int idx2 = r * 9 + a.c2;//Calcula las dos celdas de esa fila en las columnas del X-Wing.
                    //Ejemplo:
                    //Si las columnas del X-Wing son:
                    //a.c1 = 2
                    //a.c2 = 6
                    //y estamos revisando:
                    //r = 4
                    //entonces:
                    //idx1 = 4 * 9 + 2 = 38
                    //idx2 = 4 * 9 + 6 = 42
                    //Está mirando:
                    //fila 4, columna 2
                    //fila 4, columna 6
                    if (ctx.board[r, a.c1] == 0 && (ctx.notesMask[idx1] & mask) != 0)
                        //ctx.board[r, a.c1] == 0 ¿La celda está vacía?
                        //(ctx.notesMask[idx1] & mask) != 0 ¿Tiene este candidato en sus notas?
                        affected.Add(idx1);//Si ambas son verdaderas, agrega esa celda como afectada.
                    if (ctx.board[r, a.c2] == 0 && (ctx.notesMask[idx2] & mask) != 0)
                        //ctx.board[r, a.c2] == 0 ¿La celda está vacía?
                        //(ctx.notesMask[idx2] & mask) != 0 ¿Tiene este candidato en sus notas?
                        affected.Add(idx2);////Si ambas son verdaderas, agrega esa celda como afectada.
                    //Eso significa:
                    //Aquí puedo borrar este candidato.
                }
                if (affected.Count == 0)//si no hay ninguna celda donde borrar candidato,
                //entonces aunque el patrón exista, no produce una acción útil.
                //Por eso continúa buscando otro posible X-Wing.
                    continue;

                hint = BuildHint(mask, new[]//Si hay celdas afectadas, crea el hint:Aquí manda a construir la pista.
                {
                    //El segundo parámetro son las 4 esquinas del X-Wing:
                    a.row * 9 + a.c1,
                    a.row * 9 + a.c2,
                    b.row * 9 + b.c1,
                    b.row * 9 + b.c2
                }, affected);//affected son las celdas donde se eliminará el candidato.
                //ejemplo
                //a.row = 1
                //b.row = 7
                //c1 = 2
                //c2 = 6
                //Las esquinas son:
                //1 * 9 + 2 = 11
                //1 * 9 + 6 = 15
                //7 * 9 + 2 = 65
                //7 * 9 + 6 = 69
                //Esas celdas se resaltan para explicar el X-Wing.
                return true;//Devuelve true porque encontró un X-Wing aplicable.
            }
        }
        //Si termina de revisar todas las celdas y no encuentra ninguna con un solo candidato:
        hint = null;//Eso significa que no hay pista.
        return false;////Esta técnica no se puede aplicar en este tablero.
    }
    bool TryColBased(SudokuContext ctx, int mask, out SudokuHint hint)//Esta función busca X-Wing por filas.
    //SudokuContext ctx : que es El tablero y las notas.
    //int mask : que es El candidato que se está buscando.
    //Ejemplo: si mask representa el 5, esta función busca un X-Wing del número 5.
    //out SudokuHint hint : La pista que devuelve si encuentra algo.
    {
        var colRows = new List<(int col, int r1, int r2)>();//Crea una lista de tuplas.
        //Cada elemento guarda: col , r1 , r2
        //Eso significa: En esta columna, el candidato aparece exactamente en estas dos filas.
        for (int col = 0; col < 9; col++)//Recorre las columnas del tablero:
        {
            var rows = GetCandidateRowsInCol(ctx, col, mask);//Esta función busca en qué filas de esa columna aparece el candidato.
            //Ejemplo:Si estamos buscando el candidato 5, y en la columna 2 el 5 aparece en filas 1 y 6: rows = [1, 6]
            if (rows.Count == 2)//Solo guarda columnas donde el candidato aparece exactamente en 2 filas.
                //¿Por qué exactamente 2?
                //Porque X-Wing necesita dos columnas que tengan el candidato limitado a las mismas dos filas.
                //Si aparece en 1 fila, no sirve para X-Wing.
                //Si aparece en 3 o más filas, tampoco sirve para este patrón.
                colRows.Add((col, rows[0], rows[1]));////Cuando encuentra una columna válida, guarda
            //Ejemplo: colRows.Add((2, 1, 6));
            //Significa: columna 2 y filas posibles: 1 y 6
            //Después de esa primera parte, colRows podría quedar así:
            //[
            //(col: 2, r1: 1, r2: 6),
            //(col: 4, r1: 0, r2: 8),
            //(col: 7, r1: 1, r2: 6)
            //]
            //Aquí las columnas 2 y 7 tienen el mismo patrón de filas: r1 = 1 y  r2 = 6
            //Eso puede formar X-Wing.
        }
        //Estos dos ciclos comparan una columna candidata contra otra.
        for (int i = 0; i < colRows.Count; i++)//i toma una columna.
        {
            for (int j = i + 1; j < colRows.Count; j++)//j toma otra columna después de i.
            {
                var a = colRows[i];//Guarda las dos columnas que se están comparando.
                var b = colRows[j];//Guarda las dos columnas que se están comparando.
                //Ejemplo:
                //a = (col: 2, r1: 1, r2: 6)
                //b = (col: 7, r1: 1, r2: 6)
                if (a.r1 != b.r1 || a.r2 != b.r2)//Aquí pregunta:¿Las dos columnas tienen el candidato exactamente en las mismas dos filas?
                    //Si no coinciden, no hay X-Wing.
                    //Ejemplo no válido:
                    //a = (col: 2, r1: 1, r2: 6)
                    //b = (col: 7, r1: 1, r2: 8)
                    //Como r2 no coincide, hace: continue
                    //Ejemplo válido:
                    //a = (col: 2, r1: 1, r2: 6)
                    //b = (col: 7, r1: 1, r2: 6)
                    //aqui si es valido
                    continue;

                List<int> affected = new();//Crea una lista para guardar las celdas donde se puede eliminar el candidato.
                for (int c = 0; c < 9; c++)//Recorre todas las columnas del tablero.
                    //¿Por qué columnas? Porque en el X-Wing basado en columnas,
                    //se eliminan candidatos en las filas r1 y r2, recorriendo las demás columnas.
                {
                    if (c == a.col || c == b.col)//Salta las dos columnas que forman el X-Wing.
                        //No queremos borrar el candidato de las 4 esquinas.
                        //Queremos borrar el candidato en las mismas filas, pero fuera de esas columnas.
                        continue;

                    int idx1 = a.r1 * 9 + c;//Calcula los índices de dos celdas
                    int idx2 = a.r2 * 9 + c;//Calcula los índices de dos celdas
                    //ejemplo
                    //a.r1 = 1
                    //a.r2 = 6
                    //c = 4
                    //entonces
                    //idx1 = 1 * 9 + 4 = 13
                    //idx2 = 6 * 9 + 4 = 58
                    if (ctx.board[a.r1, c] == 0 && (ctx.notesMask[idx1] & mask) != 0)
                        //ctx.board[a.r1, c] == 0 ¿La celda está vacía?
                        //(ctx.notesMask[idx1] & mask) != 0 ¿Tiene el candidato que estamos buscando?
                        affected.Add(idx1);//Si ambas son verdaderas, esa celda queda afectada.
                    if (ctx.board[a.r2, c] == 0 && (ctx.notesMask[idx2] & mask) != 0)
                        //ctx.board[a.r2, c] == 0 ¿La celda está vacía?
                        //(ctx.notesMask[idx2] & mask) != 0 ¿Tiene el candidato que estamos buscando?
                        affected.Add(idx2);//Si ambas son verdaderas, esa celda queda afectada.
                }
                if (affected.Count == 0)//si no hay ninguna celda donde borrar candidato,
                //entonces aunque el patrón exista, no produce una acción útil.
                //Por eso continúa buscando otro posible X-Wing.
                    continue;

                hint = BuildHint(mask, new[]//Si sí encontró celdas afectadas:Construye el SudokuHint. 
                //El arreglo new[] { ... } contiene las 4 esquinas del X-Wing.
                {
                    a.r1 * 9 + a.col,
                    a.r2 * 9 + a.col,
                    b.r1 * 9 + b.col,
                    b.r2 * 9 + b.col
                }, affected);//affected son las celdas donde se eliminará el candidato.
                //Primera columna:
                //a.r1 * 9 + a.col
                //a.r2 * 9 + a.col
                //Segunda columna:
                //b.r1 * 9 + b.col
                //b.r2 * 9 + b.col
                //Ejemplo:
                //a = (col: 2, r1: 1, r2: 6)
                //b = (col: 7, r1: 1, r2: 6)
                //Entonces las esquinas son:
                //1 * 9 + 2 = 11
                //6 * 9 + 2 = 56
                //1 * 9 + 7 = 16
                //6 * 9 + 7 = 61
                //Esas celdas se resaltan como explicación del X-Wing.
                return true;//Devuelve true porque encontró un X-Wing aplicable.
            }
        }
        //Si termina de revisar todas las celdas y no encuentra ninguna con un solo candidato:
        hint = null;//Eso significa que no hay pista.
        return false;////Esta técnica no se puede aplicar en este tablero.
    }
    List<int> GetCandidateColsInRow(SudokuContext ctx, int row, int mask)//Esta función busca en qué columnas de una fila aparece un candidato.
        //ctx : El contexto del Sudoku.
        //row : La fila que se quiere revisar.
        //mask : El candidato que se está buscando.
        //devuelve una list<int> : Una lista de columnas donde aparece ese candidato.
    {
        List<int> cols = new();//Crea una lista vacía.Aquí se guardarán las columnas encontradas.
        for (int col = 0; col < 9; col++)//Recorre todas las columnas de esa fila.
        {
            if (ctx.board[row, col] != 0)//Si la celda ya tiene número, la salta.
                //Solo interesan celdas vacías, porque solo las celdas vacías tienen candidatos.
                continue;

            int index = row * 9 + col;//Convierte fila y columna a índice lineal.
            //ejemplo:
            //row = 3
            //col = 6
            //index = 3 * 9 + 6
            //index = 33
            //board[3, 6]
            //notesMask[33]
            if ((ctx.notesMask[index] & mask) != 0)//Pregunta si esa celda tiene el candidato.
                //Si el resultado no es cero, significa: Sí, esta celda tiene ese candidato como nota.
                cols.Add(col);//Entonces agrega la columna a la lista
            //Importante: agrega col, no index.
            //Porque esta función quiere responder:
            //¿En qué columnas de esta fila aparece el candidato?
        }
        return cols;//Devuelve la lista de columnas.
        //Si revisas la fila 4 buscando el candidato 7, y ese candidato aparece en columnas 1 y 8:
        //return [1, 8];
    }
    List<int> GetCandidateRowsInCol(SudokuContext ctx, int col, int mask)//Esta función busca en qué columnas de una fila aparece un candidato.
        //ctx : El contexto del Sudoku.
        //col : La columna que se va a revisar.
        //mask : El candidato que se está buscando.
        //devuelve una list<int> : Una lista de columnas donde aparece ese candidato.
    {
        List<int> rows = new();//Crea una lista vacía donde se guardarán las filas encontradas.
        for (int row = 0; row < 9; row++)//Recorre todas las filas de esa columna.
        {
            if (ctx.board[row, col] != 0)//Revisa si la celda ya tiene número.
                //Si la celda ya está ocupada, no puede tener candidatos, así que la salta.
                continue;

            int index = row * 9 + col;//Convierte fila y columna a índice lineal.
            //ejemplo:
            //row = 5
            //col = 4
            //index = 5 * 9 + 4
            //index = 49
            //board[5, 4]
            //notesMask[49]
            if ((ctx.notesMask[index] & mask) != 0)//Aquí revisa si esa celda tiene el candidato buscado.
                //Si el resultado no es cero:
                //significa Esta celda tiene ese candidato.
                rows.Add(row);//Entonces agrega la fila:
            //Importante: agrega row, no index.
            //Porque esta función quiere responder:
            //¿En qué filas de esta columna aparece el candidato?
        }
        return rows;//Devuelve la lista de filas donde apareció el candidato.
        //Si revisa la columna 3 buscando el candidato 5, y el 5 aparece en filas 1 y 6:
        //return [1, 6];
    }
    SudokuHint BuildHint(int mask, int[] highlights, List<int> affected)//Esta función construye el SudokuHint cuando se encuentra un X-Wing.
        //Un SudokuHint contiene:la técnica usada , el candidato involucrado , las celdas que explican el patrón ,
        //las celdas afectadas y las acciones que se aplicarán
        //int mask : El candidato involucrado.
        //int[] highlights : Las 4 celdas que forman el X-Wing. Estas serían las 4 esquinas del rectángulo.
        //List<int> affected : Las celdas donde se puede eliminar el candidato.
    {
        var hint = new SudokuHint//Crea un nuevo SudokuHint.
        {
            technique = Name,//guarda technique = Name , Como Name devuelve: "X-Wing"
            candidateMask = mask//guarda la indicacion de que cuál candidato está involucrado.
        };
        for (int i = 0; i < highlights.Length; i++)//Este ciclo agrega las celdas que deben resaltarse como explicación.
            hint.highlightCells.Add(highlights[i]);//highlights normalmente contiene las 4 esquinas del X-Wing.
        //ejemplo:
        //highlights = [11, 15, 65, 69]
        //Después del ciclo:
        //hint.highlightCells = [11, 15, 65, 69]
        //Estas celdas son las que muestran: Aquí está el patrón X-Wing.
        for (int i = 0; i < affected.Count; i++)//Luego recorre las celdas afectadas:
        {
            int idx = affected[i];//affected contiene las celdas donde el candidato debe eliminarse.
            //Ejemplo:
            //affected = [20, 29, 38]
            //En cada vuelta, idx toma una celda.
            hint.affectedCells.Add(idx);//Agrega esa celda a la lista de afectadas.
            //Estas son las celdas que visualmente podrían marcarse como: Aquí se elimina una nota.
            hint.actions.Add(new SudokuAction//Después agrega una acción:
            {
                type = SudokuActionType.RemoveNotes,//Crea una acción de tipo:RemoveNotes
                //eso significa No coloques un número; elimina una nota/candidato.
                index = idx,//La celda donde se borrará el candidato.
                mask = mask,//El candidato que se borrará.
                technique = Name//La técnica que produjo esa eliminación:
                //ejemplo
                //type = RemoveNotes
                //index = 38
                //mask = candidato 5
                //technique = "X-Wing"
                //significa En la celda 38, elimina el candidato 5 por X-Wing.
            });
        }
        return hint;//Devuelve el hint completo.
    }
}