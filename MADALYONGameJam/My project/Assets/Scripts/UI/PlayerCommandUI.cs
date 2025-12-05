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
        if (other == null) return;

        BaseController baseCtrl = other.GetComponent<BaseController>();

        if (currentBase != null && baseCtrl == currentBase)
        {
            currentBase = null;
            uiPanel.SetActive(false);
        }
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
    bool ownAnyVillage = PlayerCommander.instance.GetOwnedVillages().Count > 0;

    // Orduna ekle
    btnAddToArmy.interactable = (currentBase.owner == Team.Player);

    // Kale ise
    if (currentBase.isCastle)
    {
        btnSendCastle.gameObject.SetActive(false);
    }
    else
    {
       btnSendCastle.gameObject.SetActive(true);
btnSendCastle.interactable = hasUnits;   // ✔ Piyon varsa aktif

    }

    // 🔥 NORMALE DÖNÜŞ BURADA
    // Manuel köy butonlarını mutlaka güncelle!
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
        if (btn != null) btn.interactable = false;
        return;
    }

    bool owned = targetVillage.owner == Team.Player;
    bool hasUnits = currentBase.unitCount > 0;
    bool sameVillage = (currentBase == targetVillage);
    bool ownsAny = PlayerCommander.instance.GetOwnedVillages().Count > 0;

    // Kale → hiç köy yoksa kapat
    if (currentBase.isCastle && !ownsAny)
    {
        btn.interactable = false;
        return;
    }

    // Kural:
    // 1) hedef köy bize ait olmalı
    // 2) gönderen köyde piyon olmalı
    // 3) kendine gönderemez
    btn.interactable = (owned && hasUnits && !sameVillage);
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
        btnAddToArmy.interactable = false;
        btnSendCastle.gameObject.SetActive(false);

        if (btnSendToA != null) btnSendToA.interactable = false;
        if (btnSendToB != null) btnSendToB.interactable = false;
        if (btnSendToC != null) btnSendToC.interactable = false;
    }
}
