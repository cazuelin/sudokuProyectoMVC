public class SudokuBitMask
{
    int[] rows = new int[9];
    int[] cols = new int[9];
    int[] boxes = new int[9];

    // =========================
    // RESET COMPLETO
    // =========================
    public void Clear()
    {
        for (int i = 0; i < 9; i++)
        {
            rows[i] = 0;
            cols[i] = 0;
            boxes[i] = 0;
        }
    }

    int GetBoxIndex(int r, int c) => (r / 3) * 3 + (c / 3);

    public bool CanPlace(int r, int c, int n)
    {
        int mask = 1 << (n - 1);
        int box = GetBoxIndex(r, c);

        return (rows[r] & mask) == 0 &&
               (cols[c] & mask) == 0 &&
               (boxes[box] & mask) == 0;
    }

    public void Place(int r, int c, int n)
    {
        int mask = 1 << (n - 1);
        int box = GetBoxIndex(r, c);

        rows[r] |= mask;
        cols[c] |= mask;
        boxes[box] |= mask;
    }

    public void Remove(int r, int c, int n)
    {
        int mask = ~(1 << (n - 1));
        int box = GetBoxIndex(r, c);

        rows[r] &= mask;
        cols[c] &= mask;
        boxes[box] &= mask;
    }
}