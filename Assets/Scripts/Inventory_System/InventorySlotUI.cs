using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour,
    IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI")]
    public Image icon;
    public TMP_Text countText;
    public CanvasGroup canvasGroup;

    private int index;
    private InventoryUI owner;
    private InventoryItem cached;
    private RectTransform rt;
    private GameObject dragIcon;
    [Header("Magazine Highlight")]
    public Image highlightImage;
    public Color equippedColor = Color.yellow;
    public Color normalColor = Color.clear;


    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        rt = GetComponent<RectTransform>();
    }

    // InventoryUI tarafından çağrılır
    public void Bind(int index, InventoryUI owner)
    {
        this.index = index;
        this.owner = owner;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    // Envanterdeki slot verisini UI'a basar
   public void Render(InventoryItem item)
{
    cached = item;

    // 🔒 1️⃣ SLOT BOŞSA
    if (item == null || item.data == null)
    {
        icon.enabled = false;
        countText.text = "";

        if (highlightImage != null)
            highlightImage.color = normalColor;

        return;
    }

    // 🔫 2️⃣ MAGAZINE SLOT
if (item.magazineInstance != null)
{
    var mag = item.magazineInstance;

    // 🚨 DATA YOKSA GÜVENLİ ÇIKIŞ
    if (mag.data == null)
    {
        icon.enabled = false;
        countText.text = "";
        if (highlightImage != null)
            highlightImage.color = normalColor;
        return;
    }

    icon.enabled = true;
    icon.sprite = mag.data.icon;
    icon.preserveAspect = true;

    countText.text =
        $"{mag.currentAmmo}/{mag.data.capacity}";

    // ⭐ Equipped highlight
    if (highlightImage != null)
    {
        var pw = FindObjectOfType<PlayerWeapon>();
        if (pw != null && pw.currentMagazine == mag)
            highlightImage.color = equippedColor;
        else
            highlightImage.color = normalColor;
    }

    return;
}


    // 📦 3️⃣ NORMAL ITEM / AMMO
    icon.enabled = true;
    icon.sprite = item.data.icon;
    icon.preserveAspect = true;

    if (item.data is AmmoItemData)
        countText.text = $"x{item.count}";
    else
        countText.text = $"x{item.count}";

    if (highlightImage != null)
        highlightImage.color = normalColor;
}




    // Sağ tık / sol tık davranışı
    public void OnPointerClick(PointerEventData eventData)
    {
        if (cached == null)
            return;

        // 🔫 MAGAZINE LEFT CLICK → TAK
        if (cached.magazineInstance != null &&
            eventData.button == PointerEventData.InputButton.Left)
        {
            var pw = FindObjectOfType<PlayerWeapon>();
            if (pw == null) return;

            pw.TryEquipMagazineFromInventory(
                cached.magazineInstance
            );
        }
        if (cached.magazineInstance != null)
        {
            owner.SelectMagazine(cached.magazineInstance);
            return;
        }

    }



    // ---- DRAG & DROP ----

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (cached == null || cached.data == null) return;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f;

        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(owner.transform.root);
        dragIcon.transform.SetAsLastSibling();

        var img = dragIcon.AddComponent<Image>();
        img.sprite = icon.sprite;
        img.raycastTarget = false;
        img.preserveAspect = true;

        var drt = dragIcon.GetComponent<RectTransform>();
        drt.sizeDelta = rt.sizeDelta;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon == null) return;
        dragIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (dragIcon != null)
            Destroy(dragIcon);
    }

    public void OnDrop(PointerEventData eventData)
    {
        var src = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<InventorySlotUI>() : null;
        if (src == null || src == this) return;

        owner.MoveOrMerge(src.index, this.index);
    }
}
