using UnityEngine;

public class PlayerCommander : MonoBehaviour
{
    public static PlayerCommander instance;

    [Header("References")]
    [Tooltip("Oyuncunun etrafında dönen piyonları yöneten script")]
    public PlayerPiyon playerArmy;

    [Tooltip("Haritadaki tüm köy BaseController'ları (Inspector'dan doldur)")]
    public BaseController[] villages;

    [Tooltip("Oyuncunun kendi kalesi (Savunma için kullanılacak)")]
    public BaseController playerCastle;   // 🔥 EKLENDİ

    [Tooltip("Düşman kalesinin BaseController'ı")]
    public BaseController enemyCastle;

    private int nextVillageIndex = 0;

    private void Awake()
    {
        // Basit Singleton
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    /// <summary>
    /// Oyuncu ordusunda kaç piyon olduğunu döner.
    /// </summary>
    public int GetArmyCount()
    {
        if (playerArmy == null) return 0;
        return playerArmy.GetCount();
    }

    /// <summary>
    /// Ordudaki TÜM piyonları hedef base'e doğru gönderir.
    /// </summary>
    public void SendArmyTo(BaseController target)
    {
        if (target == null) return;
        if (playerArmy == null) return;

        // Ordudaki tüm piyon GameObject'lerini al ve listeyi boşalt
        GameObject[] army = playerArmy.ExtractAll();
        if (army == null || army.Length == 0) return;

        foreach (GameObject go in army)
        {
            if (go == null) continue;

            Piyon piyon = go.GetComponent<Piyon>();
            if (piyon == null) continue;

            // Saldırı modu: piyon AttackBase kullanır
            piyon.AttackBase(target, Team.Player);
        }
    }

    /// <summary>
    /// Ordunu listedeki bir SONRAKİ köye gönder.
    /// </summary>
    public void SendArmyToNextVillage()
{
    if (villages == null || villages.Length == 0) return;
    if (playerArmy == null || playerArmy.GetCount() == 0) return; // ordun yoksa çık

    int loopCount = 0;
    BaseController target = null;

    // villages dizisi içinde döner, uygun hedef köyü arar
    while (loopCount < villages.Length)
    {
        BaseController candidate = villages[nextVillageIndex];

        // Sonraki çağrıda baştan değil, kaldığı yerden devam etsin diye artır
        nextVillageIndex = (nextVillageIndex + 1) % villages.Length;
        loopCount++;

        if (candidate == null) continue;

        // Eğer BaseController'da isCastle kullandıysak:
        if (candidate.isCastle) continue;       // kaleleri atla

        // Kendi köyümüze saldırmayalım
        if (candidate.owner == Team.Player) continue;

        // Buraya kadar geldiyse uygun hedeftir
        target = candidate;
        break;
    }

    if (target != null)
    {
        SendArmyTo(target);
    }
    else
    {
        // Uygun hedef köy yok: istersen debug log bırak
        Debug.Log("Gönderilecek uygun köy bulunamadı.");
    }
}


    /// <summary>
    /// Ordunu düşman kalesine gönder (Saldırı).
    /// </summary>
    public void SendArmyToCastle()
    {
        if (enemyCastle == null) return;

        SendArmyTo(enemyCastle);
    }

    public void SendVillagePiyonsToNextVillage(BaseController fromBase)
{
    if (villages == null || villages.Length == 0) return;

    int loop = 0;
    BaseController target = null;

    // sıradaki köyü bul
    while (loop < villages.Length)
    {
        BaseController candidate = villages[nextVillageIndex];
        nextVillageIndex = (nextVillageIndex + 1) % villages.Length;
        loop++;

        if (candidate == null) continue;
        if (candidate.isCastle) continue;      // kale değil
        if (candidate == fromBase) continue;   // aynı köy değil

        target = candidate;
        break;
    }

    if (target == null) return;

    BasePiyonManager bpm = fromBase.GetComponent<BasePiyonManager>();
    if (bpm == null) return;

    // Köy piyonlarını savunma olarak diğer köye gönder
    bpm.SendAllToCastle(target);  // isim castle ama işlev savunma
}

}
