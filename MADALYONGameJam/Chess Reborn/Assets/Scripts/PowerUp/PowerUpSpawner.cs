using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    public GameObject[] powerUpPrefabs;
    public Vector2 spawnAreaMin;
    public Vector2 spawnAreaMax;

    public float spawnInterval = 10f;

    void Start()
    {
        InvokeRepeating(nameof(SpawnPowerUp), 2f, spawnInterval);
    }

    void SpawnPowerUp()
    {
        if (powerUpPrefabs.Length == 0) return;

        Vector2 pos = new Vector2(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            Random.Range(spawnAreaMax.y, spawnAreaMax.y)
        );

        Instantiate(powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)], pos, Quaternion.identity);
    }
}
