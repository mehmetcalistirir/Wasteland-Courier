using UnityEngine;

[CreateAssetMenu(menuName = "Items/Weapon/Part")]
public class WeaponPartItemData : ItemData
{
    public WeaponPartType partType;
    private void OnValidate()
    {
        category = ItemCategory.Weapon;
        stackable = true;
    }

}
