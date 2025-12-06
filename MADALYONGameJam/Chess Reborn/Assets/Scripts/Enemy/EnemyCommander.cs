using UnityEngine;
using System.Collections.Generic;
using TMPro;


public class EnemyCommander : MonoBehaviour
{
    public static EnemyCommander instance;
    public TextMeshPro kingCountText;

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    private BaseController currentTargetVillage;

    [Header("Enemy Army")]
    public EnemyArmy enemyArmy;      // Düşman ordusunu yöneten script
    public Transform enemyKing;      // Düşman kral objesi (ordunun merkez noktası)
    public GameObject piyonPrefab;   // Gerekirse spawn için

    [Header("References")]
    public BaseController[] villages;   // Sahnedeki TÜM köyler (köy + kaleler)
    public BaseController enemyCastle;  // Düşmanın kendi kalesi
    public BaseController playerCastle; // Oyuncunun kalesi
    public Transform playerKing;        // Oyuncu kral objesi

    [Header("AI Settings")]
    public int attackThreshold = 10;      // Ordu en az bu sayıya ulaşınca saldır
    public int retreatThreshold = 3;      // Bundan azsa geri çekil
    public float safeRadiusFromPlayer = 6f; // Köy - oyuncu mesafesi güvenli çember
    public int weakVillageUnitThreshold = 5; // Zayıf köy eşiği

    private bool isRetreating = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Her saniye bir kez “düşün”
        InvokeRepeating(nameof(Think), 1f, 1f);
    }

    void Update()
    {
        // Sürekli, akıcı hareket
        MoveToTargetVillage();
        if (kingCountText != null)
            kingCountText.text = enemyArmy.GetCount().ToString();
            MoveToTargetVillage();
        CheckFightWithPlayer();
    }
    void CheckFightWithPlayer()
{
    if (playerKing == null) return;

    float dist = Vector2.Distance(enemyKing.position, playerKing.position);

    // 👇 Savaş mesafesi
    if (dist < 1.0f)
    {
        StartKingBattle();
    }
}

