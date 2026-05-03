using System.Collections.Generic;
public class SudokuHint
{
    public string technique;
    public List<int> highlightCells = new();
    public List<int> affectedCells = new();
    public int candidateMask;
    public List<SudokuAction> actions = new();
}