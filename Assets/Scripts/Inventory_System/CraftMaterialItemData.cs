using UnityEngine;

[CreateAssetMenu(menuName = "Items/Material")]
public class CraftMaterialItemData : ItemData
{
    private void OnValidate()
    {
        category = ItemCategory.Material;
        stackable = true;
        if (maxStack < 1)
            maxStack = 99;
    }
}
