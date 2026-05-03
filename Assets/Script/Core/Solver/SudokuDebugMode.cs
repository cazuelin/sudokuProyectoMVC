using UnityEngine;

public static class SudokuDebugMode
{
    // Temporal: activar en true para depurar generación/técnicas.
    public static bool Enabled = false;

    // Si está activo, imprime cada hint aplicado por el solver humano.
    public static bool LogEachTechniqueStep = false;

    public static void Log(string message)
    {
        if (!Enabled)
            return;

        Debug.Log($"[SudokuDebug] {message}");
    }

    public static void Warn(string message)
    {
        if (!Enabled)
            return;

        Debug.LogWarning($"[SudokuDebug] {message}");
    }
}
