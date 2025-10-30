using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem; // Yeni sistem için şart

public class InventorySlotUI : MonoBehaviour,
    IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    [Header("UI")]
    public Image icon;
    public TMP_Text countText;
    public CanvasGroup canvasGroup;

    private Keyboard keyboard;

    private int index;
    private InventoryUI owner;
    private InventoryItem cached;
    public InventoryItem invSlot;
    private Inventory inventory;

    // Drag
    private bool dragging;
    private float holdTime = 0.25f;
    private float downTime;
    private Vector2 startPos;
    private RectTransform rt;
    private GameObject dragIcon;
    public GameObject inventoryPanel;

    private void Start()
    {
        inventory = FindObjectOfType<Inventory>();
    }

    void Awake()
    {
        keyboard = Keyboard.current;
    }

    void Update()
    {
        if (keyboard != null && keyboard.kKey.wasPressedThisFrame)
        {
            if (inventoryPanel != null)
                inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }
    }

    public void Bind(int index, InventoryUI owner)
    {
        this.index = index;
        this.owner = owner;
        rt = GetComponent<RectTransform>();

        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
            return;

        if (invSlot == null || invSlot.data == null)
            return;

        var wsm = WeaponSlotManager.Instance;
        if (wsm == null)
        {
            Debug.LogError("⚠️ WeaponSlotManager bulunamadı!");
            return;
        }

        // ==============================
        // 1️⃣ Mermi doldurma
        // ==============================
        if (invSlot.data.category == ItemCategory.Ammo)
        {
            HandleAmmoRightClick(wsm);
            return;
        }

        // ==============================
        // 2️⃣ Silah swap sistemi
        // ==============================
        if (invSlot.data is WeaponItemData clickedWeapon)
        {
            int activeSlot = wsm.activeSlotIndex;
            if (activeSlot < 0)
            {
                Debug.LogWarning("⚠️ Aktif slot yok!");
                return;
            }

            // --- Aktif slottaki silahı oku ---
            var oldBlueprint = wsm.GetBlueprintForSlot(activeSlot);
            var (oldClip, oldReserve) = wsm.GetAmmoStateForSlot(activeSlot);

            // --- Envanterdeki silahı al ---
            var newBlueprint = clickedWeapon.blueprint;
            var newPayload = invSlot.weapon ?? new InventoryItem.WeaponInstancePayload
            {
                id = System.Guid.NewGuid().ToString("N"),
                clip = newBlueprint.weaponData.clipSize,
                reserve = newBlueprint.weaponData.maxAmmoCapacity,
                durability = 100
            };

            // --- Aktif slottaki silahı envanter slotuna yaz ---
            if (oldBlueprint != null)
            {
                var oldPayload = new InventoryItem.WeaponInstancePayload
                {
                    id = System.Guid.NewGuid().ToString("N"),
                    clip = oldClip,
                    reserve = oldReserve,
                    durability = 100
                };

                invSlot.data = new WeaponItemData
                {
                    itemName = oldBlueprint.weaponName,
                    blueprint = oldBlueprint,
                    category = ItemCategory.Weapon,
                    stackable = false,
                    icon = invSlot.data.icon
                };
                invSlot.weapon = oldPayload;
            }
            else
            {
                Inventory.Instance.Clear(index);
            }

            // --- Yeni silahı aktif slota tak ---
            wsm.EquipWeaponInstanceIntoSlot(activeSlot, newBlueprint, newPayload);

            // 🔄 Model güncelle
            if (wsm.weaponSlots[wsm.activeSlotIndex] != null)
                wsm.weaponSlots[wsm.activeSlotIndex].SetActive(false);

            if (wsm.weaponSlots[activeSlot] != null)
            {
                wsm.weaponSlots[activeSlot].SetActive(true);
                wsm.activeWeapon = wsm.weaponSlots[activeSlot].GetComponent<PlayerWeapon>();

                if (wsm.activeWeapon != null)
                {
                    wsm.activeWeapon.weaponData = newBlueprint.weaponData;
                    wsm.activeWeapon.SetAmmoInClip(newPayload.clip);
                }
            }

            // 🔄 Model + ikon yenileme
            wsm.ForceSwapActiveWeaponPrefab(newBlueprint); // <— bunu koru

            // 🎯 SADECE aktif slotun ikonunu yenile
            if (WeaponSlotUI.Instance != null)
            {
                int activeSlotIndex = wsm.activeSlotIndex;
                var bp = wsm.GetBlueprintForSlot(activeSlotIndex);

                if (bp != null && bp.weaponData != null && bp.weaponData.weaponIcon != null)
                {
                    WeaponSlotUI.Instance.SetSlotIcon(activeSlotIndex, bp.weaponData.weaponIcon);
                    Debug.Log($"🎯 Sadece aktif slot ({activeSlotIndex}) ikonu yenilendi: {bp.weaponData.weaponName}");
                }
            }

            // 🟡 Envanter slotunun ikonunu da güncelle
            if (icon != null && invSlot != null && invSlot.data != null && invSlot.data.icon != null)
            {
                icon.sprite = invSlot.data.icon;
                icon.enabled = true;
                Debug.Log($"🟡 Envanter slotu ({index}) ikonu güncellendi: {invSlot.data.itemName}");
            }

            // 🔁 Envanter UI genelini tazele (sayım ve ikonlar için)
            Inventory.Instance?.RaiseChanged();
            WeaponSlotUI.Instance?.RefreshAllFromState();



            Debug.Log($"🔁 Silah swap tamamlandı: {newBlueprint.weaponName} aktif edildi ve modeli güncellendi.");
        }
    }

    private void HandleAmmoRightClick(WeaponSlotManager wsm)
    {
        var ammoItem = invSlot.data as AmmoItemData;
        if (ammoItem == null)
        {
            Debug.LogWarning("⚠️ AmmoItemData cast edilemedi.");
            return;
        }

        bool ammoUsed = false;

        for (int i = 0; i < wsm.weaponSlots.Length; i++)
        {
            var bp = wsm.GetBlueprintForSlot(i);
            if (bp == null || bp.weaponData == null)
                continue;

            bool matches = false;
            switch (ammoItem.resourceType)
            {
                case ResourceType.AmmoPistol:
                    matches = bp.weaponData.weaponType == WeaponType.Pistol;
                    break;
                case ResourceType.AmmoMachineGun:
                    matches = bp.weaponData.weaponType == WeaponType.MachineGun;
                    break;
                case ResourceType.AmmoShotgun:
                    matches = bp.weaponData.weaponType == WeaponType.Shotgun;
                    break;
                case ResourceType.AmmoSniper:
                    matches = bp.weaponData.weaponType == WeaponType.Sniper;
                    break;
            }

            if (!matches) continue;

            var (clip, reserve) = wsm.GetAmmoStateForSlot(i);
            int maxCap = bp.weaponData.maxAmmoCapacity;
            if (reserve >= maxCap) continue;

            int addAmount = Mathf.Min(ammoItem.ammoPerItem, maxCap - reserve);
            wsm.SetAmmoStateForSlot(i, clip, reserve + addAmount);
            Inventory.Instance.TryConsume(invSlot.data, 1);
            wsm.UpdateUI();

            Debug.Log($"✅ {bp.weaponData.weaponName} → +{addAmount} mermi eklendi.");
            ammoUsed = true;
            break;
        }

        if (!ammoUsed)
            Debug.Log($"⚠️ {ammoItem.itemName} için uygun silah bulunamadı.");
    }

    public void Render(InventoryItem item)
    {
        cached = item;
        invSlot = item; // 🟡 Burası önemli: invSlot da set edilmeli!

        if (item == null || item.data == null)
        {
            icon.enabled = false;
            countText.text = "";
            return;
        }

        icon.enabled = true;
        icon.sprite = item.data.icon;
        icon.preserveAspect = true;

        if (item.data is AmmoItemData ammoData)
        {
            int totalAmmo = item.count * ammoData.ammoPerItem;
            countText.text = $"x{totalAmmo}";
        }
        else
        {
            countText.text = $"x{item.count}";
        }
    }

    public void OnPointerDown(PointerEventData e)
    {
        downTime = Time.unscaledTime;
        startPos = rt.position;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (Time.unscaledTime - downTime < holdTime) { e.pointerDrag = null; return; }
        if (cached == null || cached.data == null) { e.pointerDrag = null; return; }

        dragging = true;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f;

        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(owner.transform.root);
        dragIcon.transform.SetAsLastSibling();

        var img = dragIcon.AddComponent<Image>();
        img.sprite = icon.sprite;
        img.raycastTarget = false;
        img.preserveAspect = true;
        dragIcon.GetComponent<RectTransform>().sizeDelta = rt.sizeDelta;
    }

    public void OnDrag(PointerEventData e)
    {
        if (!dragging || dragIcon == null) return;
        dragIcon.transform.position = e.position;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (!dragging) return;
        dragging = false;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (dragIcon != null)
            GameObject.Destroy(dragIcon);
    }

    public void OnDrop(PointerEventData e)
    {
        var src = e.pointerDrag ? e.pointerDrag.GetComponent<InventorySlotUI>() : null;
        if (src == null || src == this) return;
        owner.MoveOrMerge(src.index, this.index);
    }
}
