using UnityEngine;

[CreateAssetMenu(menuName = "Items/Magazine Item")]
public class MagazineItem : ScriptableObject
{
    public string magName;
    public Sprite icon;

    public string ammoType;   // ← ÖNEMLİ: string olmalı!
    public int capacity = 12;
    public int currentAmmo = 0;

    public bool IsFull => currentAmmo >= capacity;
    public bool IsEmpty => currentAmmo <= 0;
}
