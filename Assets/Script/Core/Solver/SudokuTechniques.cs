using System.Collections.Generic;
public class SudokuTechniques
{
    List<ISudokuTechnique> techniques;
    public SudokuTechniques(SudokuGameManager.Difficulty difficulty)
    {
        techniques = SudokuTechniqueFactory.Build(difficulty);
    }
    public bool GetHint(SudokuContext ctx, out SudokuHint hint)
    {
        foreach (var tech in techniques)
        {
            if (tech.TryApply(ctx, out hint))
                return true;
        }
        hint = null;
        return false;
    }
}