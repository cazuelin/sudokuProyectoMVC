using System.Collections.Generic;
public class SudokuDifficultyEvaluatorPro
{
    public SudokuGameManager.Difficulty Evaluate(List<string> techniques)
    {
        if (techniques == null || techniques.Count == 0)
            return SudokuGameManager.Difficulty.Easy;

        bool hasXWing = techniques.Contains("X-Wing");
        bool hasPairs = techniques.Contains("Naked Pair");
        bool hasPointing = techniques.Contains("Pointing Pair");
        if (hasXWing)
            return SudokuGameManager.Difficulty.Extreme;
        if (hasPointing)
            return SudokuGameManager.Difficulty.Expert;
        if (hasPairs)
            return SudokuGameManager.Difficulty.Hard;
        if (techniques.Contains("Hidden Single"))
            return SudokuGameManager.Difficulty.Medium;
        return SudokuGameManager.Difficulty.Easy;
    }
}