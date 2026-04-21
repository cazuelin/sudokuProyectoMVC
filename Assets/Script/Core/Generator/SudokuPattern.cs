public static class SudokuPattern 
{
    public static int Pattern(int r, int c)
    {
        return (r * 3 + r / 3 + c) % 9;
    }
}
