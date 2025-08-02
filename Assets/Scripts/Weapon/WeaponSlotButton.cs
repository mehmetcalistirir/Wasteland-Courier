// WeaponSlotButton.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponSlotButton : MonoBehaviour
{
    [SerializeField] private Image weaponIcon;
    [SerializeField] private GameObject highlightBorder;

    private int slotIndex;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnSlotClicked);
    }

    // Bu fonksiyon, WeaponSlotUI tarafından çağrılarak slotu ayarlar.
    public void Setup(int index, Sprite icon)
    {
        this.slotIndex = index;

        // Eğer bir ikon gönderildiyse...
        if (icon != null)
        {
            // ...ikonu ata ve Image'ı görünür yap.
            this.weaponIcon.sprite = icon;
            this.weaponIcon.enabled = true;
        }
        else
        {
            // Eğer ikon yoksa (slot boşsa), Image'ı gizle.
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