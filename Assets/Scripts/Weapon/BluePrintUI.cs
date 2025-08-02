using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlueprintUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image weaponIcon;
    public TextMeshProUGUI weaponNameText;
    public Button selectButton; // Ana seçim butonu (resmin kendisi)
    public TextMeshProUGUI storageCountText; // "0/1" veya "1/1" yazısı
    public Button swapButton; // "Değiştir" butonu

    private WeaponBlueprint currentBlueprint;

    public void Setup(WeaponBlueprint blueprint)
    {
        currentBlueprint = blueprint;
        weaponNameText.text = blueprint.weaponName;
        weaponIcon.sprite = blueprint.weaponIcon;
        
        // Resme tıklandığında detayları göster
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => WeaponCraftingSystem.Instance.SelectBlueprint(currentBlueprint));

        // Değiştir butonuna tıklandığında silahı değiştir
        swapButton.onClick.RemoveAllListeners();
        swapButton.onClick.AddListener(() => CaravanInventory.Instance.SwapWeapon(currentBlueprint));
    }

    // Bu fonksiyon CraftingSystem tarafından çağrılarak UI'ın durumunu ayarlar.
    public void UpdateStatus()
    {
        bool isStored = CaravanInventory.Instance.IsWeaponStored(currentBlueprint);
        bool canBeCrafted = WeaponCraftingSystem.Instance.CanCraft(currentBlueprint);

        // 1. Depo sayacını güncelle
        storageCountText.text = isStored ? "1 / 1" : "0 / 1";
        storageCountText.color = isStored ? Color.cyan : Color.white;

        // 2. Değiştir butonunu yönet
        // Silah depodaysa ve oyuncu karavan menzilindeyse değiştir butonu aktif olur.
        swapButton.gameObject.SetActive(isStored && CraftingStation.IsPlayerInRange);
        
        // 3. İkon rengini ayarla (isteğe bağlı, craft durumu için)
        if (isStored)
        {
            weaponIcon.color = Color.white; // Depodaysa parlak
        }
        else if (canBeCrafted)
        {
            weaponIcon.color = Color.yellow; // Craftlanabilirse sarı
        }
        else
        {
            weaponIcon.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Craftlanamazsa karanlık
        }
    }
}