using System.Collections.Generic;
public class PointingPairTechnique : ISudokuTechnique//PointingPairTechnique busca la técnica Pointing Pair.
    //Si dentro de una caja 3x3 un número candidato solo aparece en una misma fila o una misma columna,
    //entonces ese número no puede aparecer fuera de esa caja en esa misma fila/columna.
    //ejemplo
    //Dentro de una caja 3x3, el candidato 5 aparece solo aquí:
    //[5] [ ] [5]
    //[ ] [ ] [ ]
    //[ ] [ ] [ ]
    //Todos están en la misma fila. Entonces, en el resto de esa fila, fuera de la caja, puedes borrar el candidato 5.
{
    public string Name => "Pointing Pair";//Devuelve el nombre de la técnica.
    public bool TryApply(SudokuContext ctx, out SudokuHint hint)//esta es la funcion principal Revisar el tablero y buscar una celda vacía que tenga exactamente un candidato.
    //SudokuContext ctx : la técnica recibe el estado actual del tablero y los candidatos.
    //out SudokuHint hint : Esto permite devolver información de la jugada encontrada.
    {
        var notesMask = ctx.notesMask;//Guarda una referencia corta a las notas.
        //Así en vez de escribir: ctx.notesMask[index] escribes notesMask[index]
        for (int box = 0; box < 9; box++)//Recorre las 9 cajas 3x3 del Sudoku.
            //Las cajas se numeran así:
            //0 1 2
            //3 4 5
            //6 7 8
        {
            //Calcula dónde empieza la caja.
            //ejemplo con box 4
            int startRow = (box / 3) * 3;//int startRow = (box / 3) * 3; seria startRow = (4 / 3) * 3 y luego 1 * 3 = 3 que seria la fila
            int startCol = (box % 3) * 3;//int startCol = (box % 3) * 3; seria startCol = (4 / 3) * 3 y 4/3 es 1 y el sobrante es 1 entocnes 1 * 3 = 3 que seria la columna
            //entonces empieza la caja en la fila 3 y en la columna 3
            for (int num = 1; num <= 9; num++)//Dentro de cada caja, revisa cada número del 1 al 9.
                //pregunta ¿Dónde aparece este número como candidato dentro de esta caja?
            {
                int mask = 1 << (num - 1);//Convierte el número en un bit.
                //ejemplo n = 5
                //int mask = 1 << (num - 1);
                //int mask = 1 << (5 - 1);
                //int mask = 1 << 4;
                //Ese mask representa el candidato 5.
                List<int> positions = new();//Crea una lista para guardar las celdas de la caja donde aparece ese candidato.
                //ejemplo positions = [3, 5] significa que el candidato aparece en esas celdas.

                //Ahora recorre las 9 celdas internas de la caja:
                for (int r = 0; r < 3; r++)//primero recorre las filas internas de la caja 3x3
                {
                    for (int c = 0; c < 3; c++)//luego recorre las columnas internas de la caja 3x3
                    {
                        //ejemplo si el startRow es fila 3 y el startCol es columna 3
                        int rr = startRow + r;//Convierte esa posición interna a fila real del tablero.
                        //ejemplo int rr = 3 + 0 = 3
                        //ejemplo int rr = 3 + 1 = 4
                        //ejemplo int rr = 3 + 2 = 5
                        int cc = startCol + c;//Convierte esa posición interna a columna real del tablero.
                        //ejemplo int cc = 3 + 0 = 3
                        //ejemplo int cc = 3 + 1 = 4
                        //ejemplo int cc = 3 + 2 = 5
                        int index = rr * 9 + cc;//Convierte fila/columna a índice lineal.
                        //ejemplo con las 9 celdas
                        //int index = rr * 9 + cc luego index = 3 * 9 + 3 entonces index = 3 * 9 = 27 + 3 = 30
                        //int index = rr * 9 + cc luego index = 3 * 9 + 4 entonces index = 3 * 9 = 27 + 4 = 31
                        //int index = rr * 9 + cc luego index = 3 * 9 + 5 entonces index = 3 * 9 = 27 + 5 = 32

                        //int index = rr * 9 + cc luego index = 4 * 9 + 3 entonces index = 4 * 9 = 36 + 3 = 39
                        //int index = rr * 9 + cc luego index = 4 * 9 + 4 entonces index = 4 * 9 = 36 + 4 = 40
                        //int index = rr * 9 + cc luego index = 4 * 9 + 5 entonces index = 4 * 9 = 36 + 5 = 41

                        //int index = rr * 9 + cc luego index = 5 * 9 + 3 entonces index = 5 * 9 = 45 + 3 = 48
                        //int index = rr * 9 + cc luego index = 5 * 9 + 4 entonces index = 5 * 9 = 45 + 4 = 49
                        //int index = rr * 9 + cc luego index = 5 * 9 + 5 entonces index = 5 * 9 = 45 + 5 = 50

                        if (ctx.board[rr, cc] == 0 && (notesMask[index] & mask) != 0)//Esta línea pregunta dos cosas:
                            //ctx.board[rr, cc] == 0 si la celda esta vacia 
                            //(notesMask[index] & mask) != 0 La celda tiene este número como candidato.
                            positions.Add(index);//Si ambas son verdaderas, agrega esa celda a positions.
                    }
                }
                if (positions.Count < 2) continue;//Si el candidato aparece en menos de 2 posiciones dentro de la caja, no aplica esta técnica.
                //Para Pointing Pair normalmente se busca que el candidato esté limitado a dos o más posiciones alineadas.
                int baseRow = positions[0] / 9;//convierte índice a fila.Toma la fila de la primera posición encontrada.
                //ejemplo positions[0] = 30
                //entonces baseRow = 30 / 9 = 3
                bool sameRow = true;//Luego asume inicialmente
                foreach (var idx in positions)//Recorre todas las posiciones.
                    if (idx / 9 != baseRow)//Si alguna está en una fila distinta,entonces no están todas en la misma fila
                        sameRow = false;//y retorna false
                        //ejemplo valido -- positions = [27, 28, 29] Todas están en fila 3.
                        //27 / 9 = 3
                        //28 / 9 = 3
                        //29 / 9 = 3
                        //todas estan en la fila 3
                        //entonces sameRow = true
                        //ejemplo no valido -- positions = [27, 37] Una está en fila 3 y otra en fila 4.
                        //27 / 9 = 3
                        //37 / 9 = 4
                        //No están en la misma fila.
                        //entonces sameRow = false
                if (sameRow)//Si todas las posiciones del candidato dentro de la caja están en la misma fila
                            //entonces se puede eliminar ese candidato de otras celdas de esa fila fuera de la caja.
                {
                    List<int> affected = new();//Crea una lista para guardar las celdas afectadas, o sea, las celdas donde se borrará ese candidato.
                    for (int c = 0; c < 9; c++)//Recorre toda la fila baseRow, columna por columna.
                    {
                        int index = baseRow * 9 + c;//Convierte esa celda de la fila a índice lineal.
                        //ejemplo si baseRow = 3 y c = 6
                        //entonces index = 3 * 9 + 6 = 33
                        if (IsInsideBox(index, box)) continue;//Si la celda está dentro de la misma caja, la salta.
                        //¿Por qué? Porque las posiciones dentro de la caja son las que justifican la técnica.
                        //No queremos borrar el candidato ahí. Queremos borrarlo fuera de la caja, en la misma fila.
                        if (ctx.board[baseRow, c] == 0 && (notesMask[index] & mask) != 0)
                            //ctx.board[baseRow, c] == 0 si la celda esta vacia
                            //(notesMask[index] & mask) != 0 La celda tiene este número como candidato.
                            affected.Add(index);//la agrega como afectada.
                        //eso significa Aquí se puede eliminar este candidato.
                    }
                    if (affected.Count > 0)//Si encontró al menos una celda donde eliminar notas
                    {
                        hint = BuildHint(Name, mask, positions, affected);//crea un hint.
                        //le pasa el name La técnica: "Pointing Pair".
                        //el mask que es El candidato que se va a borrar.
                        //la position que es Las celdas dentro de la caja que justifican la técnica.
                        //el affected que es Las celdas fuera de la caja donde se eliminará el candidato.
                        return true;//Luego devuelve true porque encontró una jugada válida..
                    }
                }
                //Si no encontró eliminación por fila, revisa columnas:
                int baseCol = positions[0] % 9;//convierte índice a columna
                //ejemplo positions[0] = 32
                //baseCol = 32 % 9 = 5
                bool sameCol = true;//Luego asume inicialmente
                foreach (var idx in positions)//Comprueba si todas las posiciones están en esa columna
                    if (idx % 9 != baseCol)
                        //ejemplo valido
                        //positions = [5, 14, 23]
                        //entonces se verifica
                        //5 % 9 = 5
                        //14 % 9 = 5
                        //23 % 9 = 5
                        //todas estan en columna 5 por lo tanto es valido
                        //ejemplo no valido
                        //positions = [5, 13]
                        //entonces se verifica
                        //5 % 9 = 5
                        //13 % 9 = 4
                        //no estan los 2 en la misma columna por lo tanto no es valido
                        sameCol = false;
                if (sameCol)//pregunta ¿Si todas están en la misma columna:entonces se puede eliminar ese candidato fuera de la caja, pero en esa columna.
                {
                    List<int> affected = new();//Crea lista de afectadas
                    for (int r = 0; r < 9; r++)//Recorre toda la columna
                    {
                        int index = r * 9 + baseCol;//Calcula el índice
                        //ejemplo si r = 7 y baseCol es 5
                        //index = 7 * 9 + 5 = 68
                        if (IsInsideBox(index, box)) continue;//No se eliminan candidatos dentro de la caja que está justificando la técnica.
                        if (ctx.board[r, baseCol] == 0 && (notesMask[index] & mask) != 0)
                            //ctx.board[r, baseCol] == 0     Si la celda está vacía
                            //(notesMask[index] & mask) != 0    tiene ese candidato
                            affected.Add(index);//se agrega como afectada
                    }
                    if (affected.Count > 0)//Si encontró afectadas
                    {
                        hint = BuildHint(Name, mask, positions, affected);//Construye el hint y termina.
                        //le pasa el name La técnica: "Pointing Pair".
                        //el mask que es El candidato que se va a borrar.
                        //la position que es Las celdas dentro de la caja que justifican la técnica.
                        //el affected que es Las celdas fuera de la caja donde se eliminará el candidato.
                        return true;
                    }
                }
            }
        }
        //Si termina de revisar todas las celdas y no encuentra ninguna con un solo candidato:
        hint = null;//Eso significa que no hay pista.
        return false;////Esta técnica no se puede aplicar en este tablero.
    }
    SudokuHint BuildHint(string tech, int mask, List<int> highlights, List<int> affected)//Esta función crea el objeto SudokuHint
        //string tech : Es el nombre de la técnica. En este caso normalmente viene:"Pointing Pair"
        //int mask : Es el candidato que se va a eliminar.Por ejemplo, si el Pointing Pair detectó que hay que borrar el candidato 5,
        //este mask representa al número 5 en bits.
        //List<int> highlights : Son las celdas que justifican la técnica. Ejemplo: las celdas dentro de la caja 3x3 donde aparece el candidato alineado.
        //List<int> affected : Son las celdas afectadas, es decir, donde se va a borrar ese candidato.
    {
        var hint = new SudokuHint//Primero crea el hint:
        {
            technique = tech,//Dice qué técnica encontró la jugada.
            candidateMask = mask//Dice qué candidato está involucrado.
        };
        hint.highlightCells.AddRange(highlights);//highlightCells es una lista de celdas que se van a resaltar.
        //addRange es : Agrega todos los elementos de esta lista dentro de la otra lista.
        //ejemplo highlights = [12, 13]
        //Después de: hint.highlightCells.AddRange(highlights);
        //quedaria hint.highlightCells = [12, 13]
        foreach (int idx in affected)//Luego recorre las celdas afectadas, idx es el índice de una celda donde se va a borrar el candidato.
            //ejemplo affected = [5, 6, 8]
            //Entonces el foreach pasará por: idx = 5 , idx = 6 , idx = 8
        {
            hint.affectedCells.Add(idx);//Agrega esa celda a la lista de celdas afectadas.
            //Estas son las celdas donde visualmente podrías marcar: Aquí se eliminará una nota.
            hint.actions.Add(new SudokuAction//Después agrega una acción:
            {
                type = SudokuActionType.RemoveNotes,//Esta acción no coloca un número. Borra notas/candidatos.
                index = idx,//La celda donde se borrará la nota.
                mask = mask,//El candidato que se borrará.
                technique = tech//La técnica que causó esa eliminación.
                //ejemplo 
                //type = RemoveNotes
                //index = 42
                //mask = candidato 5
                //technique = "Pointing Pair"
                //significa En la celda 42, elimina el candidato 5 porque se detectó Pointing Pair.
            });
        }
        return hint;//Devuelve el hint completo.
    }
    bool IsInsideBox(int index, int box)//Esta función responde una pregunta: ¿La celda index está dentro de la caja 3x3 número box?
        //si esta dentro devuelve true y si esta afuera devuelve false
        //recibe un int index que Es el índice lineal de una celda. la formula es index = fila * 9 + columna
        //recibe un int box que Es la caja 3x3 que se quiere revisar.
    {
        //si index es 41
        //si box es 4
        int r = index / 9;//Primero convierte el índice a fila
        //int r = 41 / 9 = 4
        int c = index % 9;//Luego convierte el índice a columna
        //int c = 41 % 9 = 5 que es el sobrante ya que de 9 para llegar a 41 son 4 y el sobrante son 5 es decir 9 * 5 = 36 y 41 - 36 = 5
        //entonces index 41 = fila 4, columna 5
        int br = (box / 3) * 3;//Calcula fila inicial
        //int br = (4 / 3) * 3 que es br = 1 * 3 = 3
        int bc = (box % 3) * 3;//Calcula columna inicial
        //int br = (4 % 3) * 3 que es bc = 1 * 3 = 3 ya que es la division de 4 / 3 es 1 y 4 - 3 es 1 para luego sacar el 1 * 3 que son 3
        //entonces la caja 4 empieza en fila 3, columna 3
        //y cubre
        //[3,3] [3,4] [3,5]
        //[4,3] [4,4] [4,5]
        //[5,3] [5,4] [5,5]
        return r >= br && r < br + 3 && c >= bc && c < bc + 3;//La última línea decide si está dentro. Esto revisa cuatro condiciones
        //primero r >= br : La fila de la celda está desde la fila inicial de la caja hacia abajo.
        //segundo r < br + 3 : La fila no se pasa de las 3 filas de la caja.
        //tercero c >= bc : La columna está desde la columna inicial de la caja hacia la derecha.
        //cuarto c < bc + 3 : La columna no se pasa de las 3 columnas de la caja.
        //Si las cuatro son verdaderas, devuelve true.
        //ejemplo
        //si index es 41 y box 4
        //entonces index 41 = r 4, c 5
        //entocnes box 4 empieza en br 3, bc 3
        //primera condicion r >= br      -> 4 >= 3 true
        //segunda condicion r < br + 3  -> 4 < 6 true
        //tercera condicion c >= bc      -> 5 >= 3 true
        //cuarta condicion  c < bc + 3  -> 5 < 6 true
        //por lo tanto La celda 41 está dentro de la caja 4. y retorna true
    }
}