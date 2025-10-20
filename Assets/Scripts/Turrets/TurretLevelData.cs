using UnityEngine;

[System.Serializable]
public class TurretLevelData
{
    public GameObject prefab;
    public int requiredStone;
    public int requiredWood;
    public string requiredBlueprintId;
     [Header("Sample Items")]
    public ItemData stoneSO;
    public ItemData woodSO;



}
