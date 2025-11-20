using UnityEngine;

public class CraftUI : MonoBehaviour
{
    public static CraftUI Instance { get; private set; }

    [Header("UI Referansları")]
    public GameObject panel;          // Craft panel GameObject (Canvas altındaki)
    
    [Header("Sistem")]
    public CraftingSystem craftingSystem;

    [HideInInspector] 
    public WeaponRecipe selectedRecipe; // Şu an seçili tarif

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (craftingSystem == null)
            craftingSystem = FindObjectOfType<CraftingSystem>();
    }

    // Blueprint yerine artık WeaponRecipe seçiyoruz
    public void SelectRecipe(WeaponRecipe recipe)
    {
        selectedRecipe = recipe;
        if (recipe != null && recipe.weapon != null)
            Debug.Log("Seçilen tarif: " + recipe.weapon.itemName);
    }

    // Craft butonuna basıldığında çağırılacak
    public void OnCraftPressed()
    {
        if (selectedRecipe == null)
        {
            Debug.Log("CraftUI: Tarif seçilmedi.");
            return;
        }

        if (CraftingSystem.Instance.TryCraft(selectedRecipe))
        {
            Debug.Log("CraftUI: Craft başarılı.");
        }
        else
        {
            Debug.Log("CraftUI: Craft başarısız (şartlar sağlanmıyor).");
        }
    }

    public void OpenPanel()
    {
        if (panel != null) panel.SetActive(true);
    }

    public void ClosePanel()
    {
        if (panel != null) panel.SetActive(false);
    }

    public bool IsOpen => panel != null && panel.activeSelf;
}
