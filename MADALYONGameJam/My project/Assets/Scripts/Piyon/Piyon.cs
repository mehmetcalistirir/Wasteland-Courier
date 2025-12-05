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
    private BaseController targetBase;

    private Team attackerTeam = Team.Player; // 🔥 ARTIK saldıran takım bilgisi tutuluyor

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

    // ----------- BASE’E SALDIRMA ------------
    public void AttackBase(BaseController target, Team attacker)
    {
        targetBase = target;
        attackerTeam = attacker; // 🔥 SALDIRAN TAKIMI KAYDET
        currentMode = Mode.ToBase;
    }

    void BaseeDogruGit()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetBase.transform.position,
            joinSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetBase.transform.position) < 0.25f)
        {
            // 🔥 Artık gerçek takım saldırıyor
            targetBase.ReceiveAttack(1, attackerTeam);
            Destroy(gameObject);
        }
    }

    public void SetWanderSpeed(float s)
    {
        wanderSpeed = s;
    }
}
