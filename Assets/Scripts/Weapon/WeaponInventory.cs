// WeaponInventory.cs (DÜZELTİLMİŞ VE DOĞRU HALİ)

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WeaponInventory : MonoBehaviour
{
    // Bu referans artık doğrudan silahları yönetmek için değil,
    // yeni silahın verilerini merkezi ateş etme sistemine bildirmek için kullanılacak.
    public PlayerWeapon playerWeapon;
    public GameObject[] weaponPrefabs; // 0: Makineli, 1: Tabanca, 2: Yakın dövüş
    public Transform weaponHolder; // Silahların oluşturulacağı yer (firePoint yerine bu daha mantıklı)

    // Aktif silah objelerini bu dizide tutacağız.
    private GameObject[] weaponInstances;
    private int currentSlot = 1; // Başlangıç slotu

    // ... (crafting ile ilgili değişkenleriniz aynı kalabilir) ...
    public Text craftHintText;
    private HashSet<string> collectedParts = new HashSet<string>();
    private string[] requiredParts = new string[] {
        "Barrel", "Magazine", "Foregrip", "Grip", "Trigger", "TriggerGuard"
    };

    void Start()
    {
        // weaponInstances dizisini başlat
        weaponInstances = new GameObject[weaponPrefabs.Length];
        EquipSlot(currentSlot);

        if (craftHintText != null)
            craftHintText.gameObject.SetActive(false);
    }

    void Update()
    {
        // ... (Update içeriğiniz aynı kalabilir) ...
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) EquipSlot(2);

        if (Input.GetKeyDown(KeyCode.C) && HasAllParts())
        {
            CraftNewWeapon();
        }
    }

    private void EquipSlot(int slotIndex)
    {
        // Geçersiz slot veya zaten o slot seçiliyse işlem yapma
        if (slotIndex < 0 || slotIndex >= weaponPrefabs.Length || slotIndex == currentSlot) return;

        // 1. Mevcut aktif silahı yok et (eğer varsa)
        if (weaponInstances[currentSlot] != null)
        {
            Destroy(weaponInstances[currentSlot]);
        }

        // 2. Yeni silahın prefab'ını al ve oluştur
        GameObject weaponPrefabToInstantiate = weaponPrefabs[slotIndex];
        if (weaponPrefabToInstantiate == null)
        {
            Debug.LogError($"Slot {slotIndex} için silah prefab'ı atanmamış!");
            return;
        }

        // Yeni silahı weaponHolder'ın altında oluştur
        GameObject newWeaponInstance = Instantiate(weaponPrefabToInstantiate, weaponHolder.position, weaponHolder.rotation, weaponHolder);

        // 3. Oluşturulan yeni silahı dizide sakla
        weaponInstances[slotIndex] = newWeaponInstance;
        currentSlot = slotIndex; // Mevcut slotu güncelle

        // 4. Merkezi ateş etme sistemini (PlayerWeapon) yeni silahın verileriyle yapılandır.
        // Bu satır sizin kodunuzda vardı, çok doğru bir yaklaşım.
        WeaponDataHolder dataHolder = newWeaponInstance.GetComponent<WeaponDataHolder>();
        if (dataHolder != null && playerWeapon != null)
        {
            // PlayerWeapon'ın weaponData'sını yeni silahınkiyle değiştiriyoruz.
            playerWeapon.weaponData = dataHolder.weaponData;
            // Not: Mermi ve şarjör mekaniği için burada PlayerWeapon'daki mermi sayısını da güncellemeniz gerekecek.
            // Bu kısım WeaponSlotManager'daki mantığa benziyor.
        }
    }

    // ... (Crafting fonksiyonlarınız aynı kalabilir) ...
    public void CollectPart(string partName) { /* ... */ }
    private bool HasAllParts() { /* ... */ return true; }
    private void CraftNewWeapon() { /* ... */ }
}