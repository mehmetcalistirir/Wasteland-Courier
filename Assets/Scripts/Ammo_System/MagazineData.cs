using UnityEngine;

[CreateAssetMenu(menuName = "Items/Weapon/Magazine")]
public class MagazineData : ItemData
{

    public AmmoTypeData ammoType;
    public MagazineType magazineType;

    public int capacity;
    
    private void OnValidate()
    {
        category = ItemCategory.Weapon;
        stackable = false;
        maxStack = 1;
    }

}

