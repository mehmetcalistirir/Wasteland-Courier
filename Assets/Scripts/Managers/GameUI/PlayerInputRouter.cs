using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputRouter : MonoBehaviour, PlayerControls.IGameplayActions
{
    private PlayerControls controls;

    [Header("Panels")]
    public GameObject inventoryPanel;
    public GameObject craftPanel;
    public GameObject tradePanel;      // NPCInteraction bunu açıp kapatıyor
    public GameObject pauseMenuPanel;

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

        TogglePanel(craftPanel);
    }

    // ================================
    // TRADE (E)
    // ================================
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // Oyuncu NPC yakınında değilse açma
        if (NPCInteraction.Instance == null || !NPCInteraction.Instance.PlayerIsNear())
            return;

        // Trade panelini NPCInteraction yönetiyor
        NPCInteraction.Instance.ToggleTradePanel();
    }

    // ================================
    // ESCAPE
    // ================================
    public void OnEscape(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // 1) Trade açık ise önce onu kapat
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

        // 4) Pause aç/kapa
        TogglePanel(pauseMenuPanel);
    }


    // ================================
    // PANEL HELPERS
    // ================================
    private void TogglePanel(GameObject panel)
    {
        if (panel == null) return;

        bool next = !panel.activeSelf;

        CloseAllPanels();
        panel.SetActive(next);
    }

    private void CloseAllPanels()
    {
        if (inventoryPanel) inventoryPanel.SetActive(false);
        if (craftPanel) craftPanel.SetActive(false);
        if (tradePanel) tradePanel.SetActive(false);
        if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
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
