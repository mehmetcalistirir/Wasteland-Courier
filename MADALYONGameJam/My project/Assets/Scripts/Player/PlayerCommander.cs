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

        GameObject[] army = playerArmy.ExtractAll();
        if (army == null || army.Length == 0) return;

        foreach (GameObject go in army)
        {
            if (go == null) continue;

            Piyon piyon = go.GetComponent<Piyon>();
            if (piyon != null)
                piyon.AttackBase(target, Team.Player);
        }
    }

    public void SendArmyToCastle()
    {
        if (enemyCastle != null)
            SendArmyTo(enemyCastle);
    }
}
