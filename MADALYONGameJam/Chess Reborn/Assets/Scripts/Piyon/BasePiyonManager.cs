using UnityEngine;
using System.Collections.Generic;

public class BasePiyonManager : MonoBehaviour
{
    [Header("References")]
    public BaseController baseController;

    [Header("Piyon Prefabs")]
    public GameObject playerPiyonPrefab;
    public GameObject enemyPiyonPrefab;

    public Transform oyuncu;   // Oyuncuya piyon gönderirken kullanılacak

    [Header("Wander Settings")]
    public float wanderRadius = 2f;
    public float wanderSpeed = 1.5f;
    public int maxVisualPiyon = 30;

    public List<Piyon> piyonlar = new List<Piyon>();

    void Update()
    {
        if (baseController == null)
            return;

        // Tarafsız köy/kale piyon göstermez
        if (baseController.owner == Team.Neutral)
        {
            ClearVisualPiyons();
            return;
        }

        SyncVisualPiyons();
    }

    // ------------------------------------------
    // Görsel piyonlar unitCount ile senkronize
    // ------------------------------------------
    void SyncVisualPiyons()
    {
        int targetCount = Mathf.Min(baseController.unitCount, maxVisualPiyon);

        // Eksik varsa spawn et
        while (piyonlar.Count < targetCount)
            SpawnWanderPiyon();

        // Fazlaysa sil
        while (piyonlar.Count > targetCount)
        {
            Destroy(piyonlar[0].gameObject);
            piyonlar.RemoveAt(0);
        }
    }

    GameObject GetCorrectPrefab()
    {
        switch (baseController.owner)
        {
            case Team.Player:
                return playerPiyonPrefab;
            case Team.Enemy:
                return enemyPiyonPrefab;
            default:
                return null; // Neutral bölge piyon üretmez
        }
    }

    // ------------------------------------------
    // Wander eden asker üret
    // ------------------------------------------
    void SpawnWanderPiyon()
    {
        GameObject prefab = GetCorrectPrefab();
        if (prefab == null) return;

        Vector2 rand = Random.insideUnitCircle * wanderRadius;
        Vector3 pos = transform.position + new Vector3(rand.x, rand.y, 0);

        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
        Piyon p = obj.GetComponent<Piyon>();

        p.SetVillageCenter(transform);
        p.SetWanderSpeed(wanderSpeed);

        piyonlar.Add(p);
    }

    // ------------------------------------------
    // Tüm piyonları temizle
    // ------------------------------------------
    void ClearVisualPiyons()
    {
        foreach (var p in piyonlar)
            if (p != null) Destroy(p.gameObject);

        piyonlar.Clear();
    }

    // ------------------------------------------
    // Oyuncuya bir asker gönder
    // ------------------------------------------
    public void SendOneToPlayer()
    {
        if (baseController.unitCount <= 0)
            return;

        baseController.unitCount--;

        // Eğer wander listesinde piyon varsa → onu gönder
        if (piyonlar.Count > 0)
        {
            Piyon p = piyonlar[0];
            piyonlar.RemoveAt(0);
            p.OyuncuyaKatıl(oyuncu);
            return;
        }

        // Eğer wander piyon kalmadıysa → owner’a uygun yeni piyon spawn et
        GameObject prefab = GetCorrectPrefab();
        if (prefab == null)
        {
            Debug.LogWarning("This base has no valid piyon prefab assigned for its team!");
            return;
        }

        GameObject obj = Instantiate(prefab, transform.position, Quaternion.identity);
        Piyon newP = obj.GetComponent<Piyon>();
        newP.OyuncuyaKatıl(oyuncu);
    }

    public void RemovePiyons(int amount)
    {
        amount = Mathf.Min(amount, piyonlar.Count);

        for (int i = 0; i < amount; i++)
        {
            if (piyonlar[0] != null)
                Destroy(piyonlar[0].gameObject);

            piyonlar.RemoveAt(0);
        }
    }

    public void SyncTo(int count)
    {
        while (piyonlar.Count > count)
        {
            Destroy(piyonlar[0].gameObject);
            piyonlar.RemoveAt(0);
        }

        while (piyonlar.Count < count)
            AddFakePiyon();
    }

    public void AddFakePiyon()
    {
        GameObject prefab = GetCorrectPrefab();
        if (prefab == null) return;

        GameObject obj = Instantiate(prefab, transform.position, Quaternion.identity);
        Piyon p = obj.GetComponent<Piyon>();

        p.SetVillageCenter(transform);
        piyonlar.Add(p);
    }

    // ------------------------------------------
    // TÜM PİYONLARI OYUNCU ORDUSUNA AKTAR
    // ------------------------------------------
    public void TransferAllToPlayer(Transform player)
    {
        PlayerPiyon army = PlayerCommander.instance.playerArmy;
        if (army == null) return;

        int count = baseController.unitCount;

        army.PiyonEkle(count);

        baseController.unitCount = 0;

        foreach (var p in piyonlar)
            if (p != null) Destroy(p.gameObject);

        piyonlar.Clear();
    }

    public int GetPiyonCount()
    {
        return piyonlar.Count;
    }

    // ------------------------------------------
    // TÜM PİYONLARI BELİRLİ BİR KALEYE GÖNDER (SAVUNMA)
    // ------------------------------------------
    public void SendAllToCastle(BaseController castle)
    {
        if (castle == null) return;

        int count = baseController.unitCount;
        if (count <= 0) return;

        baseController.unitCount = 0;

        foreach (var p in piyonlar)
        {
            if (p != null)
                p.GoDefendBase(castle);   // SAVUNMA
        }

        piyonlar.Clear();
    }

    // ------------------------------------------
    // TÜM PİYONLARI DÜŞMAN ORDUSUNA KAT
    // (Enemy King etrafındaki orduya)
    // ------------------------------------------
    public void TransferAllToEnemy(Transform enemyKing)
    {
        foreach (var p in piyonlar)
        {
            if (p != null)
                p.DusmanaKatıl(enemyKing);
        }

        piyonlar.Clear();
        baseController.unitCount = 0;
    }

    // ------------------------------------------
    // DÜŞMAN ORDUSUNA GÖNDER (KING ETRAFINA)
    // ------------------------------------------
    public void SendAllToEnemyArmy(Transform enemyKing)
    {
        if (baseController.unitCount <= 0) return;

        foreach (var p in piyonlar)
        {
            if (p != null)
                p.DusmanaKatıl(enemyKing);
        }

        piyonlar.Clear();
        baseController.unitCount = 0;
    }

    // ------------------------------------------
    // BAŞKA BİR ENEMY KÖYÜNE GÖNDER (DEFANS)
    // ------------------------------------------
    public void SendAllToEnemyVillage(BaseController targetVillage)
    {
        if (targetVillage == null) return;
        if (baseController.unitCount <= 0) return;

        foreach (var p in piyonlar)
        {
            if (p != null)
                p.GoDefendBase(targetVillage);
        }

        piyonlar.Clear();
        baseController.unitCount = 0;
    }
}
