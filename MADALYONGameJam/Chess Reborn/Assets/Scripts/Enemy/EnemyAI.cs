using UnityEngine;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    private EnemyCommanderCore core;

    public float playerAvoidRange = 3f;
    public int minAttackArmy = 3;
    public int rushArmy = 7;

    public BaseController currentTargetVillage;

    public void Setup(EnemyCommanderCore c)
    {
        core = c;
    }

    public void Think()
    {
        if (core == null || core.enemyArmy == null)
            return;

        int armyCount = core.enemyArmy.GetCount();

        // Eğer tüm köyler düşmana aitse → saldırgan moda geç
        if (AllVillagesOwnedByEnemy())
        {
            // küçük ordu → player king'e yaklaş
            if (armyCount < 4)
            {
                currentTargetVillage = FindNextTarget();

                return;
            }

            // güçlü ordu → player castle'a saldır
            currentTargetVillage = core.playerCastle;
            return;
        }

        // PLAYER köylerine saldır
        if (TryAttackPlayerVillage()) return;

        // Tarafsız köyleri ele geçir
        if (TryCaptureNearestNeutral()) return;

        // Son çare → player king'i takip et
        currentTargetVillage = FindClosestTo(core.playerKing.position);
    }

    // Castle rush
    bool TryRushPlayerCastle()
    {
        if (core.enemyArmy.GetCount() < rushArmy)
            return false;

        BaseController castle = core.playerCastle;
        if (castle == null)
            return false;

        if (castle.unitCount < core.enemyArmy.GetCount())
        {
            currentTargetVillage = castle;
            return true;
        }

        return false;
    }

    // Tarafsız köyleri ele geçir
    bool TryCaptureNearestNeutral()
    {
        BaseController best = null;
        float bestDist = Mathf.Infinity;

        foreach (var v in core.villages)
        {
            if (v.owner != Team.Neutral)
                continue;

            float d = Vector2.Distance(core.enemyKing.position, v.transform.position);

            if (d < bestDist)
            {
                bestDist = d;
                best = v;
            }
        }

        if (best != null)
        {
            currentTargetVillage = best;
            return true;
        }

        return false;
    }

    // Player köylerine saldır
    bool TryAttackPlayerVillage()
    {
        BaseController best = null;
        float bestScore = Mathf.Infinity;

        foreach (var v in core.villages)
        {
            if (v.owner != Team.Player)
                continue;

            // Bu köy kazanılamaz → saldırma
            if (core.enemyArmy.GetCount() <= v.unitCount)
                continue;

            float score = Vector2.Distance(core.enemyKing.position, v.transform.position);

            if (score < bestScore)
            {
                bestScore = score;
                best = v;
            }
        }

        if (best != null)
        {
            currentTargetVillage = best;
            return true;
        }

        return false;
    }

    // Kendi köyleri içinde en yakınına git
    void MoveToClosestOwnedVillage()
    {
        BaseController best = null;
        float bestDist = Mathf.Infinity;

        foreach (var v in core.villages)
        {
            if (v.owner != Team.Enemy)
                continue;

            float d = Vector2.Distance(core.enemyKing.position, v.transform.position);

            if (d < bestDist)
            {
                bestDist = d;
                best = v;
            }
        }

        currentTargetVillage = best;
    }

    // ------------------------------------------------------------
    // DÜŞMAN KÖYE ULAŞTIĞINDA NE OLACAK?
    // ------------------------------------------------------------
    public void OnReachVillage(BaseController v)
    {
        if (v == null) return;

        BasePiyonManager bpm = v.GetComponent<BasePiyonManager>();

        // Tarafsız köy
        if (v.owner == Team.Neutral)
        {
            v.owner = Team.Enemy;
            return;
        }

        // Player köyü -> savaş
        if (v.owner == Team.Player)
        {
            int attacker = core.enemyArmy.GetCount();
            v.ResolveBattle(attacker, Team.Enemy);
            core.enemyArmy.ExtractAll();
            return;
        }

        // Kendi köyündeyse → asker topla (SADECE BURASI)
        if (v.owner == Team.Enemy && bpm != null)
        {
            bpm.TransferAllToEnemy(core.enemyKing);
        }
    }

    // Tüm köyler düşmana ait mi?
    bool AllVillagesOwnedByEnemy()
    {
        foreach (var v in core.villages)
            if (v.owner != Team.Enemy)
                return false;

        return true;
    }

    // Pozisyona göre en yakın köy
    BaseController FindClosestTo(Vector3 pos)
    {
        BaseController best = null;
        float bestDist = Mathf.Infinity;

        foreach (var v in core.villages)
        {
            float d = Vector2.Distance(v.transform.position, pos);
            if (d < bestDist)
            {
                bestDist = d;
                best = v;
            }
        }

        return best;
    }
    public BaseController FindNextTarget()
{
    BaseController best = null;
    float bestDist = Mathf.Infinity;

    foreach (var v in core.villages)
    {
        float dist = Vector2.Distance(v.transform.position, core.enemyKing.position);

        // bulunduğu köyü atla
        if (dist < 0.5f)
            continue;

        if (dist < bestDist)
        {
            bestDist = dist;
            best = v;
        }
    }

    // Eğer başka köy yoksa → Player Castle'a yönel
    if (best == null)
        return core.playerCastle;

    return best;
}


}
