
public interface ISudokuTechnique
{
    bool TryApply(SudokuContext ctx, out SudokuHint hint);
    string Name { get; }
}
