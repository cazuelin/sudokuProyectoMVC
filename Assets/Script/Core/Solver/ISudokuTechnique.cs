public interface ISudokuTechnique
{
    bool TryApply(int[,] board, out int row, out int col, out int value);
    string GetName();
}