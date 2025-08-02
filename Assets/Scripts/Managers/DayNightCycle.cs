using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public float dayDuration = 30f;
    public float nightDuration = 30f;
    private float timer = 0f;

    private bool isDay = true;
    public GameObject[] enemyPrefabs; // Inspector'dan atanır

    public ResourceSpawner spawner;  // Kaynak spawn sistemi
    [Range(0f, 1f)]
    public float regenerationRatio = 0.5f; // Sabah kaynakların ne kadarı yenilensin

    public Transform[] spawnPoints; // Düşman spawn noktaları

    void Start()
    {
        isDay = true;
        timer = dayDuration;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            isDay = !isDay;

            if (isDay)
            {
                Debug.Log("☀️ Sabah oldu - Kaynaklar yenileniyor");
                spawner.RegenerateResources(regenerationRatio);
                SetAnimalsNightState(false);
                timer = dayDuration;
            }
            else
            {
                Debug.Log("🌙 Gece başladı - Düşmanlar geliyor!");
                SpawnEnemies();
                SetAnimalsNightState(true);
                timer = nightDuration;
            }
        }
    }
    void SetAnimalsNightState(bool isNight)
    {
        Animal[] allAnimals = FindObjectsOfType<Animal>();
        foreach (var animal in allAnimals)
        {
            animal.SetNight(isNight);
        }
    }



    void SpawnEnemies()
    {
        foreach (Transform point in spawnPoints)
        {
            int index = Random.Range(0, enemyPrefabs.Length);
            GameObject enemy = Instantiate(enemyPrefabs[index], point.position, Quaternion.identity);
        }
    }

}
