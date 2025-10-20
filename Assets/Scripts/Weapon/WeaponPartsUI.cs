using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class WeaponPartsUI : MonoBehaviour
{
    public static WeaponPartsUI Instance { get; private set; }

    public TextMeshProUGUI barrelText;
    public TextMeshProUGUI magazineText;
    public TextMeshProUGUI handguardText;
    public TextMeshProUGUI gripText;
    public TextMeshProUGUI triggerText;
    public TextMeshProUGUI triggerGuardText;

    private Dictionary<WeaponPartType, TextMeshProUGUI> partTexts;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        partTexts = new Dictionary<WeaponPartType, TextMeshProUGUI>
        {
            { WeaponPartType.Barrel, barrelText },
            { WeaponPartType.Magazine, magazineText },
            { WeaponPartType.Handguard, handguardText },
            { WeaponPartType.Grip, gripText },
            { WeaponPartType.Trigger, triggerText },
            { WeaponPartType.TriggerGuard, triggerGuardText }
        };
    }

    public void UpdatePartText(WeaponPartType part, int count)
    {
        if (partTexts.TryGetValue(part, out var text))
        {
            text.text = $"{part}: {count}";
        }
    }
}
