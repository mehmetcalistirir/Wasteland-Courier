using UnityEngine;
using System.Collections.Generic;

public class RoadNetworkDrawer : MonoBehaviour
{
    public GameObject linePrefab;  // LineRenderer prefab (kırmızı çizgi)
    public List<Transform> points = new List<Transform>(); // Manuel ekle

    void Start()
    {
        DrawConnections();
    }

    // --------------------------------------------------
    // Inspector'da verilen noktaları sırayla bağla
    // --------------------------------------------------
    void DrawConnections()
    {
        if (points.Count < 2)
        {
            Debug.LogWarning("⚠ Points listesinde yol çizmek için en az 2 nokta olmalı.");
            return;
        }

        for (int i = 0; i < points.Count - 1; i++)
        {
            CreateLine(points[i].position, points[i + 1].position);
        }
    }

    // --------------------------------------------------
    // Tek çizgi oluştur
    // --------------------------------------------------
    void CreateLine(Vector3 a, Vector3 b)
    {
        GameObject line = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
        LineRenderer lr = line.GetComponent<LineRenderer>();

        lr.positionCount = 2;
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);

        lr.startWidth = 0.15f;
        lr.endWidth = 0.15f;

        lr.startColor = Color.black;
        lr.endColor = Color.brown;

        // Dünya uzayında çalışmalı
        lr.useWorldSpace = true;
    }
}
