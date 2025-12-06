using UnityEngine;
using System.Collections;


public class Piyon : MonoBehaviour
{
    public float wanderRadius = 2f;
    public float wanderSpeed = 1f;
    public float joinSpeed = 4f;

    private Vector3 hedefPozisyon;
    private Transform villageCenter;

    private enum Mode { Wander, ToPlayer, ToEnemy, ToBase }

    private Mode currentMode = Mode.Wander;

    private Transform player;

    // --- SALDIRI ---
    private BaseController targetBase;
    private Team attackerTeam = Team.Player;

    // --- SAVUNMA ---
    private BaseController defendBase;

    public void SetVillageCenter(Transform center)
    {
        villageCenter = center;
        YeniHedefBelirle();
    }

    void Update()
    {
        switch (currentMode)
        {
            case Mode.Wander: Wander(); break;
            case Mode.ToPlayer: OyuncuyaDogruGit(); break;
            case Mode.ToBase: BaseeDogruGit(); break;
            case Mode.ToEnemy: DusmanaDogruGit(); break;

        }
    }

    // ----------- WANDER ------------
    void Wander()
    {
        if (villageCenter == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            hedefPozisyon,
            wanderSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, hedefPozisyon) < 0.15f)
            YeniHedefBelirle();
    }

    void YeniHedefBelirle()
    {
        Vector2 random = Random.insideUnitCircle * wanderRadius;
        hedefPozisyon = villageCenter.position + (Vector3)random;
    }

    // ----------- PLAYER’A KATILMA ------------
    public void OyuncuyaKatıl(Transform p)
    {
        player = p;
        currentMode = Mode.ToPlayer;
    }

    void OyuncuyaDogruGit()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            joinSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, player.position) < 0.25f)
        {
            player.GetComponent<PlayerPiyon>().PiyonEkle(1);
            Destroy(gameObject);
        }
    }

    // ----------- SALDIRI AMAÇLI BASE’E GİTME ------------
    public void AttackBase(BaseController target, Team team)
    {
        attackerTeam = team;
        targetBase = target;
        StartCoroutine(DoAttack());
    }

    IEnumerator DoAttack()
    {
        while (Vector2.Distance(transform.position, targetBase.transform.position) > 0.15f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetBase.transform.position, 5f * Time.deltaTime);
            yield return null;
        }

        // SALDIRAN TARAF SAYISINI BUL
        int attackerCount = 1;

        // KÖYDE SAVAŞI ÇALIŞTIR
        targetBase.ResolveBattle(attackerCount, attackerTeam);

        // Bu piyon artık saldırıya katıldığı için yok edilir
        Destroy(gameObject);
    }

    // ----------- SAVUNMA AMAÇLI KALEYE GİTME ------------
    public void GoDefendBase(BaseController castle)
    {
        defendBase = castle;
        currentMode = Mode.ToBase;
    }

    // ----------- BASE'E DOĞRU HAREKET (SALDIRI + SAVUNMA) ------------
    void BaseeDogruGit()
    {
        BaseController goal = defendBase != null ? defendBase : targetBase;

        transform.position = Vector3.MoveTowards(
            transform.position,
            goal.transform.position,
            joinSpeed * Time.deltaTime
        );

        // --- SAVUNMA (Oyuncu kalesine varınca) ---
        if (defendBase != null &&
            Vector3.Distance(transform.position, defendBase.transform.position) < 0.25f)
        {
            defendBase.unitCount++;  // savunmaya ekle
            Destroy(gameObject);
            return;
        }

        // --- SALDIRI (düşman kaleye) ---
        if (targetBase != null &&
            Vector3.Distance(transform.position, targetBase.transform.position) < 0.25f)
        {
            targetBase.ReceiveAttack(1, attackerTeam);
            Destroy(gameObject);
        }
    }


    public void SetWanderSpeed(float s)
    {
        wanderSpeed = s;
    }

    public void DusmanaKatıl(Transform enemyKing)
    {
        player = enemyKing;
        currentMode = Mode.ToEnemy;

        // hedef = enemy army
    }

    void DusmanaDogruGit()
    {
        if (Vector3.Distance(transform.position, player.position) < 0.25f)
        {
            EnemyCommander.instance.enemyArmy.AddUnit(gameObject);

            Destroy(gameObject); // ❗ Artık GameObject yok edilmeli
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            joinSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, player.position) < 0.25f)
        {
            EnemyCommander.instance.enemyArmy.AddUnit(gameObject);
        }
    }


}
