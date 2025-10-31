using UnityEngine;
using TMPro;

public class GameClock : MonoBehaviour
{
    [Header("Bağlantılar")]
    public DayNightCycle dayNightCycle;
    public TextMeshProUGUI timeText;

    [Header("Zaman Ayarları")]
    public float dayStartHour = 6f;   // Sabah 06:00
    public float nightStartHour = 19f;// Akşam 19:00

    private float currentTime;        // Gerçek oyun saati (0–24)
    private float timeSpeed;          // Akış hızı (gündüz/gece süresine göre hesaplanır)

    void Start()
    {
        if (dayNightCycle == null)
            dayNightCycle = FindObjectOfType<DayNightCycle>();

        currentTime = dayStartHour; // oyun başında 06:00
        timeSpeed = (nightStartHour - dayStartHour) / dayNightCycle.dayDuration;
    }

    void Update()
    {
        // Gündüz veya geceye göre zamanı ilerlet
        if (dayNightCycle.IsDay)
            currentTime += Time.deltaTime * timeSpeed; // Gündüz akışı
        else
            currentTime += Time.deltaTime * ((24 - nightStartHour + dayStartHour) / dayNightCycle.nightDuration);

        // 24 saati aşarsa başa sar
        if (currentTime >= 24f)
            currentTime -= 24f;

        // 06:00 → 19:00 gündüz, 19:00 → 06:00 gece
        string hourStr = Mathf.FloorToInt(currentTime).ToString("00");
        string minStr = Mathf.FloorToInt((currentTime % 1f) * 60f).ToString("00");
        if (timeText != null)
            timeText.text = $"Saat: {hourStr}:{minStr}";
    }
}
