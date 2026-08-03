public class SudokuBitMask
{
    int[] rows = new int[9];//guarda los numeros que existen en cada fila
    int[] cols = new int[9];//guarda los numeros que existen en cada columna
    int[] boxes = new int[9];//guarda los numeros que existen en cada caja
    public void Clear()//limpia toda las mascaras deja filas,columnas y cajas en cero
    {
        for (int i = 0; i < 9; i++)
        {
            rows[i] = 0;
            cols[i] = 0;
            boxes[i] = 0;
        }
    }
    public void Init(int[,] board)//recibe un tablero y registra todos los numeros existentes en las mascaras
    {
        Clear();//limpiar el bitmask
        for (int r = 0; r < 9; r++)//recorre todas la filas
            for (int c = 0; c < 9; c++)//recorre todas las columnas
            {
                int number = board[r, c];
                if (number != 0)//si encuentra un numero distinto a 0
                    Place(r, c, number);//llama al metodo place
            }
    }
    int GetBoxIndex(int r, int c) => (r / 3) * 3 + (c / 3);//calcula a que caja 3x3 pertenece una celda
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
