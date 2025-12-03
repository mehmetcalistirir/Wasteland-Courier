using System;

[Serializable]
public class WeaponSlotSaveData
{
    public string[] equippedWeaponIDs = new string[3];
    public int[] clip = new int[3];
    public int[] reserve = new int[3];
    public int activeSlotIndex;
}
