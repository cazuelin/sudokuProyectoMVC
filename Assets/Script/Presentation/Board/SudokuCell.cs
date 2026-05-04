using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
public class SudokuCell : MonoBehaviour
{
    [Header("Data")]
    public int row;
    public int column;
    [Header("UI")]
    [SerializeField] TMP_Text numberText;
    [SerializeField] Image image;
    [SerializeField] GameObject notesGrid;
    [SerializeField] TMP_Text[] notes;
    [SerializeField] GameObject errorHighlight;
    public static Action<SudokuCell> OnCellClicked;
    public void Render(int value, bool isFixed, int notesMask)
    {
        SetError(false);
        numberText.text = value == 0 ? "" : value.ToString();
        numberText.color = isFixed ? Color.black : Color.blue;
        for (int i = 0; i < 9; i++)
        {
            bool active = (notesMask & (1 << i)) != 0;
            notes[i].text = active ? (i + 1).ToString() : "";
        }
        notesGrid.SetActive(value == 0 && notesMask != 0);
    }
    public void SetHighlight(Color color)
    {
        image.color = color;
    }
    public void OnClick()
    {
        OnCellClicked?.Invoke(this);
    }
    public void SetError(bool active)
    {
        errorHighlight.SetActive(active);
    }
}
