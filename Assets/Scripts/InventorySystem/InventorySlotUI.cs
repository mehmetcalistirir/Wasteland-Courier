using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class InventorySlotUI : MonoBehaviour,
    IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler, IPointerUpHandler
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
        // Envanteri aç/kapat
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

    public void OnPointerUp(PointerEventData e)
    {
        // Bu class içinde kullanılmıyor artık
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
        // 1️⃣ Ammo kullanma
        // ==============================
        if (invSlot.data.category == ItemCategory.Ammo)
        {
            HandleAmmoRightClick(wsm);
            return;
        }

        // ==============================
        // 2️⃣ BANDAGE → Aktif slota gönderme
        // ==============================
        if (invSlot.data is GenericItemData g && g.isConsumable)
        {
            int slotIndex = wsm.activeSlotIndex;
            if (slotIndex < 0)
            {
                Debug.LogWarning("⚠️ Aktif weapon slot yok.");
                return;
            }

            // Bandajı aktif slota kaydet
            wsm.slotTypes[slotIndex] = SlotItemType.Bandage;
            wsm.bandageSlots[slotIndex] = g;

            // Envanterden 1 adet tüket
            Inventory.Instance.TryConsume(g, 1);

            // WeaponSlot UI icon’u bandaj ile değiş
            WeaponSlotUI.Instance?.SetSlotIcon(slotIndex, g.icon);

            Debug.Log($"🩹 Bandaj aktif slota takıldı (Slot {slotIndex})");
            return;
        }

        // ==============================
        // 3️⃣ Silah swap sistemi (senin mevcut kodun)
        // ==============================
        if (invSlot.data is WeaponItemData clickedWeapon)
        {
            // ... senin silah swap kodun burada devam eder
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
        invSlot = item;

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
        // ARTIK SAĞ TIK BASILI TUTMA YOK → WeaponSlotUI yapacak
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
