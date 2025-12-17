using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public AmmoTypeData ammoType;
    public int amount = 15;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Inventory.Instance.AddAmmo(ammoType, amount);
        Destroy(gameObject);
    }
}
