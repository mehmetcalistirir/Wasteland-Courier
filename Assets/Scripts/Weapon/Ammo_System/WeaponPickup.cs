using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public WeaponData weaponData;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        CaravanInventory.Instance.StoreWeapon(weaponData);
        Destroy(gameObject);
    }
}
