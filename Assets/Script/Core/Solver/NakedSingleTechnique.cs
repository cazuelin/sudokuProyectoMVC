public class NakedSingleTechnique : ISudokuTechnique//Este script representa la técnica Naked Single.En español sería algo como:Candidato único visible.
//La idea es simple: Si una celda vacía solo tiene un candidato posible, entonces ese número necesariamente va en esa celda.
{
    public string Name => "Naked Single";//Esta propiedad devuelve el nombre de la técnica.
    public bool TryApply(SudokuContext ctx, out SudokuHint hint)//esta es la funcion principal Revisar el tablero y buscar una celda vacía que tenga exactamente un candidato.
        //SudokuContext ctx : la técnica recibe el estado actual del tablero y los candidatos.
        //out SudokuHint hint : Esto permite devolver información de la jugada encontrada.
    {
        for (int r = 0; r < SudokuRules.Size; r++)//Este primer for recorre las filas del tablero.
        {
            for (int c = 0; c < SudokuRules.Size; c++)//Este segundo for recorre las columnas.
            {
                if (ctx.board[r, c] != 0) continue;//Aquí revisa si la celda ya tiene número. Si esta celda ya tiene número, no la analices y continua.
                //0 = celda vacía
                //1-9 = número colocado
                int index = SudokuRules.GetCellIndex(r, c);//Aquí convierte fila y columna a un índice lineal.
                //ejemplo
                //r = 2 y c = 5
                //int index = r * 9 + c
                //index = 2 * 9 + 5 entonces 2 * 9 = 18 + 5 = 23
                //entonces ctx.board[2,5] = 23
                int mask = ctx.notesMask[index];//Aquí obtiene los candidatos/notas de esa celda.
                //mask es un número entero que guarda candidatos usando bits.
                //ejemplo lo bits se representan como 0 y 1 entonces cuando no hay ningun candidato todos estarian en 0 del 1 al 9 asi 000000000
                //si los candidades son el 2,4 y el 7 
                //para el 2 seria asi 000000010
                //para el 4 seria asi 000001000
                //para el 7 seria asi 001000000
                //entonces si los 3 estan seleccionados como candidatos se veria completo asi 001001010
                if (SudokuSolverUtils.CountBits(mask) == 1)//Aquí está la comprobación principal de la técnica.CountBits(mask) cuenta cuántos candidatos tiene la celda.
                    //como antes la celda tenia de candidatos el 2,4 y 7 entonces CountBits(mask) == 3 pero la formula solo dice == 1 solo busca 1 candidato
                    //entonces no sirve esta formula como Naked Single
                    //pero si la solucion en los candidades encontro un mask solo con 1 candidatos como un mask con el candidato 5 entonces CountBits(mask) == 1
                    //Si eso ocurre, encontramos un Naked Single.
                {
                    int value = SudokuSolverUtils.GetSingleValue(mask);//Como ya sabemos que el mask tiene exactamente un candidato, esta función obtiene cuál es ese número.
                    //ejemplo mask = candidato 5
                    //entonces value = 5
                    hint = new SudokuHint//Ahora crea el hint:SudokuHint es el objeto que describe la jugada encontrada.
                    {
                        technique = Name,//Como Name devuelve "Naked Single", el hint guarda:technique = "Naked Single"
                        candidateMask = mask//Guarda los candidatos que causaron el hint.En este caso, el mask tiene un solo candidato.
                    };
                    hint.highlightCells.Add(index);//Aquí agrega la celda encontrada a la lista de celdas que deberían resaltarse.
                    //Por ejemplo, si el Naked Single está en la celda 23:hint.highlightCells.Add(23);
                    //Eso permite que otro sistema pueda decir:Resalta visualmente esta celda porque ahí está la pista.
                    hint.actions.Add(new SudokuAction//Aquí agrega una acción al hint.Una acción indica qué debe hacerse con la jugada.
                    {
                        type = SudokuActionType.Place,//Esta acción coloca un número.
                        index = index,//Dice en qué celda se coloca.
                        value = value,//Dice qué número se coloca.
                        technique = Name//Guarda que esta acción viene de la técnica "Naked Single".
                        //ejemplo
                        //type = Place
                        //index = 23
                        //value = 5
                        //technique = "Naked Single"
                        //en palabras simples Coloca el número 5 en la celda 23 usando Naked Single.
                    });
                    return true;//Cuando encuentra una jugada, devuelve true.
                    //Esto le dice al SudokuSolverEngine:Sí, encontré una técnica aplicable.
                }
            }
        }
        //Si termina de revisar todas las celdas y no encuentra ninguna con un solo candidato:
        hint = null;//Eso significa que no hay pista.
        return false;//Esta técnica no se puede aplicar en este tablero.
    }
}
