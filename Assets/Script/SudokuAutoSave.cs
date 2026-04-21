using UnityEngine;
public class SudokuAutoSave : MonoBehaviour
{
    [SerializeField] SudokuSaveManager saveManager;
    [SerializeField] SudokuBoardController board;
    float saveDelay = 1.0f;
    float timer;
    bool dirty;
    void Start()
    {
        board.OnBoardChanged += MarkDirty;
    }
    void OnDestroy()
    {
        board.OnBoardChanged -= MarkDirty;
    }
    void MarkDirty()
    {
        dirty = true;
        timer = 0f;
    }
    void Update()
    {
        if (!dirty) return;
        timer += Time.deltaTime;
        if (timer >= saveDelay)
        {
            Save();
            dirty = false;
        }
    }
    void Save()
    {
        int slot = SudokuGameSession.SelectedSlot;
        if (slot < 0) return;
        saveManager.SaveGame(slot);
        Debug.Log("AutoSave ejecutado");
    }
    void OnApplicationPause(bool pause)
    {
        if (pause)
            Save();
    }
    void OnApplicationQuit()
    {
        Save();
    }
}