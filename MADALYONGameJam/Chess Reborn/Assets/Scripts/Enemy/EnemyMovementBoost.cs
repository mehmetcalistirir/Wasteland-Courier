using UnityEngine;

public class EnemyMovementBoost : MonoBehaviour
{
    public EnemyCommander commander;
    public float zoneBonus = 2f;

    private BaseController zone;
    private float baseSpeed;
    private BaseController insideZone = null;
    private bool slowApplied = false;
    private float originalSpeed;
    public float moveSpeed = 3f; // örnek

    private bool zoneApplied = false;
    private float zoneSpeedModifier = 0f;



    public float zoneSlow = -2f;


    void Start()
    {
        baseSpeed = commander.stepSpeed;
    }

    public void SetInsideZone(BaseController zone)
    {
        insideZone = zone;

        if (zone != null && !zoneApplied)
        {
            zoneApplied = true;

            zoneSpeedModifier = -1f; // yavaşlatma örneği
            moveSpeed += zoneSpeedModifier;
        }
        else if (zone == null && zoneApplied)
        {
            moveSpeed -= zoneSpeedModifier;
            zoneApplied = false;
            zoneSpeedModifier = 0f;
        }
    }

    void Update()
    {
        float zoneEffect = 0f;

        if (zone != null)
        {
            if (zone.owner == Team.Enemy)
                zoneEffect = zoneBonus;  // kendi bölgesi → hızlan
            else if (zone.owner == Team.Player)
                zoneEffect = zoneSlow;   // oyuncu bölgesi → yavaşla
        }

        commander.stepSpeed = baseSpeed + zoneEffect;
    }

}

