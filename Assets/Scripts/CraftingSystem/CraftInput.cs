using UnityEngine;
using UnityEngine.InputSystem;

public class CraftInput : MonoBehaviour
{
    public CraftUIController craftUI;
    public GameObject inventoryPanel;

    public CaravanInteraction caravan;      // ZATEN VAR
    public GameObject caravanWeaponPanel;   // YENİ: silah swap paneli

    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Gameplay.Enable();
        controls.Gameplay.Craft.performed += OnCraftPressed;
        controls.Gameplay.CaravanWeapons.performed += OnCaravanWeaponsPressed; // YENİ
    }

    private void OnDisable()
    {
        controls.Gameplay.Craft.performed -= OnCraftPressed;
        controls.Gameplay.CaravanWeapons.performed -= OnCaravanWeaponsPressed; // YENİ
        controls.Gameplay.Disable();
    }

    // CRAFT TUŞU (eski)
    private void OnCraftPressed(InputAction.CallbackContext ctx)
    {
        if (craftUI == null) return;

        // Karavana yakın değilse craft açılmasın
        if (caravan != null && !caravan.playerInRange)
        {
            Debug.Log("Craft açılamadı → Karavana yakın değilsin.");
            return;
        }

        bool isOpen = craftUI.craftPanel.activeSelf;

        // Craft açılacaksa inventory kapat
        if (!isOpen && inventoryPanel != null && inventoryPanel.activeSelf)
            inventoryPanel.SetActive(false);

        // Craft açılırken karavan silah panelini de kapat
        if (!isOpen && caravanWeaponPanel != null && caravanWeaponPanel.activeSelf)
            caravanWeaponPanel.SetActive(false);

        if (isOpen)
            craftUI.Close();
        else
            craftUI.Open();
    }

    // KARAVAN SİLAH PANEL TUŞU (yeni)
    private void OnCaravanWeaponsPressed(InputAction.CallbackContext ctx)
    {
        Debug.Log("V tuşu algılandı!");
        if (caravan == null || !caravan.playerInRange)
        {
            Debug.Log("Karavan silah paneli açılamadı → Karavana yakın değilsin.");
            return;
        }

        if (caravanWeaponPanel == null)
        {
            Debug.LogWarning("CaravanWeaponPanel referansı atanmadı!");
            return;
        }

        bool isOpen = caravanWeaponPanel.activeSelf;

        if (!isOpen)
        {
            // Açılırken diğer UI'ları kapat
            if (craftUI != null && craftUI.craftPanel.activeSelf)
                craftUI.Close();

            if (inventoryPanel != null && inventoryPanel.activeSelf)
                inventoryPanel.SetActive(false);
        }

        caravanWeaponPanel.SetActive(!isOpen);
    }
}
