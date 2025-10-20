using UnityEngine;
using System.Collections.Generic;

// Silah üretim tariflerini tanımlayan ScriptableObject
[CreateAssetMenu(fileName = "New Weapon Blueprint", menuName = "Crafting/Weapon Blueprint")]
public class WeaponBlueprint : ScriptableObject
{
    public WeaponType weaponType;
    [Header("Genel Bilgiler")]
    public string weaponName;
    [TextArea] public string description;
    public Sprite weaponIcon;
    public WeaponData weaponData;
    public WeaponItemData weaponItemSO;


    [Tooltip("Bu silah hangi slotun kilidini açar?")]
    public int weaponSlotIndexToUnlock = 0;

    [Header("Kaynak Gereksinimleri")]
    public int requiredStone = 0;
    public int requiredWood = 0;
    public int requiredScrapMetal = 0;

    [Header("Parça Gereksinimleri")]
    public List<PartRequirement> requiredParts; // Örn: Barrel x1, Trigger x1
}
