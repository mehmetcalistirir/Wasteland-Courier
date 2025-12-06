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

    // -------------------------
    // 0) ASLA boşta kalmamalı!
    // Eğer tüm köyleri ele geçirmişse:
    // - Asker toplasın
    // - Player'a saldırıya geçsin
    // -------------------------

    if (AllVillagesOwnedByEnemy())
    {
        GatherArmy();

        // Eğer ordusu küçükse player king’e saldır
        if (armyCount < 4)
        {
            currentTargetVillage = FindClosestTo(core.playerKing.position);
            return;
        }

        // Ordu yeterliyse PLAYER CASTLE'a RUSH
        currentTargetVillage = core.playerCastle;
        return;
    }

    // -------------------------
    // 1) Player tehdit oluşturuyorsa → Asker topla
    // -------------------------
    if (PlayerIsNear())
        GatherArmy();

    // -------------------------
    // 2) Düşman köyü tehdit altında mı?
    // -------------------------
    BaseController threatened = FindThreatenedVillage();
    if (threatened != null)
    {
        ReinforceVillage(threatened);
        currentTargetVillage = threatened;
        return;
    }

    // -------------------------
    // 3) Büyük ordu → Castle Rush
    // -------------------------
    if (armyCount >= rushArmy)
    {
        PrepForCastleRush();
        currentTargetVillage = core.playerCastle;
        return;
    }

    // -------------------------
    // 4) Player köylerine saldır
    // -------------------------
    if (TryAttackPlayerVillage()) return;

    // -------------------------
    // 5) Tarafsız köy varsa → hemen ele geçir
    // -------------------------
    if (TryCaptureNearestNeutral()) return;

    // -------------------------
    // 6) Hâlâ hedef yoksa → Player King’i takip et
    // -------------------------
    currentTargetVillage = FindClosestTo(core.playerKing.position);
}



    // ----------- 1) PLAYER KALESİNE RUSH -----------

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

    // ----------- 2) Tarafsız köyleri hızlıca ele geçir -----------

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

    // ----------- 3) Player köylerine saldır -----------

    bool TryAttackPlayerVillage()
    {
        BaseController best = null;
        float bestScore = Mathf.Infinity;

        foreach (var v in core.villages)
        {
            if (v.owner != Team.Player)
                continue;

            float distPlayer = Vector2.Distance(core.playerKing.position, v.transform.position);
            if (distPlayer < playerAvoidRange)
                continue;

            if (core.enemyArmy.GetCount() <= v.unitCount)
                continue;

            float dist = Vector2.Distance(core.enemyKing.position, v.transform.position);

            if (dist < bestScore)
            {
                bestScore = dist;
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

    // ----------- 4) Sahip olduğu en yakın köye git -----------

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
    public void OnReachVillage(BaseController v)
{
    if (v == null) return;

    // 1) Tarafsız köy -> direkt ele geçir
    if (v.owner == Team.Neutral)
    {
        v.owner = Team.Enemy;
        v.unitCount = Mathf.Max(1, v.unitCount); 
        return;
    }

    // 2) PLAYER köyü -> saldır
    if (v.owner == Team.Player)
    {
        int attacker = core.enemyArmy.GetCount();
        v.ResolveBattle(attacker, Team.Enemy);

        // saldırıya katılan tüm piyonları ordudan düş
        core.enemyArmy.ExtractAll();
        return;
    }

    // 3) KENDİ köyümüz -> köydeki piyonları army'e kat
    if (v.owner == Team.Enemy)
    {
        BasePiyonManager bpm = v.GetComponent<BasePiyonManager>();
        if (bpm != null)
            bpm.TransferAllToEnemy(core.enemyKing);

        return;
    }
}

// Oyuncu çok yakın mı?
bool PlayerIsNear()
{
    float dist = Vector2.Distance(core.enemyKing.position, core.playerKing.position);
    return dist < 4f;
}

// Köylerdeki tüm piyonları orduya ekleme
void GatherArmy()
{
    foreach (var v in core.villages)
    {
        if (v.owner == Team.Enemy)
        {
            BasePiyonManager bpm = v.GetComponent<BasePiyonManager>();
            if (bpm != null)
                bpm.SendAllToEnemyArmy(core.enemyKing);
        }
    }
}

// Tehdit altındaki köyü bul
BaseController FindThreatenedVillage()
{
    foreach (var v in core.villages)
    {
        if (v.owner == Team.Enemy)
        {
            float playerDist = Vector2.Distance(core.playerKing.position, v.transform.position);

            if (playerDist < 3f)  // saldırı menzilinde
                return v;
        }
    }

    return null;
}

// Köyü savunmak için diğer köylerden piyon gönder
void ReinforceVillage(BaseController threatened)
{
    foreach (var v in core.villages)
    {
        if (v.owner == Team.Enemy && v != threatened)
        {
            BasePiyonManager bpm = v.GetComponent<BasePiyonManager>();
            if (bpm != null)
                bpm.SendAllToEnemyVillage(threatened);
        }
    }
}

// Castle rush öncesi bütün piyonları orduya çek
void PrepForCastleRush()
{
    foreach (var v in core.villages)
    {
        if (v.owner == Team.Enemy)
        {
            BasePiyonManager bpm = v.GetComponent<BasePiyonManager>();
            if (bpm != null)
                bpm.SendAllToEnemyArmy(core.enemyKing);
        }
    }
}

bool AllVillagesOwnedByEnemy()
{
    foreach (var v in core.villages)
        if (v.owner != Team.Enemy)
            return false;

    return true;
}

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


}
