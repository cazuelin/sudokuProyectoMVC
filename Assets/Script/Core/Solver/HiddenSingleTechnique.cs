using System.Collections.Generic;

public class HiddenSingleTechnique : ISudokuTechnique
{
    public string Name => "Hidden Single";
    public int DifficultyScore => 2;

    public bool TryApply(SudokuContext ctx, out SudokuHint hint)
    {
        for (int r = 0; r < 9; r++)
        {
            for (int n = 1; n <= 9; n++)
            {
                int count = 0;
                int lastC = -1;

                for (int c = 0; c < 9; c++)
                {
                    if (ctx.board[r, c] == 0 && ctx.CanPlace(r, c, n))
                    {
                        count++;
                        lastC = c;
                    }
                }

                if (count == 1)
                {
                    int index = r * 9 + lastC;

                    hint = new SudokuHint
                    {
                        technique = Name,
                        candidateMask = 1 << (n - 1)
                    };

                    hint.highlightCells.Add(index);

                    hint.actions.Add(new SudokuAction
                    {
                        type = SudokuActionType.Place,
                        index = index,
                        value = n,
                        technique = Name
                    });

                    return true;
                }
            }
        }

        hint = null;
        return false;
    }
}