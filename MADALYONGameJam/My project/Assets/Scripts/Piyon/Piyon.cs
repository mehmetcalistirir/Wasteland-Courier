using UnityEngine;

public class Piyon : MonoBehaviour
{
    public float wanderRadius = 2f;
    public float wanderSpeed = 1f;
    public float joinSpeed = 4f;

    private Vector3 hedefPozisyon;
    private Transform villageCenter;

    private enum Mode { Wander, ToPlayer, ToBase }
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
    public void AttackBase(BaseController target, Team attacker)
    {
        targetBase = target;
        attackerTeam = attacker;
        currentMode = Mode.ToBase;
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
}
