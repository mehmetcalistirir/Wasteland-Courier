using UnityEngine;

[CreateAssetMenu(menuName = "Items/Consumable")]
public class ConsumableItemData : ItemData
{
    [Header("Consumable Settings")]
    public int healAmount = 25;
    public float useDuration = 2f;

    private void OnValidate()
    {
        category = ItemCategory.Consumable;
        stackable = true;
    }
}
