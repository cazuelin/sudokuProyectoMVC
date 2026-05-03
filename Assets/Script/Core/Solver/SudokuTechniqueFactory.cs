using System.Collections.Generic;

public static class SudokuTechniqueFactory
{
    public static List<ISudokuTechnique> Build(SudokuGameManager.Difficulty difficulty)
    {
        var techniques = new List<ISudokuTechnique>();

        // Easy: solo Naked Single
        if (difficulty >= SudokuGameManager.Difficulty.Easy)
            techniques.Add(new NakedSingleTechnique());

        // Medium: agrega Hidden Single
        if (difficulty >= SudokuGameManager.Difficulty.Medium)
            techniques.Add(new HiddenSingleTechnique());

        // Hard: agrega Naked Pair
        if (difficulty >= SudokuGameManager.Difficulty.Hard)
            techniques.Add(new NakedPairTechnique());

        // Expert: agrega Pointing Pair
        if (difficulty >= SudokuGameManager.Difficulty.Expert)
            techniques.Add(new PointingPairTechnique());

        // Extreme: agrega X-Wing
        if (difficulty >= SudokuGameManager.Difficulty.Extreme)
            techniques.Add(new XWingTechnique());

        return techniques;
    }
}
