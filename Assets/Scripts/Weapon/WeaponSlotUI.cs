using UnityEngine;

public class WeaponSlotUI : MonoBehaviour
{
    public static WeaponSlotUI Instance { get; private set; }

    [Header("Ayarlar")]
    [SerializeField] private int slotCount = 11;

    [Header("Prefab")]
    [SerializeField] private GameObject slotTemplatePrefab;

    [Header("Slot Butonları (Otomatik Doldurulur)")]
    public WeaponSlotButton[] slotButtons;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // Eğer prefab eksikse hata ver
        if (slotTemplatePrefab == null)
        {
            Debug.LogError("❌ WeaponSlotUI: Slot_Template prefab atanmadı!");
            return;
        }

        GenerateSlots();
    }

    private void Start()
    {
        RefreshAllFromState();
    }

    //==========================================================
    // SLOT GENERATOR — WeaponSlotsPanel içine slotları oluşturur
    //==========================================================
    private void GenerateSlots()
    {
        // Tüm eski slotları temizle
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        slotButtons = new WeaponSlotButton[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotObj = Instantiate(slotTemplatePrefab, transform);
            slotObj.name = $"Slot_{i}";

            WeaponSlotButton btn = slotObj.GetComponent<WeaponSlotButton>();
            if (btn == null)
            {
                Debug.LogError("❌ Slot_Template içinde WeaponSlotButton script'i bulunamadı!");
                continue;
            }

            btn.Init(i);
            slotButtons[i] = btn;
        }

        Debug.Log($"✅ {slotCount} adet Slot_Template başarıyla oluşturuldu.");
    }

    //==========================================================
    // UI Güncelleme Fonksiyonları
    //==========================================================
    public void SetSlotIcon(int index, Sprite icon)
    {
        if (!IsValidIndex(index)) return;
        slotButtons[index].SetIcon(icon);
    }

    public void ClearSlotIcon(int index)
    {
        if (!IsValidIndex(index)) return;
        slotButtons[index].Clear();
    }

    public void UpdateHighlight(int activeIndex)
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (slotButtons[i] != null)
                slotButtons[i].SetHighlight(i == activeIndex);
        }
    }

    public void RefreshAllFromState()
    {
        var wsm = WeaponSlotManager.Instance;
        if (wsm == null) return;

        for (int i = 0; i < slotButtons.Length; i++)
        {
            Sprite icon = null;

            // BANDAJ SLOTLARI
            if (wsm.slotTypes != null &&
                wsm.slotTypes[i] == SlotItemType.Bandage &&
                wsm.bandageSlots[i] != null)
            {
                icon = wsm.bandageSlots[i].icon;
            }
            else
            {
                var bp = wsm.GetBlueprintForSlot(i);
                if (bp != null && bp.weaponData != null)
                    icon = bp.weaponData.weaponIcon;
            }

            slotButtons[i].SetIcon(icon);
        }

        UpdateHighlight(wsm.activeSlotIndex);
    }

    private bool IsValidIndex(int index)
    {
        return slotButtons != null &&
               index >= 0 &&
               index < slotButtons.Length &&
               slotButtons[index] != null;
    }
}
