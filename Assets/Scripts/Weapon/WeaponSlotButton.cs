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
        this.slotIndex = index;

        if (icon != null)
        {
            this.weaponIcon.sprite = icon;
            this.weaponIcon.enabled = true;

            // Inspector'dan verilen boyutu uygula
            RectTransform rt = weaponIcon.GetComponent<RectTransform>();
            if (rt != null)
                rt.sizeDelta = iconSize;
        }
        else
        {
            this.weaponIcon.enabled = false;
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
