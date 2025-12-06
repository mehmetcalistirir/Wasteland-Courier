using UnityEngine;

public class EnemyMovementBoost : MonoBehaviour
{
    public EnemyCommander commander;
    public float zoneBonus = 2f;

    private BaseController zone;
    private float baseSpeed;

    void Start()
    {
        baseSpeed = commander.stepSpeed;
    }

    public void SetInsideZone(BaseController b)
    {
        zone = b;
    }

    void Update()
    {
        // Anlık kontrol edilen buff sistemi
        if (zone != null && zone.owner == Team.Enemy)
            commander.stepSpeed = baseSpeed + zoneBonus;
        else
            commander.stepSpeed = baseSpeed;
    }
}

