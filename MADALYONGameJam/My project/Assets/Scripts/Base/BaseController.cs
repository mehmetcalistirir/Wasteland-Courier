using UnityEngine;

public enum Team { Neutral, Player, Enemy }

public class BaseController : MonoBehaviour
{
    public Team owner = Team.Neutral;
    public int unitCount = 0;
    public int maxUnits = 20;
    public float productionRate = 0.25f; // saniyede üretilen oran

    private float timer = 0f;
    public bool isCastle = false;


    void Update()
    {
        // sadece sahip olunan köy/kale üretim yapar
        if (owner != Team.Neutral && unitCount < maxUnits)
        {
            timer += Time.deltaTime;
            if (timer >= 1f / productionRate)
            {
                unitCount++;
                timer = 0f;
            }
        }
    }

    // 1v1 kayıp sistemi
    public void ReceiveAttack(int attackingUnits, Team attacker)
    {
        int result = attackingUnits - unitCount;

        if (result > 0)
        {
            owner = attacker;
            unitCount = result;
        }
        else
        {
            unitCount = Mathf.Abs(result);
        }
    }

    // --- ELE GEÇİRME SİSTEMİ ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Player köye girmişse
        if (other.CompareTag("Player"))
        {
            if (owner == Team.Neutral)
            {
                owner = Team.Player;
                Debug.Log("Köy PLAYER tarafından ele geçirildi!");
            }
        }

        // Enemy köye girmişse
        if (other.CompareTag("Enemy"))
        {
            if (owner == Team.Neutral)
            {
                owner = Team.Enemy;
                Debug.Log("Köy ENEMY tarafından ele geçirildi!");
            }
        }
    }
}
