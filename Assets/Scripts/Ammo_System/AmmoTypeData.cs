using UnityEngine;
[CreateAssetMenu(menuName = "Items/Weapon/Ammo Type")]
public class AmmoTypeData : ScriptableObject
{
    public string ammoId;     // "9mm", "556", vs (unique)
    public string ammoName;   // UI için
    public Sprite icon;
}