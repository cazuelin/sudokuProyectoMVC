public class SudokuBitMask
{
    int size;
    int[] rows;
    int[] cols;
    int[] boxes;
    public void Clear()//limpia toda las mascaras deja filas,columnas y cajas en cero
    {
        size = SudokuRules.Size;
        if (rows == null || rows.Length != size)
        {
            rows = new int[size];
            cols = new int[size];
            boxes = new int[SudokuRules.TotalBoxCount];
        }
        else
        {
            for (int i = 0; i < size; i++)
            {
                rows[i] = 0;
                cols[i] = 0;
            }
            int boxCount = boxes.Length;
            for (int i = 0; i < boxCount; i++)
            {
                boxes[i] = 0;
            }
        }
    }
    public void Init(int[,] board)//recibe un tablero y registra todos los numeros existentes en las mascaras
    {
        Clear();//limpiar el bitmask
        for (int r = 0; r < size; r++)//recorre todas la filas
            for (int c = 0; c < size; c++)//recorre todas las columnas
            {
                int number = board[r, c];
                if (number != 0)//si encuentra un numero distinto a 0
                    Place(r, c, number);//llama al metodo place
            }
    }
    int GetBoxIndex(int r, int c) => SudokuRules.GetBoxIndex(r, c);//calcula a que caja pertenece una celda
    public bool CanPlace(int r, int c, int n)
    {
        int mask = 1 << (n - 1);//significa mover bits hacia la izquierda
        int box = GetBoxIndex(r, c);

        return (rows[r] & mask) == 0 && // revisa si este bit está encendido en la fila
               (cols[c] & mask) == 0 && // revisa si este bit está encendido en la columna
               (boxes[box] & mask) == 0;// revisa si este bit está encendido en la caja
    }
    public void Place(int r, int c, int n)
    {
        int mask = 1 << (n - 1);//significa mover bits hacia la izquierda
        //1 = 1 << 0 = 000000001
        //2 = 1 << 1 = 000000010
        //3 = 1 << 2 = 000000100
        //4 = 1 << 3 = 000001000
        //5 = 1 << 4 = 000010000
        //6 = 1 << 5 = 000100000
        //7 = 1 << 6 = 001000000
        //8 = 1 << 7 = 010000000
        //9 = 1 << 8 = 100000000
        int box = GetBoxIndex(r, c);

        rows[r] |= mask;//enciende este bit sin apagar los otros enfocado en la fila
        cols[c] |= mask;//enciende este bit sin apagar los otros enfocado en la columna
        boxes[box] |= mask;//enciende este bit sin apagar los otros enfocado en la caja
    }
    public void Remove(int r, int c, int n)
    {
        int mask = ~(1 << (n - 1));// ~ Invierte la máscara para poder borrar.
        int box = GetBoxIndex(r, c);

        rows[r] &= mask;
        cols[c] &= mask;
        boxes[box] &= mask;
    }
}
