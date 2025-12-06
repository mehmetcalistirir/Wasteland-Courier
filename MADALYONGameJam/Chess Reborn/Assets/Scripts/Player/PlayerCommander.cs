using UnityEngine;
using System.Collections.Generic;

public class PlayerCommander : MonoBehaviour
{
    public static PlayerCommander instance;

    [Header("References")]
    public PlayerPiyon playerArmy;
    public BaseController[] villages;  // Sahnedeki tüm köyler
    public BaseController playerCastle;
    public BaseController enemyCastle;

    private void Awake()
    {
        instance = this;
    }

    // -----------------------------------------------------------
    // OYUNCUNUN SAHİP OLDUĞU KÖYLERİ LİSTELE
    // -----------------------------------------------------------
    public List<BaseController> GetOwnedVillages()
    {
        List<BaseController> owned = new List<BaseController>();

        foreach (var v in villages)
        {
            if (v != null && v.owner == Team.Player && !v.isCastle)
                owned.Add(v);
        }

        return owned;
    }

    // -----------------------------------------------------------
    // KÖY/KALENİN TÜM PIYONLARINI BELİRLİ BİR KÖYE GÖNDER (SAVUNMA)
    // -----------------------------------------------------------
    public void SendVillagePiyonsTo(BaseController fromBase, BaseController target)
    {
        if (fromBase.owner != Team.Player)
            return; // Player düşman köyüne komut veremez

        if (fromBase == null || target == null) return;

        BasePiyonManager bpm = fromBase.GetComponent<BasePiyonManager>();
        if (bpm == null) return;

        // Piyonları hedef köyün savunmasına aktar
        bpm.SendAllToCastle(target);
    }

    // -----------------------------------------------------------
    // ORDUDAN SALDIRI GÖNDERME
    // -----------------------------------------------------------
    public int GetArmyCount()
    {
        if (playerArmy == null) return 0;
        return playerArmy.GetCount();
    }

    public void SendArmyTo(BaseController target)
{
    if (target == null || playerArmy == null) return;

    // 1) Oyuncu ordusunun gücünü al
    int attackerCount = playerArmy.GetCount();

    // 2) Savaşı başlat (OYUNCU tarafı)
    target.ResolveBattle(attackerCount, Team.Player);

    // 3) Saldırıya katılan tüm piyonları ordudan çıkar
    playerArmy.ExtractAll();
}


public void FormFightLine(Vector3 playerPos, Vector3 enemyPos)
{
    // İki kralın tam ortası
    Vector3 center = (playerPos + enemyPos) / 2f;

    // Oyuncuya ait piyonları bulmak için bir yarıçap
    float radius = 3f;

    // Sahnede tüm Piyon’ları bul
    Piyon[] allPions = FindObjectsOfType<Piyon>();
    List<Piyon> playerSidePions = new List<Piyon>();

    foreach (var p in allPions)
    {
        // Oyuncu kralına yakın olanları oyuncu hattı sayıyoruz
        if (Vector2.Distance(p.transform.position, playerPos) < radius)
        {
            playerSidePions.Add(p);
        }
    }

    int count = playerSidePions.Count;
    for (int i = 0; i < count; i++)
    {
        playerSidePions[i].EnterFightLine(center, i, count, true); // true = player tarafı
    }
}



    public void SendArmyToCastle()
    {
        if (enemyCastle != null)
            SendArmyTo(enemyCastle);
    }

    private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("EnemyKing"))   // Tag ismini sen nasıl verdiysen ona göre
    {
        GameMode.Instance.KingBattle();
    }
}


}
