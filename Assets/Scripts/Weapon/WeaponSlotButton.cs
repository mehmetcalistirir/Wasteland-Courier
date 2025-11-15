using UnityEngine;
using UnityEngine.UI;

public class WeaponSlotButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image weaponIcon;
    [SerializeField] private GameObject highlightBorder;

    private int slotIndex;
    private Button button;

    private void Awake()
    {
        // Otomatik bul (Inspector’dan atamayı unutsan bile)
        if (weaponIcon == null)
        {
            var iconTr = transform.Find("WeaponIcon");
            if (iconTr != null) weaponIcon = iconTr.GetComponent<Image>();
        }

        if (highlightBorder == null)
        {
            var hbTr = transform.Find("HighlightBorder");
            if (hbTr != null) highlightBorder = hbTr.gameObject;
        }

        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnSlotClicked);

        if (weaponIcon != null)
            weaponIcon.enabled = false;

        if (highlightBorder != null)
            highlightBorder.SetActive(false);
    }

    /// <summary>WeaponSlotUI, her butona kendi index'ini verir.</summary>
    public void Init(int index)
    {
        slotIndex = index;
    }

    /// <summary>Bu slota ait ikonu ayarla.</summary>
    public void SetIcon(Sprite icon)
    {
        if (weaponIcon == null) return;

        if (icon == null)
        {
            weaponIcon.sprite = null;
            weaponIcon.enabled = false;
        }
        else
        {
            weaponIcon.sprite = icon;
            weaponIcon.enabled = true;
            weaponIcon.preserveAspect = true;
            weaponIcon.color = Color.white;   // yanlışlıkla şeffaf olmasın
        }
    }

    public void Clear()
    {
        SetIcon(null);
    }

    public void SetHighlight(bool active)
    {
        if (highlightBorder != null)
            highlightBorder.SetActive(active);
    }

    private void OnSlotClicked()
    {
        if (WeaponSlotManager.Instance != null)
            WeaponSlotManager.Instance.SwitchToSlot(slotIndex);

        WeaponSlotUI.Instance?.UpdateHighlight(slotIndex);
    }
}
