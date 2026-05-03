using System.Collections.Generic;
public class XWingTechnique : ISudokuTechnique
{
    public string Name => "X-Wing";
    public bool TryApply(SudokuContext ctx, out SudokuHint hint)
    {
        for (int num = 1; num <= 9; num++)
        {
            int mask = 1 << (num - 1);
            if (TryRowBased(ctx, mask, out hint))
                return true;
            if (TryColBased(ctx, mask, out hint))
                return true;
        }
        hint = null;
        return false;
    }

    bool TryRowBased(SudokuContext ctx, int mask, out SudokuHint hint)
    {
        var rowCols = new List<(int row, int c1, int c2)>();
        for (int row = 0; row < 9; row++)
        {
            var cols = GetCandidateColsInRow(ctx, row, mask);
            if (cols.Count == 2)
                rowCols.Add((row, cols[0], cols[1]));
        }

        for (int i = 0; i < rowCols.Count; i++)
        {
            for (int j = i + 1; j < rowCols.Count; j++)
            {
                var a = rowCols[i];
                var b = rowCols[j];
                if (a.c1 != b.c1 || a.c2 != b.c2)
                    continue;

                List<int> affected = new();
                for (int r = 0; r < 9; r++)
                {
                    if (r == a.row || r == b.row)
                        continue;

                    int idx1 = r * 9 + a.c1;
                    int idx2 = r * 9 + a.c2;
                    if (ctx.board[r, a.c1] == 0 && (ctx.notesMask[idx1] & mask) != 0)
                        affected.Add(idx1);
                    if (ctx.board[r, a.c2] == 0 && (ctx.notesMask[idx2] & mask) != 0)
                        affected.Add(idx2);
                }

                if (affected.Count == 0)
                    continue;

                hint = BuildHint(mask, new[]
                {
                    a.row * 9 + a.c1, a.row * 9 + a.c2,
                    b.row * 9 + b.c1, b.row * 9 + b.c2
                }, affected);
                return true;
            }
        }

        hint = null;
        return false;
    }

    bool TryColBased(SudokuContext ctx, int mask, out SudokuHint hint)
    {
        var colRows = new List<(int col, int r1, int r2)>();
        for (int col = 0; col < 9; col++)
        {
            var rows = GetCandidateRowsInCol(ctx, col, mask);
            if (rows.Count == 2)
                colRows.Add((col, rows[0], rows[1]));
        }

        for (int i = 0; i < colRows.Count; i++)
        {
            for (int j = i + 1; j < colRows.Count; j++)
            {
                var a = colRows[i];
                var b = colRows[j];
                if (a.r1 != b.r1 || a.r2 != b.r2)
                    continue;

                List<int> affected = new();
                for (int c = 0; c < 9; c++)
                {
                    if (c == a.col || c == b.col)
                        continue;

                    int idx1 = a.r1 * 9 + c;
                    int idx2 = a.r2 * 9 + c;
                    if (ctx.board[a.r1, c] == 0 && (ctx.notesMask[idx1] & mask) != 0)
                        affected.Add(idx1);
                    if (ctx.board[a.r2, c] == 0 && (ctx.notesMask[idx2] & mask) != 0)
                        affected.Add(idx2);
                }

                if (affected.Count == 0)
                    continue;

                hint = BuildHint(mask, new[]
                {
                    a.r1 * 9 + a.col, a.r2 * 9 + a.col,
                    b.r1 * 9 + b.col, b.r2 * 9 + b.col
                }, affected);
                return true;
            }
        }

        hint = null;
        return false;
    }

    List<int> GetCandidateColsInRow(SudokuContext ctx, int row, int mask)
    {
        List<int> cols = new();
        for (int col = 0; col < 9; col++)
        {
            if (ctx.board[row, col] != 0)
                continue;

            int index = row * 9 + col;
            if ((ctx.notesMask[index] & mask) != 0)
                cols.Add(col);
        }
        return cols;
    }

    List<int> GetCandidateRowsInCol(SudokuContext ctx, int col, int mask)
    {
        List<int> rows = new();
        for (int row = 0; row < 9; row++)
        {
            if (ctx.board[row, col] != 0)
                continue;

            int index = row * 9 + col;
            if ((ctx.notesMask[index] & mask) != 0)
                rows.Add(row);
        }
        return rows;
    }

    SudokuHint BuildHint(int mask, int[] highlights, List<int> affected)
    {
        var hint = new SudokuHint
        {
            technique = Name,
            candidateMask = mask
        };

        for (int i = 0; i < highlights.Length; i++)
            hint.highlightCells.Add(highlights[i]);

        for (int i = 0; i < affected.Count; i++)
        {
            int idx = affected[i];
            hint.affectedCells.Add(idx);
            hint.actions.Add(new SudokuAction
            {
                type = SudokuActionType.RemoveNotes,
                index = idx,
                mask = mask,
                technique = Name
            });
        }

        return hint;
    }
}