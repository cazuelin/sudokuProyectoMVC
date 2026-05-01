public class HiddenSingleTechnique : ISudokuTechnique
{
    public string Name => "Hidden Single";
    public int DifficultyScore => 2;
    public bool TryApply(SudokuContext ctx, out SudokuHint hint)
    {
        // 1) Filas
        for (int r = 0; r < 9; r++)
        {
            if (TryInUnit(ctx, GetRowCellIndex(r), out hint))
                return true;
        }

        // 2) Columnas
        for (int c = 0; c < 9; c++)
        {
            if (TryInUnit(ctx, GetColCellIndex(c), out hint))
                return true;
        }

        // 3) Cajas 3x3
        for (int box = 0; box < 9; box++)
        {
            if (TryInUnit(ctx, GetBoxCellIndex(box), out hint))
                return true;
        }

        hint = null;
        return false;
    }

    bool TryInUnit(SudokuContext ctx, int[] unit, out SudokuHint hint)
    {
        for (int n = 1; n <= 9; n++)
        {
            int bit = 1 << (n - 1);
            int count = 0;
            int lastIndex = -1;

            for (int i = 0; i < unit.Length; i++)
            {
                int index = unit[i];
                int r = index / 9;
                int c = index % 9;

                if (ctx.board[r, c] != 0)
                    continue;

                int mask = ctx.notesMask[index];
                if ((mask & bit) == 0)
                    continue;

                count++;
                lastIndex = index;
                if (count > 1)
                    break;
            }

            if (count == 1)
            {
                hint = new SudokuHint
                {
                    technique = Name,
                    candidateMask = bit
                };
                hint.highlightCells.Add(lastIndex);
                hint.actions.Add(new SudokuAction
                {
                    type = SudokuActionType.Place,
                    index = lastIndex,
                    value = n,
                    technique = Name
                });
                return true;
            }
        }

        hint = null;
        return false;
    }

    int[] GetRowCellIndex(int row)
    {
        int[] unit = new int[9];
        for (int c = 0; c < 9; c++)
            unit[c] = row * 9 + c;
        return unit;
    }

    int[] GetColCellIndex(int col)
    {
        int[] unit = new int[9];
        for (int r = 0; r < 9; r++)
            unit[r] = r * 9 + col;
        return unit;
    }

    int[] GetBoxCellIndex(int box)
    {
        int[] unit = new int[9];
        int startRow = (box / 3) * 3;
        int startCol = (box % 3) * 3;
        int k = 0;
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
                unit[k++] = (startRow + r) * 9 + (startCol + c);
        return unit;
    }
}