void StartKingBattle()
{
    int enemyCount = enemyArmy.GetCount();
    int playerCount = PlayerCommander.instance.GetArmyCount();

    // Aynı köy mantığı:
    int kill = Mathf.Min(enemyCount, playerCount);

    int enemyRemaining = enemyCount - kill;
    int playerRemaining = playerCount - kill;

    // --- PLAYER KAYIPLARI ---
    PlayerCommander.instance.playerArmy.ExtractAll(); // tüm piyonlar silinsin

    // --- ENEMY KAYIPLARI ---
    enemyArmy.RemovePiyons(kill);

    // Eğer ENEMY kazandıysa kalanları koru
    // RemovePiyons zaten gerekeni sildiğinden ekstra işlem yok.

    // Eğer PLAYER kazanırsa enemyRemaining = 0 zaten
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

        // Hiç köyü yoksa → tarafsız köy kovala
        if (owned.Count == 0)
        {
            isRetreating = false;
            TryCaptureNeutralVillage();
            return;
        }

        // Ordunun durumuna göre geri çekilme kararı
        SmartRetreatCheck();

        // Tehdit altındaki köyleri takviye et
        DefendThreatenedVillages(owned);

        // Kendi köy ve kalesinden ordu toparla
        GatherArmy();

        // Saldırı denemesi
        TryAttack();
    }

    // -----------------------------------------------------
    // 3) HAREKET
    // -----------------------------------------------------
    void MoveToTargetVillage()
    {
        if (enemyKing == null || villages == null || villages.Length == 0)
            return;

        // Hedef yoksa yeni hedef belirle
        if (currentTargetVillage == null)
        {
            if (isRetreating)
            {
                // Geri çekiliyorsak öncelik kale
                currentTargetVillage = enemyCastle;
            }
            else
            {
                // Önce akıllı nötr köy
                currentTargetVillage = PickSmartNeutralVillage();

                // Akıllı nötr köy yoksa, herhangi bir nötr köy
                if (currentTargetVillage == null)
                    currentTargetVillage = PickAnyNeutralVillage();

                // O da yoksa, sahip olunan köylerden birine devriye
                if (currentTargetVillage == null)
                    currentTargetVillage = PickOwnedVillage();
            }
        }

        if (currentTargetVillage == null)
            return;

        // Hedefe doğru yürü
        enemyKing.position = Vector3.MoveTowards(
            enemyKing.position,
            currentTargetVillage.transform.position,
            moveSpeed * Time.deltaTime
        );

        float dist = Vector3.Distance(enemyKing.position, currentTargetVillage.transform.position);

        // Hedefe ulaştı
        if (dist < 0.5f)
        {
            OnReachVillage(currentTargetVillage);

            // Yeniden hedef seç
            currentTargetVillage = null;
        }
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

            // Oyuncu bu köye bariz şekilde daha yakınsa → gitmeye değmez
            if (distPlayer + 1f < distEnemy)
                continue;

            // Köyün önemini üretim hızına göre arttır (daha hızlı üreten daha değerli)
            BaseController bc = v.GetComponent<BaseController>();
            float valueFactor = bc != null ? (1f / Mathf.Max(0.1f, bc.productionRate)) : 1f;

            // Basit skor: mesafe + değer faktörü
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
    // 5) KÖYE ULAŞINCA NE YAPACAK?
    // -----------------------------------------------------
    void OnReachVillage(BaseController village)
{
    if (village == null) return;

    // Tarafsız köy → tek seferlik ele geçir
    if (village.owner == Team.Neutral)
    {
        village.owner = Team.Enemy;
        return;
    }

    // Kendi köyüne geldiyse → piyon toplama YASAK (bug engellendi)
    if (village.owner == Team.Enemy)
    {
        // Artık hiçbir şey yapılmıyor
        return;
    }

    // Player köyü → Saldırı başlat
    if (village.owner == Team.Player)
    {
        EnemyAttack(village);
        return;
    }
}


    // -----------------------------------------------------
    // 6) KÖYLERDEN ORDUYA ASKER TOPLAMA
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



    // Bulunduğu köydeki piyonları ordusuna ekle
    public void EnemyAddVillagePiyonsToArmy(BaseController village)
{
    if (village == null) return;
    if (village.unitCount <= 0) return;

    // ❗ Köy düşmana ait değilse çık
    if (village.owner != Team.Enemy) return;

    // ❗ King bu köyün üstünde değilse çık
    if (!EnemyIsAt(village)) return;

    BasePiyonManager bpm = village.GetComponent<BasePiyonManager>();
    if (bpm == null) return;

    bpm.TransferAllToEnemy(enemyKing);
}





    // Bulunduğu köydeki piyonları kendi kalesine gönder (istersen kullanırsın)
    public void EnemySendVillagePiyonsToCastle(BaseController village)
{
    if (village == null || enemyCastle == null) return;

    // ❗ Düşman köyü değilse yok
    if (village.owner != Team.Enemy) return;

    // ❗ King o köyde olmalı
    if (!EnemyIsAt(village)) return;

    BasePiyonManager bpm = village.GetComponent<BasePiyonManager>();
    if (bpm == null) return;

    bpm.SendAllToCastle(enemyCastle);
}




    // Bulunduğu köyden başka bir köye piyon gönder
    public void EnemySendVillagePiyonsTo(BaseController from, BaseController to)
{
    if (from == null || to == null) return;

    // ❗ Düşmana ait olmalı
    if (from.owner != Team.Enemy) return;

    // ❗ King 'from' köyünde olmalı
    if (!EnemyIsAt(from)) return;

    BasePiyonManager bpm = from.GetComponent<BasePiyonManager>();
    if (bpm == null) return;

    bpm.SendAllToCastle(to);
}





    // -----------------------------------------------------
    // 7) TARAFSIZ KÖY ELE GEÇİRMEYE ÇALIŞMA
    // -----------------------------------------------------
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
    // 8) TEHDİT ANALİZİ ve TAKVİYE
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

        // Düşmanın köyüyse:
        if (v.owner == Team.Enemy)
        {
            // Az asker varsa daha tehlikeli
            threat += Mathf.Max(0, weakVillageUnitThreshold - v.unitCount) * 2f;

            // Oyuncu çok yakınsa
            if (distPlayer < safeRadiusFromPlayer)
                threat += (safeRadiusFromPlayer - distPlayer) * 2f;

            // Player ordusu büyüdükçe genel tehdit artsın
            threat += playerArmyCount * 0.2f;
        }

        // Eğer köy Player'a aitse, düşman açısından saldırı hedefi olabilir (ama savunma tehditi değil)
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

        // Tehdit belirli seviyenin üzerindeyse takviye gönder
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

    // -----------------------------------------------------
    // 9) SAVUNMA BİRLİĞİ GÖNDER (KÖYDEN KÖYE)
    // -----------------------------------------------------
    void SendDefense(BaseController from, BaseController to)
{
    if (from == null || to == null) return;

    // ❗ King 'from' köyünde değilse savunma gönderemez
    if (!EnemyIsAt(from)) return;

    BasePiyonManager bpm = from.GetComponent<BasePiyonManager>();
    if (bpm == null) return;

    bpm.SendAllToCastle(to);
}


    // -----------------------------------------------------
    // 10) GERİ ÇEKİLME MANTIĞI
    // -----------------------------------------------------
    void SmartRetreatCheck()
    {
        if (enemyArmy == null) return;

        int count = enemyArmy.GetCount();

        // Ordu çok zayıfladıysa → geri çekil
        if (count <= retreatThreshold)
        {
            isRetreating = true;
            currentTargetVillage = enemyCastle;
        }
        else if (count >= attackThreshold)
        {
            // Yeterince güçlüysek saldırı moduna çık
            isRetreating = false;
        }
    }

    // -----------------------------------------------------
    // 11) TAKTİKSEL SALDIRI / HEDEF SEÇME
    // -----------------------------------------------------
    bool CanConquer(BaseController targetVillage)
    {
        if (targetVillage == null || enemyArmy == null)
            return false;

        BasePiyonManager bpm = targetVillage.GetComponent<BasePiyonManager>();
        int villagePiyon = bpm != null ? bpm.GetPiyonCount() : targetVillage.unitCount;

        int myArmy = enemyArmy.GetCount();

        // Ordum köydeki piyonlardan fazlaysa saldırmaya değer
        return myArmy > villagePiyon;
    }

    void TryAttack()
    {
        if (enemyArmy == null) return;

        // Önce ele geçirebileceği Player köyü ara
        BaseController target = FindTargetPlayerVillage();

        if (target != null)
        {
            SendArmyTo(target);
            return;
        }

        // Hiç uygun Player köyü yoksa ve ordu zayıfsa → saldırma
        if (enemyArmy.GetCount() < attackThreshold)
            return;

        // Son çare: Player kalesine saldır
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

            // Köyü ele geçirecek gücü var mı?
            if (!CanConquer(v))
                continue;

            float distEnemy = Vector2.Distance(enemyKing.position, v.transform.position);
            float distPlayer = playerKing != null
                ? Vector2.Distance(playerKing.position, v.transform.position)
                : Mathf.Infinity;

            // Player çok yakınsa, saldırı riskli olabilir → skora ekle
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
    // 12) ORDUNUN TAMAMINI HEDEFE SALDIRIYA GÖNDER
    // -----------------------------------------------------
    public void SendArmyTo(BaseController target)
{
    if (target == null || enemyArmy == null) return;

    int attackerCount = enemyArmy.GetCount();

    // PlayerCommander ile aynı savaş sistemi
    target.ResolveBattle(attackerCount, Team.Enemy);

    // Saldırıya katılan piyonlar yok edilir
    enemyArmy.ExtractAll();
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



}
