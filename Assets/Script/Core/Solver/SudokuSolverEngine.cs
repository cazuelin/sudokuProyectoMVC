using System.Collections.Generic;
public class SudokuSolverEngine//Este script es el motor de técnicas humanas.
//No resuelve con fuerza bruta como SudokuSolver; este prueba técnicas como Naked Single, Hidden Single, Naked Pair, etc.
{
    List<ISudokuTechnique> techniques;//Este es el campo principal del script.Guarda la lista de técnicas que el engine puede usar.
    public SudokuSolverEngine(SudokuGameManager.Difficulty diff)//Este es el constructor.El constructor se ejecuta cuando haces:
        //new SudokuSolverEngine(diff) y Recibe una dificultad: SudokuGameManager.Difficulty diff
    {
        techniques = SudokuTechniqueFactory.Build(diff);//Aquí llama a SudokuTechniqueFactory.La fábrica decide qué técnicas estarán disponibles según la dificultad.
        //ejemplo diff = SudokuGameManager.Difficulty.Medium Entonces Build(diff) devuelve una lista con:NakedSingleTechnique y HiddenSingleTechnique 
        //Y esa lista se guarda en: techniques
        //en palabras simples Cuando se crea el engine, se prepara con las técnicas permitidas para esa dificultad.
    }
    public List<string> GetTechniqueNames()//Esta función devuelve una lista con los nombres de las técnicas cargadas.Sirve para saber qué técnicas está usando el engine.
    {
        var names = new List<string>(techniques.Count);//Crea una lista de textos. techniques.Count es la cantidad de técnicas que tiene el engine.
        //Por ejemplo, si hay 3 técnicas:techniques.Count = 3
        //Entonces crea una lista preparada para 3 nombres.No significa que ya tenga 3 elementos; significa que reserva espacio para trabajar más eficiente.
        foreach (var t in techniques)//Recorre cada técnica dentro de la lista.
            //Si la lista tiene:NakedSingleTechnique, HiddenSingleTechnique, NakedPairTechnique
            //El foreach pasa por cada una.
            names.Add(t.Name);//Agrega el nombre de la técnica a la lista names.
        //Cada técnica tiene una propiedad:Name
        //por ejemplo = la tecnica Naked Single sale como name = "Naked Single" la tecnica Hidden Single aparece como name = "Hidden Single"
        //Entonces names va quedando así:["Naked Single", "Hidden Single", "Naked Pair"]
        return names;//Devuelve la lista de nombres.
        //en simple Dime qué técnicas tiene permitido usar este engine.
    }
    public bool Step(SudokuContext ctx, out SudokuHint hint)//Esta es la función más importante del script.Step significa “dar un paso”.
        //En vez de resolver todo el Sudoku de golpe, intenta encontrar una jugada lógica usando las técnicas disponibles.
        //recibe SudokuContext ctx : Ese es el estado actual del tablero:
        //recibe out SudokuHint hint : Esto permite devolver información sobre la jugada encontrada.
    {
        foreach (var tech in techniques)//Recorre cada técnica en orden.El orden viene desde SudokuTechniqueFactory.
            //Por ejemplo, en Hard: 1.Naked Single 2.Hidden Single 3.Naked Pair
            //Eso significa que primero intenta resolver con la técnica más simple.
        {
            if (tech.TryApply(ctx, out hint))//Aquí intenta aplicar la técnica actual.
                //ejemplo NakedSingleTechnique.TryApply(ctx, out hint)
                //La técnica revisa el tablero y responde:true si encontró una jugada. o false si no encontró nada.
                //Si devuelve true, también llena el hint.
                //ese hint puede contener informacion como : qué técnica se usó, qué celda afecta, qué número colocar, qué candidatos eliminar
                return true;//Si una técnica encontró algo, Step se detiene inmediatamente.No sigue revisando técnicas más difíciles.
            //Si puedo encontrar una jugada fácil, uso esa antes que una técnica difícil.
        }
        //Si ninguna técnica encuentra nada, llega aquí:
        hint = null;//No hay hint porque no encontré jugada.
        return false;//Ninguna técnica pudo avanzar con este tablero.
    }
}