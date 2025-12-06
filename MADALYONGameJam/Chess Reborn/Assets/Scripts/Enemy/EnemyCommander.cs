using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class EnemyCommander : MonoBehaviour
{
    public static EnemyCommander instance;

    [Header("Grid Step Movement")]
    public float straightStepSize = 1f;
    public float diagonalStepSize = 1.4f;
    public float stepSpeed = 6f;
    public float stepCooldown = 0.25f;
    private bool canStep = true;

    [Header("UI")]
    public TextMeshPro kingCountText;

    [Header("References")]
    public EnemyArmy enemyArmy;
    public Transform enemyKing;
    public Transform playerKing;

    public BaseController[] villages;
    public BaseController enemyCastle;
    public BaseController playerCastle;

    [Header("AI Combat Settings")]
    public float chaseRange = 4f;
    public float attackRange = 1.2f;
    public int attackThreshold = 10;
    public int retreatThreshold = 3;

    private BaseController currentTargetVillage;
    private bool isRetreating = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        InvokeRepeating(nameof(Think), 1f, 1f);
    }

    // -------------------------------------------------------
    // GRID YÖNLÜ NORMALİZE
    // -------------------------------------------------------
    Vector2 NormalizeDirection(Vector2 input)
    {
        float x = Mathf.Sign(input.x);
        float y = Mathf.Sign(input.y);

        if (Mathf.Abs(input.x) < 0.3f) x = 0;
        if (Mathf.Abs(input.y) < 0.3f) y = 0;

        return new Vector2(x, y).normalized;
    }

    IEnumerator MoveOneStep(Vector2 direction)
    {
        canStep = false;

        float step = (direction.x != 0 && direction.y != 0)
            ? diagonalStepSize
            : straightStepSize;

        Vector2 start = enemyKing.position;
        Vector2 end = start + direction * step;

        float t = 0f;
        float duration = step / stepSpeed;

        while (t < duration)
        {
            enemyKing.position = Vector2.Lerp(start, end, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        enemyKing.position = end;

        yield return new WaitForSeconds(stepCooldown);

        canStep = true;
    }

    void Update()
    {
        CheckChasePlayer();
        CheckFightWithPlayer();
        MoveToTargetVillage();

        if (kingCountText != null)
            kingCountText.text = enemyArmy.GetCount().ToString();
    }

    // -------------------------------------------------------
    // PLAYER KING YAKINSA TAKİP ET
    // -------------------------------------------------------
    void CheckChasePlayer()
    {
        if (playerKing == null) return;

        float dist = Vector2.Distance(enemyKing.position, playerKing.position);

        if (dist < chaseRange)
        {
            Vector2 dir = (playerKing.position - enemyKing.position).normalized;
            enemyKing.position += (Vector3)dir * Time.deltaTime * stepSpeed;
        }
    }

    // -------------------------------------------------------
    // PLAYER İLE KRAL SAVAŞI
    // -------------------------------------------------------
    void CheckFightWithPlayer()
    {
        float dist = Vector2.Distance(enemyKing.position, playerKing.position);

        if (dist < attackRange)
            StartKingBattle();
    }

    void StartKingBattle()
    {
        int enemyCount = enemyArmy.GetCount();
        int playerCount = PlayerCommander.instance.GetArmyCount();

        int kill = Mathf.Min(enemyCount, playerCount);

        PlayerCommander.instance.playerArmy.RemovePiyons(kill);
        enemyArmy.RemovePiyons(kill);

        enemyCount = enemyArmy.GetCount();
        playerCount = PlayerCommander.instance.GetArmyCount();

        if (playerCount == 0 && enemyCount >= 2)
        {
            GameMode.Instance.LoseGame();
            return;
        }

        if (enemyCount == 0 && playerCount >= 2)
        {
            GameMode.Instance.WinGame();
            return;
        }
    }

    // -------------------------------------------------------
    // AI — DÜŞÜNME SİSTEMİ
    // -------------------------------------------------------
    void Think()
    {
        if (enemyArmy == null || villages == null || villages.Length == 0) return;

        List<BaseController> owned = GetOwnedVillages();

        if (owned.Count == 0)
        {
            isRetreating = false;
            PickNeutralAsTarget();
            return;
        }

        SmartRetreatCheck();
        GatherArmy();   // 🧠 Artık sadece KING kendi köy/kalede iken komut verecek
        TryAttack();
    }

    // -------------------------------------------------------
    // DÜŞMANIN SAHİP OLDUĞU KÖYLER
    // -------------------------------------------------------
    public List<BaseController> GetOwnedVillages()
    {

        List<BaseController> result = new List<BaseController>();
        foreach (var v in villages)
            if (v != null && v.owner == Team.Enemy && !v.isCastle)
                result.Add(v);
        return result;
    }

    // -------------------------------------------------------
    // NÖTR KÖY BUL
    // -------------------------------------------------------
    void PickNeutralAsTarget()
    {
        foreach (var v in villages)
        {
            if (v != null && v.owner == Team.Neutral)
            {
                currentTargetVillage = v;
                return;
            }
        }
    }

    // -------------------------------------------------------
    // GERİ ÇEKİLME
    // -------------------------------------------------------
    void SmartRetreatCheck()
    {
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

    // -------------------------------------------------------
    // KING SADECE KENDİ KÖY/KALEDEYKEN KOMUT VEREBİLİR
    // -------------------------------------------------------
    void GatherArmy()
    {
        BaseController baseHere = GetCurrentEnemyBase();
        if (baseHere == null) return;
        if (baseHere.owner != Team.Enemy) return;

        BasePiyonManager bpm = baseHere.GetComponent<BasePiyonManager>();
        if (bpm == null) return;

        // KALEDEYSE
        if (baseHere.isCastle)
        {
            // ✔ Komut: Kaledeki piyonları ordusuna dahil et
            if (baseHere.unitCount > 0 || bpm.GetPiyonCount() > 0)
            {
                bpm.TransferAllToEnemy(enemyKing); // piyonlar EnemyKing'e koşup orduya katılır
            }
            return;
        }

        // KÖYDEYSE
        int unitCount = baseHere.unitCount;
        int visualCount = bpm.GetPiyonCount();

        if (unitCount <= 0 && visualCount <= 0)
            return;

        // 1) ÖNCE: Köy piyonlarını kaleye yollama ihtimali
        bool canSendToCastle = enemyCastle != null &&
                               enemyCastle.owner == Team.Enemy &&
                               unitCount >= 5;

        if (canSendToCastle)
        {
            // ✔ Komut: Bu köydeki piyonları kendi kalesine gönder
            bpm.SendAllToCastle(enemyCastle);
            return;
        }

        // 2) SONRA: Köy piyonlarını başka bir düşman köyüne yollama
        BaseController otherVillage = FindAnotherEnemyVillage(baseHere);
        bool canSendToVillage = otherVillage != null && unitCount >= 3;

        if (canSendToVillage)
        {
            // ✔ Komut: Bu köydeki piyonları başka düşman köyüne gönder
            bpm.SendAllToCastle(otherVillage);
            return;
        }

        // 3) SON OLARAK: Köy piyonlarını kendi ordusuna kat
        // ✔ Komut: Bu köydeki piyonları orduya dahil et
        bpm.TransferAllToEnemy(enemyKing);
    }

    // Mevcut köyden farklı bir düşman köyü bul
    BaseController FindAnotherEnemyVillage(BaseController current)
    {
        foreach (var v in villages)
        {
            if (v == null) continue;
            if (v == current) continue;
            if (v.owner != Team.Enemy) continue;
            if (v.isCastle) continue;

            return v;
        }
        return null;
    }

    // Şu anda KING hangi kendi base'inin üstünde?
    BaseController GetCurrentEnemyBase()
    {
        // KÖYLER
        foreach (var v in villages)
        {
            if (v == null) continue;
            if (v.owner != Team.Enemy) continue;
            if (EnemyIsAt(v))
                return v;
        }

        // KALE
        if (enemyCastle != null && enemyCastle.owner == Team.Enemy && EnemyIsAt(enemyCastle))
            return enemyCastle;

        return null;
    }

    // -------------------------------------------------------
    // SALDIRI
    // -------------------------------------------------------
    void TryAttack()
    {
        BaseController target = FindAttackablePlayerVillage();

        if (target != null)
        {
            SendArmyTo(target);
            return;
        }

        if (enemyArmy.GetCount() < attackThreshold)
            return;

        if (playerCastle != null && CanConquer(playerCastle))
            SendArmyTo(playerCastle);
    }

    BaseController FindAttackablePlayerVillage()
    {
        BaseController best = null;
        float bestScore = Mathf.Infinity;

        foreach (var v in villages)
        {
            if (v == null || v.owner != Team.Player) continue;
            if (!CanConquer(v)) continue;

            float dist = Vector2.Distance(enemyKing.position, v.transform.position);
            if (dist < bestScore)
            {
                bestScore = dist;
                best = v;
            }
        }

        return best;
    }

    bool CanConquer(BaseController baseC)
    {
        BasePiyonManager bpm = baseC.GetComponent<BasePiyonManager>();
        int defenders = bpm ? bpm.GetPiyonCount() : baseC.unitCount;

        return enemyArmy.GetCount() > defenders;
    }

    public void SendArmyTo(BaseController target)
    {
        if (target == null) return;

        int attackerCount = enemyArmy.GetCount();
        target.ResolveBattle(attackerCount, Team.Enemy);
        enemyArmy.ExtractAll();
    }

    // -------------------------------------------------------
    // KRAL KÖY/KALE ÜSTÜNDE Mİ?
    // -------------------------------------------------------
    bool EnemyIsAt(BaseController village)
    {
        if (village == null || enemyKing == null)
            return false;

        float dist = Vector2.Distance(enemyKing.position, village.transform.position);
        return dist < 0.8f;
    }

    // -------------------------------------------------------
    // HEDEF KÖYE DOĞRU HAREKET
    // -------------------------------------------------------
    void MoveToTargetVillage()
    {
        if (!canStep) return;
        if (currentTargetVillage == null) return;

        Vector2 diff = currentTargetVillage.transform.position - enemyKing.position;

        if (diff.magnitude < 0.35f)
        {
            OnReachVillage(currentTargetVillage);
            currentTargetVillage = null;
            return;
        }

        Vector2 dir = NormalizeDirection(diff);
        StartCoroutine(MoveOneStep(dir));
    }

    void OnReachVillage(BaseController v)
    {
        if (v.owner == Team.Neutral)
        {
            v.owner = Team.Enemy;
            return;
        }

        if (v.owner == Team.Player)
        {
            SendArmyTo(v);
        }
    }
}
