using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Linq;

public class WeaponCraftingSystem : MonoBehaviour
{
    public static WeaponCraftingSystem Instance { get; private set; }

    private int TypeKey(WeaponBlueprint bp) => (bp != null) ? bp.weaponSlotIndexToUnlock : -1;

    [Header("Crafting Data")]
    public List<WeaponBlueprint> availableBlueprints;

    [Header("UI References")]
    public GameObject craftingPanel;
    public Transform requirementsContainer;
    public GameObject requirementLinePrefab;
    public TextMeshProUGUI craftPromptText;
    public Button craftButton;
    public Button swapButton;

    private readonly List<BlueprintUI> blueprintUIElements = new List<BlueprintUI>();
    private PlayerStats playerStats;
    private WeaponBlueprint selectedBlueprint;

    [SerializeField] private WeaponSlotManager slotManager;

    public static bool IsCraftingOpen =>
        Instance != null && Instance.craftingPanel != null && Instance.craftingPanel.activeSelf;

    void Awake()
    {
        if (Instance == null) Instance = this; else { Destroy(gameObject); return; }
    }

    void Start()
    {

        for (int i = 0; i < blueprintUIElements.Count; i++)
        {
            var ui = blueprintUIElements[i];
            Debug.Log($"[CraftUI] Card {i} -> {ui.blueprint?.weaponName} (slotIndex={ui.blueprint?.weaponSlotIndexToUnlock})");
        }


        playerStats = FindObjectOfType<PlayerStats>();
        if (craftingPanel != null) craftingPanel.SetActive(false);
        ClearDetailInfo();

        WireGlobalButtons();
        InitializeBlueprintList();
        UpdateAllBlueprintUI();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame && CraftingStation.IsPlayerInRange)
            ToggleCraftingPanel();
    }

    private void WireGlobalButtons()
    {
        if (craftButton != null)
        {
            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(TryCraftWeapon);
            craftButton.interactable = false;
        }
        if (swapButton != null)
        {
            swapButton.onClick.RemoveAllListeners();
            swapButton.onClick.AddListener(TrySwapSelected);
            swapButton.interactable = false;
        }
    }

    private void InitializeBlueprintList()
    {
        blueprintUIElements.Clear();

        // 1) UI kartlarını al ve pozisyona göre (soldan sağa, yukarıdan aşağıya) sırala
        var uis = craftingPanel.GetComponentsInChildren<BlueprintUI>(true)
                               .OrderBy(ui =>
                               {
                                   var rt = ui.transform as RectTransform;
                                   // önce satır (y küçük -> üst), sonra sütun (x küçük -> sol)
                                   return (rt ? (rt.anchoredPosition.y * -10000f + rt.anchoredPosition.x) : 0f);
                               })
                               .ToList();
        blueprintUIElements.AddRange(uis);

        // 2) Blueprint’leri slot indexine göre sırala (0:Machinegun, 1:Pistol, 2:Sniper, 3:Shotgun ...)
        var bps = (availableBlueprints ?? new List<WeaponBlueprint>())
                      .Where(bp => bp != null)
                      .OrderBy(bp => bp.weaponSlotIndexToUnlock)
                      .ToList();

        // 3) Eşleştir ve kur
        int count = Mathf.Min(bps.Count, blueprintUIElements.Count);
        for (int i = 0; i < count; i++)
            blueprintUIElements[i].Setup(bps[i]);

        // 4) İlk açılışta depo sayaçlarını doğru yaz
        UpdateAllBlueprintUI();
    }


    public void ToggleCraftingPanel()
    {
        bool isActive = !craftingPanel.activeSelf;
        craftingPanel.SetActive(isActive);

        if (isActive)
        {
            Time.timeScale = 0f;
            UpdateAllBlueprintUI();
            selectedBlueprint = null;
            ClearDetailInfo();
            SetGlobalButtons(false, false);
        }
        else
        {
            Time.timeScale = 1f;
            selectedBlueprint = null;
            ClearDetailInfo();
            StartCoroutine(ReapplyAmmoNextFrame());
        }


    }

    public void SelectBlueprint(WeaponBlueprint blueprint)
    {
        selectedBlueprint = blueprint;
        UpdateDetailPanel();
    }

    private void UpdateDetailPanel()
    {
        foreach (Transform c in requirementsContainer) Destroy(c.gameObject);

        if (selectedBlueprint == null)
        {
            SetGlobalButtons(false, false);
            return;
        }

        // Gereksinimleri yaz
        foreach (var req in selectedBlueprint.requiredParts)
        {
            int have = playerStats.GetWeaponPartCount(req.partType);
            AddRequirementLine(req.partType.ToString(), have, req.amount);
        }

        if (craftPromptText) craftPromptText.gameObject.SetActive(false);

        // --- Yeni mantık ---
        bool canCraftNow = CanCraft(selectedBlueprint);

        var wsm = WeaponSlotManager.Instance;
        int selectedKey = TypeKey(selectedBlueprint);
        int active = wsm?.activeSlotIndex ?? -1;
        bool activeEmpty = (wsm != null && wsm.GetBlueprintForSlot(active) == null);

        // Depo sayıları (seçili tür + toplam)
        int storedSelected = CaravanInventory.Instance.GetStoredCountForType(selectedKey);
        int totalStored = 0;
        if (wsm != null && wsm.weaponSlots != null)
        {
            for (int i = 0; i < wsm.weaponSlots.Length; i++)
                totalStored += CaravanInventory.Instance.GetStoredCountForType(i);
        }

        bool inRange = CraftingStation.IsPlayerInRange;
        bool rightSlot = (wsm != null && active == selectedKey);

        // Aktif slot boşsa: seçili tür varsa ya da depoda herhangi bir tür varsa swap butonu aktif
        // Aktif slot doluysa: eski kural; tür eşleşmeli ve o türden depoda olmalı
        bool canSwapNow = inRange && (
                            (activeEmpty && (storedSelected > 0 || totalStored > 0)) ||
                            (!activeEmpty && rightSlot && storedSelected > 0)
                          );

        SetGlobalButtons(canCraftNow, canSwapNow);

        Debug.Log($"[DETAIL] selectedKey={selectedKey}  active={active}  activeEmpty={activeEmpty}  " +
                  $"storedSel={storedSelected} totalStored={totalStored} canCraft={canCraftNow} canSwap={canSwapNow}");
    }




    private void SetGlobalButtons(bool craftInteractable, bool swapInteractable)
    {
        if (craftButton != null) craftButton.interactable = craftInteractable;
        if (swapButton != null) swapButton.interactable = swapInteractable;
    }

    private void ClearDetailInfo()
    {
        foreach (Transform child in requirementsContainer) Destroy(child.gameObject);
        if (craftPromptText != null) craftPromptText.gameObject.SetActive(false);
    }

    private void AddRequirementLine(string itemName, int current, int required)
    {
        if (requirementLinePrefab == null || required <= 0) return;
        var go = Instantiate(requirementLinePrefab, requirementsContainer);
        var ui = go.GetComponent<RequirementLineUI>();
        ui.Setup(itemName, current, required);
    }

    public bool CanCraft(WeaponBlueprint bp)
    {
        if (bp == null || playerStats == null) return false;
        foreach (var r in bp.requiredParts)
            if (playerStats.GetWeaponPartCount(r.partType) < r.amount)
                return false;
        return true;
    }


    private void ShowMsg(string text)
    {
        if (craftPromptText == null) return;
        craftPromptText.text = text;
        craftPromptText.gameObject.SetActive(true);
    }

    // === CRAFT ===
    public void TryCraftWeapon()
{
    if (selectedBlueprint == null) { ShowMsg("Önce bir silah seç."); return; }
    foreach (var r in selectedBlueprint.requiredParts)
        if (playerStats.GetWeaponPartCount(r.partType) < r.amount)
        { ShowMsg("Eksik parçalar var"); return; }

    // Parçaları harca
    playerStats.ConsumeWeaponParts(selectedBlueprint.requiredParts);

    // --- Yeni silah payload oluştur ---
    var wid = selectedBlueprint.weaponItemSO;
    if (wid == null)
    {
        Debug.LogError("[Crafting] Blueprint içinde WeaponItemData atanmadı!");
        return;
    }

    var payload = new InventoryItem.WeaponInstancePayload
    {
        id = System.Guid.NewGuid().ToString("N"), // benzersiz ID
        clip = wid.blueprint.weaponData.clipSize,
        reserve = wid.blueprint.weaponData.maxAmmoCapacity,
        durability = 100
    };

    // ✅ SADECE ENVANTERE EKLE
    bool added = Inventory.Instance.TryAdd(wid, 1, payload);

    if (added)
    {
        ShowMsg($"{wid.itemName} craft edildi → envantere eklendi.");
        Debug.Log($"[Craft] Yeni silah oluşturuldu: {wid.itemName} (ID={payload.id})");
    }
    else
    {
        ShowMsg("Envanterde yer yok.");
    }

    UpdateAllBlueprintUI();
}



    // === SWAP ===
    public void TrySwapSelected()
    {
        if (!CraftingStation.IsPlayerInRange) { ShowMsg("Karavan menzilinde değilsin."); return; }
        if (selectedBlueprint == null) { ShowMsg("Önce bir silah seç."); return; }

        var wsm = WeaponSlotManager.Instance;
        int active = wsm.activeSlotIndex;
        int selectedKey = TypeKey(selectedBlueprint);

        bool activeEmpty = (wsm.GetBlueprintForSlot(active) == null);

        if (activeEmpty)
        {
            // Önce seçili türden dene
            int countSel = CaravanInventory.Instance.GetStoredCountForType(selectedKey);
            if (countSel > 0)
            {
                // O tür slota geç ve depodan o türü tak
                wsm.SwitchToSlot(selectedKey);
                CaravanInventory.Instance.SwapNextStoredForActiveType();

                UpdateAllBlueprintUI();
                StartCoroutine(ReapplyAmmoNextFrame());

                var bpNow = wsm.GetBlueprintForSlot(wsm.activeSlotIndex);
                ShowMsg($"{bpNow?.weaponName ?? "Silah"} kuşanıldı.");
                return;
            }

            // Seçili tür yoksa depoda olan ilk türü tak
            int typeCount = wsm.weaponSlots?.Length ?? 0;
            for (int t = 0; t < typeCount; t++)
            {
                if (CaravanInventory.Instance.GetStoredCountForType(t) > 0)
                {
                    wsm.SwitchToSlot(t);
                    CaravanInventory.Instance.SwapNextStoredForActiveType();

                    UpdateAllBlueprintUI();
                    StartCoroutine(ReapplyAmmoNextFrame());

                    var bpNow = wsm.GetBlueprintForSlot(wsm.activeSlotIndex);
                    ShowMsg($"{bpNow?.weaponName ?? "Silah"} kuşanıldı.");
                    return;
                }
            }

            ShowMsg("Depoda hiç silah yok.");
            return;
        }

        // Aktif slot doluysa eski kural (tür eşleşmeli)
        if (selectedKey != active) { ShowMsg("Önce bu türün slotuna geç."); return; }

        int count = CaravanInventory.Instance.GetStoredCountForType(selectedKey);
        if (count <= 0) { ShowMsg("Depoda bu türden silah yok."); return; }

        CaravanInventory.Instance.SwapNextStoredForActiveType();

        UpdateAllBlueprintUI();
        StartCoroutine(ReapplyAmmoNextFrame());

        var bpAfter = wsm.GetBlueprintForSlot(active);
        ShowMsg($"{bpAfter?.weaponName ?? "Silah"} kuşanıldı. (depo:{CaravanInventory.Instance.GetStoredCountForType(selectedKey)} kaldı)");
    }


    // Tüm blueprint kartlarının "depo adedi" vb. durumlarını tazeler
    public void UpdateAllBlueprintUI()
    {
        foreach (var ui in blueprintUIElements)
            if (ui != null) ui.UpdateStatus();
    }

    private IEnumerator ReapplyAmmoNextFrame()
    {
        yield return null;
        WeaponSlotManager.Instance?.ForceReapplyActiveAmmo();
    }

    public void CloseCraftingPanel()
    {
        if (craftingPanel != null && craftingPanel.activeSelf)
        {
            craftingPanel.SetActive(false);
            Time.timeScale = 1f;

            selectedBlueprint = null;
            ClearDetailInfo();
            SetGlobalButtons(false, false);

            // panel kapandıktan sonra aktif silahın mermisini 1 frame sonra tekrar bastır
            StartCoroutine(ReapplyAmmoNextFrame());
        }
    }

}
