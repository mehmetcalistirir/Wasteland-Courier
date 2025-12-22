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


    //envantere inspectordan ekleneleri siler
    /*
        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            slots = new InventoryItem[capacity];
            for (int i = 0; i < capacity; i++)
                slots[i] = new InventoryItem();
        }
        */

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // ❗ SADECE boşsa oluştur
        if (slots == null || slots.Length == 0)
        {
            slots = new InventoryItem[capacity];
            for (int i = 0; i < capacity; i++)
                slots[i] = new InventoryItem();
        }
    }

    public bool IsMagazineEquipped(MagazineInstance mag)
    {
        if (mag == null) return false;

        PlayerWeapon pw = FindObjectOfType<PlayerWeapon>();
        if (pw == null) return false;

        return pw.GetCurrentMagazine() == mag;
    }


    // ---------------- ADD ----------------
    public bool TryAdd(ItemData data, int amount = 1)
    {

        if (data == null || amount <= 0)
            return false;

        // 🔥 MAGAZINE ITEM
        if (data is MagazineData magData)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                var s = slots[i];
                if (s?.data != null)
                    Debug.Log($"[INV SLOT] {s.data.itemName} id='{s.data.itemID}' count={s.count}");
                if (slots[i].data == null)
                {
                    slots[i] = new InventoryItem
                    {
                        data = magData,
                        count = 1,
                        magazineInstance = new MagazineInstance(magData)
                    };

                    RaiseChanged();
                    return true;
                }
            }

            return false; // envanter dolu
        }

        // 🔫 AMMO ITEM
        if (data is AmmoItemData ammoData)
        {
            return AddStackableItem(ammoData, amount);
        }


        // 📦 STACKLENEBİLEN ITEM
        if (data.stackable)
        {
            int remain = amount;

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

        // 📦 STACKLENMEYEN NORMAL ITEM
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

    bool AddStackableItem(ItemData data, int amount)
    {
        int remain = amount;

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
    public InventoryItem GetLastAddedSlot()
    {
        for (int i = slots.Length - 1; i >= 0; i--)
        {
            if (slots[i] != null && slots[i].data != null)
                return slots[i];
        }
        return null;
    }

    public bool UnloadOneFromMagazine(MagazineInstance mag)
    {
        if (IsMagazineEquipped(mag))
            return false;

        if (mag == null || mag.data == null)
            return false;

        if (mag.currentAmmo <= 0)
            return false;

        AmmoItemData ammoItem = FindAmmoItem(mag.data.ammoType);
        if (ammoItem == null)
            return false;

        mag.currentAmmo -= 1;

        TryAdd(ammoItem, 1);
        RaiseChanged();
        return true;
    }


    public bool UnloadAllFromMagazine(MagazineInstance mag)
    {
        if (IsMagazineEquipped(mag))
            return false;

        if (mag == null || mag.data == null)
            return false;

        int amount = mag.currentAmmo;
        if (amount <= 0)
            return false;

        AmmoItemData ammoItem = FindAmmoItem(mag.data.ammoType);
        if (ammoItem == null)
            return false;

        TryAdd(ammoItem, amount);


        mag.currentAmmo = 0;
        RaiseChanged();
        return true;
    }

    AmmoItemData FindAmmoItem(AmmoTypeData type)
    {
        foreach (var s in slots)
        {
            if (s.data is AmmoItemData ammo &&
                ammo.ammoType == type)
                return ammo;
        }

        Debug.LogError($"AmmoItemData bulunamadı: {type.ammoName}");
        return null;
    }


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
    // Inventory.cs





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
    public bool TryAddMagazine(MagazineInstance mag)
    {
        if (mag == null || mag.data == null)
            return false;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].data == null)
            {
                slots[i] = new InventoryItem
                {
                    data = mag.data,
                    count = 1,
                    magazineInstance = mag
                };

                RaiseChanged();
                return true;
            }
        }

        Debug.Log("Envanter dolu! Şarjör alınamadı.");
        return false;
    }

    public int GetTotalAmmo(AmmoTypeData type)
    {
        int total = 0;

        foreach (var s in slots)
        {
            if (s.data is AmmoItemData ammo &&
                ammo.ammoType == type)
            {
                total += s.count;

            }
        }

        return total;
    }
    public int TakeAmmoFromInventory(
    AmmoTypeData type,
    int amount
)
    {
        int remaining = amount;

        for (int i = 0; i < slots.Length && remaining > 0; i++)
        {
            var s = slots[i];

            if (s.data is AmmoItemData ammo &&
                ammo.ammoType == type)
            {
                int take = Mathf.Min(s.count, remaining);

                s.count -= take;
                remaining -= take;

                if (s.count <= 0)
                    slots[i] = new InventoryItem();
            }
        }

        RaiseChanged();
        return amount - remaining;
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






    public bool FullLoadMagazine(MagazineInstance mag)
    {
        if (mag == null || mag.data == null)
            return false;

        int needed = mag.data.capacity - mag.currentAmmo;
        if (needed <= 0)
            return false;

        int available = GetTotalAmmo(mag.data.ammoType);
        if (available <= 0)
            return false;

        return LoadAmmoIntoMagazine(mag, needed);
    }




    public bool LoadAmmoIntoMagazine(
    MagazineInstance mag,
    int amount
)
    {
        if (mag == null || mag.data == null)
            return false;

        int space = mag.data.capacity - mag.currentAmmo;
        if (space <= 0)
            return false;

        int taken = TakeAmmoFromInventory(
            mag.data.ammoType,
            Mathf.Min(space, amount)
        );

        if (taken <= 0)
            return false;

        mag.currentAmmo += taken;
        RaiseChanged();
        return true;
    }

    public bool CanAdd(ItemData data, int amount)
    {
        if (data == null || amount <= 0)
            return false;

        // Magazine özel durumu
        if (data is MagazineData)
        {
            // En az 1 boş slot var mı?
            foreach (var s in slots)
                if (s.data == null)
                    return true;

            return false;
        }

        int remain = amount;

        // Stack doldurma
        for (int i = 0; i < slots.Length && remain > 0; i++)
        {
            var s = slots[i];
            if (s.data == data && s.count < data.maxStack)
            {
                int space = data.maxStack - s.count;
                remain -= space;
            }
        }

        // Boş slotlar
        for (int i = 0; i < slots.Length && remain > 0; i++)
        {
            if (slots[i].data == null)
            {
                remain -= data.maxStack;
            }
        }

        return remain <= 0;
    }
    public int GetItemCountByID(string itemID)
    {
        if (string.IsNullOrEmpty(itemID)) return 0;

        int total = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (s == null || s.data == null) continue;

            if (s.data.itemID == itemID)
                total += s.count;
        }
        return total;
    }


    public bool HasEnoughByID(string itemID, int amount)
    {
        int total = 0;

        foreach (var s in slots)
        {
            if (s.data != null && s.data.itemID == itemID)
                total += s.count;
        }

        return total >= amount;
    }

    public bool TryConsumeByID(string itemID, int amount)
    {
        int remaining = amount;

        for (int i = 0; i < slots.Length && remaining > 0; i++)
        {
            var s = slots[i];
            if (s.data != null && s.data.itemID == itemID)
            {
                int take = Mathf.Min(remaining, s.count);
                s.count -= take;
                remaining -= take;

                if (s.count <= 0)
                    slots[i] = new InventoryItem();
            }
        }

        RaiseChanged();
        return remaining == 0;
    }

}
