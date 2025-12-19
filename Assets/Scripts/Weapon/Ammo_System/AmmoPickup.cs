using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public AmmoTypeData ammoType;

    [Header("Ammo Amount Range")]
    public int minAmount = 5;
    public int maxAmount = 15;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        int amount = Random.Range(minAmount, maxAmount + 1); // int için +1 şart

        Inventory.Instance.AddAmmo(ammoType, amount);
        Destroy(gameObject);
    }
}
