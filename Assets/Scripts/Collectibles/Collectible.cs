using UnityEngine;

public class Collectible : MonoBehaviour
{
    public CollectibleType type;

    [Header("Inventory Item")]
    public ItemData item;
    public int minAmount = 1;
    public int maxAmount = 1;

    [Header("Ammo")]
    public AmmoTypeData ammoType;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        int amount = Random.Range(minAmount, maxAmount + 1);

        switch (type)
        {
            case CollectibleType.InventoryItem:
                Inventory.Instance.TryAdd(item, amount);
                break;

            case CollectibleType.Ammo:
                Inventory.Instance.AddAmmo(ammoType, amount);
                break;
        }

        Destroy(gameObject);
    }
}
