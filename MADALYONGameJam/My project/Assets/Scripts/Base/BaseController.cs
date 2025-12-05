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
    BasePiyonManager bpm = GetComponent<BasePiyonManager>();
    int defenderCount = bpm != null ? bpm.GetPiyonCount() : unitCount;

    // 1) Karşılıklı öldürme
    int kill = Mathf.Min(attackerCount, defenderCount);

    int attackerRemaining = attackerCount - kill;
    int defenderRemaining = defenderCount - kill;

    // Savunmacı piyonları sil
    if (bpm != null)
        bpm.RemovePiyons(kill);
    else
        unitCount = defenderRemaining;

    // 2) Saldıran taraf PİYON KAYBETTİ → Bunları ilgili ordudan düşeceğiz (dışarıda)

    // 3) Eğer saldıran kazanırsa → köy owner değişir
    if (attackerRemaining > 0)
    {
        owner = attackerTeam;

        // Kalan saldıran piyonlar köye yerleşsin
        if (bpm != null)
        {
            bpm.RemovePiyons(defenderRemaining); // bütün savunma temizlendi

            for (int i = 0; i < attackerRemaining; i++)
                bpm.AddFakePiyon();
        }

        unitCount = attackerRemaining;
    }
    else
    {
        // Köy savundu
        unitCount = defenderRemaining;
    }
}


    // --- ELE GEÇİRME SİSTEMİ ---
    private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        if (owner == Team.Neutral)
            owner = Team.Player;
    }

    if (other.CompareTag("Enemy"))
    {
        if (owner == Team.Neutral)
            owner = Team.Enemy;
    }
}

}
