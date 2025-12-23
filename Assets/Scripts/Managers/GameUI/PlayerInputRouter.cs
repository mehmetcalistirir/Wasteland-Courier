using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputRouter : MonoBehaviour
{
    public static PlayerInputRouter Instance;

    private PlayerControls controls;

    [Header("Trade")]
public TradeUIController tradeUI;
public NPCTradeInteract currentNPC;


    [Header("Panels")]
    public GameObject craftPanel;
    public GameObject tradePanel;
    public GameObject pauseMenuPanel;

    [Header("References")]
    public CaravanInteraction caravan;

    private void Awake()
    {
        Instance = this;
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Gameplay.Inventory.performed += OnInventory;
        controls.Gameplay.Interact.performed += OnInteract; // 🔴 BU ŞART
        controls.Gameplay.Craft.performed += OnCraft;
        controls.Gameplay.Escape.performed += OnEscape;

        controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        controls.Gameplay.Inventory.performed -= OnInventory;
        controls.Gameplay.Interact.performed -= OnInteract; // 🔴 BU ŞART
        controls.Gameplay.Craft.performed -= OnCraft;
        controls.Gameplay.Escape.performed -= OnEscape;

        controls.Gameplay.Disable();
    }

    // ==============================
    // INVENTORY (I) → PIPBOY
    // ==============================
   private void OnInventory(InputAction.CallbackContext ctx)
{
    if (!ctx.performed) return;

    if (PipBoyController.Instance == null)
        return;

    // 🔁 TOGGLE
    if (PipBoyController.Instance.IsOpen)
    {
        PipBoyController.Instance.Close();
        GameStateManager.SetPaused(false);
    }
    else
    {
        PipBoyController.Instance.Open(0);
        GameStateManager.SetPaused(true);
    }
}


private void OnInteract(InputAction.CallbackContext ctx)
{
    if (!ctx.performed) return;
    if (IsPauseOpen()) return;

    // Trade açıksa → kapat
    if (tradeUI != null && tradeUI.gameObject.activeSelf)
    {
        CloseTrade();
        return;
    }

    // Trade kapalıysa ve NPC uygunsa → aç
    if (currentNPC != null && currentNPC.playerInRange)
    {
        OpenTrade(currentNPC.tradeInventory);
    }
}
void OpenTrade(NPCTradeInventory inventory)
{
    tradeUI.Open(inventory);
    GameStateManager.SetPaused(true);
    
}

void CloseTrade()
{
    tradeUI.Close();
    GameStateManager.SetPaused(false);
    
}


    // ==============================
    // CRAFT (C)
    // ==============================
    private void OnCraft(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (IsPauseOpen()) return;

        if (caravan == null || !caravan.playerInRange)
            return;

        ToggleCraft();
    }

    // ==============================
    // ESC
    // ==============================
    private void OnEscape(InputAction.CallbackContext ctx)
    {
        // 0️⃣ Trade açıksa → kapat
if (tradeUI != null && tradeUI.gameObject.activeSelf)
{
    CloseTrade();
    return;
}

        if (!ctx.performed) return;
        if (GameStateManager.IsGameOver) return;

        // 1️⃣ Pause açıksa → kapat
        if (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
        {
            PauseMenu.Instance.HidePause();
            GameStateManager.SetPaused(false);
            return;
        }

        // 2️⃣ Trade açıksa → kapat
        if (tradePanel != null && tradePanel.activeSelf)
        {
            tradePanel.SetActive(false);
            GameStateManager.SetPaused(false);
            return;
        }

        // 3️⃣ Craft açıksa → kapat
        if (craftPanel != null && craftPanel.activeSelf)
        {
            craftPanel.SetActive(false);
            GameStateManager.SetPaused(false);
            return;
        }

        // 4️⃣ Hiçbiri açık değil → Pause aç
        PauseMenu.Instance.ShowPause();
        GameStateManager.SetPaused(true);
    }

    public void ForceCloseCraft()
{
    if (craftPanel != null && craftPanel.activeSelf)
    {
        craftPanel.SetActive(false);
        GameStateManager.SetPaused(false);
    }
}


    // ==============================
    // HELPERS
    // ==============================
    private bool IsPauseOpen()
    {
        return pauseMenuPanel != null && pauseMenuPanel.activeSelf;
    }

    private void ToggleCraft()
    {
        bool open = !craftPanel.activeSelf;
        craftPanel.SetActive(open);
        GameStateManager.SetPaused(open);
    }

    // ==============================
    // INPUT CONTROL (PipBoy çağırır)
    // ==============================
    public void SetGameplayInput(bool enabled)
    {
        if (enabled)
            controls.Gameplay.Enable();
        else
            controls.Gameplay.Disable();
    }

    // ==============================
    // UNUSED INPUTS
    // ==============================
    public void OnMove(InputAction.CallbackContext ctx) { }
    public void OnSprint(InputAction.CallbackContext ctx) { }
    public void OnMap(InputAction.CallbackContext ctx) { }
    public void OnReload(InputAction.CallbackContext ctx) { }
    public void OnWeapon1(InputAction.CallbackContext ctx) { }
    public void OnWeapon2(InputAction.CallbackContext ctx) { }
    public void OnWeapon3(InputAction.CallbackContext ctx) { }
    public void OnMelee(InputAction.CallbackContext ctx) { }
    public void OnADS(InputAction.CallbackContext ctx) { }
    public void OnCaravanWeapons(InputAction.CallbackContext ctx) { }
}
