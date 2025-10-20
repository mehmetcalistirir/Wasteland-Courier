using System;
using System.Collections.Generic;  
using UnityEngine;


public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    private HashSet<string> unlockedBlueprints = new HashSet<string>();

    [Header("Config")]
    [Min(1)] public int capacity = 30;

    [Header("State")]
    public InventoryItem[] slots;

    // Event: Dışarı sadece abone olunabilir (+= / -=). Tetikleme içeriden yapılır.
    public event Action OnChanged;

    // Dışarıdan tetiklemek gerektiğinde kullanılacak güvenli kapı:
    public void RaiseChanged() => OnChanged?.Invoke();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        slots = new InventoryItem[capacity];
        for (int i = 0; i < capacity; i++) slots[i] = new InventoryItem();
    }

   public bool TryAdd(ItemData data, int amount = 1,
                   InventoryItem.WeaponInstancePayload weaponPayload = null)
{
    if (data == null || amount <= 0) return false;

    // 🔹 AmmoItemData için amount'u "mermi sayısı"na çevir
    if (data is AmmoItemData ammoData)
    {
        amount *= ammoData.ammoPerItem; // 1 item = ammoPerItem mermi
    }

    if (data.stackable)
    {
        int remain = amount;

        // 1) Var olan aynı tür stack’leri doldur
        for (int i = 0; i < slots.Length && remain > 0; i++)
        {
            var s = slots[i];
            if (s.data == data && s.count < data.maxStack)
            {
                int add = Mathf.Min(remain, data.maxStack - s.count);
                s.count += add;
                remain -= add;
            }
        }
        // 2) Boş slotlara yeni stack aç
        for (int i = 0; i < slots.Length && remain > 0; i++)
        {
            var s = slots[i];
            if (s.data == null)
            {
                int add = Mathf.Min(remain, data.maxStack);
                slots[i] = new InventoryItem(data, add);
                remain -= add;
            }
        }

        if (remain == 0) { RaiseChanged(); return true; }
        return false;
    }
    else
    {
        int needed = amount;
        for (int i = 0; i < slots.Length && needed > 0; i++)
        {
            if (slots[i].data == null)
            {
                var item = new InventoryItem(data, 1);
                if (data is WeaponItemData wid)
                {
                    item.weapon = weaponPayload ?? new InventoryItem.WeaponInstancePayload
                    {
                        id = System.Guid.NewGuid().ToString("N"),
                        clip = wid.blueprint?.weaponData?.clipSize ?? 0,
                        reserve = wid.blueprint?.weaponData?.maxAmmoCapacity ?? 0
                    };
                }
                slots[i] = item;
                needed--;
            }
        }
        bool ok = needed == 0;
        if (ok) RaiseChanged();
        return ok;
    }
}

public void UpdateSlot(int index, ItemData newData, InventoryItem.WeaponInstancePayload payload)
{
    if (index < 0 || index >= slots.Length) return;

    if (slots[index] == null)
        slots[index] = new InventoryItem();

    slots[index].data = newData;
    slots[index].count = 1; // Silahlar stacklenmez
    slots[index].weapon = payload;

    OnChanged?.Invoke();
}



public bool HasEnough(ItemData data, int amount)
    {
        if (data == null) return false;
        int total = 0;
        foreach (var s in slots)
            if (s.data == data) total += s.count;
        return total >= amount;
    }

public void UnlockBlueprint(string id)
    {
        if (!string.IsNullOrEmpty(id))
            unlockedBlueprints.Add(id);
    }


public bool HasBlueprint(string id)
    {
        if (string.IsNullOrEmpty(id)) return true;
        return unlockedBlueprints.Contains(id);
    }

public bool TryConsume(ItemData data, int amount)
{
    if (data == null || amount <= 0) return false;

    // 🔹 AmmoItemData ise, resourceType'a göre eşleştir
    bool IsSameItem(InventoryItem s)
    {
        if (s.data == null) return false;

        if (data is AmmoItemData ammoA && s.data is AmmoItemData ammoB)
            return ammoA.resourceType == ammoB.resourceType;

        return s.data == data;
    }

    // 🔹 Envanterde yeterli sayıda var mı?
    int total = 0;
    foreach (var s in slots)
        if (IsSameItem(s))
            total += s.count;

    if (total < amount)
        return false;

    // 🔹 Tüketme işlemi
    int remain = amount;
    for (int i = 0; i < slots.Length && remain > 0; i++)
    {
        var s = slots[i];
        if (IsSameItem(s))
        {
            int take = Mathf.Min(remain, s.count);
            s.count -= take;
            remain -= take;
            if (s.count <= 0)
                Clear(i);
        }
    }

    RaiseChanged();
    return true;
}


public int GetTotalCount(ItemData data)
{
    if (data == null) return 0;
    int total = 0;
    foreach (var s in slots)
        if (s.data == data) total += s.count;
    return total;
}


    public bool TryRemoveAt(int index, int amount = 1)
    {
        if (!Valid(index)) return false;
        var s = slots[index];
        if (s.data == null) return false;

        if (s.data.stackable)
        {
            s.count -= amount;
            if (s.count <= 0) Clear(index);
        }
        else
        {
            Clear(index);
        }
        RaiseChanged();
        return true;
    }

    public void Clear(int index) { slots[index] = new InventoryItem(); }

    public bool TryMoveOrMerge(int from, int to)
    {
        if (!Valid(from) || !Valid(to) || from == to) return false;

        var a = slots[from];
        var b = slots[to];
        if (a.data == null) return false;

        if (b.data == null)
        {
            slots[to] = a;
            slots[from] = new InventoryItem();
            RaiseChanged();
            return true;
        }

        if (a.data == b.data && a.data.stackable)
        {
            int move = Mathf.Min(a.count, a.data.maxStack - b.count);
            b.count += move;
            a.count -= move;
            if (a.count <= 0) slots[from] = new InventoryItem();
            RaiseChanged();
            return true;
        }

        slots[to] = a;
        slots[from] = b;
        RaiseChanged();
        return true;
    }

    private bool Valid(int i) => i >= 0 && i < slots.Length;
}
