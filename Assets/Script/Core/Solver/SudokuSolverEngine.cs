using System.Collections.Generic;

public class SudokuSolverEngine
{
    List<ISudokuTechnique> techniques;

    public SudokuSolverEngine(SudokuGameManager.Difficulty diff)
    {
        techniques = new List<ISudokuTechnique>
        {
            new NakedSingleTechnique(),
            new HiddenSingleTechnique()
        };

        if (diff >= SudokuGameManager.Difficulty.Medium)
            techniques.Add(new PointingPairTechnique());

        if (diff >= SudokuGameManager.Difficulty.Hard)
            techniques.Add(new NakedPairTechnique());

        if (diff >= SudokuGameManager.Difficulty.Expert)
            techniques.Add(new XWingTechnique());
    }

    public bool Step(SudokuContext ctx, out SudokuHint hint)
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