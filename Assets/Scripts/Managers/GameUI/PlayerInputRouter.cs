using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputRouter : MonoBehaviour, PlayerControls.IGameplayActions
{
    public static PlayerInputRouter Instance;

    private PlayerControls controls;

    [Header("Panels")]
    public GameObject inventoryPanel;
    public GameObject craftPanel;
    public GameObject tradePanel;
    public GameObject pauseMenuPanel;

   
    [Header("References")]
    public CaravanInteraction caravan; // Craft için gerekecek
    


private void Update()
{
    // Craft açıksa ama artık menzilde değilsek → kapat
    if (craftPanel != null && craftPanel.activeSelf)
    {
        if (caravan == null || !caravan.playerInRange)
        {
            craftPanel.SetActive(false);
            GameStateManager.SetPaused(false);
        }
    }
}
public void ForceCloseCraft()
{
    if (craftPanel != null && craftPanel.activeSelf)
    {
        craftPanel.SetActive(false);
        GameStateManager.SetPaused(false);
    }
}


    private void Awake()
    {
        Instance = this;
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

    // ❌ Caravan yoksa veya menzilde değilse → açma
    if (caravan == null || !caravan.playerInRange)
    {
        Debug.Log("Craft açılamadı → Caravan menzilinde değilsin.");
        return;
    }

    TogglePanel(craftPanel);
}


public void SetGameplayInput(bool enabled)
{
    if (enabled)
        controls.Gameplay.Enable();
    else
        controls.Gameplay.Disable();
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
    if (GameStateManager.IsGameOver) return;

    // 1️⃣ Trade açıksa → kapat
    if (tradePanel != null && tradePanel.activeSelf)
    {
        tradePanel.SetActive(false);
        GameStateManager.SetPaused(false);
        return;
    }

    // 2️⃣ Craft açıksa → kapat
    if (craftPanel != null && craftPanel.activeSelf)
    {
        craftPanel.SetActive(false);
        GameStateManager.SetPaused(false);
        return;
    }

    // 3️⃣ Inventory açıksa → kapat
    if (inventoryPanel != null && inventoryPanel.activeSelf)
    {
        inventoryPanel.SetActive(false);
        GameStateManager.SetPaused(false);
        return;
    }

    // 4️⃣ Pause açıksa → kapat
    if (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
    {
        GameStateManager.SetPaused(false);
        PauseMenu.Instance.HidePause();
        return;
    }

    // 5️⃣ Hiçbiri açık değil → Pause aç
    GameStateManager.SetPaused(true);
    PauseMenu.Instance.ShowPause();
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
