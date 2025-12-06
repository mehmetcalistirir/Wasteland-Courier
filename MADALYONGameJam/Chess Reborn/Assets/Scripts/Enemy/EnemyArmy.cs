using UnityEngine;
using System.Collections.Generic;

public class EnemyArmy : MonoBehaviour
{
    public List<GameObject> army = new List<GameObject>();

    [Header("Visual Settings")]
    public GameObject enemyPiyonVisualPrefab;
    public Transform enemyKing;
    public float orbitRadius = 2.4f;
    public float wobbleAmount = 0.3f;
    public float wobbleSpeed = 1.5f;

    private List<Transform> visualUnits = new List<Transform>();

    void Update()
{
    if (enemyKing == null) return;

    // VISUAL SENKRONİZASYON
    while (visualUnits.Count < army.Count)
        SpawnVisualPiyon();

    while (visualUnits.Count > army.Count)
    {
        Destroy(visualUnits[0].gameObject);
        visualUnits.RemoveAt(0);
    }

    // PLAYERKING İLE AYNI: RING FORMASYONU
    for (int i = 0; i < visualUnits.Count; i++)
    {
        int ring, indexInRing, ringSize;
        HesaplaRingBilgisi(i, out ring, out indexInRing, out ringSize);

        float radius = 1.5f + ring * 1.0f; // Player ile aynı baseRadius/radiusStep
        float angle = (float)indexInRing / ringSize * Mathf.PI * 2f;

        Vector3 hedefPoz = enemyKing.position +
            new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;

        // PlayerKing’teki gibi YUMUŞAK hareket
        visualUnits[i].position =
            Vector3.Lerp(visualUnits[i].position, hedefPoz, Time.deltaTime * 5f);
    }
}

void HesaplaRingBilgisi(int index, out int ring, out int indexInRing, out int ringSize)
{
    ring = 0;
    ringSize = 8;  // ilk halka 8
    int count = index;

    while (count >= ringSize)
    {
        count -= ringSize;
        ring++;
        ringSize *= 2;
    }

    indexInRing = count;
}


    // -----------------------------
    // VISUAL SPAWN
    // -----------------------------
    void SpawnVisualPiyon()
    {
        if (enemyPiyonVisualPrefab == null)
        {
            Debug.LogError("EnemyArmy: enemyPiyonVisualPrefab atanmadı!");
            return;
        }

        GameObject go = Instantiate(enemyPiyonVisualPrefab);
        go.transform.localScale = Vector3.one * 0.9f;
        visualUnits.Add(go.transform);
    }

    // -----------------------------
    // UNIT ADD
    // -----------------------------
    public void AddUnit(GameObject p)
    {
        if (!army.Contains(p))
        {
            army.Add(p);
            SpawnVisualPiyon();
        }
    }

    // -----------------------------
    // REMOVE ALL
    // -----------------------------
    public GameObject[] ExtractAll()
    {
        GameObject[] arr = army.ToArray();

        foreach (GameObject p in arr)
            if (p != null) Destroy(p);

        army.Clear();

        foreach (var vis in visualUnits)
            if (vis != null) Destroy(vis.gameObject);
        visualUnits.Clear();

        return arr;
    }

    // -----------------------------
    // REMOVE SOME
    // -----------------------------
    public void RemovePiyons(int amount)
    {
        amount = Mathf.Min(amount, army.Count);

        for (int i = 0; i < amount; i++)
        {
            var p = army[0];
            if (p != null) Destroy(p.gameObject);
            army.RemoveAt(0);

            if (visualUnits.Count > 0)
            {
                Destroy(visualUnits[0].gameObject);
                visualUnits.RemoveAt(0);
            }
        }
    }

    public int GetCount()
    {
        return army.Count;
    }
}
