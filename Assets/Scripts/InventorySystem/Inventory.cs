using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    [Header("Config")]
    [Min(1)] public int capacity = 30;

    [Header("State")]
    public InventoryItem[] slots;

    // Blueprint kilitleri (turret / craft için)
    private HashSet<string> unlockedBlueprints = new HashSet<string>();

    public event Action OnChanged;
    public void RaiseChanged() => OnChanged?.Invoke();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        slots = new InventoryItem[capacity];
        for (int i = 0; i < capacity; i++)
            slots[i] = new InventoryItem();
    }

    // ---------------- ADD ----------------
    public bool TryAdd(ItemData data, int amount = 1)
    {
        if (data == null || amount <= 0) return false;

        // Ammo ise: item sayısını mermi sayısına çevir
        if (data is AmmoItemData ammo)
            amount *= ammo.ammoPerItem;

        if (data.stackable)
        {
            int remain = amount;

            // var olan stack'leri doldur
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

            // boş slotlara yeni stack aç
            for (int i = 0; i < slots.Length && remain > 0; i++)
            {
                if (slots[i].data == null)
                {
                    int add = Mathf.Min(remain, data.maxStack);
                    slots[i] = new InventoryItem(data, add);
                    remain -= add;
                }
            }

            if (remain == 0)
            {
                RaiseChanged();
                return true;
            }

            return false;
        }
        else
        {
            // stacklenmeyen item (ama artık silah koymuyoruz)
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].data == null)
                {
                    slots[i] = new InventoryItem(data, 1);
                    RaiseChanged();
                    return true;
                }
            }
            return false;
        }
    }

    // --------- YARDIMCI SAYIM METOTLARI ---------

    public int GetTotalCount(ItemData data)
    {
        if (data == null) return 0;
        int total = 0;
        foreach (var s in slots)
            if (s.data == data)
                total += s.count;
        return total;
    }

    // 👇 HATALARIN İSTEDİĞİ METOTLAR

    // Envanterde yeterince var mı?
    public bool HasEnough(ItemData data, int amount)
    {
        if (data == null || amount <= 0) return false;
        return GetTotalCount(data) >= amount;
    }

    // Blueprint kilidi aç
    public void UnlockBlueprint(string id)
    {
        if (!string.IsNullOrEmpty(id))
            unlockedBlueprints.Add(id);
    }

    // Blueprint var mı?
    public bool HasBlueprint(string id)
    {
        if (string.IsNullOrEmpty(id)) return true; // id yoksa serbest
        return unlockedBlueprints.Contains(id);
    }

    // ---------------- CONSUME / REMOVE ----------------

    public bool TryConsume(ItemData data, int amount)
    {
        if (data == null || amount <= 0) return false;

        int total = GetTotalCount(data);
        if (total < amount) return false;

        int remain = amount;

        for (int i = 0; i < slots.Length && remain > 0; i++)
        {
            var s = slots[i];
            if (s.data == data)
            {
                int take = Mathf.Min(remain, s.count);
                s.count -= take;
                remain -= take;

                if (s.count <= 0)
                    slots[i] = new InventoryItem();
            }
        }

        RaiseChanged();
        return true;
    }

    public bool TryMoveOrMerge(int from, int to)
    {
        if (from == to ||
            from < 0 || from >= slots.Length ||
            to < 0 || to >= slots.Length)
            return false;

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

            if (a.count <= 0)
                slots[from] = new InventoryItem();

            RaiseChanged();
            return true;
        }

        // swap
        slots[to] = a;
        slots[from] = b;
        RaiseChanged();
        return true;
    }

    public void ClearInventory()
    {
        for (int i = 0; i < slots.Length; i++)
            slots[i] = new InventoryItem();
        RaiseChanged();
    }
    public int GetItemCount(ItemData item)
{
    if (item == null) return 0;

    int total = 0;

    // slots dizin ise:
    for (int i = 0; i < slots.Length; i++)
    {
        InventoryItem invItem = slots[i];
        if (invItem == null || invItem.data == null) 
            continue;

        if (invItem.data == item)
            total += invItem.count;
    }

    return total;
}

}
