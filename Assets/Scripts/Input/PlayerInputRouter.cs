using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputRouter : MonoBehaviour
{
    public static PlayerInputRouter Instance;

    public InputActionAsset inputActions;
    private InputActionMap gameplay;

    private PlayerControls controls;

    [Header("Panels")]
    public GameObject craftPanel;
    public GameObject tradePanel;
    public GameObject pauseMenuPanel;

    [Header("References")]
    public CaravanInteraction caravan;

    // Aktif NPC (trigger içinde olan)
    private NPCTradeInteract activeTradeNPC;

    // ======================================================
    // LIFECYCLE
    // ======================================================
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        gameplay = inputActions.FindActionMap("Gameplay");

    }
private void Update()
{
    if (Input.GetKeyDown(KeyCode.F12))
    {
        Debug.Log("Gameplay enabled: " +
            inputActions.FindActionMap("Gameplay").enabled);
    }
}

private void OnEnable()
{
    gameplay.FindAction("Inventory").performed += OnInventory;
    gameplay.FindAction("Interact").performed += OnInteract;
    gameplay.FindAction("Escape").performed += OnEscape;

    gameplay.Enable();
}

private void OnDisable()
{
    gameplay.FindAction("Inventory").performed -= OnInventory;
    gameplay.FindAction("Interact").performed -= OnInteract;
    gameplay.FindAction("Escape").performed -= OnEscape;

    gameplay.Disable();
}


    // ======================================================
    // INPUT HANDLERS
    // ======================================================

    // INVENTORY (I)
    private void OnInventory(InputAction.CallbackContext ctx)
{
    if (!ctx.performed) return;
    if (PipBoyController.Instance == null) return;

    // Eğer envanter AÇIKSA → KAPAT
    if (PipBoyController.Instance.IsOpen)
    {
        PipBoyController.Instance.Close();
        ResumeGame();
        return;
    }

    // Eğer başka bloklayıcı panel açıksa → açma
    if (IsAnyBlockingPanelOpen()) return;

    // Envanteri AÇ
    InteractionPromptUI.Instance?.Hide();
    PipBoyController.Instance.Open(0);
    GameStateManager.SetPaused(true);
}



    // INTERACT (E)
    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // Interact basıldı → prompt her zaman kapanır
        InteractionPromptUI.Instance?.Hide();

        // 1️⃣ Trade açıksa → kapat
        if (tradePanel != null && tradePanel.activeSelf)
        {
            tradePanel.SetActive(false);
            ResumeGame();
            return;
        }

        // 2️⃣ Craft açıksa → kapat
        if (craftPanel != null && craftPanel.activeSelf)
        {
            craftPanel.SetActive(false);
            ResumeGame();
            return;
        }

        // 3️⃣ NPC Trade (öncelikli)
        if (activeTradeNPC != null && activeTradeNPC.playerInRange)
        {
            OpenTrade();
            return;
        }

        // 4️⃣ Caravan Craft
        if (caravan != null && caravan.playerInRange)
        {
            ToggleCraft();
            return;
        }
    }

    // ESC
    private void OnEscape(InputAction.CallbackContext ctx)
{
    if (!ctx.performed) return;
    if (GameStateManager.IsGameOver) return;

    // 1️⃣ Inventory (PipBoy) AÇIKSA → KAPAT
    if (PipBoyController.Instance != null &&
        PipBoyController.Instance.IsOpen)
    {
        PipBoyController.Instance.Close();
        ResumeGame();
        return;
    }

    // 2️⃣ Trade AÇIKSA → KAPAT
    if (tradePanel != null && tradePanel.activeSelf)
    {
        tradePanel.SetActive(false);
        ResumeGame();
        return;
    }

    // 3️⃣ Craft AÇIKSA → KAPAT
    if (craftPanel != null && craftPanel.activeSelf)
    {
        craftPanel.SetActive(false);
        ResumeGame();
        return;
    }

    // 4️⃣ Pause AÇIKSA → KAPAT
    if (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
    {
        PauseMenu.Instance.HidePause();
        ResumeGame();
        return;
    }

    // 5️⃣ Hiç panel yok → Pause AÇ
    PauseMenu.Instance.ShowPause();
    GameStateManager.SetPaused(true);
}


    // ======================================================
    // TRADE
    // ======================================================
    private void OpenTrade()
    {
        if (activeTradeNPC == null) return;

        if (tradePanel != null)
            tradePanel.SetActive(true);

        activeTradeNPC.OpenTrade();
        GameStateManager.SetPaused(true);
    }

    public void SetActiveTradeNPC(NPCTradeInteract npc)
    {
        activeTradeNPC = npc;
    }

    // ======================================================
    // CRAFT
    // ======================================================
    private void ToggleCraft()
    {
        if (craftPanel == null) return;

        bool open = !craftPanel.activeSelf;
        craftPanel.SetActive(open);
        GameStateManager.SetPaused(open);
    }

    public void ForceCloseCraft()
    {
        if (craftPanel != null && craftPanel.activeSelf)
        {
            craftPanel.SetActive(false);
            ResumeGame();
        }
    }

    // ======================================================
    // HELPERS
    // ======================================================
    private bool IsAnyBlockingPanelOpen()
    {
        return (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
            || (tradePanel != null && tradePanel.activeSelf)
            || (craftPanel != null && craftPanel.activeSelf);
    }

    private void ResumeGame()
    {
        GameStateManager.SetPaused(false);
        Time.timeScale = 1f;
    }

public void EnableGameplay()
{
    inputActions.FindActionMap("UI").Disable();
    inputActions.FindActionMap("Gameplay").Enable();
}


    // ======================================================
    // UNUSED INPUTS (Input System gereği)
    // ======================================================
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
