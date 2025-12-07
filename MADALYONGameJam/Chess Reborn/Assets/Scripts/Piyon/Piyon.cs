using UnityEngine;
using System.Collections;

public class Piyon : MonoBehaviour
{
    public float wanderRadius = 2f;
    public float wanderSpeed = 1f;
    public float joinSpeed = 4f;

    // GRID HAREKETİ
    public float stepSize = 1f;
    public float stepSpeed = 6f;
    public float stepCooldown = 0.15f;
    bool canStep = true;

    private Vector3 hedefPozisyon;
    private Transform villageCenter;

    private enum Mode { Wander, ToPlayer, ToEnemy, ToBase, FightLine }
    private Mode currentMode = Mode.Wander;

    private Transform player;

    // Base saldırı/savunma
    private BaseController targetBase;
    private Team attackerTeam = Team.Player;
    private BaseController defendBase;

    // ------------------------------------------------------------
    // SAVAŞ ÇİZGİSİ
    // ------------------------------------------------------------
    public void EnterFightLine(Vector3 lineCenter, int index, int totalCount, bool isPlayerSide)
    {
        currentMode = Mode.FightLine;

        float spacing = 0.7f;
        float dir = isPlayerSide ? 1f : -1f;

        float offsetIndex = index - (totalCount - 1) / 2f;
        Vector3 offset = new Vector3(offsetIndex * spacing * dir, 0f, 0f);

        hedefPozisyon = lineCenter + offset;
    }

    void FightLineMove()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            hedefPozisyon,
            joinSpeed * Time.deltaTime
        );
    }

    // ------------------------------------------------------------
    // KÖY WANDER
    // ------------------------------------------------------------
    public void SetVillageCenter(Transform center)
    {
         Debug.Log("[PİYON] Village center set: " + center.name + " pawn=" + gameObject.name);
        villageCenter = center;
        YeniHedefBelirle();
    }

    void Update()
    {
        if (currentMode == Mode.ToEnemy && player == null)
{
    Debug.LogError("[AUTO-JOIN DETECTED] Pawn " + gameObject.name + " ToEnemy modunda ama manuel tetikleme yok!");
}

        switch (currentMode)
        {
            case Mode.Wander:    Wander(); break;
            case Mode.ToPlayer:  OyuncuyaDogruGit(); break;
            case Mode.ToEnemy:   DusmanaDogruGit(); break;
            case Mode.ToBase:    BaseGridMovement(); break;
            case Mode.FightLine: FightLineMove(); break;
        }
    }

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

    // ------------------------------------------------------------
    // PLAYER’A KATILMA
    // ------------------------------------------------------------
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

    // ------------------------------------------------------------
    // BASE’E SALDIRI veya SAVUNMA
    // ------------------------------------------------------------
    public void AttackBase(BaseController target, Team team)
    {
        attackerTeam = team;
        targetBase = target;
        defendBase = null;
        currentMode = Mode.ToBase;
    }

    public void GoDefendBase(BaseController castle)
    {
        defendBase = castle;
        targetBase = null;
        currentMode = Mode.ToBase;
    }

    void BaseGridMovement()
{
    BaseController goal = defendBase != null ? defendBase : targetBase;
    if (goal == null) return;

    // 🔥 HEDEFE VARINCA SALDIR veya SAVUN
    if (Vector2.Distance(transform.position, goal.transform.position) < 0.2f)
    {
        if (defendBase != null)
        {
            defendBase.unitCount++;
            Destroy(gameObject);
            return;
        }

        if (targetBase != null)
        {
            targetBase.ReceiveAttack(1, attackerTeam);
            Destroy(gameObject);
            return;
        }
    }

    // Grid-step ile yürü
    if (canStep)
        StartCoroutine(GridStep(goal.transform.position));
}


    IEnumerator GridStep(Vector3 hedef)
    {
        canStep = false;

        Vector2 diff = hedef - transform.position;
        Vector2 dir = NormalizeDirection(diff);

        Vector2 start = transform.position;
        Vector2 end = start + dir * stepSize;

        float t = 0f;
        float duration = stepSize / stepSpeed;

        while (t < duration)
        {
            transform.position = Vector2.Lerp(start, end, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        transform.position = end;

        yield return new WaitForSeconds(stepCooldown);
        canStep = true;
    }

    Vector2 NormalizeDirection(Vector2 input)
    {
        float x = Mathf.Sign(input.x);
        float y = Mathf.Sign(input.y);

        if (Mathf.Abs(input.x) < 0.3f) x = 0;
        if (Mathf.Abs(input.y) < 0.3f) y = 0;

        return new Vector2(x, y).normalized;
    }

    // ------------------------------------------------------------
    // DÜŞMANA KATILMA (SADECE TransferAllToEnemy ile)
    // ------------------------------------------------------------
    public void DusmanaKatıl(Transform enemyKing)
    {

        player = enemyKing;
        currentMode = Mode.ToEnemy;
    }
    public void SetWanderSpeed(float s)
{
    wanderSpeed = s;
}




    void DusmanaDogruGit()
    {
        if (player == null)
        {
            currentMode = Mode.Wander;
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            joinSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, player.position) < 0.25f)
        {
            EnemyCommanderCore.instance.enemyArmy.AddUnit(gameObject);
currentMode = Mode.ToEnemy;

        }
    }
}
