public static class SudokuRules
{
    public enum SudokuVariant
    {
        Variant2x3,
        Standard3x3,
        Variant3x4,
        Standard4x4
    }

    public static SudokuVariant CurrentVariant { get; private set; } = SudokuVariant.Standard3x3;
    public static int Size { get; private set; } = 9;
    public static int BoxRows { get; private set; } = 3;
    public static int BoxCols { get; private set; } = 3;
    public static int BoxSize => BoxRows;
    public static int MaxValue => Size;
    public static int CellCount => Size * Size;
    public static int BoxCount => BoxRows;
    public static int TotalBoxCount => Size;

    public static void SetVariant(SudokuVariant variant)
    {
        switch (variant)
        {
            case SudokuVariant.Variant2x3:
                CurrentVariant = variant;
                Size = 6;
                BoxRows = 2;
                BoxCols = 3;
                break;
            case SudokuVariant.Standard3x3:
                CurrentVariant = variant;
                Size = 9;
                BoxRows = 3;
                BoxCols = 3;
                break;
            case SudokuVariant.Variant3x4:
                CurrentVariant = variant;
                Size = 12;
                BoxRows = 3;
                BoxCols = 4;
                break;
            case SudokuVariant.Standard4x4:
                CurrentVariant = variant;
                Size = 16;
                BoxRows = 4;
                BoxCols = 4;
                break;
            default:
                CurrentVariant = SudokuVariant.Standard3x3;
                Size = 9;
                BoxRows = 3;
                BoxCols = 3;
                break;
        }
    }

    public static void SetVariant(int size, int boxRows, int boxCols)
    {
        if (size <= 0 || boxRows <= 0 || boxCols <= 0 || size != boxRows * boxCols)
            throw new System.ArgumentException("El tamaño debe ser igual a BoxRows * BoxCols.");

        Size = size;
        BoxRows = boxRows;
        BoxCols = boxCols;

        if (size == 6) CurrentVariant = SudokuVariant.Variant2x3;
        else if (size == 9) CurrentVariant = SudokuVariant.Standard3x3;
        else if (size == 12) CurrentVariant = SudokuVariant.Variant3x4;
        else if (size == 16) CurrentVariant = SudokuVariant.Standard4x4;
        else CurrentVariant = SudokuVariant.Standard3x3;
    }

    public static int GetCellIndex(int row, int col) => row * Size + col;
    public static int GetRow(int index) => index / Size;
    public static int GetCol(int index) => index % Size;
    public static int GetBoxIndex(int row, int col) => (row / BoxRows) * BoxRows + (col / BoxCols);

    // Convierte un valor numérico interno a la etiqueta que se muestra en pantalla.
    // Valores 1-9 muestran su número. Valores 10+ muestran letras: 10=A, 11=B, 12=C...
    public static string ValueToLabel(int value)
    {
        if (value <= 0) return string.Empty;
        if (value <= 9) return value.ToString();
        return ((char)('A' + value - 10)).ToString();//10->A, 11->B, 12->C ... 25->P
    }

    // Convierte una etiqueta de pantalla de vuelta al valor numérico interno.
    // "A"->10, "B"->11, ... Devuelve 0 si no es válido.
    public static int LabelToValue(string label)
    {
        if (string.IsNullOrEmpty(label)) return 0;
        if (int.TryParse(label, out int n)) return n;
        char c = char.ToUpper(label[0]);
        if (c >= 'A' && c <= 'Z') return c - 'A' + 10;
        return 0;
    }
}
