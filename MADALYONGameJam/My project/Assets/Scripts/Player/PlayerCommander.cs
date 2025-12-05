using UnityEngine;

public class PlayerCommander : MonoBehaviour
{
    public static PlayerCommander instance;

    [Header("References")]
    [Tooltip("Oyuncunun etrafında dönen piyonları yöneten script")]
    public PlayerPiyon playerArmy;

    [Tooltip("Haritadaki tüm köy BaseController'ları (Inspector'dan doldur)")]
    public BaseController[] villages;

    [Tooltip("Düşman kalesinin BaseController'ı")]
    public BaseController enemyCastle;

    private int nextVillageIndex = 0;

    private void Awake()
    {
        // Basit singleton
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    /// <summary>
    /// Şu an oyuncunun ordusunda kaç piyon var?
    /// </summary>
    public int GetArmyCount()
    {
        if (playerArmy == null) return 0;
        return playerArmy.GetCount();
    }

    /// <summary>
    /// Ordudaki TÜM piyonları hedef base'e doğru yürütür.
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

            // Piyonun kendi AttackBase fonksiyonunu kullanıyoruz
            piyon.AttackBase(target, Team.Player);

        }
    }

    /// <summary>
    /// Ordunu listedeki bir SONRAKİ köye gönder.
    /// </summary>
    public void SendArmyToNextVillage()
    {
        if (villages == null || villages.Length == 0) return;

        // Güvenlik: null olanları atla
        int safety = 0;
        while (villages[nextVillageIndex] == null && safety < villages.Length)
        {
            nextVillageIndex = (nextVillageIndex + 1) % villages.Length;
            safety++;
        }

        BaseController target = villages[nextVillageIndex];

        // Sonraki çağrıda bir sonrakini seçmesi için index'i ilerlet
        nextVillageIndex = (nextVillageIndex + 1) % villages.Length;

        if (target != null)
        {
            SendArmyTo(target);
        }
    }

    /// <summary>
    /// Ordunu düşman kalesine gönder.
    /// </summary>
    public void SendArmyToCastle()
    {
        if (enemyCastle == null) return;

        SendArmyTo(enemyCastle);
    }
}
