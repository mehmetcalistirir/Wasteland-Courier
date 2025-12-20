using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [Header("Referanslar")]
    public Transform player;       // Player Transform
    public ItemRegistry registry;  // Tüm item SO'ları
    public Inventory inventory;    // Player üzerindeki Inventory
    public PlayerStats stats;      // PlayerStats

    [Header("Yeni Oyun Spawn Noktası")]
    public Transform defaultSpawnPoint;   // Inspector’dan bir boş obje ver (spawn noktası)
    public Vector3 fallbackSpawnPosition; // İstersen elle de girebilirsin

    private void Awake()
    {
        if (registry != null && registry.items != null && registry.items.Length > 0)
        {
            ItemDatabase.RegisterAll(registry.items);
        }
        else
        {
            Debug.LogError("ItemDatabase boooş! registry veya registry.items atanmadı!");
        }

    }

    private void Start()
    {
        Debug.Log("GameInitializer: ShouldLoad = " + SaveBootstrap.ShouldLoadFromSave);
        Debug.Log("GameInitializer: HasSave = " + SaveSystem.HasSave());

        // 1) Save yükleme...
        if (SaveBootstrap.ShouldLoadFromSave && SaveSystem.HasSave())
        {
            SaveSystem.LoadPlayerAndInventory(
        player,
        inventory,
        stats,
        FindObjectOfType<PlayerWeapon>()
    );

            SaveBootstrap.ShouldLoadFromSave = false;
            return;
        }

        // 2) Yeni oyun → default spawn
        Vector3 spawnPos = player.position;

        if (defaultSpawnPoint != null)
            spawnPos = defaultSpawnPoint.position;
        else if (fallbackSpawnPosition != Vector3.zero)
            spawnPos = fallbackSpawnPosition;

        player.position = spawnPos;


        // 3) BAŞLANGIÇ PİSTOL
        ItemData pistolItem = ItemDatabase.Get("weapon_glock18");
        if (pistolItem is WeaponItemData pistolWeapon)
        {
            WeaponSlotManager.Instance.EquipWeapon(pistolWeapon);
            Debug.Log("▶ Yeni oyun → Başlangıç silahı verildi: Pistol");
        }




        // Aktif slot pistol olsun
        WeaponSlotManager.Instance.activeSlotIndex = 0;
    }


}
