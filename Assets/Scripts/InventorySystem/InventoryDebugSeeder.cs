using UnityEngine;

public class InventoryDebugSeeder : MonoBehaviour
{
    [Header("Sample Items")]
    public GenericItemData stoneSO;
    public GenericItemData ammo9mmSO;

    public GenericItemData BluePrintSO;

    public GenericItemData CookedMeatSO;

    public GenericItemData DeerHideSO;

    public GenericItemData MeatSO;

    public GenericItemData RabbitHideSO;

    public GenericItemData ScrapSO;

    public GenericItemData WoodSO;

    public WeaponItemData  machinegunSO;

    public WeaponItemData pistolSO;

    public WeaponItemData shotgunSO;

    public WeaponItemData sniperSO;

    public WeaponItemData throwingSpearSO;

    public WeaponItemData bowSO;

    public WeaponItemData meeleSpearSO;

    public WeaponItemData meeleSwordSO;


    void Start()
    {
        // Senin örneklerin birebir burada:
        Inventory.Instance.TryAdd(stoneSO, 18);
        Inventory.Instance.TryAdd(ammo9mmSO, 120);
        Inventory.Instance.TryAdd(BluePrintSO, 10);
        Inventory.Instance.TryAdd(CookedMeatSO, 90);
        Inventory.Instance.TryAdd(MeatSO, 90);
        Inventory.Instance.TryAdd(RabbitHideSO, 50);
        Inventory.Instance.TryAdd(ScrapSO, 99);
        Inventory.Instance.TryAdd(WoodSO, 99);
        Inventory.Instance.TryAdd(machinegunSO, 1);
        Inventory.Instance.TryAdd(pistolSO, 1);
        Inventory.Instance.TryAdd(machinegunSO, 1);
        Inventory.Instance.TryAdd(shotgunSO, 1);
        Inventory.Instance.TryAdd(sniperSO, 1);
        Inventory.Instance.TryAdd(throwingSpearSO, 1);
        Inventory.Instance.TryAdd(bowSO, 1);
        Inventory.Instance.TryAdd(meeleSpearSO, 1);
        Inventory.Instance.TryAdd(meeleSwordSO, 1);


    }
}
