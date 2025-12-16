using UnityEngine;
using TMPro;

public class GameClock : MonoBehaviour
{
    [Header("Bağlantılar")]
    public DayNightCycle dayNightCycle;
    public TextMeshProUGUI timeText;
    public Transform player; // <<< AUTOSAVE İÇİN EKLENDİ

    [Header("Zaman Ayarları")]
    public float dayStartHour = 6f;    // Oyun 06:00'da başlar
    public float nightStartHour = 19f; // Akşam 19:00

    private float currentTime;   // 0–24 arası gerçek saat
    private float timeSpeed;     // Gündüz hızı
    private float nightSpeed;    // Gece hızı

    // AUTOSAVE
    private bool savedThisMorning = false;
    private const int autoSaveHour = 8;   // 08:00'de kayıt

    void Start()
    {
        if (dayNightCycle == null)
            dayNightCycle = FindObjectOfType<DayNightCycle>();

        // Saat başlangıcı
        currentTime = dayStartHour;

        // Gündüz/gece hızları
        timeSpeed = (nightStartHour - dayStartHour) / dayNightCycle.dayDuration;
        nightSpeed = ((24f - nightStartHour) + dayStartHour) / dayNightCycle.nightDuration;
    }

    private void Awake()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player")?.transform;
            if (player == null) return;
        }

    }



    void Update()
    {


        // ---- ZAMAN İLERLETME ----
        if (dayNightCycle.IsDay)
            currentTime += Time.deltaTime * timeSpeed;
        else
            currentTime += Time.deltaTime * nightSpeed;

        if (currentTime >= 24f)
        {
            currentTime -= 24f;
            savedThisMorning = false; // Yeni gün başladı
        }

        // ---- EKRANDA GÖSTER ----
        int hour = Mathf.FloorToInt(currentTime);
        int minute = Mathf.FloorToInt((currentTime % 1f) * 60f);

        if (timeText != null)
            timeText.text = $"Saat: {hour:00}:{minute:00}";

        // ---- AUTOSAVE ----
        CheckAutoSave(hour, minute);
    }

    private void CheckAutoSave(int hour, int minute)
    {
        // Eğer tam 08:00'deysek ve daha önce kaydetmediysek
        if (!savedThisMorning && hour == autoSaveHour && minute == 0)
        {
            if (player != null)
            {
                SaveSystem.SavePlayerAndInventory(
    player,
    Inventory.Instance,
    FindObjectOfType<PlayerStats>(),
    FindObjectOfType<PlayerWeapon>()
);



                Debug.Log("🟢 08:00 → Otomatik kayıt alındı!");
            }
            else
            {
                Debug.LogWarning("❌ GameClock: Player referansı eksik, autosave yapılamadı!");
            }

            savedThisMorning = true;
        }
    }
}
