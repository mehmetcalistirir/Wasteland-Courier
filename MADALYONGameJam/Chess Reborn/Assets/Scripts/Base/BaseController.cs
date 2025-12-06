using UnityEngine;
using TMPro;

public enum Team { Neutral, Player, Enemy }

public class BaseController : MonoBehaviour
{
    public TextMeshPro countText;
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
    void LateUpdate()
    {
        if (countText != null)
        {
            countText.text = unitCount.ToString();
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
        // --- KALE ELE GEÇİRME KONTROLÜ ---
        if (isCastle)
        {
            GameMode.Instance.CheckCastleWinLose(this);
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
        // PLAYER KING tarafsız köye girerse → savaş yok → direkt ele geçir
        if (other.CompareTag("PlayerKing") && owner == Team.Neutral)
        {
            owner = Team.Player;
            return;
        }

        // ENEMY KING tarafsız köye girerse → savaş yok → direkt ele geçir
        if (other.CompareTag("EnemyKing") && owner == Team.Neutral)
        {
            owner = Team.Enemy;
            return;
        }

        // PLAYER ORDUSU rakip köye girerse savaş
        if (other.CompareTag("PlayerKing") && owner == Team.Enemy)
        {
            int attackerCount = PlayerCommander.instance.playerArmy.GetCount();
            StartBattle(attackerCount, Team.Player);
            return;
        }

        // ENEMY ORDUSU rakip köye girerse savaş
        if (other.CompareTag("EnemyKing") && owner == Team.Player)
        {
            int attackerCount = EnemyCommander.instance.enemyArmy.GetCount();
            StartBattle(attackerCount, Team.Enemy);
            return;
        }
    }




}
