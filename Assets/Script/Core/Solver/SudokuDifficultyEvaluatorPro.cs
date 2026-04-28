using System.Collections.Generic;
public class SudokuDifficultyEvaluatorPro
{
    public SudokuGameManager.Difficulty Evaluate(List<string> techniques)
    {
        bool hasXWing = techniques.Contains("X-Wing");
        bool hasPairs = techniques.Contains("Naked Pair");
        bool hasPointing = techniques.Contains("Pointing Pair");
        if (hasXWing)
            return SudokuGameManager.Difficulty.Expert;
        if (hasPairs || hasPointing)
            return SudokuGameManager.Difficulty.Hard;
        if (techniques.Count > 30)
            return SudokuGameManager.Difficulty.Medium;
        return SudokuGameManager.Difficulty.Easy;
    }
}