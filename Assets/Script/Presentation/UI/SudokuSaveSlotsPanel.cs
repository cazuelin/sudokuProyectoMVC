using UnityEngine;
public class SudokuSaveSlotsPanel : MonoBehaviour
{
    [SerializeField] SudokuSaveSlotItem prefab;
    [SerializeField] Transform container;
    [SerializeField] SudokuSaveManager saveManager;
    [SerializeField] int slotCount = 3;
    void Start()
    {
        Build();
    }
    void Build()
    {
        foreach (Transform child in container)
            Destroy(child.gameObject);
        for (int i = 0; i < slotCount; i++)
        {
            var slot = Instantiate(prefab, container);
            slot.Init(i, saveManager);
        }
    }
}