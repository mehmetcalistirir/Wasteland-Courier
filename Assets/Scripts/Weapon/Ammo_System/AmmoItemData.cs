using UnityEngine;

[CreateAssetMenu(menuName = "Items/Ammo Data")]
public class AmmoItemData : ItemData
{
    [Header("Ammo Settings")]
    public string ammoType;          // 9mm, 5.56, 7.62
    public int ammoAmount = 10;      // Bir kutudan gelen toplam mermi miktarı
}




[CreateAssetMenu(menuName = "Items/Ammo Type")]
public class AmmoType : ScriptableObject
{
    public string ammoId;     // dictionary için benzersiz ID
    public string ammoName;
    public Sprite icon;
}


