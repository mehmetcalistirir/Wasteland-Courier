using UnityEngine;
using System.Collections.Generic;

public class EnemyArmy : MonoBehaviour
{
    public List<GameObject> army = new List<GameObject>();
    public float orbitRadius = 1.5f;
    public float orbitSpeed = 50f;

    void Update()
    {
        
        OrbitUnits();
    }

    // -------------------------------
    // Orduya piyon ekleme
    // -------------------------------
    public void AddUnit(GameObject p)
    {
        if (!army.Contains(p))
            army.Add(p);
    }

    // -------------------------------
    // Orduyu tamamen boşaltıp piyonları DÖNDÜRMEK YOK!
    // Sadece listeyi boşaltıyoruz.
    // -------------------------------
    public GameObject[] ExtractAll()
    {
        GameObject[] arr = army.ToArray();
        army.Clear();
        return arr;
    }

    // -------------------------------
    // Belirli sayıda piyon yok et
    // -------------------------------
    public void RemovePiyons(int amount)
    {
        amount = Mathf.Min(amount, army.Count);

        for (int i = 0; i < amount; i++)
        {
            var p = army[0];
            if (p != null)
                Destroy(p);

            army.RemoveAt(0);
        }
    }

    // -------------------------------
    // King etrafında halka dönüş animasyonu
    // -------------------------------
    void OrbitUnits()
    {
        if (army.Count == 0) return;

        for (int i = 0; i < army.Count; i++)
        {
            if (army[i] == null) continue;

            float angle = i * (360f / army.Count) + Time.time * orbitSpeed;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad),
                0
            ) * orbitRadius;

            army[i].transform.position = transform.position + offset;
        }
    }

    public int GetCount()
    {
        return army.Count;
    }
}
