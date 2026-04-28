using System.Collections.Generic;
public class SudokuTechniques
{
    List<ISudokuTechnique> techniques;
    public SudokuTechniques(SudokuGameManager.Difficulty difficulty)
    {
        techniques = new List<ISudokuTechnique>
        {
            new NakedSingleTechnique()
        };
        if (difficulty >= SudokuGameManager.Difficulty.Medium)
            techniques.Add(new HiddenSingleTechnique());
        if (difficulty >= SudokuGameManager.Difficulty.Hard)
        {
            techniques.Add(new NakedPairTechnique());
            techniques.Add(new PointingPairTechnique());
        }
        if (difficulty >= SudokuGameManager.Difficulty.Expert)
        {
            techniques.Add(new XWingTechnique());
        }
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