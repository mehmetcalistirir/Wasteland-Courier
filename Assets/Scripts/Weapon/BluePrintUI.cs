using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlueprintUI : MonoBehaviour
{
    [Header("Wiring")]
    public TMP_Text weaponNameText;
    public Image    weaponIconImage;
    public Button   selectButton;        // “ikon”a tıklama
    public TMP_Text storageCountText;    // SADECE 0/1 – Inspector’da StorageWeaponText’e bunu bağla

    [Header("Data (assigned by WeaponCraftingSystem)")]
    public WeaponBlueprint blueprint;    // Setup ile atanır

    void Awake()
    {
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnSelectClicked);
        }
    }

    public void Setup(WeaponBlueprint bp)
    {
        blueprint = bp;

        if (weaponNameText != null)  weaponNameText.text  = bp != null ? bp.weaponName : "-";
        if (weaponIconImage != null) weaponIconImage.sprite = bp != null ? bp.weaponIcon : null;

        if (bp != null && bp.weaponData != null && bp.weaponData.isMolotov)
    {
        weaponNameText.text = "🔥 " + bp.weaponName + " (Bomba)";
        weaponNameText.color = new Color(1f, 0.6f, 0.2f); // turuncumsu yazı
    }

        UpdateStatus(); // ilk açılışta sayacı yaz
    }

    void OnEnable()
    {
        // Panel tekrar görünür olduğunda da tazele
        UpdateStatus();
    }

   public void UpdateStatus()
{
    if (blueprint == null) return;
    int typeKey = blueprint.weaponSlotIndexToUnlock;
    int count = CaravanInventory.Instance.GetStoredCountForType(typeKey);
    if (storageCountText != null) storageCountText.text = $"{count} adet";
}



    private void OnSelectClicked()
    {
        if (blueprint == null) return;
        WeaponCraftingSystem.Instance.SelectBlueprint(blueprint);
    }
}
