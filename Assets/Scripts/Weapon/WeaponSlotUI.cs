// WeaponSlotUI.cs (GÜNCELLENMİŞ)
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class WeaponSlotUI : MonoBehaviour
{
    public static WeaponSlotUI Instance { get; private set; }

    [Header("UI")]
    public Transform slotContainer;
    public GameObject slotPrefab;

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

    // --- Slotları ilk kez kur ---
    public void InitializeSlots()
    {
        if (slotContainer == null || slotPrefab == null || WeaponSlotManager.Instance == null) return;

        foreach (Transform child in slotContainer) Destroy(child.gameObject);
        slotButtons.Clear();

        WeaponBlueprint[] equipped = WeaponSlotManager.Instance.GetEquippedBlueprints();
        if (equipped == null) return;

        for (int i = 0; i < equipped.Length; i++)
        {
            GameObject slotObject = Instantiate(slotPrefab, slotContainer);
            var slotButton = slotObject.GetComponent<WeaponSlotButton>();
            slotButtons.Add(slotButton);

            Sprite icon = (equipped[i] != null) ? equipped[i].weaponIcon : null;
            // Slot butonunu index ve ikonla hazırla (tıklama vb. kendi içinde)
            slotButton.Setup(i, icon);
        }

        initialized = true;
        UpdateHighlight(WeaponSlotManager.Instance.activeSlotIndex);
    }

    // --- Bir slot tıklandığında çağrılır (WeaponSlotButton içinden) ---
    public void OnSlotClicked(int index)
    {
        WeaponSlotManager.Instance.SwitchToSlot(index);
        UpdateHighlight(index);
    }

    // --- Aktif slot vurgusu ---
    public void UpdateHighlight(int activeIndex)
    {
        for (int i = 0; i < slotButtons.Count; i++)
            slotButtons[i]?.SetHighlight(i == activeIndex);
    }

    // === YENİ: Tek bir slotun ikonunu direkt güncelle ===
    public void SetSlotIcon(int index, Sprite icon)
    {
        if (!initialized) InitializeSlots();
        if (index < 0 || index >= slotButtons.Count) return;

        // WeaponSlotButton'da özel bir UpdateIcon yoksa, çocuk Image'ı bularak güncelle
        var img = slotButtons[index].GetComponentInChildren<Image>(true);
        if (img != null)
        {
            img.sprite = icon;
            img.enabled = (icon != null);
            // img.SetNativeSize(); // İstersen aç
        }
    }

    // === YENİ: Yalnızca tek bir slota, manager'daki blueprint'e göre ikonu yenile ===
    public void RefreshIconForSlot(int index)
    {
        if (WeaponSlotManager.Instance == null) return;
        var bp = WeaponSlotManager.Instance.GetBlueprintForSlot(index);
        SetSlotIcon(index, bp ? bp.weaponIcon : null);
    }

    // === YENİ: Tüm slot ikonlarını manager’dan yeniden çek ve güncelle ===
    public void RefreshAllIconsFromManager()
    {
        if (WeaponSlotManager.Instance == null) return;
        var equipped = WeaponSlotManager.Instance.GetEquippedBlueprints();
        if (equipped == null) return;

        // Slot sayısı değiştiyse baştan kur
        if (!initialized || slotButtons.Count != equipped.Length)
        {
            InitializeSlots();
            return;
        }

        for (int i = 0; i < equipped.Length; i++)
            SetSlotIcon(i, equipped[i] ? equipped[i].weaponIcon : null);

        UpdateHighlight(WeaponSlotManager.Instance.activeSlotIndex);
    }
}
