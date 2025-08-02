using UnityEngine;

public class TribeUIFunctions : MonoBehaviour
{
    public PlayerStats playerStats;

    public void OnUpgradeSpeed()
    {
        playerStats.UpgradeSpeed();
    }

    public void OnUpgradeInventory()
    {
        playerStats.UpgradeInventory();
    }
}
