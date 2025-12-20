using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon Item (Inventory)")]
    public WeaponItemData weaponItem;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (weaponItem == null)
        {
            Debug.LogError("[WeaponPickup] weaponItem atanmadı!");
            return;
        }

        // Silahı karavana / envantere ekle
        CaravanInventory.Instance.StoreWeapon(weaponItem);

        Debug.Log($"🟢 Weapon pickup alındı: {weaponItem.itemName}");

        Destroy(gameObject);
    }
}
