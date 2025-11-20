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

        // 1) Eğer kayıt yükleniyorsa → direkt save yükle
        if (SaveBootstrap.ShouldLoadFromSave && SaveSystem.HasSave())
        {
            SaveSystem.LoadPlayerAndInventory(player, inventory, stats);
            SaveBootstrap.ShouldLoadFromSave = false;
            return; // Kaydı yükledik → artık bitirebiliriz
        }

        // 2) Yeni oyun → Default spawn
        Vector3 spawnPos = player.position;

        if (defaultSpawnPoint != null)
            spawnPos = defaultSpawnPoint.position;
        else if (fallbackSpawnPosition != Vector3.zero)
            spawnPos = fallbackSpawnPosition;

        player.position = spawnPos;
        Debug.Log("Yeni oyun → Player default pozisyona yerleştirildi: " + spawnPos);

        // 3) Yeni oyuna başlangıç silahını ver
        ItemData pistolItem = ItemDatabase.Get("weapon_pistol");
        if (pistolItem != null)
        {
            WeaponSlotManager.Instance.EquipWeapon(pistolItem);
            WeaponSlotManager.Instance.activeSlotIndex = 0;

            // PlayerWeapon'ın SetWeapon çağrıldığını logla
            Debug.Log("▶ Yeni oyun → Başlangıç silahı verildi: weapon_pistol");
        }
        else
        {
            Debug.LogError("❌ ItemDatabase GET weapon_pistol başarısız! ItemID doğru mu?");
        }
    }

}
