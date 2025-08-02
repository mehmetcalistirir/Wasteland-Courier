using UnityEngine;

public class BlueprintManager : MonoBehaviour
{
    public static BlueprintManager Instance;

    public bool hasBarrel;
    public bool hasMagazine;
    public bool hasGrip;
    public bool hasHandle;
    public bool hasTrigger;
    public bool hasGuard;

    public GameObject machineGunPrefab; // Üretilecek silah

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddPart(string partName)
    {
        switch (partName)
        {
            case "Barrel": hasBarrel = true; break;
            case "Magazine": hasMagazine = true; break;
            case "Grip": hasGrip = true; break;
            case "Handle": hasHandle = true; break;
            case "Trigger": hasTrigger = true; break;
            case "Guard": hasGuard = true; break;
        }

        if (CanCraftWeapon())
            UIController.Instance.ShowCraftMessage(); // Uyarý mesajý göster
    }

    public bool CanCraftWeapon()
    {
        return hasBarrel && hasMagazine && hasGrip && hasHandle && hasTrigger && hasGuard;
    }

    public void ConsumeBlueprintParts()
    {
        hasBarrel = hasMagazine = hasGrip = hasHandle = hasTrigger = hasGuard = false;
    }
}
