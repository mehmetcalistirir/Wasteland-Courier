using UnityEngine;

[CreateAssetMenu(menuName = "Items/Blueprint")]
public class BlueprintItemData : ItemData
{
    [Tooltip("ID used by Inventory.UnlockBlueprint")]
    public string blueprintID;
}
