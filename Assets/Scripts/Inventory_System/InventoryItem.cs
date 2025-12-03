using System;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    public ItemData data;
    public int count = 1;

    [Serializable]
    public class WeaponInstancePayload
    {
        public string id;
        public int clip;
        public int reserve;
        public int durability;
    }

    public WeaponInstancePayload weapon; // silahlar için payload

    public bool IsWeapon => weapon != null;

    public InventoryItem() 
    {
        data = null;
        count = 0;
    }

    public InventoryItem(ItemData data, int count = 1)
    {
        this.data = data;
        this.count = count;
    }
}
