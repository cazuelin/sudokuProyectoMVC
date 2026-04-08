using System.IO;
using UnityEngine;

public class SaveService
{
    string GetPath(int slot)
    {
        return Application.persistentDataPath + $"/sudoku_save_{slot}.json";
    }

    public void Save(SudokuSaveData data, int slot)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(slot), json);
    }

    public bool Load(int slot, out SudokuSaveData data)
    {
        string path = GetPath(slot);

        if (!File.Exists(path))
        {
            data = null;
            return false;
        }

        string json = File.ReadAllText(path);
        data = JsonUtility.FromJson<SudokuSaveData>(json);
        return true;
    }

    public void Delete(int slot)
    {
        string path = GetPath(slot);

        if (File.Exists(path))
            File.Delete(path);
    }

    public bool Exists(int slot)
    {
        return File.Exists(GetPath(slot));
    }
}