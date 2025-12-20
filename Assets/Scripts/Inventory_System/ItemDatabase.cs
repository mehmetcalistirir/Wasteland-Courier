using System.Collections.Generic;
using UnityEngine;

public static class ItemDatabase
{
    private static Dictionary<string, ItemData> dict;

    public static void RegisterAll(ItemData[] items)
    {
        dict = new Dictionary<string, ItemData>();

        if (items == null)
        {
            Debug.LogError("❌ ItemDatabase: items array NULL!");
            return;
        }

        foreach (var item in items)
        {
            if (item == null)
            {
                Debug.LogWarning("⚠️ ItemDatabase: NULL item atlandı.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(item.itemID))
            {
                Debug.LogError($"❌ Item '{item.name}' için itemID atanmadı!");
                continue;
            }

            if (dict.ContainsKey(item.itemID))
            {
                Debug.LogError($"❌ Duplicate itemID bulundu: {item.itemID}");
                continue;
            }

            dict.Add(item.itemID, item);
        }

        Debug.Log($"ItemDatabase → {dict.Count} item yüklendi.");
    }


    public static ItemData Get(string id)
    {
        if (dict == null)
        {
            Debug.LogError("❌ ItemDatabase: RegisterAll daha çağrılmadı!");
            return null;
        }

        if (dict.TryGetValue(id, out var data))
            return data;

        Debug.LogError($"❌ Item bulunamadı → ID = {id}");
        return null;
    }
}
