public class SudokuContext
{
    public int[,] board;
    public int[] notesMask;

    public bool CanPlace(int r, int c, int n)
    {
        for (int i = 0; i < 9; i++)
        {
            if (board[r, i] == n) return false;
            if (board[i, c] == n) return false;
        }

        int sr = (r / 3) * 3;
        int sc = (c / 3) * 3;

        for (int rr = 0; rr < 3; rr++)
            for (int cc = 0; cc < 3; cc++)
                if (board[sr + rr, sc + cc] == n)
                    return false;

        return true;
    }
}