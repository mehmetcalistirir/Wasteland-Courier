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
    public void ResolveBattle(int attackerCount, Team attackerTeam)
{
    // SAVUNMA GÜCÜ = gerçek piyon sayısı
    int defenderCount = unitCount;

    // 1) Karşılıklı öldürme
    int kill = Mathf.Min(attackerCount, defenderCount);

    int attackerRemaining = attackerCount - kill;
    int defenderRemaining = defenderCount - kill;

    // ---- Savunmacı kaybı ----
    unitCount = defenderRemaining;

    // BPM varsa görsel piyonları da azalt
    BasePiyonManager bpm = GetComponent<BasePiyonManager>();
    if (bpm != null)
        bpm.SyncTo(unitCount);

    // ---- Saldıran kazandı ----
    if (attackerRemaining > 0)
    {
        owner = attackerTeam;
        unitCount = attackerRemaining;

        if (bpm != null)
            bpm.SyncTo(unitCount);
    }
}

void StartBattle(int attackerCount, Team attackerTeam)
{
    BasePiyonManager bpm = GetComponent<BasePiyonManager>();

    int defenderCount = bpm != null ? bpm.GetPiyonCount() : unitCount;

    int kill = Mathf.Min(attackerCount, defenderCount);

    int attackerRemaining = attackerCount - kill;
    int defenderRemaining = defenderCount - kill;

    // Savunma kayıpları
    if (bpm != null)
        bpm.RemovePiyons(kill);
    unitCount = defenderRemaining;

    // Saldıran taraf kayıpları
    if (attackerTeam == Team.Player)
    {
        PlayerCommander.instance.playerArmy.RemovePiyons(kill);
    }
    else
    {
        EnemyCommander.instance.enemyArmy.RemovePiyons(kill);
    }

    // Eğer saldıran kazandıysa köyü ele geçir
    if (attackerRemaining > 0)
    {
        owner = attackerTeam;

        // Kazanan taraf köye kalan ordusunu yerleştirir
        unitCount = attackerRemaining;

        if (bpm != null)
            bpm.SyncTo(attackerRemaining);
    }
}




    // --- ELE GEÇİRME SİSTEMİ ---
    private void OnTriggerEnter2D(Collider2D other)
{
    // PLAYER ORDUSU GİRERSE
    if (other.CompareTag("PlayerKing"))
    {
        int attackerCount = PlayerCommander.instance.playerArmy.GetCount();
        StartBattle(attackerCount, Team.Player);
    }

    // ENEMY ORDUSU GİRERSE
    if (other.CompareTag("EnemyKing"))
    {
        int attackerCount = EnemyCommander.instance.enemyArmy.GetCount();
        StartBattle(attackerCount, Team.Enemy);
    }
}



}
