using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
public class NakedPairTechnique : ISudokuTechnique//La técnica Naked Pair significa:
//Si dos celdas de la misma unidad tienen exactamente los mismos dos candidatos,
//esos dos números pertenecen a esas dos celdas.Por eso se pueden eliminar de las demás celdas de esa unidad.
//ejemplo:
//Celda A: candidatos 2 y 7
//Celda B: candidatos 2 y 7
//Si ambas están en la misma fila, entonces el 2 y el 7 tienen que ir en esas dos celdas. Ninguna otra celda de esa fila debería tener 2 o 7 como candidato.
{
    public string Name => "Naked Pair";//Esta propiedad devuelve el nombre de la técnica.
    public bool TryApply(SudokuContext ctx, out SudokuHint hint)//esta es la funcion principal Revisar el tablero y buscar una celda vacía que tenga exactamente un candidato.
    //SudokuContext ctx : la técnica recibe el estado actual del tablero y los candidatos.
    //out SudokuHint hint : Esto permite devolver información de la jugada encontrada.

    {
        for (int row = 0; row < SudokuRules.Size; row++)//primero revisa las filas
        {
            if (TryInUnit(ctx, SudokuSolverUtils.GetRowUnit(row), out hint))//cada vuelta obtiene un fila completa
                //Ejemplo: GetRowUnit(0) devuelve [0, 1, 2, 3, 4, 5, 6, 7, 8] Eso representa la primera fila.
                return true;//Si encuentra algo, también retorna true.
        }
        for (int col = 0; col < SudokuRules.Size; col++)//despues revisa las columnas
        {
            if (TryInUnit(ctx, SudokuSolverUtils.GetColUnit(col), out hint))//cada vuelta obtine una columna completa
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
        var notesMask = ctx.notesMask;//Aquí guarda una referencia corta a las notas del contexto.
        //En vez de escribir todo el rato: ctx.notesMask[index] puedes escribir notesMask[index]
        Dictionary<int, List<int>> pairs = new();//Aquí crea un diccionario.Este diccionario guarda: mask -> lista de celdas que tienen ese mask
        //ejemplo
        //mask de candidatos 2 y 7 -> [12, 15]
        //mask de candidatos 1 y 9 -> [10]
        //¿Por qué usa Dictionary? Porque necesita agrupar celdas que tengan exactamente los mismos dos candidatos.
        for (int i = 0; i < unit.Length; i++)//Recorre las celdas de la unidad.
        {
            int index = unit[i];//Obtiene el índice real de la celda.
            //ejemplo unit[i] = 23 Eso significa celda índice 23.
            int r = SudokuRules.GetRow(index);//Convierte el índice lineal a fila
            int c = SudokuRules.GetCol(index);//Convierte el índice lineal a columna.
            
            //entonces index 23 = board[2, 5]
            if (ctx.board[r, c] != 0)//Si la celda ya tiene número, la salta.
                continue;
            int mask = notesMask[index];//guarda el index en una variable del mask
            if (SudokuSolverUtils.CountBits(mask) != 2)//Aquí revisa si la celda tiene exactamente 2 candidatos.
                //Si tiene 1 candidato, no sirve para Naked Pair.
                //Si tiene 3 o más candidatos, tampoco sirve.
                //Necesita exactamente 2: ejemplo en una celda los candidatos son 2 y 7 si sirve
                continue;
            if (!pairs.ContainsKey(mask))//el containsKey pregunta ¿Ya existe una lista para este mismo par de candidatos?
                pairs[mask] = new List<int>();//Si no existe, la crea.
            //Ejemplo: primera vez que encuentra el par 2 y 7:
            pairs[mask].Add(index);//Agrega esta celda a la lista de ese par.
            //ejemplo pares 2 y 7 -> [12]
            //luego si encuentra otra celda con los mismos candidatos pares 2 y 7 -> [12, 15]
        }
        foreach (var kv in pairs)//Recorre cada entrada del diccionario.
            //kv significa key-value.
        {
            if (kv.Value.Count != 2)//ahora pregunta de los candidatos que encontro hay alguno que posea exactamente 2 pares iguales en 2 celdas
                //Un Naked Pair válido necesita exactamente dos celdas con el mismo par.
                //Si aparece solo una vez, no sirve.
                //Si aparece tres veces, tampoco es Naked Pair válido.
                //ejemplo si en la primera parte de if (SudokuSolverUtils.CountBits(mask) != 2) encuentra
                //{2,7} -> [0, 2]
                //{3,8} -> [5]
                //{4,9} -> [6, 7, 8]
                //entonces esta verificacion solo acepta la que tenga los 2 pares iguales
                //{2,7} -> [0, 2]
                continue;//si encuntra pares continua
            int pairMask = kv.Key;
            //pairMask = es el mask con los dos candidatos.
            //kv.Key = Es el mask, o sea el par de candidatos.
            int a = kv.Value[0];
            //a = es la primera celda del par.
            //kv.Value = Es la lista de celdas que tienen ese mismo par.
            int b = kv.Value[1];
            //b = es la segunda celda del par.
            //kv.Value = Es la lista de celdas que tienen ese mismo par.

            for (int i = 0; i < unit.Length; i++)//Vuelve a recorrer las celdas de la unidad.
            {
                int index = unit[i];//Obtiene la celda actual.
                if (index == a || index == b)
                    //Si la celda actual es una de las dos celdas del par, la salta.
                    //No queremos borrar los candidatos del par.
                    //Queremos borrar esos candidatos de las otras celdas de la unidad.
                    continue;

                int r = SudokuRules.GetRow(index); //Convierte el índice a fila y salta celdas ya ocupadas.
                int c = SudokuRules.GetCol(index); //Convierte el índice a columna y salta celdas ya ocupadas.
                if (ctx.board[r, c] != 0)//entonces index 23 = board[2, 5] y si esta celda esta ocupada por un numero la salta
                    //Solo se eliminan notas de celdas vacías.
                    continue;

                if ((notesMask[index] & pairMask) == 0)//esta linea pregunta ¿Esta celda tiene alguno de los candidatos del par?
                    //ejemplo
                    //pairMask = candidatos 2 y 7
                    //notesMask[index] = candidatos 1, 2, 5
                    //Como tiene el 2, el resultado no será 0.
                    //Si la celda no tiene ni 2 ni 7, entonces: (notesMask[index] & pairMask) == 0 y la salta.
                    continue;

                //Si llega hasta aquí, encontró una celda afectada. O sea, una celda donde se pueden borrar notas.
                //Entonces crea el hint:
                hint = new SudokuHint
                {
                    technique = Name,//el nombre de la tecnica para guardar
                    candidateMask = pairMask//los candidados de la celda para guardar
                };
                hint.highlightCells.Add(a);//Marca las dos celdas del par para resaltarlas.
                hint.highlightCells.Add(b);//Marca las dos celdas del par para resaltarlas.
                hint.affectedCells.Add(index);//Marca la celda afectada.Esta es la celda donde se eliminarán candidatos.
                hint.actions.Add(new SudokuAction//Crea una acción 
                {
                    type = SudokuActionType.RemoveNotes,//sera de tipo RemoveNotes que es Borra estas notas de esta celda.
                    index = index,//La celda afectada.
                    mask = pairMask,//Los candidatos que se van a borrar.
                    technique = Name//La técnica que produjo la eliminación.
                    //ejemplo
                    //type = RemoveNotes
                    //index = 20
                    //mask = candidatos 2 y 7
                    //technique = "Naked Pair"
                });
                return true;//Devuelve true porque encontró una aplicación válida de Naked Pair.
            }
        }
        //Si termina de revisar todas las celdas y no encuentra ninguna con un solo candidato:
        hint = null;//Eso significa que no hay pista.
        return false;//Esta técnica no se puede aplicar en este tablero.
    }
}
