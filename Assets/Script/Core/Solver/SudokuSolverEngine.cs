using System.Collections.Generic;
public class SudokuSolverEngine
{
    List<ISudokuTechnique> techniques;
    public SudokuSolverEngine(SudokuGameManager.Difficulty diff)
    {
        techniques = SudokuTechniqueFactory.Build(diff);
    }
    public List<string> GetTechniqueNames()
    {
        var names = new List<string>(techniques.Count);
        foreach (var t in techniques)
            names.Add(t.Name);
        return names;
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