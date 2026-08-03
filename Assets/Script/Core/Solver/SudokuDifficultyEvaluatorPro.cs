using System.Collections.Generic;
public class SudokuDifficultyEvaluatorPro//Este script sirve para decidir la dificultad real de un Sudoku según las técnicas que fueron necesarias para resolverlo.
    //Recibir una lista de técnicas usadas y devolver una dificultad.
{
    public SudokuGameManager.Difficulty Evaluate(List<string> techniques)
    //recibe List<string> techniques Que es una lista con nombres de técnicas.
    //Ejemplo:techniques = ["Naked Single", "Hidden Single"]
    //y devuelve SudokuGameManager.Difficulty osea una dificultad del juego Easy, medium, hard, expert, extreme
    {
        if (techniques == null || techniques.Count == 0)
            //Primera validación:
            //techniques == null significa si la lista no existe. No es que esté vacía, sino que no hay lista.
            //segundatechniques.Count == 0 significa La lista existe, pero no tiene ninguna técnica dentro.

            return SudokuGameManager.Difficulty.Easy;//Si ocurre cualquiera de esos dos casos, devuelve: easy
        //Porque si no se registró ninguna técnica difícil, el evaluator asume que es fácil.

        //Aquí revisa si la lista contiene ciertas técnicas.
        bool hasXWing = techniques.Contains("X-Wing");//¿En la lista aparece la técnica "X-Wing"?
        //si aparece guarda hasXWing = true;
        //si no aparece guarda hasXWing = false;
        bool hasPairs = techniques.Contains("Naked Pair");//¿En la lista aparece la técnica "Naked Pair"?
        //si aparece guarda Naked Pair = true;
        //si no aparece guarda Naked Pair = false;
        bool hasPointing = techniques.Contains("Pointing Pair");//¿En la lista aparece la técnica "Pointing Pair"?
        //si aparece guarda Pointing Pair = true;
        //si no aparece guarda Pointing Pair = false;

        //Después empieza a decidir dificultad.
        //Si se usó X-Wing, el Sudoku se considera Extreme.
        if (hasXWing)//Si para resolverlo hizo falta una técnica extrema, entonces el puzzle completo debe clasificarse como extremo.
            //Aunque también se hayan usado técnicas fáciles, la dificultad la marca la técnica más difícil necesaria.
            //ejemplo ["Naked Single", "Hidden Single", "X-Wing"] resultado Extreme
            return SudokuGameManager.Difficulty.Extreme;//retorna dificultad extreme

        if (hasPointing)//Si no hubo X-Wing, pero sí hubo Pointing Pair, entonces devuelve Expert.
            //ejemplo ["Naked Single", "Hidden Single", "Pointing Pair"]
            return SudokuGameManager.Difficulty.Expert;//retorna dificultad expert

        if (hasPairs)//Si no hubo X-Wing ni Pointing Pair, pero sí hubo Naked Pair, devuelve Hard.
            //ejemplo ["Naked Single", "Naked Pair"]
            return SudokuGameManager.Difficulty.Hard;//retorna dificultad hard

        if (techniques.Contains("Hidden Single"))//Si no hubo técnicas más avanzadas, pero sí se usó Hidden Single, devuelve Medium.
            //ejemplo ["Naked Single", "Hidden Single"]
            return SudokuGameManager.Difficulty.Medium;//retorna dificultad medium

        return SudokuGameManager.Difficulty.Easy;//Si no apareció ninguna técnica avanzada, el Sudoku queda como Easy.
        //Normalmente eso significa que se pudo resolver con:Naked Single o con tecnicas muy basicas
    }
}