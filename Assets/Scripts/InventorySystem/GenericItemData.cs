using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Generic Item")]
public class GenericItemData : ItemData
{
    [Header("Consumable Settings")]
    public bool isConsumable = false;   // Bandaj mı, ilaç mı vs.
    public int healAmount = 25;        // Kaç can dolduruyor
    public float useDuration = 2f;     // Kullanma süresi (anim / bekleme)
}
