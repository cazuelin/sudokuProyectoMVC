using System.Collections.Generic;

public class SudokuTechniques
{
    List<ISudokuTechnique> techniques;

    public SudokuTechniques(SudokuGameManager.Difficulty difficulty)
    {
        techniques = new List<ISudokuTechnique>();

        // EASY
        techniques.Add(new NakedSingleTechnique());

        // MEDIUM
        if (difficulty >= SudokuGameManager.Difficulty.Medium)
            techniques.Add(new HiddenSingleTechnique());

        // FUTURO
        // if (difficulty >= SudokuGameManager.Difficulty.Hard)
        //     techniques.Add(new NakedPairTechnique());
    }

    public bool GetHint(int[,] board, out int row, out int col, out int value, out string techniqueName)
    {
        foreach (var tech in techniques)
        {
            if (tech.TryApply(board, out row, out col, out value))
            {
                techniqueName = tech.GetName();
                return true;
            }
        }

        row = col = value = -1;
        techniqueName = "Sin pista";
        return false;
    }
}

        //Easy     +  NakedSingle
        //Medium   +  HiddenSingle
        //Hard	   +  NakedPair
        //Expert   +  X-Wing
        //Master   +  Swordfish
        //Extreme  +  Todo
