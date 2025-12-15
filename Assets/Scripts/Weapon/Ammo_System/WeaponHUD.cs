using UnityEngine;
using TMPro;

public class WeaponHUD : MonoBehaviour
{
    [Header("Texts")]
    public TMP_Text magazineText;   // 12 / 30
    public TMP_Text spareMagText;   // x3
    public TMP_Text ammoPoolText;   // 90

    private PlayerWeapon playerWeapon;

    void Start()
    {
        playerWeapon = FindObjectOfType<PlayerWeapon>();
    }

    void Update()
    {
        if (playerWeapon == null)
            return;

        UpdateMagazine();
        UpdateSpareMags();
        UpdateAmmoPool();
    }

    void UpdateMagazine()
    {
        var mag = playerWeapon.currentMagazine;

        if (mag == null || mag.data == null)
        {
            magazineText.text = "-- / --";
            return;
        }

        magazineText.text =
            $"{mag.currentAmmo} / {mag.data.capacity}";
    }

    void UpdateSpareMags()
    {
        if (playerWeapon.weaponData == null)
        {
            spareMagText.text = "x0";
            return;
        }

        int count = playerWeapon.inventoryMags.FindAll(m =>
            m != null &&
            m.data != null &&
            m.data.ammoType == playerWeapon.weaponData.ammoType
        ).Count;

        spareMagText.text = $"x{count}";
    }

    void UpdateAmmoPool()
    {
        if (playerWeapon.weaponData == null)
        {
            ammoPoolText.text = "0";
            return;
        }

        int ammo = Inventory.Instance.GetAmmoAmount(
            playerWeapon.weaponData.ammoType
        );

        ammoPoolText.text = ammo.ToString();
    }
}
