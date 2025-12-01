using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftUIController : MonoBehaviour
{
    [Header("UI")]
    public GameObject craftPanel;
    public Transform gridParent;
    public GameObject craftSlotPrefab;

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
        Debug.Log($"📌 Seçilen tarif: {recipe.resultWeapon.itemName}");
    }

    // ---------------------------------------------------------
    //  Craft Butonu
    // ---------------------------------------------------------
    public void OnCraftButtonPressed()
    {
        if (selectedRecipe == null)
        {
            Debug.Log("❗ Craft yapmak için tarif seçilmedi.");
            return;
        }

        bool success = CraftingSystem.Instance.TryCraft(selectedRecipe);

        if (success)
            Debug.Log($"✅ Craft başarılı → {selectedRecipe.resultWeapon.itemName}");
        else
            Debug.Log($"❌ Craft başarısız → {selectedRecipe.resultWeapon.itemName}");
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
        Time.timeScale = 1f;
    }
}
