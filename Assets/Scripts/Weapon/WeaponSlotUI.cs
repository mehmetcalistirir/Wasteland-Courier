using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class WeaponSlotUI : MonoBehaviour
{
    public static WeaponSlotUI Instance { get; private set; }

    [Header("UI")]
    public Transform slotContainer;
    public GameObject slotPrefab;

    [Header("Slot Görselleri")]
    [Tooltip("Hotbar üzerindeki silah ikonlarını sırayla buraya atayın.")]
    public Image[] slotIcons;

    [Tooltip("Boş slot için kullanılacak sprite.")]
    public Sprite emptySlotSprite;

    private readonly List<WeaponSlotButton> slotButtons = new List<WeaponSlotButton>();
    private bool initialized;

    void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    void Start()
    {
        InitializeSlots();
    }

    public void InitializeSlots()
    {
        if (slotContainer == null || slotPrefab == null || WeaponSlotManager.Instance == null) return;

        foreach (Transform child in slotContainer) Destroy(child.gameObject);
        slotButtons.Clear();

        var equipped = WeaponSlotManager.Instance.GetEquippedBlueprints();
        if (equipped == null) return;

        for (int i = 0; i < equipped.Length; i++)
        {
            GameObject slotObject = Instantiate(slotPrefab, slotContainer);
            var slotButton = slotObject.GetComponent<WeaponSlotButton>();
            slotButtons.Add(slotButton);

            // 🔴 DİKKAT: Blueprint.weaponIcon yerine her zaman weaponData.weaponIcon
            Sprite icon = GetIconForSlot(i);
            slotButton.Setup(i, icon);
        }

        initialized = true;
        UpdateHighlight(WeaponSlotManager.Instance.activeSlotIndex);
    }

    private Sprite GetIconForSlot(int i)
    {
        var wsm = WeaponSlotManager.Instance;
        var bp = wsm != null ? wsm.GetBlueprintForSlot(i) : null;
        return bp != null ? bp.weaponData?.weaponIcon : null;
    }
    
     public void RefreshAllFromState()
    {
        if (!initialized) InitializeSlots();
        for (int i = 0; i < slotButtons.Count; i++)
            slotButtons[i].Setup(i, GetIconForSlot(i));

        UpdateHighlight(WeaponSlotManager.Instance.activeSlotIndex);
    }
    

    public void OnSlotClicked(int index)
    {
        WeaponSlotManager.Instance.SwitchToSlot(index);
        UpdateHighlight(index);
    }

    public void UpdateHighlight(int activeIndex)
    {
        for (int i = 0; i < slotButtons.Count; i++)
            slotButtons[i]?.SetHighlight(i == activeIndex);
    }

    // 🔹 Tek bir slotun ikonunu güncelle (diğerlerini etkilemeden)
 public void SetSlotIcon(int index, Sprite icon)
    {
        if (!initialized) InitializeSlots();
        if (index < 0 || index >= slotButtons.Count) return;

        slotButtons[index].Setup(index, icon); // WeaponSlotButton içindeki serialized WeaponIcon’u kullanır
        Debug.Log($"🎯 Slot {index} ikonu: {icon?.name ?? "NULL"}");
    }



}
