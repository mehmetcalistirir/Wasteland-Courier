using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Weapon Blueprint")]
public class WeaponBlueprint : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName;

    [Header("Output Weapon")]
    public WeaponData weaponItemSO;

    [Header("Required Parts")]
    public List<PartRequirement> requiredParts = new List<PartRequirement>();
}


