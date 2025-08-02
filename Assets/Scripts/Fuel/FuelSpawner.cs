using UnityEngine;

public class FuelSpawner : MonoBehaviour
{
    public GameObject fuelPrefab;
    public int fuelCount = 4;
    public Vector2 spawnMin = new Vector2(-10, -5);
    public Vector2 spawnMax = new Vector2(10, 5);

    void Start()
    {
        for (int i = 0; i < fuelCount; i++)
        {
            Vector2 randomPos = new Vector2(
                Random.Range(spawnMin.x, spawnMax.x),
                Random.Range(spawnMin.y, spawnMax.y)
            );

            Instantiate(fuelPrefab, randomPos, Quaternion.identity);
        }
    }
}
