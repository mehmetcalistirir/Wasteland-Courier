using UnityEngine;
using System.Collections.Generic;

public class EnemyCommander : MonoBehaviour
{
    public static EnemyCommander instance;

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

        // Tarafsız köy → ele geçir
        if (village.owner == Team.Neutral)
        {
            village.owner = Team.Enemy;
        }

        // Kendi köyü veya kalesi → piyonları ordusuna ekle
        if (village.owner == Team.Enemy)
        {
            EnemyAddVillagePiyonsToArmy(village);
        }

        // Oyuncu köyü → ordu ile saldır
        if (village.owner == Team.Player)
        {
            SendArmyTo(village);
        }
    }

    // -----------------------------------------------------
    // 6) KÖYLERDEN ORDUYA ASKER TOPLAMA
    // -----------------------------------------------------
    void GatherArmy()
    {
        foreach (var v in villages)
        {
            if (EnemyIsAt(v) && v.owner == Team.Enemy)
            {
                EnemyAddVillagePiyonsToArmy(v);
            }
        }

        if (EnemyIsAt(enemyCastle))
        {
            EnemyAddVillagePiyonsToArmy(enemyCastle);
        }
    }


    // Bulunduğu köydeki piyonları ordusuna ekle
    public void EnemyAddVillagePiyonsToArmy(BaseController village)
    {
        if (!EnemyIsAt(village)) return;  // Uzaksa komut yasak!

        BasePiyonManager bpm = village.GetComponent<BasePiyonManager>();
        if (bpm == null) return;

        bpm.TransferAllToEnemy(enemyKing);
    }


    // Bulunduğu köydeki piyonları kendi kalesine gönder (istersen kullanırsın)
    public void EnemySendVillagePiyonsToCastle(BaseController village)
    {
        if (!EnemyIsAt(village)) return; // King köyde değil → gönderemez

        BasePiyonManager bpm = village.GetComponent<BasePiyonManager>();
        if (bpm == null) return;

        bpm.SendAllToCastle(enemyCastle);
    }


    // Bulunduğu köyden başka bir köye piyon gönder
    public void EnemySendVillagePiyonsTo(BaseController from, BaseController to)
    {
        if (!EnemyIsAt(from)) return; // Uzaksa izin yok

        BasePiyonManager bpm = from.GetComponent<BasePiyonManager>();
        if (bpm == null) return;

        if (!EnemyIsAt(from)) return;
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

        BasePiyonManager bpm = from.GetComponent<BasePiyonManager>();
        if (bpm == null) return;

        bpm.SendAllToCastle(to); // savunma piyonlarını gönder
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

        GameObject[] arr = enemyArmy.ExtractAll(); // Ordunun tamamını al

        foreach (GameObject go in arr)
        {
            if (go == null) continue;

            Piyon p = go.GetComponent<Piyon>();
            if (p != null)
            {
                // SALDIRI MODU → Team.Enemy ile
                p.AttackBase(target, Team.Enemy);
            }
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

        // Saldırıya katılan düşman piyonlarını yok et
        enemyArmy.ExtractAll();
    }

}
