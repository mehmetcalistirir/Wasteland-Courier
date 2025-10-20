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
        public int durability;  // dayanıklılık

        
    }

    public WeaponInstancePayload weapon; // sadece Weapon için
    

    public InventoryItem() {}
    public InventoryItem(ItemData data = null, int count = 0)
    {
        this.data = data;
        this.count = count;
        this.weapon = null;
    }
}
