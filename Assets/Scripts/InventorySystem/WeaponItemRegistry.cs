using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Weapon Item Registry")]
public class WeaponItemRegistry : ScriptableObject
{
    public List<WeaponItemData> entries = new();

    public WeaponItemData FindByBlueprint(WeaponBlueprint bp)
    {
        if (bp == null) return null;
        for (int i = 0; i < entries.Count; i++)
            if (entries[i] != null && entries[i].blueprint == bp) return entries[i];
        return null;
    }
}
