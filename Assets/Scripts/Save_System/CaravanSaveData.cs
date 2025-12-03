using System;
using System.Collections.Generic;  // 🔴 EKLE
using UnityEngine;    

[System.Serializable]
public class CaravanSaveData
{
    public List<WeaponSaveEntry> weaponEntries = new();
}

[System.Serializable]
public class WeaponSaveEntry
{
    public WeaponType type;
    public List<string> weaponIDs;
}
