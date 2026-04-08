using UnityEngine;

public class SudokuSaveSlotsPanel : MonoBehaviour
{
    [SerializeField] SudokuSaveSlotItem prefab;
    [SerializeField] Transform container;

    [SerializeField] SudokuSaveManager saveManager;
    [SerializeField] SudokuGameFlowController flow;

    [SerializeField] int slotCount = 3;

    void Start()
    {
        Build();
    }

    void Build()
    {
        for (int i = 0; i < slotCount; i++)
        {
            var slot = Instantiate(prefab, container);
            slot.Init(i, saveManager, flow);
        }
    }
}