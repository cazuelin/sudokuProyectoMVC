using System.Collections.Generic;

public static class SudokuTechniqueFactory//Este script sirve para crear la lista de técnicas que el solver humano puede usar según la dificultad.
    //Se llama Factory porque funciona como una fábrica: recibe una dificultad y fabrica/devuelve una lista de técnicas.
{
    public static List<ISudokuTechnique> Build(SudokuGameManager.Difficulty difficulty)//Esta es la única función del script.
        //devuelve List<ISudokuTechnique> O sea, una lista de técnicas de Sudoku.
        //recibe SudokuGameManager.Difficulty difficulty. Ese parámetro indica la dificultad del puzzle:Easy,medium,hard,expert,extreme
    {
        var techniques = new List<ISudokuTechnique>();//Aquí crea una lista vacía.var significa que C# deduce el tipo automáticamente.
        //esta lista guardara objetos como:
        //new NakedSingleTechnique()
        //new HiddenSingleTechnique()
        //new NakedPairTechnique()
        //Aunque sean clases distintas, todas caben en la lista porque implementan ISudokuTechnique.
        if (difficulty >= SudokuGameManager.Difficulty.Easy)//Si la dificultad es Easy o mayor, 
            techniques.Add(new NakedSingleTechnique());//agrega NakedSingleTechnique.
        //Si una celda solo tiene un candidato posible, ese número debe ir ahí.

        if (difficulty >= SudokuGameManager.Difficulty.Medium)//Si la dificultad es Medium o mayor
            techniques.Add(new HiddenSingleTechnique());//agrega HiddenSingleTechnique.
        //Un número puede estar oculto entre varios candidatos, pero dentro de una fila, columna o caja solo aparece en una celda posible.

        if (difficulty >= SudokuGameManager.Difficulty.Hard)//Si la dificultad es Hard o mayor
            techniques.Add(new NakedPairTechnique());//agrega NakedPairTechnique.
        //Naked Pair detecta pares de candidatos iguales.
        //Celda A: 2, 8
        //Celda B: 2, 8
        //Si esas dos celdas están en la misma fila, columna o caja, entonces los números 2 y 8 deben ocupar esas dos celdas.
        //Por eso se pueden eliminar de otras celdas del mismo grupo.

        if (difficulty >= SudokuGameManager.Difficulty.Expert)//Si la dificultad es Expert o mayor
            techniques.Add(new PointingPairTechnique());//agrega PointingPairTechnique.
        //Pointing Pair se usa cuando dentro de una caja 3x3 los candidatos de un número están alineados en una sola fila o columna.
        //Entonces puedes eliminar ese candidato fuera de la caja, pero en esa misma fila o columna.

        if (difficulty >= SudokuGameManager.Difficulty.Extreme)//Si la dificultad es Extreme o mayor
            techniques.Add(new XWingTechnique());//agrega XWingTechnique.
        //X-Wing es una técnica más avanzada. Busca un patrón de dos filas y dos columnas donde un número solo puede aparecer en posiciones cruzadas.
        //Con eso elimina candidatos en otras celdas.

        return techniques;//Finalmente devuelve la lista de técnicas construida.

        //Si la dificultad es Easy, devuelve:
        //NakedSingleTechnique

        //Si la dificultad es Hard, devuelve:
        //NakedSingleTechnique
        //HiddenSingleTechnique
        //NakedPairTechnique

        //Si la dificultad es Extreme, devuelve:
        //NakedSingleTechnique
        //HiddenSingleTechnique
        //NakedPairTechnique
        //PointingPairTechnique
        //XWingTechnique
    }
}
