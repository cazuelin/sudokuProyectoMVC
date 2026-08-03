using System.Collections.Generic;
public class SudokuHumanSolver
{
    SudokuSolverEngine engine;//Este campo guarda el motor de técnicas.es quien prueba técnica por técnica.
    //El HumanSolver usa el engine para decir: Dame el siguiente paso lógico que puedo aplicar.
    SudokuGameManager.Difficulty targetDifficulty;//Guarda la dificultad con la que fue creado este solver.
    //easy, medium, hard, expert, extreme. Esto importa porque según la dificultad se permiten ciertas técnicas.
    public List<string> techniquesUsed = new();//Esta lista guarda los nombres de las técnicas que realmente se usaron mientras intentaba resolver.
    //ejemplo ["Naked Single", "pointing pair", "Hidden Single", "Naked Pair"]
    //Después, SudokuDifficultyEvaluatorPro puede revisar esta lista y decir: Si usaste Naked Pair, entonces mínimo es Hard.
    public SudokuHumanSolver(SudokuGameManager.Difficulty diff)//este es El constructor se ejecuta cuando haces: new SudokuHumanSolver(diff)
    {
        targetDifficulty = diff;//Guarda la dificultad.
        engine = new SudokuSolverEngine(diff);//Crea el engine con esa dificultad.
        //El engine internamente llama a: SudokuTechniqueFactory.Build(diff)
        //Eso construye la lista de técnicas permitidas.
        //Por ejemplo, si diff = Hard, el engine tendrá: Naked Single , Hidden Single , Naked Pair
    }
    public bool Solve(SudokuContext ctx)//Esta es la función principal. Recibe un contexto: SudokuContext ctx
    {
        techniquesUsed.Clear();//Limpia la lista de técnicas usadas. Esto es importante porque el mismo solver podría usarse más de una vez.
        InitializeCandidates(ctx);//Antes de resolver, calcula las notas/candidatos iniciales de todas las celdas vacías.
        //Ejemplo: Celda 23 puede tener 2, 5, 8  Entonces guarda eso en: ctx.notesMask[23]
        int safety = 0;//Crea un contador de seguridad. Sirve para evitar un bucle infinito.
        //Si por algún error el solver sigue aplicando pasos sin terminar, este contador lo corta.
        while (engine.Step(ctx, out var hint))//Este es el ciclo principal. engine.Step intenta encontrar una jugada usando las técnicas disponibles.
            //Si encuentra una jugada: return true y entrega un hint
            //Si no encuentra nada: return false y el while termina.
            //En palabras simples: Mientras el engine encuentre pasos lógicos, sigue aplicándolos.
        {
            techniquesUsed.Add(hint.technique);//Guarda el nombre de la técnica usada en este paso.
            foreach (var action in hint.actions)//Ahora aplica las acciones del hint: Un hint puede tener una o más acciones.
                //Hay dos tipos principales: Place y RemoveNotes
            {
                if (action.type == SudokuActionType.Place)//Si la acción es de tipo Place, significa: Hay que colocar un número en el tablero.
                {
                    int r = action.index / 9;//Convierte el índice lineal a fila
                    //si index = 23 entonces seria int r = action.index / 9 que seria r = 23 / 9 que son 2
                    int c = action.index % 9;//Convierte el índice lineal a columna.
                    //si index = 23 entonces seria int c = action.index % 9 que seria c = 23 / 9  que son 2 pero aqui es el sobrante osea 9 * 2 = 18 y de 18 a 23 son 5

                    //entonces index 23 = board[2, 5]
                    ctx.board[r, c] = action.value;//Coloca el número en el tablero.
                    //ejemplo ctx.board[2, 5] = 7;
                }
                else if (action.type == SudokuActionType.RemoveNotes)//Si la acción es RemoveNotes, elimina candidatos de una celda.                                                                     
                {
                    ctx.notesMask[action.index] &= ~action.mask;//pasa los bits de los numeros candidatos a 0 para desactivarlos
                    //action.index = es la celda afectada.
                    //action.mask = son los candidatos que se deben borrar.
                    //al hacer ~action.mask invierte los bits.
                    //Si action.mask representa candidatos 2 y 7, entonces ~action.mask significa: Todos los bits menos 2 y 7.
                    //Al hacer &=, borra esos candidatos de la celda.
                }
            }
            RebuildCandidates(ctx);//Después de aplicar acciones, reconstruye todos los candidatos.
            //Actualmente esta función llama a: InitializeCandidates(ctx); Esto recalcula las notas desde cero según el tablero actual.
            safety++;//Aumenta el contador de pasos.
            if (safety > 500)//Si supera 500 pasos, corta el ciclo.Esto es una protección contra errores.
            //Un Sudoku normal no debería necesitar tantos pasos para este sistema.
                break;
        }
        bool solved = IsSolved(ctx.board);//Cuando el while termina: Revisa si el tablero quedó completamente lleno.
        return solved;//Devuelve si logró resolver o no.
    }
    void InitializeCandidates(SudokuContext ctx)//Esta función calcula los candidatos posibles para cada celda vacía.
    {
        for (int i = 0; i < 81; i++)//Recorre las 81 celdas del tablero.Usa índice lineal: de 0 a 80
        {
            if (ctx.board[i / 9, i % 9] != 0)//Convierte i a fila/columna: Si esa celda ya tiene número, borra sus notas
            {
                ctx.notesMask[i] = 0;//borra las notas
                continue;//Y pasa a la siguiente celda.
            }
            int mask = 0;//Crea un mask vacío para guardar candidatos.
            for (int n = 1; n <= 9; n++)//Prueba números del 1 al 9.
            {
                if (ctx.CanPlace(i / 9, i % 9, n))//Pregunta si el número n puede colocarse en esa celda según las reglas del Sudoku.
                    //Conviertiendo i a fila/columna y Si puede, agrega ese número al mask.
                    mask |= 1 << (n - 1);//esta funcion enciende el mask del numero que este en n
                    //ejemplo Si puede colocar 5: mask |= 1 << (5 - 1); Eso enciende el bit del candidato 5.
            }
            ctx.notesMask[i] = mask;//Guarda los candidatos calculados en esa celda.
        }
    }
    void RebuildCandidates(SudokuContext ctx)//Esta función recalcula todos los candidatos.
    {
        InitializeCandidates(ctx);//Ahora mismo solo llama a InitializeCandidates.
        //¿Por qué existe entonces? Porque semánticamente se entiende mejor:
        //ejemplo para el inicio InitializeCandidates
        //para despues de aplicar una jugada RebuildCandidates.
        //Aunque hacen lo mismo, el nombre ayuda a entender el momento en que se usa.
    }
    bool IsSolved(int[,] board)//Esta función revisa si el tablero está completo.
    {
        for (int i = 0; i < 81; i++)//Recorre las 81 celdas.
            if (board[i / 9, i % 9] == 0)//Convierte i a fila/columna para calcular la celda y pasarla a index y Si encuentra un 0,
            //significa que todavía hay una celda vacía:
                return false;//si hay un cero aun entonces devuelve false
        return true;//Si termina todo el ciclo sin encontrar ceros:
    }
}