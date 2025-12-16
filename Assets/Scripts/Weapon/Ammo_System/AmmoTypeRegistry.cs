using System.Collections.Generic;
using UnityEngine;

public class AmmoTypeRegistry : MonoBehaviour
{
    public static AmmoTypeRegistry Instance { get; private set; }

    [Header("All AmmoTypeData assets")]
    public AmmoTypeData[] ammoTypes;

    private Dictionary<string, AmmoTypeData> map = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        map.Clear();
        if (ammoTypes == null) return;

        foreach (var a in ammoTypes)
        {
            if (a == null || string.IsNullOrEmpty(a.ammoId)) continue;
            map[a.ammoId] = a;
        }
    }

    public static AmmoTypeData Get(string ammoId)
    {
        if (Instance == null || string.IsNullOrEmpty(ammoId)) return null;
        return Instance.map.TryGetValue(ammoId, out var v) ? v : null;
    }
}
