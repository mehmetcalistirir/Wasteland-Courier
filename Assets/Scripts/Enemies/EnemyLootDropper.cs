using UnityEngine;
using System.Collections.Generic;

public class EnemyLootDropper : MonoBehaviour
{
    [Header("Drop Chances")]
    [Range(0f, 1f)] public float goldDropChance = 0.25f;
    [Range(0f, 1f)] public float blueprintDropChance = 0.75f;
    [Range(0f, 1f)] public float ammoDropChance = 0.4f;

    [Header("Prefabs")]
    public GameObject goldPrefab;
    public List<GameObject> blueprintPrefabs;
    public List<GameObject> ammoPrefabs;

    public void DropLoot(Vector3 position)
    {
        if (goldPrefab && Random.value < goldDropChance)
            Instantiate(goldPrefab, position, Quaternion.identity);

        if (blueprintPrefabs.Count > 0 && Random.value < blueprintDropChance)
        {
            var bp = blueprintPrefabs[Random.Range(0, blueprintPrefabs.Count)];
            Instantiate(bp, position, Quaternion.identity);
        }

        if (ammoPrefabs.Count > 0 && Random.value < ammoDropChance)
        {
            var ammo = ammoPrefabs[Random.Range(0, ammoPrefabs.Count)];
            Instantiate(ammo, position, Quaternion.identity);
        }
    }
}
