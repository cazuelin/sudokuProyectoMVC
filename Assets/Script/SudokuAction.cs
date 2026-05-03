public enum SudokuActionType
{
    Place,
    RemoveNotes
}
public struct SudokuAction
{
    public SudokuActionType type;
    public int index;
    public int value;
    public int mask;
    public string technique;
}
