using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponSlotButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image weaponIcon;
    [SerializeField] private GameObject highlightBorder;

    [Header("Icon Settings")]
    [Tooltip("Bu boyut weaponIcon'un RectTransform'una uygulanır.")]
    [SerializeField] private Vector2 iconSize = new Vector2(64, 64);

    private int slotIndex;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnSlotClicked);
    }

    // WeaponSlotUI tarafından çağrılır
   public void Setup(int index, Sprite icon)
{
    slotIndex = index;

    if (icon != null)
    {
        weaponIcon.sprite = icon;
        weaponIcon.enabled = true;

        // Slot boyutuna göre orantılı küçültme
        RectTransform rt = weaponIcon.GetComponent<RectTransform>();
        if (rt != null)
        {
            // slotun içine sığacak maksimum boyut
            rt.anchorMin = new Vector2(0.1f, 0.1f);
            rt.anchorMax = new Vector2(0.9f, 0.9f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        
    }
    else
    {
        weaponIcon.enabled = false;
        weaponIcon.sprite = null;
    }
}


    private void OnSlotClicked()
    {
        WeaponSlotUI.Instance.OnSlotClicked(slotIndex);
    }

    public void SetHighlight(bool isActive)
    {
        highlightBorder.SetActive(isActive);
    }
}
