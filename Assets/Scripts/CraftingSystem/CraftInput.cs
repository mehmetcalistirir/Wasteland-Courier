using UnityEngine;
using UnityEngine.InputSystem;

public class CraftInput : MonoBehaviour
{
    public CraftUIController craftUI;
    public GameObject inventoryPanel;

    public CaravanInteraction caravan;   // Karavan referansı → oyuncu yakında mı?

    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Gameplay.Enable();
        controls.Gameplay.Craft.performed += OnCraftPressed;
    }

    private void OnDisable()
    {
        controls.Gameplay.Craft.performed -= OnCraftPressed;
        controls.Gameplay.Disable();
    }

    private void OnCraftPressed(InputAction.CallbackContext ctx)
    {
        if (craftUI == null) return;

        // 🟡 Karavana yakın değilse craft açılmasın
        if (caravan != null && !caravan.playerInRange)
        {
            Debug.Log("Craft açılamadı → Karavana yakın değilsin.");
            return;
        }

        bool isOpen = craftUI.craftPanel.activeSelf;

        // Craft açılacaksa inventory kapat
        if (!isOpen && inventoryPanel != null && inventoryPanel.activeSelf)
            inventoryPanel.SetActive(false);

        if (isOpen)
            craftUI.Close();
        else
            craftUI.Open();
    }
}
