public static class NotesUtil
{
    public static void Toggle(ref int mask, int number)
    {
        mask ^= 1 << (number - 1);
    }
    public static bool Has(int mask, int number)
    {
        return (mask & (1 << (number - 1))) != 0;
    }
    public static void Clear(ref int mask)
    {
        mask = 0;
    }
}
