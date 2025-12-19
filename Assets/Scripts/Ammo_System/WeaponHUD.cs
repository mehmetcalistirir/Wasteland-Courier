using System.Linq;
using TMPro;
using UnityEngine;

public class WeaponHUD : MonoBehaviour
{
    [Header("Texts")]
    public TMP_Text magazineText;   // "8 / 12" gibi
    public TMP_Text spareMagText;   // İstersen "x3" gösterebiliriz (opsiyonel)

    private PlayerWeapon pw;
    private Inventory inv;

    private void Awake()
    {
        inv = Inventory.Instance;
    }

    private void OnEnable()
    {
            inv = Inventory.Instance;

    var sm = WeaponSlotManager.Instance;
    if (sm != null)
        pw = sm.ActiveWeapon;

    BindEvents();
    RefreshAll();

        // Weapon eventleri
        if (pw != null)
        {
            pw.OnMagazineChanged += OnWeaponAmmoChanged;
            pw.OnSpareMagazineCountChanged += OnSpareMagCountChanged;
        }

        // Inventory event
        if (inv != null)
            inv.OnChanged += RefreshAll;

        RefreshAll();
    }

    private void OnDisable()
    {
        if (pw != null)
        {
            pw.OnMagazineChanged -= OnWeaponAmmoChanged;
            pw.OnSpareMagazineCountChanged -= OnSpareMagCountChanged;
        }

        if (inv != null)
            inv.OnChanged -= RefreshAll;
    }

    private void OnWeaponAmmoChanged(int current, int capacity)
    {
        // current değişince ikisini de yeniden hesaplamak daha güvenli
        RefreshAll();
    }
    void BindEvents()
{
    if (pw == null) return;

    pw.OnMagazineChanged += OnWeaponAmmoChanged;
    pw.OnSpareMagazineCountChanged += OnSpareMagCountChanged;

    if (inv != null)
        inv.OnChanged += RefreshAll;
}

    private void OnSpareMagCountChanged(int count)
    {
        // Sadece count yazmak istersen:
        if (spareMagText != null)
            spareMagText.text = $"x{count}";

        // Ama esas değerler yine güncellensin:
        RefreshAll();
    }
    void Update()
{
    var sm = WeaponSlotManager.Instance;
    if (sm == null) return;

    if (pw != sm.ActiveWeapon)
    {
        UnbindEvents();
        pw = sm.ActiveWeapon;
        BindEvents();
        RefreshAll();
    }
}

void UnbindEvents()
{
    if (pw == null) return;

    pw.OnMagazineChanged -= OnWeaponAmmoChanged;
    pw.OnSpareMagazineCountChanged -= OnSpareMagCountChanged;

    if (inv != null)
        inv.OnChanged -= RefreshAll;
}

    private void RefreshAll()
{
    if (pw == null || pw.weaponData == null)
    {
        magazineText.text = "-- / --";
        spareMagText.text = "x0";
        return;
    }

    int equippedAmmo = 0;
    if (pw.currentMagazine != null && pw.currentMagazine.data != null)
        equippedAmmo = pw.currentMagazine.currentAmmo;

    int inventoryAmmo = 0;
    for (int i = 0; i < pw.inventoryMags.Count; i++)
    {
        var m = pw.inventoryMags[i];
        if (m == null || m.data == null) continue;
        inventoryAmmo += m.currentAmmo;
    }

    magazineText.text = $"{equippedAmmo} / {inventoryAmmo}";
    spareMagText.text = $"x{pw.inventoryMags.Count}";
}

}
