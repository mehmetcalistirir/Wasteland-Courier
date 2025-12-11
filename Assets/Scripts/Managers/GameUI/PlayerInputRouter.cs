using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputRouter : MonoBehaviour, PlayerControls.IGameplayActions
{
    private PlayerControls controls;

    [Header("Panels")]
    public GameObject inventoryPanel;
    public GameObject craftPanel;
    public GameObject tradePanel;
    public GameObject pauseMenuPanel;

   
    [Header("References")]
    public CaravanInteraction caravan; // Craft için gerekecek

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Gameplay.SetCallbacks(this);
    }

    private void OnEnable()  => controls.Gameplay.Enable();
    private void OnDisable() => controls.Gameplay.Disable();


    // ================================
    // INVENTORY (I)
    // ================================
    public void OnInventory(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        TogglePanel(inventoryPanel);
    }


    // ================================
    // CRAFT (C)
    // ================================
    public void OnCraft(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // ❗ Craft sadece karavana yakınken açılabilir
        if (caravan != null && !caravan.playerInRange)
        {
            Debug.Log("Craft açılamadı → Karavana yakın olmalısın.");
            return;
        }

        TogglePanel(craftPanel);
    }


    // ================================
    // TRADE (E)
    // ================================
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        if (NPCInteraction.Instance == null) return;
        if (!NPCInteraction.Instance.PlayerIsNear()) return;

        NPCInteraction.Instance.ToggleTradePanel();
    }


    // ================================
    // ESC (PAUSE + PANEL KAPAMA)
    // ================================
    public void OnEscape(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // 1) Trade açık → kapat
        if (tradePanel != null && tradePanel.activeSelf)
        {
            tradePanel.SetActive(false);
            
            return;
        }

        // 2) Inventory kapat
        if (inventoryPanel != null && inventoryPanel.activeSelf)
        {
            inventoryPanel.SetActive(false);
            
            return;
        }

        // 3) Craft kapat
        if (craftPanel != null && craftPanel.activeSelf)
        {
            craftPanel.SetActive(false);
            
            return;
        }

        // 4) Pause açık → kapat
        if (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
        {
            pauseMenuPanel.SetActive(false);
            Time.timeScale = 0f;
            return;
        }

        // 5) Hiç panel yok → pause aç
        TogglePanel(pauseMenuPanel);
    }


    // ================================
    // PANEL HELPERS
    // ================================
    private void TogglePanel(GameObject panel)
    {
        if (panel == null) return;

        bool open = !panel.activeSelf;

        CloseAllPanels();
        panel.SetActive(open);

        // Pause logic
        if (panel == pauseMenuPanel)
        {
            Time.timeScale = open ? 0f : 1f;
        }

        
    }

    private void CloseAllPanels()
    {
        if (inventoryPanel) inventoryPanel.SetActive(false);
        if (craftPanel) craftPanel.SetActive(false);
        if (tradePanel) tradePanel.SetActive(false);

        // HUD geri gelsin
        
        // Pause kapandıysa oyun devam etsin
        if (pauseMenuPanel && pauseMenuPanel.activeSelf)
        {
            pauseMenuPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }


    // ================================
    // UNUSED INPUTS
    // ================================
    public void OnMove(InputAction.CallbackContext ctx) {}
    public void OnSprint(InputAction.CallbackContext ctx) {}
    public void OnMap(InputAction.CallbackContext ctx) {}
    public void OnReload(InputAction.CallbackContext ctx) {}
    public void OnWeapon1(InputAction.CallbackContext ctx) {}
    public void OnWeapon2(InputAction.CallbackContext ctx) {}
    public void OnWeapon3(InputAction.CallbackContext ctx) {}
    public void OnMelee(InputAction.CallbackContext ctx) {}
    public void OnADS(InputAction.CallbackContext ctx) {}
    public void OnCaravanWeapons(InputAction.CallbackContext ctx) {}
}
