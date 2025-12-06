using UnityEngine;

public class EnemyMovementBoost : MonoBehaviour
{
    public EnemyCommanderCore commander;

    public float zoneBonus = 2f;
    public float zoneSlow = -2f;

    private BaseController insideZone = null;
    private bool zoneApplied = false;
    private float zoneSpeedModifier = 0f;

    void Start()
    {
        // Başlangıçta biraz hız bonusu verebilirsin
        commander.movement.moveSpeed *= 1.2f;
    }

    public void SetInsideZone(BaseController zone)
    {
        insideZone = zone;

        if (zone != null && !zoneApplied)
        {
            zoneApplied = true;

            // Eğer düşmanın kendi bölgesiyse hız arttır
            if (zone.owner == Team.Enemy)
                zoneSpeedModifier = zoneBonus;
            else
                zoneSpeedModifier = zoneSlow;

            commander.movement.moveSpeed += zoneSpeedModifier;
        }
        else if (zone == null && zoneApplied)
        {
            commander.movement.moveSpeed -= zoneSpeedModifier;
            zoneApplied = false;
            zoneSpeedModifier = 0f;
        }
    }
}
