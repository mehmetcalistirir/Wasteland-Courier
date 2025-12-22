using UnityEngine;

public class CraftUIController : MonoBehaviour
{
    [Header("UI Refs")]
    public Transform craftContainer;
    public GameObject craftSlotPrefab;

    private void OnEnable()
    {
        RefreshUI();
        Inventory.Instance.OnChanged += RefreshUI;
    }

    private void OnDisable()
    {
        Inventory.Instance.OnChanged -= RefreshUI;
    }

    // ------------------------------------------------
    // 🔄 UI Yenile
    // ------------------------------------------------
    public void RefreshUI()
{
    // 1️⃣ Temizle
    for (int i = craftContainer.childCount - 1; i >= 0; i--)
        Destroy(craftContainer.GetChild(i).gameObject);

    // ===============================
    // 🔫 WEAPON RECIPES
    // ===============================
   foreach (var recipe in CraftingSystem.Instance.recipes)
{
    GameObject go = Instantiate(craftSlotPrefab, craftContainer);
    CraftSlotUI slotUI = go.GetComponent<CraftSlotUI>();

    slotUI.Setup(recipe, this);

    WeaponItemData weapon = recipe.resultWeapon;

    bool isUnlocked = CraftingSystem.Instance.IsUnlocked(recipe);
    bool canCraft = CraftingSystem.Instance.CanCraft(recipe);
    bool canSwapHere = CanSwapThisWeapon(weapon);

    // 🔑 BUTON AKTİFLİĞİ
    slotUI.craftButton.interactable =
        (!isUnlocked && canCraft) || (isUnlocked && canSwapHere);

    // 🔍 GÖRSEL GERİ BİLDİRİM
    CanvasGroup cg = go.GetComponent<CanvasGroup>();
    if (cg != null)
        cg.alpha = slotUI.craftButton.interactable ? 1f : 0.35f;
}


    // ===============================
    // 📦 ITEM RECIPES
    // ===============================
    foreach (var recipe in CraftingSystem.Instance.itemRecipes)
    {
        GameObject go = Instantiate(craftSlotPrefab, craftContainer);
        CraftSlotUI slotUI = go.GetComponent<CraftSlotUI>();

        slotUI.Setup(recipe, this);

        bool canCraft = CraftingSystem.Instance.CanCraftItem(recipe);
        slotUI.craftButton.interactable = canCraft;

        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg != null)
            cg.alpha = canCraft ? 1f : 0.4f;
    }
}
private bool CanSwapThisWeapon(WeaponItemData weapon)
{
    if (weapon == null)
        return false;

    WeaponSlotManager sm = WeaponSlotManager.Instance;

    WeaponSlotType slotForWeapon =
        sm.GetSlotForWeapon(weapon.weaponType);

    return (int)slotForWeapon == sm.activeSlotIndex;
}


    // ------------------------------------------------
    // 🔴 Button burayı çağırır
    // ------------------------------------------------
    public void OnCraftButtonClicked(WeaponCraftRecipe recipe)
    {
        bool success = CraftingSystem.Instance.TryCraft(recipe);

        if (success)
            RefreshUI();
    }
}

