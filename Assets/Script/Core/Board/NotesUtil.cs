public static class NotesUtil //sirve para activar y desactivar notas en una celda
{
    public static void Toggle(ref int mask, int number)
    {
        //1 << (number - 1) = Esto crea el bit de la nota.
        //El operador ^ se llama XOR. - XOR sirve para alternar bits:
        //si la nota estaba apagada, la prende
        //si la nota estaba prendida, la apaga
                mask ^= 1 << (number - 1);
    }
}
