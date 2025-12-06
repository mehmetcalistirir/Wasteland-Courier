using UnityEngine;

public class EnemyMovementBoost : MonoBehaviour
{
    public EnemyCommander commander;
    public float zoneBonus = 2f;

    private BaseController zone;
    private float baseSpeed;

public float zoneSlow = -2f;


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

