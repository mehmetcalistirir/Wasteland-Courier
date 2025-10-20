using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    public GameObject stonePrefab;
    public GameObject woodPrefab;
    public GameObject scrapMetalPrefab;

    public int stoneCount = 5;
    public int woodCount = 3;
    public int scrapMetalCount = 2;

    public Vector2 spawnAreaMin = new Vector2(-10, -5);
    public Vector2 spawnAreaMax = new Vector2(10, 5);

    private Transform resourceParent;

    void Start()
    {
        // Resources adlı GameObject'i bul
        GameObject container = GameObject.Find("Resources(Silme)");
        if (container != null)
        {
            resourceParent = container.transform;
        }
        else
        {
            Debug.LogWarning("Resources GameObject bulunamadı! Kök objeye ekleniyor.");
        }

        SpawnResources(stonePrefab, stoneCount);
        SpawnResources(woodPrefab, woodCount);
        SpawnResources(scrapMetalPrefab, scrapMetalCount);
    }

    void SpawnResources(GameObject prefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 randomPos = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );

            // Parent olarak Resources objesini ata
            GameObject spawned = Instantiate(prefab, randomPos, Quaternion.identity);
            if (resourceParent != null)
                spawned.transform.parent = resourceParent;
        }
    }
    public void RegenerateResources(float ratio)
    {
        int newStones = Mathf.RoundToInt(stoneCount * ratio);
        int newWoods = Mathf.RoundToInt(woodCount * ratio);
        int newMeteors = Mathf.RoundToInt(scrapMetalCount * ratio);

        SpawnResources(stonePrefab, newStones);
        SpawnResources(woodPrefab, newWoods);
        SpawnResources(scrapMetalPrefab, newMeteors);
    }

}
