using UnityEngine;
using System.Collections.Generic;

public class CaravanInventory : MonoBehaviour
{
    public static CaravanInventory Instance { get; private set; }

    [Header("Starting Equipped Blueprints (optional)")]
    public List<WeaponBlueprint> startingWeapons;

    [Header("Starting Stored Weapons")]
    public List<WeaponBlueprint> startingStoredWeapons;

    // === Instance tabanlı depo ===
    // key = weaponSlotIndexToUnlock (tür), value = o türde depodaki kopyalar
    private readonly Dictionary<int, List<WeaponInstance>> storageByType = new();
    // Hangi slotta şu an hangi instance takılı?
    private readonly Dictionary<int, WeaponInstance> equippedInstanceByType = new();
    // "Machinegun 1, 2, 3..." üretmek için sayaç
    private readonly Dictionary<int, int> serialByType = new();

    public struct AmmoState
    {
        public int clip;
        public int reserve;
        public AmmoState(int c, int r) { clip = c; reserve = r; }
        public static AmmoState Full(WeaponData d) =>
            new AmmoState(d ? d.clipSize : 0, d ? d.maxAmmoCapacity : 0);
    }

    public class WeaponInstance
    {
        public string id;               // benzersiz kimlik
        public string displayName;      // örn. "Machinegun 2"
        public WeaponBlueprint blueprint;
        public AmmoState ammo;
    }

    private int TypeKey(WeaponBlueprint bp) => bp ? bp.weaponSlotIndexToUnlock : -1;

    private int NextSerial(int type)
    {
        serialByType.TryGetValue(type, out int n);
        n++;
        serialByType[type] = n;
        return n;
    }

    void Awake()
    {
        if (Instance == null) Instance = this; else { Destroy(gameObject); return; }

        // Başlangıç depoya FULL kopyalar ekle (isteğe bağlı)
        if (startingStoredWeapons != null)
        {
            foreach (var bp in startingStoredWeapons)
                if (bp != null) StoreWeaponInstance(MakeFreshInstance(bp));
        }
    }

    void Start()
    {
        // Başlangıçta oyuncuya takılacaklar (instance olarak işaretle)
        var wsm = WeaponSlotManager.Instance;
    if (wsm != null)
    {
        var equipped = wsm.GetEquippedBlueprints();
        for (int i = 0; i < equipped.Length; i++)
        {
            var bp = equipped[i];
            if (bp == null) continue;

            if (!equippedInstanceByType.TryGetValue(i, out var inst) || inst == null)
            {
                equippedInstanceByType[i] = new WeaponInstance
                {
                    id = System.Guid.NewGuid().ToString("N"),
                    displayName = bp.weaponName,
                    blueprint = bp,
                    ammo = AmmoState.Full(bp.weaponData)
                };
            }
        }
    }
    }

    // === Dışarıdan UI'nın ihtiyacı olan basit yardımcılar ===
    public int GetStoredCountForType(int typeKey)
        => storageByType.TryGetValue(typeKey, out var list) ? list.Count : 0;

    public string GetActiveInstanceNameForSlot(int typeKey)
        => equippedInstanceByType.TryGetValue(typeKey, out var inst) ? inst.displayName : null;

    // === Craft sonucu: yeni instance üret + depoya koy ===
    public bool StoreCraftResult(WeaponBlueprint bp)
    {
        if (!bp) return false;
        var inst = MakeFreshInstance(bp);
        StoreWeaponInstance(inst);
        Debug.Log($"[Craft] {inst.displayName} depoya eklendi (FULL).");
        return true;
        
    }

    // FIFO: aktif tür için depodaki ilk kopyayla değiştir
    public void SwapNextStoredForActiveType()
    {
        var wsm = WeaponSlotManager.Instance;
        int slot = (wsm != null) ? wsm.activeSlotIndex : -1;
        if (slot < 0) { Debug.LogWarning("[SWAP] aktif slot yok"); return; }

        if (!storageByType.TryGetValue(slot, out var list) || list.Count == 0)
        { Debug.LogWarning("[SWAP] depoda bu türden kopya yok"); return; }

        if (wsm.activeWeapon != null && wsm.activeWeapon.IsReloading())
            wsm.activeWeapon.StopAllCoroutines();

        // Çıkacak (eldeki) kopyayı ve canlı mermiyi al
        equippedInstanceByType.TryGetValue(slot, out var outgoing);
        var (clipNow, reserveNow) = wsm.GetAmmoStateForSlot(slot);
        var currentBp = wsm.GetBlueprintForSlot(slot);

        // Depodan gelecek kopyayı al
        var incoming = list[0];
        list.RemoveAt(0);

        // 1) Gelen kopyanın blueprint’ini aktif slota tak
        wsm.EquipBlueprintIntoActiveSlot(incoming.blueprint);
        // 2) Gelen kopyanın kayıtlı mermisini yaz
        wsm.SetAmmoStateForSlot(slot, incoming.ammo.clip, incoming.ammo.reserve);
        // 3) UI + silaha zorla bastır
        wsm.ForceReapplyActiveAmmo();

        // 4) Çıkan kopyayı depoya geri koy (kendi canlı mermisiyle)
        if (currentBp != null)
        {
            if (outgoing == null || outgoing.blueprint != currentBp)
            {
                // ilk kez geliyor olabilir → ad-hoc bir instance oluştur
                outgoing = new WeaponInstance
                {
                    id = System.Guid.NewGuid().ToString("N"),
                    displayName = currentBp.weaponName,
                    blueprint = currentBp
                };
            }
            outgoing.ammo = new AmmoState(clipNow, reserveNow);
            StoreWeaponInstance(outgoing);
        }

        // 5) Gelen artık elde takılı kopyadır
        equippedInstanceByType[slot] = incoming;

        Debug.Log($"[Swap] slot {slot} ⇐ {incoming.displayName} {incoming.ammo.clip}/{incoming.ammo.reserve}");
    }

    // === İç yardımcılar ===
    private WeaponInstance MakeFreshInstance(WeaponBlueprint bp)
    {
        int t = TypeKey(bp);
        int serial = NextSerial(t);
        return new WeaponInstance
        {
            id = System.Guid.NewGuid().ToString("N"),
            displayName = $"{bp.weaponName} {serial}",
            blueprint = bp,
            ammo = AmmoState.Full(bp.weaponData)
        };
    }

    private void StoreWeaponInstance(WeaponInstance inst)
    {
        int t = TypeKey(inst.blueprint);
        if (!storageByType.TryGetValue(t, out var list))
        {
            list = new List<WeaponInstance>();
            storageByType[t] = list;
        }
        list.Add(inst);
    }
}
