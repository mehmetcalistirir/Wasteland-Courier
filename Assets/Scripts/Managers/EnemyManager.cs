using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Düşman Ayarları")]
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;

    [Header("Gece Başına Ayarlar")]
    public int baseEnemyCount = 5;
    public float enemyCountIncreasePerDay = 2f;
    private int currentDay = 1;

    public SpawnZoneManager spawnZoneManager; // Inspector’dan sürükle bırak


    // 🔹 EK: Belirli bölgelerde ekstra GEZGİN (sadece Normal/Fast)
    [System.Serializable]
    public class ExtraWanderGroup
    {
        public WanderArea area;                 // Bölge
        public GameObject[] wanderPrefabs;      // Sadece EnemyType.Normal / EnemyType.Fast olan prefabler
        public int extraCount = 3;              // Kaç tane
    }
    public ExtraWanderGroup[] extraWanderGroups;

    public void SpawnEnemies()
    {
        if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0)
        {
            Debug.LogWarning("❌ EnemyManager: Prefab veya spawn noktası eksik!");
            return;
        }

        int total = Mathf.RoundToInt(baseEnemyCount + (currentDay - 1) * enemyCountIncreasePerDay);
        Debug.Log($"🌙 Gece {currentDay}. Toplam düşman: {total}");

        // 🔸 1) NORMAL spawnlar (kovalamaya devam eder)
        for (int i = 0; i < total; i++)
        {
            int enemyIndex = Random.Range(0, enemyPrefabs.Length);
            int spawnPointIndex = i % spawnPoints.Length;
            Instantiate(enemyPrefabs[enemyIndex], spawnPoints[spawnPointIndex].position, Quaternion.identity);
        }

        // 🔹 2) EKSTRA gezginler (yalnızca Normal/Fast)
        SpawnExtraWanderers();

        currentDay++;

        if (spawnZoneManager != null)
            spawnZoneManager.SpawnAllZones();

    }

    private void SpawnExtraWanderers()
    {
        if (extraWanderGroups == null) return;

        foreach (var g in extraWanderGroups)
        {
            if (g == null || g.area == null || g.wanderPrefabs == null || g.wanderPrefabs.Length == 0) continue;

            for (int i = 0; i < g.extraCount; i++)
            {
                var prefab = g.wanderPrefabs[Random.Range(0, g.wanderPrefabs.Length)];
                Vector2 pos = g.area.GetRandomPoint();
                var go = Instantiate(prefab, pos, Quaternion.identity);

                // Tip kontrolü (sadece Normal/Fast olmalı)
                var e = go.GetComponent<Enemy>();
                if (e == null)
                {
                    Debug.LogWarning("[EnemyManager] Ekstra wander prefabında Enemy component yok.");
                    continue;
                }
                if (e.enemyType != EnemyType.Normal && e.enemyType != EnemyType.Fast)
                {
                    Debug.LogWarning($"[EnemyManager] {go.name} enemyType={e.enemyType}. Ekstra wander için Normal/Fast seçin.");
                }

                // GEZGİN sürücüyü ekle ve alanı bağla
                var driver = go.GetComponent<EnemyWanderDriver>();
                if (!driver) driver = go.AddComponent<EnemyWanderDriver>();
                driver.area = g.area;
            }
        }
    }

    public void ResetDayCount() { currentDay = 1; }
}
