using UnityEngine;

public class InventoryTestSeeder : MonoBehaviour
{
    void Start()
    {
        var item = Resources.Load<WeaponItemData>("Items/MachinegunItem");
        Inventory.Instance.TryAdd(item, 1);
    }
}
