using UnityEngine;
using UnityEngine.UI;

public class PlayerCommandUI : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject uiPanel;

    [Header("Main Buttons")]
    public Button btnAddToArmy;
    public Button btnSendCastle;

    [Header("Specific Village Buttons (Manual Setup)")]
    public Button btnSendToA;
    public BaseController targetVillageA;

    public Button btnSendToB;
    public BaseController targetVillageB;

    public Button btnSendToC;
    public BaseController targetVillageC;

    private BaseController currentBase;
    private PlayerPiyon playerArmy;

    void Update()
{
    if (uiPanel.activeSelf && currentBase != null)
    {
        UpdateButtons();
    }
}


    void Start()
    {
        if (uiPanel != null)
            uiPanel.SetActive(false);

        playerArmy = GetComponent<PlayerPiyon>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BaseController baseCtrl = other.GetComponent<BaseController>();

        if (baseCtrl != null)
        {
            currentBase = baseCtrl;
            uiPanel.SetActive(true);
            UpdateButtons();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
{
    // Eğer collider zaten yok edilmişse → çık
    if (other == null) 
        return;

    BaseController baseCtrl = other.GetComponent<BaseController>();

    // Eğer currentBase hâlâ geçerli değilse → çık
    if (currentBase == null)
        return;

    // Eğer bu çıkan collider currentBase değilse → çık
    if (baseCtrl != currentBase)
        return;

    // Artık UI güvenle kapatılabilir
    if (uiPanel != null)
        uiPanel.SetActive(false);

    currentBase = null;
}


    // ---------------------------------------------------
    // BUTTON DURUMLARINI GÜNCELLE
    // ---------------------------------------------------
void UpdateButtons()
{
    if (currentBase == null)
    {
        DisableAllButtons();
        return;
    }

    bool hasUnits = currentBase.unitCount > 0;
    bool ownedByPlayer = currentBase.owner == Team.Player;

    // ---------------------------------------------
    // 1) ORDUNA EKLE BUTONU (Sadece Player köyünde ve piyon varsa)
    // ---------------------------------------------
    if (ownedByPlayer && hasUnits)
    {
        btnAddToArmy.gameObject.SetActive(true);
        btnAddToArmy.interactable = true;
    }
    else
    {
        btnAddToArmy.gameObject.SetActive(false);
    }

    // ---------------------------------------------
    // 2) KALEYE GÖNDER BUTONU
    // ---------------------------------------------
    // Şartlar:
    // - Köy bize ait olmalı
    // - Köy kale olmamalı
    // - Köyde piyon olmalı
    if (ownedByPlayer && !currentBase.isCastle && hasUnits)
    {
        btnSendCastle.gameObject.SetActive(true);
        btnSendCastle.interactable = true;
    }
    else
    {
        btnSendCastle.gameObject.SetActive(false);
    }

    // ---------------------------------------------
    // 3) ÖZEL KÖY BUTONLARI
    // ---------------------------------------------
    UpdateVillageButton(btnSendToA, targetVillageA);
    UpdateVillageButton(btnSendToB, targetVillageB);
    UpdateVillageButton(btnSendToC, targetVillageC);
}





    // ---------------------------------------------------
    // Özel köy butonu aktiflik hesaplama
    // ---------------------------------------------------
    void UpdateVillageButton(Button btn, BaseController targetVillage)
{
    if (btn == null || targetVillage == null)
    {
        if (btn != null) btn.gameObject.SetActive(false);
        return;
    }

    bool owned = targetVillage.owner == Team.Player;
    bool hasUnits = currentBase.unitCount > 0;
    bool sameVillage = (currentBase == targetVillage);

    // Gönderme kuralları:
    // - Hedef köy bize ait olacak
    // - Gönderen köyde piyon olacak
    // - Kendine gönderemez
    bool canSend = owned && hasUnits && !sameVillage;

    btn.gameObject.SetActive(canSend);
}





    // ---------------------------------------------------
    // TÜM PIYONLARI OYUNCU ORDUSUNA KAT
    // ---------------------------------------------------
    public void Cmd_AddToArmy()
    {
        if (currentBase == null) return;
        if (playerArmy == null) return;
        if (currentBase.owner != Team.Player) return;

        BasePiyonManager bpm = currentBase.GetComponent<BasePiyonManager>();
        if (bpm != null)
            bpm.TransferAllToPlayer(transform);

        currentBase.unitCount = 0;
        UpdateButtons();
    }

    public void Cmd_SendToCastle()
{
    if (currentBase == null) return;
    if (currentBase.owner != Team.Player) return;

    BasePiyonManager bpm = currentBase.GetComponent<BasePiyonManager>();
    if (bpm == null) return;

    // Savunma birliği oyuncu kalesine gönderilecek
    bpm.SendAllToCastle(PlayerCommander.instance.playerCastle);

    UpdateButtons();
}



    // ---------------------------------------------------
    // BELİRLİ KÖYE GÖNDER
    // ---------------------------------------------------
    public void Cmd_SendToSpecificVillage(BaseController targetVillage)
    {
        if (currentBase == null) return;

        if (currentBase == targetVillage)
        {
            Debug.Log("Bir köy kendisine piyon gönderemez!");
            return;
        }

        if (currentBase.owner != Team.Player) return;

        if (targetVillage.owner != Team.Player)
        {
            Debug.Log("Bu köy bize ait değil → gönderilemez.");
            return;
        }

        if (currentBase.unitCount <= 0)
        {
            Debug.Log("Gönderen köyde piyon yok.");
            return;
        }

        // Kale → hiç köy yoksa gönderemez
        if (currentBase.isCastle && PlayerCommander.instance.GetOwnedVillages().Count == 0)
        {
            Debug.Log("Hiç köyünüz yok → kaleden gönderilemez!");
            return;
        }

        PlayerCommander.instance.SendVillagePiyonsTo(currentBase, targetVillage);
        UpdateButtons();
    }

    // ---------------------------------------------------
    void DisableAllButtons()
{
    btnAddToArmy.gameObject.SetActive(false);
    btnSendCastle.gameObject.SetActive(false);

    if (btnSendToA != null) btnSendToA.gameObject.SetActive(false);
    if (btnSendToB != null) btnSendToB.gameObject.SetActive(false);
    if (btnSendToC != null) btnSendToC.gameObject.SetActive(false);
}

}
