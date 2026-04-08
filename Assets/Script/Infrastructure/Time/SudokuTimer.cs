using TMPro;
using UnityEngine;

public class SudokuTimer : MonoBehaviour
{
    [SerializeField] TMP_Text timerText;

    float timeElapsed;
    bool running;
    int lastSecond = -1;

    void Update()
    {
        if (!running) return;

        timeElapsed += Time.deltaTime;

        int currentSecond = Mathf.FloorToInt(timeElapsed);

        if (currentSecond != lastSecond)
        {
            lastSecond = currentSecond;
            UpdateTimerText();
        }
    }

    void UpdateTimerText()
    {
        int totalSeconds = Mathf.FloorToInt(timeElapsed);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    // =========================
    // CONTROL
    // =========================
    public void StartTimer() => running = true;

    public void StopTimer() => running = false;

    public void ResetTimer()
    {
        timeElapsed = 0;
        UpdateTimerText();
    }

    // =========================
    // DATA
    // =========================
    public float GetTime() => timeElapsed;

    public void SetTime(float time)
    {
        timeElapsed = time;
        UpdateTimerText();
    }
}