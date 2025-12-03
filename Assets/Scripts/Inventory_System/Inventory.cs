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
    public Dictionary<string, int> ammoStorage = new Dictionary<string, int>();

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
        if (data == null || amount <= 0) return false;

        // ✅ Eğer AmmoItemData ise: slot'a koyma, direkt ammoStorage'a mermi ekle
        if (data is AmmoItemData ammoData)
        {
            int totalAmmo = ammoData.ammoAmount * amount;
            AddAmmo(ammoData.ammoType, totalAmmo);
            Debug.Log($"[Inventory] {ammoData.ammoType} için +{totalAmmo} mermi eklendi.");
            // Slot kullanmadığımız için RaiseChanged çağırmaya gerek yok ama istersen ekleyebilirsin
            return true;
        }

        // Normal stacklenebilir item mantığı
        if (data.stackable)
        {
            int remain = amount;

            // Var olan stack'leri doldur
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

            // Boş slotlara yeni stack aç
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
            // Stacklenmeyen item
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
    public void AddAmmo(string ammoId, int amount)
{
    if (!ammoStorage.ContainsKey(ammoId))
        ammoStorage[ammoId] = 0;

    ammoStorage[ammoId] += amount;

    Debug.Log($"Ammo Added: {ammoId} -> {ammoStorage[ammoId]}");
}


    // Belirli ammo tipinden kaç adet var?
    public int GetAmmoAmount(string ammoType)
    {
        if (string.IsNullOrEmpty(ammoType)) return 0;
        return ammoStorage.TryGetValue(ammoType, out int value) ? value : 0;
    }

    // Ammo tüket (şarjör doldurma vb.)
    public bool TryUseAmmo(string ammoType, int amount)
    {
        if (string.IsNullOrEmpty(ammoType) || amount <= 0) return false;

        int current = GetAmmoAmount(ammoType);
        if (current < amount) return false;

        ammoStorage[ammoType] = current - amount;
        return true;
    }

    // Şarjöre ammo yükleme - ammoStorage'dan çeker
    public void LoadAmmoIntoMag(MagazineItem mag, int amount)
    {
        if (mag == null)
        {
            Debug.LogWarning("LoadAmmoIntoMag: mag == null");
            return;
        }

        if (mag.IsFull)
        {
            Debug.Log("LoadAmmoIntoMag: Şarjör zaten dolu.");
            return;
        }

        string ammoType = mag.ammoType;
        if (string.IsNullOrEmpty(ammoType))
        {
            Debug.LogWarning("LoadAmmoIntoMag: Şarjörün ammoType'ı tanımlı değil!");
            return;
        }

        int available = GetAmmoAmount(ammoType);
        if (available <= 0)
        {
            Debug.Log($"LoadAmmoIntoMag: Envanterde {ammoType} mermisi yok.");
            return;
        }

        int space = mag.capacity - mag.currentAmmo;
        int loadAmount = Mathf.Min(space, amount, available);
        if (loadAmount <= 0)
        {
            Debug.Log("LoadAmmoIntoMag: Yüklenecek mermi yok / yer yok.");
            return;
        }

        // Envanterden mermi düş
        if (!TryUseAmmo(ammoType, loadAmount))
        {
            Debug.LogWarning("LoadAmmoIntoMag: TryUseAmmo başarısız oldu, eşzamanlılık sorunu olabilir.");
            return;
        }

        // Şarjörü doldur
        mag.currentAmmo += loadAmount;

        Debug.Log($"Şarjör {mag.magName} mermi yüklendi: +{loadAmount} ({ammoType}), Envanterde kalan: {GetAmmoAmount(ammoType)}");
    }
}
