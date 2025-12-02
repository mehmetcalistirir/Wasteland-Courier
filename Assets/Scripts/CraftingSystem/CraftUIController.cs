using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;



public class CraftUIController : MonoBehaviour
{
    [Header("UI")]
    public GameObject craftPanel;
    public Transform gridParent;
    public GameObject craftSlotPrefab;
    public Button craftButton;
    public TMP_Text craftButtonText;


    [Header("Logic")]
    public WeaponCraftRecipe selectedRecipe;

    public static CraftUIController Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        PopulateRecipes();
        craftPanel.SetActive(false);
    }

    // ---------------------------------------------------------
    //  Tarif slotlarını oluştur
    // ---------------------------------------------------------
    public void PopulateRecipes()
    {
        // Grid üzerindeki eski slotları sil
        foreach (Transform t in gridParent)
            Destroy(t.gameObject);

        if (CraftingSystem.Instance == null)
        {
            Debug.LogError("CraftingSystem.Instance bulunamadı!");
            return;
        }

        // Tüm WeaponCraftRecipe tariflerini slot olarak ekle
        foreach (var recipe in CraftingSystem.Instance.recipes)
        {
            GameObject slotGO = Instantiate(craftSlotPrefab, gridParent);

            CraftSlotUI slotUI = slotGO.GetComponent<CraftSlotUI>();
            if (slotUI == null)
            {
                Debug.LogError("CraftSlot prefabında CraftSlotUI component yok!");
                continue;
            }

            // WeaponCraftRecipe → WeaponData
            WeaponData weapon = recipe.resultWeapon;

            if (weapon == null)
            {
                Debug.LogError("Tarifte resultWeapon eksik!");
                continue;
            }

            // Slot UI setup
            slotUI.Setup(
                recipe,
                weapon.icon,     // WeaponData'daki icon
                weapon.itemName  // WeaponData'daki isim
            );
        }
    }

    // ---------------------------------------------------------
    //  Slot tıklayınca tarif seçilir
    // ---------------------------------------------------------
    public void SelectRecipe(WeaponCraftRecipe recipe)
{
    selectedRecipe = recipe;

    WeaponData weapon = recipe.resultWeapon;

    bool existsInCaravan = CaravanInventory.Instance.HasWeapon(weapon);

    if (existsInCaravan)
    {
        craftButtonText.text = "Swap";      // ücretsiz değişim
    }
    else
    {
        craftButtonText.text = "Craft";     // malzeme gerektirir
    }
}


    // ---------------------------------------------------------
    //  Craft Butonu
    // ---------------------------------------------------------
    public void OnCraftButtonPressed()
{
    if (selectedRecipe == null)
    {
        Debug.Log("❗ Tarif seçilmedi.");
        return;
    }

    WeaponData weapon = selectedRecipe.resultWeapon;

    bool existsInCaravan = CaravanInventory.Instance.HasWeapon(weapon);

    // ------------------------------------------------
    // 1) EĞER SİLAH KARAVANDA VARSA → SWAP
    // ------------------------------------------------
    if (existsInCaravan)
    {
        SwapWithCaravan(weapon);
        Debug.Log($"🔄 Swap → {weapon.itemName} karavandan alındı.");
        
        Close();
        return;
    }

    // ------------------------------------------------
    // 2) DEĞİLSE → NORMAL CRAFT
    // ------------------------------------------------
    bool success = CraftingSystem.Instance.TryCraft(selectedRecipe);

    if (success)
    {
        Debug.Log($"✔ Craft başarılı → {weapon.itemName} üretildi");
        PopulateRecipes();
        selectedRecipe = null;
        Close();
    }
    else
    {
        Debug.Log($"❌ Craft başarısız → {weapon.itemName}");
    }
}


private void SwapWithCaravan(WeaponData weapon)
{
    // Silah hangi slotta kullanılacak?
    WeaponSlotType slotType = WeaponSlotManager.Instance.GetSlotForWeapon(weapon);
    int slotIndex = (int)slotType;

    // Oyuncunun elindeki silah
    WeaponData currentWeapon = WeaponSlotManager.Instance.slots[slotIndex];

    // 1) Oyuncunun mevcut silahını karavana koy
    if (currentWeapon != null)
        CaravanInventory.Instance.StoreWeapon(currentWeapon);

    // 2) Karavandan bu silahı al
    List<WeaponData> list = CaravanInventory.Instance.GetWeapons(weapon.weaponType);

    for (int i = 0; i < list.Count; i++)
    {
        if (list[i].itemID == weapon.itemID)
        {
            list.RemoveAt(i);
            break;
        }
    }

    // 3) Oyuncuya tak
    WeaponSlotManager.Instance.slots[slotIndex] = weapon;
    WeaponSlotManager.Instance.clip[slotIndex] = weapon.clipSize;
    WeaponSlotManager.Instance.reserve[slotIndex] = weapon.maxAmmoCapacity;

    WeaponSlotManager.Instance.SwitchSlot(slotIndex);
}



    // ---------------------------------------------------------
    //  Craft UI Aç / Kapat
    // ---------------------------------------------------------
    public void Open()
    {
        PopulateRecipes(); 
        craftPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Close()
{
    craftPanel.SetActive(false);
    selectedRecipe = null;
    Time.timeScale = 1f;
}

}
