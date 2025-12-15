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

    // Yeni mermi depolama sistemi: ammoType(string) -> toplam mermi sayısı
    private Dictionary<AmmoTypeData, int> ammoPool =
        new Dictionary<AmmoTypeData, int>();

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
        int totalAmmo = ammoData.ammoAmount * amount;
        AddAmmo(ammoData.ammoType, totalAmmo);

        Debug.Log($"[Inventory] {ammoData.ammoType.ammoName} +{totalAmmo}");
        return true;
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

    // ---------------- AMMO SİSTEMİ ----------------

    // Ammo ekleme (pickup vs. burayı kullanacak)
    public void AddAmmo(AmmoTypeData ammoType, int amount)
    {
        if (ammoType == null || amount <= 0) return;

        if (!ammoPool.ContainsKey(ammoType))
            ammoPool[ammoType] = 0;

        ammoPool[ammoType] += amount;

        Debug.Log($"[Ammo] +{amount} {ammoType.ammoName} → Total: {ammoPool[ammoType]}");
    }



    // Belirli ammo tipinden kaç adet var?
    public int GetAmmoAmount(AmmoTypeData ammoType)
    {
        if (ammoType == null) return 0;

        return ammoPool.TryGetValue(ammoType, out int amount)
            ? amount
            : 0;
    }


    // Ammo tüket (şarjör doldurma vb.)
    public bool TryUseAmmo(AmmoTypeData ammoType, int amount)
    {
        if (ammoType == null || amount <= 0) return false;

        if (!ammoPool.ContainsKey(ammoType)) return false;
        if (ammoPool[ammoType] < amount) return false;

        ammoPool[ammoType] -= amount;

        return true;
    }

    public bool FullLoadMagazine(MagazineInstance mag)
{
    if (mag == null || mag.data == null)
        return false;

    int needed = mag.data.capacity - mag.currentAmmo;
    if (needed <= 0)
        return false;

    int available = GetAmmoAmount(mag.data.ammoType);
    if (available <= 0)
        return false;

    int load = Mathf.Min(needed, available);
    return LoadAmmoIntoMagazine(mag, load);
}




    // Şarjöre ammo yükleme - ammoStorage'dan çeker
    public bool LoadAmmoIntoMagazine(
    MagazineInstance mag,
    int amount
)
{
    if (mag == null || mag.data == null)
        return false;

    if (mag.IsFull)
        return false;

    AmmoTypeData ammoType = mag.data.ammoType;
    if (ammoType == null)
        return false;

    int available = GetAmmoAmount(ammoType);
    if (available <= 0)
        return false;

    int space = mag.data.capacity - mag.currentAmmo;
    int load = Mathf.Min(space, Mathf.Min(amount, available));

    if (load <= 0)
        return false;

    // 🔥 AMMO HAVUZUNDAN ÇEK
    TryUseAmmo(ammoType, load);

    // 🔫 ŞARJÖRE KOY
    mag.currentAmmo += load;

    RaiseChanged();

    Debug.Log(
        $"[Inventory] Şarjör dolduruldu → +{load} " +
        $"({mag.currentAmmo}/{mag.data.capacity})"
    );

    return true;
}




}
