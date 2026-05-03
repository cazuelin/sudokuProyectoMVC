using UnityEngine;
public class SudokuSaveSlotsPanel : MonoBehaviour
{
    [SerializeField] SudokuMenuSlotsController menuController;

    void Start()
    {
        if (menuController == null)
            Debug.LogWarning("SudokuMenuSlotsController no asignado en SudokuSaveSlotsPanel");
    }
}