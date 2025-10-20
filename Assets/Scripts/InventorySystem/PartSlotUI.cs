using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PartSlotUI : MonoBehaviour
{
    public Image icon;                           // Bu Image component'i olmalı
    public TextMeshProUGUI countText;            // Bu TMP component'i olmalı

    public void Setup(Sprite iconSprite, int amount)
    {
        if (icon != null)
            icon.sprite = iconSprite;

        if (countText != null)
            countText.text = $"x{amount}";
    }
}

