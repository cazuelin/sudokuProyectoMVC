using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SudokuHintSystem : MonoBehaviour//Este script se encarga de entregar una pista directa al jugador.
//En tu versión actual, no usa las técnicas humanas como Naked Single o X-Wing;
//simplemente busca una celda vacía aleatoria y coloca el valor correcto usando la solución.
{
    [SerializeField] SudokuBoardController boardController;//Referencia al controlador del tablero.
    //Se usa para leer: boardController.boardData.  De ahí toma: data.values , data.fixedCells , data.solution
    //en simple : boardController le da al hint system el estado actual del Sudoku y la solución.
    [SerializeField] public int maxHints = 3;//Cantidad máxima de pistas disponibles.
    //Está en SudokuHintSystem, pero quien realmente usa este valor es SudokuInputController,
    //en ResetHints: remainingHints = hintSystem.maxHints;
    public bool TryGetHint(out SudokuHint hint)//Esta es la función principal. Intenta crear una pista.
        //devuelve true : si logró crear una pista.
        //devuelve false : si no pudo.
        //También devuelve por out: SudokuHint hint : La pista creada.
    {
        if (boardController == null)//Si no hay boardController, el script no puede leer el tablero.
        {
            //Entonces muestra error, 
            hint = null; //pone la pista en null
            return false;//y devuelve false.
        }
        var data = boardController.boardData;//Obtiene los datos actuales del tablero.
        if (data == null || data.solution == null)
        //primera validacion data == null : Si no hay datos
        //segunda validacion data.solution == null : no hay solución
        //La pista necesita la solución para saber qué número correcto colocar.
        {
            //no puede crear una pista.
            hint = null;//pone la pista en null
            return false;//y devuelve false.
        }
        //si pasa las anteriores validacion pasa aca:
        //aqui verifica que la celda existe y este vacia
        List<int> emptyCells = new List<int>();//Crea una lista donde guardará los índices de celdas vacías.
        for (int i = 0; i < 81; i++)//Recorre las 81 celdas del Sudoku.
        {
            if (data.values[i] == 0 && !data.fixedCells[i])
            //primera validacion data.values[i] == 0 : La celda está vacía.
            //segunda validacion !data.fixedCells[i] : La celda no es fija.
            {
                emptyCells.Add(i);//Si ambas se cumplen, agrega esa celda a la lista
            }
        }
        if (emptyCells.Count == 0)//Si no hay celdas vacías, no puede dar pista.
            //Eso normalmente significa que el tablero está completo.
        {
            // Tablero completo
            hint = null;//pone la pista en null
            return false;//y devuelve false.
        }
        // Elegir celda aleatoria
        int randomIndex = Random.Range(0, emptyCells.Count);//Elige una posición aleatoria dentro de la lista.
        int cellIndex = emptyCells[randomIndex];//Obtiene el índice real de la celda elegida.
        //Ejemplo: cellIndex = 23
        int correctValue = data.solution[cellIndex];//Busca cuál es el número correcto para esa celda usando la solución.
        //Ejemplo: data.solution[23] = 7 y correctValue = 7

        // Crear hint que coloca el número correcto
        hint = new SudokuHint
        {
            technique = "Direct Hint",//Indica que esta pista es directa, no viene de una técnica como Hidden Single.
            highlightCells = new List<int> { cellIndex },//Marca la celda que se debe resaltar.
            //Esa celda se pinta luego en SudokuHighlightSystem.ShowHint.
            affectedCells = new List<int>(),//No marca celdas afectadas.
            //Está vacío porque esta pista solo coloca un número, no elimina notas de otras celdas.
            actions = new List<SudokuAction>//Crea una lista de acciones.
            {
                new SudokuAction//Dentro agrega una acción
                {
                    type = SudokuActionType.Place,//Significa colocar un número.
                    index = cellIndex,//Celda donde se colocará.
                    value = correctValue//Número correcto que se colocará.
                }
            }
        };
        return true;//Devuelve que sí logró crear una pista.
    }
}
