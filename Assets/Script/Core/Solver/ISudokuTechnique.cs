public interface ISudokuTechnique
//Primero, esto no es una clase normal. Es una interfaz.
//Cualquier técnica de Sudoku que quiera ser usada por el solver debe tener estas funciones/propiedades.
//define el contrato de una técnica de Sudoku.
{
    bool TryApply(SudokuContext ctx, out SudokuHint hint);//Esta es la función principal que toda técnica debe tener.
    //Intenta aplicar esta técnica al tablero actual.intenta encontrar una jugada usando esa técnica.
    //al ser bool La función devuelve true o false.  Devuelve true si la técnica encontró algo útil.  Devuelve false si la técnica no encontró nada.
    //el sudokuContext ctx Este parámetro es el contexto del Sudoku. O sea, la técnica recibe el estado actual del tablero y los candidatos. le da a la técnica el tablero y las notas.
    //out sudokuHint hint. out significa que la función va a “sacar” un resultado adicional. Encontré una jugada. Aquí está el hint con la celda, número, técnica y acciones.
    //devuelve la jugada encontrada si existe.
    string Name { get; }//Esta es una propiedad, no una función.Sirve para obtener el nombre de la técnica.
    //El { get; } significa que la propiedad se puede leer, pero la interfaz no exige que se pueda modificar desde fuera.
}
