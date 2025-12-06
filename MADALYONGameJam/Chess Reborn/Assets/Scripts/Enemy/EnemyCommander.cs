using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class EnemyCommander : MonoBehaviour
{
    public static EnemyCommander instance;
    public TextMeshPro kingCountText;

    [Header("Movement Settings (Grid-Based)")]
    public float stepSize = 1f;          // Bir adım = 1 kare
    public float stepSpeed = 5f;         // Kareye doğru ilerleme hızı
    public float stepCooldown = 0.3f;    // İki adım arası bekleme süresi
    private bool canMove = true;
    public float chaseRange = 4f;   // oyuncuyu görünce kovalama mesafesi
    public float attackRange = 1.2f; // orduların savaşacağı mesafe


    private BaseController currentTargetVillage;
    [Header("Step Sizes")]
    public float straightStepSize = 1f;
    public float diagonalStepSize = 1.4f;


    [Header("Enemy Army")]
    public EnemyArmy enemyArmy;
    public Transform enemyKing;
    public GameObject piyonPrefab;

    [Header("References")]
    public BaseController[] villages;
    public BaseController enemyCastle;
    public BaseController playerCastle;
    public Transform playerKing;

    [Header("AI Settings")]
    public int attackThreshold = 10;
    public int retreatThreshold = 3;
    public float safeRadiusFromPlayer = 6f;
    public int weakVillageUnitThreshold = 5;

    private bool isRetreating = false;

    private Vector2 avoidDir = Vector2.zero;
    private float avoidTimer = 0f;
    public float avoidDuration = 0.5f;
    public float avoidSpeed = 2f;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        InvokeRepeating(nameof(Think), 1f, 1f);
    }

    void Update()
    {
        CheckChasePlayer();
        CheckFightWithPlayer();

        MoveToTargetVillage();

        if (kingCountText != null)
            kingCountText.text = enemyArmy.GetCount().ToString();
        MoveToTargetVillage();
        CheckFightWithPlayer();
    }
    void CheckFightWithPlayer()
    {
        float dist = Vector2.Distance(enemyKing.position, playerKing.position);

        if (dist < attackRange)
        {
            StartKingBattle();
        }
    }


    void CheckChasePlayer()
    {
        if (playerKing == null) return;

        float dist = Vector2.Distance(enemyKing.position, playerKing.position);

        if (dist < chaseRange)
        {
            // Oyuncuya doğru hareket et
            Vector2 dir = (playerKing.position - enemyKing.position).normalized;
            enemyKing.position += (Vector3)dir * Time.deltaTime * 2f; // 2f → hız
        }
    }

    void StartKingBattle()
{
    int enemyCount = enemyArmy.GetCount();
    int playerCount = PlayerCommander.instance.GetArmyCount();

    // --- ÖZEL ÖLÜM KURALI ---
    // Oyuncunun piyonları bittiyse ve düşmanın en az 2 piyonu varsa → Oyuncu ölür
    if (playerCount == 0 && enemyCount >= 2)
    {
        Debug.Log("OYUNCU KRAL ÖLDÜ — OYUN KAYIP!");
        GameMode.Instance.LoseGame();
        return;
    }

    // Düşmanın piyonları bittiyse ve oyuncunun en az 2 piyonu varsa → Düşman ölür
    if (enemyCount == 0 && playerCount >= 2)
    {
        Debug.Log("DÜŞMAN KRAL ÖLDÜ — OYUN KAZANILDI!");
        GameMode.Instance.WinGame();
        return;
    }

    // --- NORMAL 1'e 1 PIYON SAVAŞI ---
    int kill = Mathf.Min(enemyCount, playerCount);

    // Piyon kayıpları
    PlayerCommander.instance.playerArmy.RemovePiyons(kill);
    enemyArmy.RemovePiyons(kill);

    // Güncel sayıları tekrar çek
    enemyCount = enemyArmy.GetCount();
    playerCount = PlayerCommander.instance.GetArmyCount();

    // --- Savaş sonrası ölüm kuralını tekrar kontrol et ---
    if (playerCount == 0 && enemyCount >= 2)
    {
        Debug.Log("OYUNCU KRAL ÖLDÜ — OYUN KAYIP!");
        GameMode.Instance.LoseGame();
        return;
    }

    if (enemyCount == 0 && playerCount >= 2)
    {
        Debug.Log("DÜŞMAN KRAL ÖLDÜ — OYUN KAZANILDI!");
        GameMode.Instance.WinGame();
        return;
    }
}





    // -----------------------------------------------------
    // GRID YÖNLÜ NORMALİZE ETME (8 yön)
    // -----------------------------------------------------
    Vector2 NormalizeDirection(Vector2 input)
    {
        float x = Mathf.Sign(input.x);
        float y = Mathf.Sign(input.y);

        if (Mathf.Abs(input.x) < 0.3f) x = 0;
        if (Mathf.Abs(input.y) < 0.3f) y = 0;

        return new Vector2(x, y).normalized;
    }

    // -----------------------------------------------------
    // GRID ADEDİM HAREKETİ
    // -----------------------------------------------------
    IEnumerator MoveOneStep(Vector2 direction)
    {
        canMove = false;

        float step = (direction.x != 0 && direction.y != 0)
            ? diagonalStepSize
            : straightStepSize;

        Vector2 startPos = enemyKing.position;
        Vector2 targetPos = startPos + direction * step;

        float t = 0f;
        float duration = step / stepSpeed;

        while (t < duration)
        {
            enemyKing.position = Vector2.Lerp(startPos, targetPos, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        enemyKing.position = targetPos;

        yield return new WaitForSeconds(stepCooldown);

        canMove = true;
    }



    // -----------------------------------------------------
    // 3) HAREKET — SATRANÇ ŞAHI GİBİ ADIM ADIM
    // -----------------------------------------------------
    void MoveToTargetVillage()
    {
        if (avoidTimer > 0)
        {
            avoidTimer -= Time.deltaTime;
            enemyKing.position += (Vector3)(avoidDir * avoidSpeed * Time.deltaTime);
            return;
        }

        if (!canMove) return;
        if (enemyKing == null || villages == null || villages.Length == 0) return;

        if (currentTargetVillage == null)
        {
            if (isRetreating)
            {
                currentTargetVillage = enemyCastle;
            }
            else
            {
                currentTargetVillage = PickSmartNeutralVillage();
                if (currentTargetVillage == null)
                    currentTargetVillage = PickAnyNeutralVillage();
                if (currentTargetVillage == null)
                    currentTargetVillage = PickOwnedVillage();
            }
        }

        if (currentTargetVillage == null) return;

        Vector2 diff = currentTargetVillage.transform.position - enemyKing.position;

        if (diff.magnitude < 0.5f)
        {
            OnReachVillage(currentTargetVillage);
            currentTargetVillage = null;
            return;
        }

        Vector2 dir = NormalizeDirection(diff);

        // 🔥 DÜZ / ÇAPRAZ ADIM HESABI
        float step = (dir.x != 0 && dir.y != 0)
            ? diagonalStepSize
            : straightStepSize;

        Vector2 targetPos = (Vector2)enemyKing.position + dir * step;

        StartCoroutine(MoveOneStep(dir));

    }


    // -----------------------------------------------------
    // 1) DÜŞMANIN SAHİP OLDUĞU KÖYLER
    // -----------------------------------------------------
    public List<BaseController> GetOwnedVillages()
    {
        List<BaseController> owned = new List<BaseController>();

        foreach (var v in villages)
        {
            if (v != null && v.owner == Team.Enemy && !v.isCastle)
                owned.Add(v);
        }

        return owned;
    }

    // -----------------------------------------------------
    // 2) AI ZEKÂSINI YÖNETEN ANA FONKSİYON
    // -----------------------------------------------------
    void Think()
    {
        if (enemyArmy == null || enemyKing == null || villages == null || villages.Length == 0)
            return;

        List<BaseController> owned = GetOwnedVillages();

        if (owned.Count == 0)
        {
            isRetreating = false;
            TryCaptureNeutralVillage();
            return;
        }

        SmartRetreatCheck();
        DefendThreatenedVillages(owned);
        GatherArmy();
        TryAttack();
    }

    // -----------------------------------------------------
    // 4) KÖY SEÇME — AKILLI NÖTR KÖY
    // -----------------------------------------------------
    BaseController PickSmartNeutralVillage()
    {
        BaseController best = null;
        float bestScore = Mathf.Infinity;

        foreach (var v in villages)
        {
            if (v == null) continue;
            if (v.owner != Team.Neutral) continue;

            float distEnemy = Vector2.Distance(enemyKing.position, v.transform.position);
            float distPlayer = playerKing != null
                ? Vector2.Distance(playerKing.position, v.transform.position)
                : Mathf.Infinity;

            if (distPlayer + 1f < distEnemy)
                continue;

            BaseController bc = v.GetComponent<BaseController>();
            float valueFactor = bc != null ? (1f / Mathf.Max(0.1f, bc.productionRate)) : 1f;

            float score = distEnemy * 0.7f + valueFactor * 3f;

            if (score < bestScore)
            {
                bestScore = score;
                best = v;
            }
        }

        return best;
    }

    BaseController PickAnyNeutralVillage()
    {
        List<BaseController> list = new List<BaseController>();
        foreach (var v in villages)
            if (v != null && v.owner == Team.Neutral)
                list.Add(v);

        if (list.Count == 0)
            return null;

        return list[Random.Range(0, list.Count)];
    }

    BaseController PickOwnedVillage()
    {
        List<BaseController> owned = GetOwnedVillages();
        if (owned.Count == 0)
            return null;

        return owned[Random.Range(0, owned.Count)];
    }

    // -----------------------------------------------------
    // 5) KÖYE ULAŞINCA NE YAPAR?
    // -----------------------------------------------------
    void OnReachVillage(BaseController village)
    {
        if (village == null) return;

        if (village.owner == Team.Neutral)
        {
            village.owner = Team.Enemy;
            return;
        }

        if (village.owner == Team.Enemy)
        {
            return;
        }

        if (village.owner == Team.Player)
        {
            EnemyAttack(village);
            return;
        }
    }

    // -----------------------------------------------------
    // 6) KÖYLERDEN ORDU TOPLAMA
    // -----------------------------------------------------
    void GatherArmy()
    {
        foreach (var v in villages)
        {
            if (v != null && v.owner == Team.Enemy && EnemyIsAt(v))
            {
                EnemyAddVillagePiyonsToArmy(v);
            }
        }

        if (enemyCastle != null && EnemyIsAt(enemyCastle))
        {
            EnemyAddVillagePiyonsToArmy(enemyCastle);
        }
    }

    public void EnemyAddVillagePiyonsToArmy(BaseController village)
    {
        if (village == null) return;
        if (village.unitCount <= 0) return;
        if (village.owner != Team.Enemy) return;
        if (!EnemyIsAt(village)) return;

        BasePiyonManager bpm = village.GetComponent<BasePiyonManager>();
        if (bpm == null) return;

        bpm.TransferAllToEnemy(enemyKing);
    }

    void TryCaptureNeutralVillage()
    {
        if (villages == null) return;

        foreach (var v in villages)
        {
            if (v != null && v.owner == Team.Neutral)
            {
                currentTargetVillage = v;
                return;
            }
        }
    }

    // -----------------------------------------------------
    // 8) TEHDİT ANALİZİ
    // -----------------------------------------------------
    float EvaluateVillageThreat(BaseController v)
    {
        if (v == null) return 0f;

        float threat = 0f;
        int playerArmyCount = PlayerCommander.instance != null
            ? PlayerCommander.instance.GetArmyCount()
            : 0;

        float distPlayer = playerKing != null
            ? Vector2.Distance(playerKing.position, v.transform.position)
            : Mathf.Infinity;

        if (v.owner == Team.Enemy)
        {
            threat += Mathf.Max(0, weakVillageUnitThreshold - v.unitCount) * 2f;

            if (distPlayer < safeRadiusFromPlayer)
                threat += (safeRadiusFromPlayer - distPlayer) * 2f;

            threat += playerArmyCount * 0.2f;
        }

        return threat;
    }

    void DefendThreatenedVillages(List<BaseController> owned)
    {
        if (owned == null || owned.Count == 0) return;

        BaseController mostThreatened = null;
        float maxThreat = 0f;

        foreach (var v in owned)
        {
            float t = EvaluateVillageThreat(v);
            if (t > maxThreat)
            {
                maxThreat = t;
                mostThreatened = v;
            }
        }

        if (mostThreatened != null && maxThreat > 3f)
        {
            BaseController reinforceSource = FindStrongestOwnedVillage(owned);
            if (reinforceSource != null && reinforceSource != mostThreatened)
            {
                SendDefense(reinforceSource, mostThreatened);
            }
        }
    }

    BaseController FindStrongestOwnedVillage(List<BaseController> owned)
    {
        BaseController best = null;
        int max = 0;

        foreach (var v in owned)
        {
            if (v.unitCount > max)
            {
                max = v.unitCount;
                best = v;
            }
        }

        return best;
    }

    void SendDefense(BaseController from, BaseController to)
    {
        if (from == null || to == null) return;
        if (!EnemyIsAt(from)) return;

        BasePiyonManager bpm = from.GetComponent<BasePiyonManager>();
        if (bpm == null) return;

        bpm.SendAllToCastle(to);
    }

    // -----------------------------------------------------
    // 10) GERİ ÇEKİLME
    // -----------------------------------------------------
    void SmartRetreatCheck()
    {
        if (enemyArmy == null) return;

        int count = enemyArmy.GetCount();

        if (count <= retreatThreshold)
        {
            isRetreating = true;
            currentTargetVillage = enemyCastle;
        }
        else if (count >= attackThreshold)
        {
            isRetreating = false;
        }
    }

    // -----------------------------------------------------
    // 11) SALDIRI
    // -----------------------------------------------------
    bool CanConquer(BaseController targetVillage)
    {
        if (targetVillage == null || enemyArmy == null)
            return false;

        BasePiyonManager bpm = targetVillage.GetComponent<BasePiyonManager>();
        int villagePiyon = bpm != null ? bpm.GetPiyonCount() : targetVillage.unitCount;

        int myArmy = enemyArmy.GetCount();

        return myArmy > villagePiyon;
    }

    void TryAttack()
    {
        if (enemyArmy == null) return;

        BaseController target = FindTargetPlayerVillage();

        if (target != null)
        {
            SendArmyTo(target);
            return;
        }

        if (enemyArmy.GetCount() < attackThreshold)
            return;

        if (playerCastle != null && CanConquer(playerCastle))
        {
            SendArmyTo(playerCastle);
        }
    }

    BaseController FindTargetPlayerVillage()
    {
        BaseController best = null;
        float bestScore = Mathf.Infinity;

        foreach (var v in villages)
        {
            if (v == null) continue;
            if (v.owner != Team.Player) continue;

            if (!CanConquer(v))
                continue;

            float distEnemy = Vector2.Distance(enemyKing.position, v.transform.position);
            float distPlayer = playerKing != null
                ? Vector2.Distance(playerKing.position, v.transform.position)
                : Mathf.Infinity;

            float score = distEnemy + Mathf.Max(0, safeRadiusFromPlayer - distPlayer) * 3f;

            if (score < bestScore)
            {
                bestScore = score;
                best = v;
            }
        }

        return best;
    }

    // -----------------------------------------------------
    // 12) SALDIRI BAŞLATMA
    // -----------------------------------------------------
    public void SendArmyTo(BaseController target)
{
    if (target == null || enemyArmy == null) return;

    // Eğer hiç piyon yoksa saldırı yapma!
    int attackerCount = enemyArmy.GetCount();
    if (attackerCount <= 0) return;

    // 1) Tüm piyonları ordudan çıkar
    GameObject[] units = enemyArmy.ExtractAll();

    // 2) Her piyonu hedefe yürümeye gönder
    foreach (var p in units)
    {
        if (p == null) continue;

        Piyon pawn = p.GetComponent<Piyon>();
        pawn.AttackBase(target, Team.Enemy);
    }
}



    bool EnemyIsAt(BaseController village)
    {
        return Vector2.Distance(enemyKing.position, village.transform.position) < 0.5f;
    }

    public void EnemyAttack(BaseController target)
    {
        int attackerCount = enemyArmy.GetCount();

        target.ResolveBattle(attackerCount, Team.Enemy);

        enemyArmy.ExtractAll();
    }

    public void OnObstacleDetected(Collider2D obstacle)
    {
        Vector2 toObstacle = obstacle.transform.position - enemyKing.position;

        // Engel x ekseninde ise → yukarı/aşağı kaç
        if (Mathf.Abs(toObstacle.x) > Mathf.Abs(toObstacle.y))
        {
            avoidDir = new Vector2(0, toObstacle.y > 0 ? -1 : 1);
        }
        else
        {
            // Engel y ekseninde ise → sağ/sol kaç
            avoidDir = new Vector2(toObstacle.x > 0 ? -1 : 1, 0);
        }

        avoidTimer = avoidDuration;
    }
}
