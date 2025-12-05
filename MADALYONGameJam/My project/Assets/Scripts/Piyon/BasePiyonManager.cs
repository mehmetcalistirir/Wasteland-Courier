using UnityEngine;
using System.Collections.Generic;

public class BasePiyonManager : MonoBehaviour
{
    [Header("References")]
    public BaseController baseController;
    public GameObject piyonPrefab;
    public Transform oyuncu;   // Oyuncuya piyon gönderirken kullanılacak

    [Header("Wander Settings")]
    public float wanderRadius = 2f;
    public float wanderSpeed = 1.5f;
    public int maxVisualPiyon = 30;

    private List<Piyon> piyonlar = new List<Piyon>();

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

    // ------------------------------------------
    // Wander eden asker üret
    // ------------------------------------------
    void SpawnWanderPiyon()
    {
        Vector2 rand = Random.insideUnitCircle * wanderRadius;
        Vector3 pos = transform.position + new Vector3(rand.x, rand.y, 0);

        GameObject obj = Instantiate(piyonPrefab, pos, Quaternion.identity);
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
    // (Progress bar yok — komutla çalışacak)
    // ------------------------------------------
    public void SendOneToPlayer()
    {
        if (baseController.unitCount <= 0)
            return;

        baseController.unitCount--;

        // Önce wander piyon varsa onu gönder
        if (piyonlar.Count > 0)
        {
            Piyon p = piyonlar[0];
            piyonlar.RemoveAt(0);
            p.OyuncuyaKatıl(oyuncu);
        }
        else
        {
            // Yedek: wander piyon yoksa yeni üret
            GameObject obj = Instantiate(piyonPrefab, transform.position, Quaternion.identity);
            Piyon p = obj.GetComponent<Piyon>();
            p.OyuncuyaKatıl(oyuncu);
        }
    }
    public void TransferAllToPlayer(Transform player)
{
    foreach (var p in piyonlar)
    {
        if (p != null)
            p.OyuncuyaKatıl(player);
    }

    piyonlar.Clear();
}
public void SendAllToCastle(BaseController castle)
{
    if (castle == null) return;

    // Köyün sahip olduğu toplam sayısal piyon
    int count = baseController.unitCount;
    if (count <= 0) return;

    baseController.unitCount = 0;

    // Görsel wander piyonları tek tek gönder
    foreach (var p in piyonlar)
    {
        if (p != null)
            p.AttackBase(castle, baseController.owner);
    }

    piyonlar.Clear();
}


}
