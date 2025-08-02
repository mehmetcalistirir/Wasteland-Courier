using UnityEngine;
using System.Collections.Generic;

// Bu, bir silah� craftlamak i�in gereken par�alar� ve miktarlar�n� tan�mlar.
[System.Serializable]
public class PartRequirement
{
    public WeaponPartType partType;
    public int amount;
}

[CreateAssetMenu(fileName = "New Weapon Blueprint", menuName = "Crafting/Weapon Blueprint")]
public class WeaponBlueprint : ScriptableObject
{
    [Tooltip("Bu tarifin üreteceği silaha ait tüm oyun içi verileri (hasar, hız vb.) içeren WeaponData dosyası.")]
    public WeaponData weaponData;
    public string weaponName;
    [TextArea] public string description; // Silah açıklaması için
    public Sprite weaponIcon;
    public int weaponSlotIndexToUnlock;

    // YENİ: Temel kaynak gereksinimleri
    [Header("Resource Requirements")]
    public int requiredStone = 0;
    public int requiredWood = 0;
    public int requiredScrapMetal = 0;

    [Header("Part Requirements")]
    public List<PartRequirement> requiredParts;
    

    
}