public enum SudokuActionType//define el tipo de accion que puede producir una pista o una tecnica
{
    Place,//significa colocar un numero en una celda
    RemoveNotes//significa eliminar candidades/notas de una celda
}
public struct SudokuAction//representa una accion producida por una tecnica o una pista
{
    public SudokuActionType type;//dice que tipo de accion es
    public int index;//celda afectada
    public int value;//numero que se va a colocar se usa cual el tipo es place
    public int mask;//mascara de notas que se van a quitar se usa cuando el tipo es removeNotes
    public string technique;//nombre de la tecnica que genero la accion
}
