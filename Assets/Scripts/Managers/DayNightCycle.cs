// DayNightCycle.cs
using UnityEngine;
using System;
public class DayNightCycle : MonoBehaviour
{

public static DayNightCycle Instance { get; private set; }
public static event System.Action<bool> OnDayNightChanged; // true = day, false = night

public bool IsDay => isDay;



    public float dayDuration = 30f;
    public float nightDuration = 30f;
    private float timer = 0f;

    public LightController lightController;
    public ResourceSpawner spawner;
    public EnemyManager enemyManager;


    [Range(0f, 1f)]
    public float regenerationRatio = 0.5f;

    private bool isDay = true;

    // ✅ SAHNE HER AÇILDIĞINDA ÇAĞRILACAK
    void Awake()
    {
         Instance = this;
        ResetCycle();
    }

    // (İstersen OnEnable’da da güvenceye alabilirsin)
    void OnEnable()
    {
        // ResetCycle();  // Awake yetmiyorsa bunu da aç
    }

    void Update()
{
    timer -= Time.deltaTime;

    if (timer <= 0f)
    {
        isDay = !isDay;
        timer = isDay ? dayDuration : nightDuration;

        OnDayNightChanged?.Invoke(isDay); // ✅ GÜNDEMİZDE BU ÇOK ÖNEMLİ

        if (isDay) HandleDayStart();
        else       HandleNightStart();
    }
}


    // ✅ YENİ: Baştan kurulum
    public void ResetCycle()
    {
        isDay = true;
        timer = dayDuration;

        // Gündüz başlangıç durumunu tüm sistemlere uygula
        lightController?.SetDay(true);
        spawner?.RegenerateResources(0f);      // İstersen 0f; açılışta respawn yapma
        enemyManager?.ResetDayCount();         // Gece sayacını sıfırla

        SetAnimalsNightState(false);

        // MusicManager sahneler arası yaşıyorsa (DontDestroyOnLoad), ilk state’i bildir
        if (MusicManager.Instance != null)
            MusicManager.Instance.SetDay(true);
    }

    void HandleDayStart()
    {
        spawner?.RegenerateResources(regenerationRatio);
        SetAnimalsNightState(false);
        lightController?.SetDay(true);
        MusicManager.Instance?.SetDay(true);
    }

    void HandleNightStart()
    {
        enemyManager?.SpawnEnemies();
        SetAnimalsNightState(true);
        lightController?.SetDay(false);
        MusicManager.Instance?.SetDay(false);
    }

    void SetAnimalsNightState(bool night)
    {
        foreach (var a in FindObjectsOfType<Animal>())
            a.SetNight(night);
    }
}
