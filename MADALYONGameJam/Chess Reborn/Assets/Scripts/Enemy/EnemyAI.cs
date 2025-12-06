using UnityEngine;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    private EnemyCommanderCore core;

    public int attackThreshold = 10;
    public int retreatThreshold = 3;
    public float safeRadiusFromPlayer = 6f;
    public int weakVillageUnitThreshold = 5;

    public BaseController currentTargetVillage;
    private bool isRetreating = false;

    public void Setup(EnemyCommanderCore c)
    {
        core = c;
    }

    public void Think()
    {
        if (core.enemyArmy == null) return;

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

    public List<BaseController> GetOwnedVillages()
    {
        List<BaseController> list = new List<BaseController>();

        foreach (var v in core.villages)
            if (v != null && v.owner == Team.Enemy && !v.isCastle)
                list.Add(v);

        return list;
    }

    public void OnReachVillage(BaseController v)
    {
        if (v.owner == Team.Neutral)
            v.owner = Team.Enemy;
        else if (v.owner == Team.Player)
            EnemyAttack(v);
    }

    void EnemyAttack(BaseController v)
    {
        float dist = Vector2.Distance(core.enemyKing.position, v.transform.position);
        if (dist > 0.8f) return;

        int attackerCount = core.enemyArmy.GetCount();

        v.ResolveBattle(attackerCount, Team.Enemy);
        core.enemyArmy.ExtractAll();
    }

    void SmartRetreatCheck()
    {
        int count = core.enemyArmy.GetCount();

        if (count <= retreatThreshold)
        {
            isRetreating = true;
            currentTargetVillage = core.enemyCastle;
        }
        else if (count >= attackThreshold)
        {
            isRetreating = false;
        }
    }

    void TryCaptureNeutralVillage()
    {
        foreach (var v in core.villages)
        {
            if (v != null && v.owner == Team.Neutral)
            {
                currentTargetVillage = v;
                return;
            }
        }
    }

    void GatherArmy()
    {
        foreach (var v in core.villages)
        {
            if (v != null && v.owner == Team.Enemy && EnemyIsAt(v))
                AddVillageUnits(v);
        }

        if (core.enemyCastle != null && EnemyIsAt(core.enemyCastle))
            AddVillageUnits(core.enemyCastle);
    }

    void AddVillageUnits(BaseController v)
    {
        BasePiyonManager bpm = v.GetComponent<BasePiyonManager>();
        if (bpm == null) return;

        bpm.TransferAllToEnemy(core.enemyKing);
    }

    bool EnemyIsAt(BaseController v)
    {
        return Vector2.Distance(core.enemyKing.position, v.transform.position) < 0.5f;
    }

    // ----------------
    // SALDIRI KARARLARI
    // ----------------

    void TryAttack()
    {
        BaseController target = FindTargetPlayerVillage();

        if (target != null)
        {
            currentTargetVillage = target;
            return;
        }

        if (core.enemyArmy.GetCount() < attackThreshold)
            return;

        if (core.playerCastle != null && CanConquer(core.playerCastle))
            SendArmyTo(core.playerCastle);
    }

    BaseController FindTargetPlayerVillage()
    {
        BaseController best = null;
        float bestScore = Mathf.Infinity;

        foreach (var v in core.villages)
        {
            if (v.owner != Team.Player)
                continue;

            if (!CanConquer(v))
                continue;

            float distEnemy = Vector2.Distance(core.enemyKing.position, v.transform.position);
            float distPlayer = Vector2.Distance(core.playerKing.position, v.transform.position);

            float score = distEnemy + Mathf.Max(0, safeRadiusFromPlayer - distPlayer) * 3f;

            if (score < bestScore)
            {
                bestScore = score;
                best = v;
            }
        }

        return best;
    }
    

    public bool CanConquer(BaseController v)
    {
        BasePiyonManager bpm = v.GetComponent<BasePiyonManager>();
        int villageUnits = bpm != null ? bpm.GetPiyonCount() : v.unitCount;

        int myArmy = core.enemyArmy.GetCount();

        return myArmy > villageUnits;
    }

    public void SendArmyTo(BaseController target)
    {
        if (core.enemyArmy.GetCount() <= 0) return;

        float dist = Vector2.Distance(core.enemyKing.position, target.transform.position);
        if (dist > 0.8f) return;

        GameObject[] units = core.enemyArmy.ExtractAll();

        foreach (var p in units)
        {
            if (p == null) continue;
            p.GetComponent<Piyon>().AttackBase(target, Team.Enemy);
        }
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

float EvaluateVillageThreat(BaseController v)
{
    if (v == null) return 0f;

    float threat = 0f;

    float distPlayer = Vector2.Distance(core.playerKing.position, v.transform.position);
    int playerArmyCount = PlayerCommander.instance.GetArmyCount();

    if (v.owner == Team.Enemy)
    {
        threat += Mathf.Max(0, core.enemyArmy.GetCount() - v.unitCount);
        if (distPlayer < 6f)
            threat += 6f - distPlayer;

        threat += playerArmyCount * 0.2f;
    }

    return threat;
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
    BasePiyonManager bpm = from.GetComponent<BasePiyonManager>();
    if (bpm == null) return;

    bpm.SendAllToCastle(to);
}

}
