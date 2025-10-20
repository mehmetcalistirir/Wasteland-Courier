using UnityEngine;

public class SpawnZoneManager : MonoBehaviour
{
    [System.Serializable]
    public class ZoneSpawn
    {
        public WanderArea area;
        public GameObject[] enemyPrefabs;
        public int count = 5;
        public bool addWanderer = true;  // Artık EnemyWanderDriver anlamına geliyor
        public float spreadPadding = 0.2f;
    }

    public ZoneSpawn[] zones;

    [ContextMenu("Spawn All Zones")]
    public void SpawnAllZones()
    {
        foreach (var z in zones)
        {
            if (z.area == null || z.enemyPrefabs == null || z.enemyPrefabs.Length == 0) continue;

            for (int i = 0; i < z.count; i++)
            {
                var prefab = z.enemyPrefabs[Random.Range(0, z.enemyPrefabs.Length)];
                Vector2 p = z.area.GetRandomPoint() + Random.insideUnitCircle * z.spreadPadding;

                var go = Instantiate(prefab, p, Quaternion.identity);

                if (!z.addWanderer) continue;

                var enemy = go.GetComponent<Enemy>();
                if (!enemy)
                {
                    Debug.LogWarning("[SpawnZoneManager] Prefab'ta Enemy yok.");
                    continue;
                }

                // Sadece Normal / Fast olan ekstra düşmanları wander yap
                if (enemy.enemyType != EnemyType.Normal && enemy.enemyType != EnemyType.Fast)
                    continue;

                var driver = go.GetComponent<EnemyWanderDriver>();
                if (!driver) driver = go.AddComponent<EnemyWanderDriver>();

                // Alanı bağla ve waypoint'i hemen hedef yap
                driver.Setup(z.area); // ↓ aşağıdaki küçük metodu ekle
            }
        }
    }
}
