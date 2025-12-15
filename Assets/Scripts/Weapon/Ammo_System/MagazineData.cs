using UnityEngine;

[CreateAssetMenu(menuName = "Items/Magazine Data")]
public class MagazineData : ItemData
{

    public AmmoTypeData ammoType;
    public MagazineType magazineType;

    public int capacity;
    
    private void OnValidate()
    {
        stackable = false;
        maxStack = 1;
    }

}
