using UnityEngine;
using UnityEngine.UI;

public class PlayerCommandUI : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject uiPanel;

    [Header("Buttons")]
    public Button btnAddToArmy;
    public Button btnSendNext;
    public Button btnSendCastle;

    private BaseController currentBase;
    private PlayerPiyon playerArmy;

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

            if (uiPanel != null)
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
            btnAddToArmy.interactable = false;
            return;
        }

        // Sadece oyuncuya aitse "Orduna Ekle" çalışır
        btnAddToArmy.interactable = (currentBase.owner == Team.Player);

        // Diğer iki buton her zaman aktif
        btnSendNext.interactable = true;
        btnSendCastle.interactable = true;
    }

    // ---------------------------------------------------
    // KÖYDEKİ TÜM PIYONLARI OYUNCU ORDUSUNA KAT
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

    // ---------------------------------------------------
    // ORDUDAN SONRAKİ KÖYE SALDIRI
    // ---------------------------------------------------
    public void Cmd_SendToNextVillage()
    {
        PlayerCommander.instance.SendArmyToNextVillage();
    }

    // ---------------------------------------------------
    // KÖY PİYONLARINI SAVUNMA AMAÇLI OYUNCU KALESİNE GÖNDER
    // ---------------------------------------------------
    public void Cmd_SendToCastle()
    {
        if (currentBase == null) return;
        if (currentBase.owner != Team.Player) return;

        BasePiyonManager bpm = currentBase.GetComponent<BasePiyonManager>();
        if (bpm == null) return;

        // 🔥 Artık savunma için oyuncu kalesine gidiyor
        bpm.SendAllToCastle(PlayerCommander.instance.playerCastle);
    }
}
