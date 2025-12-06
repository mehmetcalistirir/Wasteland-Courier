using UnityEngine;
using System.Collections;


public class Piyon : MonoBehaviour
{
    public float wanderRadius = 2f;
    public float wanderSpeed = 1f;
    public float joinSpeed = 4f;

    // GRID HAREKETİ PARAMETRELERİ
    public float stepSize = 1f;
    public float stepSpeed = 6f;
    public float stepCooldown = 0.15f;
    bool canStep = true;

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
            case Mode.ToEnemy: DusmanaDogruGit(); break;
            case Mode.ToBase: BaseGridMovement(); break;   // 🔥 SATRANÇ MANTIĞI BURADA
        }
    }

    // ---------------- WANDER ----------------
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

    // ---------------- PLAYER’A KATILMA ----------------
    public void OyuncuyaKatıl(Transform p)
    {
        player = p;
        currentMode = Mode.ToPlayer;
    }

    void OyuncuyaDogruGit()
    {
        // ❗ Oyuncuya giderken eski MoveTowards sistemi kullanılacak
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

    // ---------------- SALDIRI (BASE’E GİDİŞ) ----------------
    public void AttackBase(BaseController target, Team team)
    {
        attackerTeam = team;
        targetBase = target;
        defendBase = null;
        currentMode = Mode.ToBase;
    }

    // ---------------- SAVUNMA (KALEYE GİDİŞ) ----------------
    public void GoDefendBase(BaseController castle)
    {
        defendBase = castle;
        targetBase = null;
        currentMode = Mode.ToBase;
    }

    // ============================================================
    // 🔥 BASE'E DOĞRU SATRANÇ HAREKETİ (1 BİRİM + COOLDOWN)
    // ============================================================
    void BaseGridMovement()
    {
        BaseController goal = defendBase != null ? defendBase : targetBase;
        if (goal == null) return;

        // VARRIŞ
        if (Vector2.Distance(transform.position, goal.transform.position) < 0.2f)
        {
            if (defendBase != null)
            {
                defendBase.unitCount++;
                Destroy(gameObject);
            }
            else if (targetBase != null)
            {
                targetBase.ReceiveAttack(1, attackerTeam);
                Destroy(gameObject);
            }
            return;
        }

        // 🔥 sadece cooldown yoksa hareket edebilir
        if (canStep)
        {
            StartCoroutine(GridStep(goal.transform.position));
        }
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

    public void SetWanderSpeed(float s)
{
    wanderSpeed = s;
}


    // 8 yönlü normalize — satranç şahı gibi hareket
    Vector2 NormalizeDirection(Vector2 input)
    {
        float x = Mathf.Sign(input.x);
        float y = Mathf.Sign(input.y);

        if (Mathf.Abs(input.x) < 0.3f) x = 0;
        if (Mathf.Abs(input.y) < 0.3f) y = 0;

        return new Vector2(x, y).normalized;
    }

    // ---------------- ENEMY'E KATILMA ----------------
    public void DusmanaKatıl(Transform enemyKing)
    {
        player = enemyKing;
        currentMode = Mode.ToEnemy;
    }

    void DusmanaDogruGit()
    {
        // ❗ Düşmana giderken de MoveTowards devam edecek
        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            joinSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, player.position) < 0.25f)
        {
            EnemyCommander.instance.enemyArmy.AddUnit(gameObject);
            Destroy(gameObject);
        }
    }
}
