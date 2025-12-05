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
    // Eğer other null olduysa (Destroy edilmişse) hata olmasın
    if (other == null) return;
    
    BaseController baseCtrl = other.GetComponent<BaseController>();

    if (currentBase != null && baseCtrl == currentBase)
    {
        currentBase = null;

        if (uiPanel != null)
            uiPanel.SetActive(false);
    }
}


    // ------------------------------
    // BUTTON LOGIC
    // ------------------------------

    void UpdateButtons()
    {
        if (currentBase == null)
        {
            btnAddToArmy.interactable = false;
            return;
        }

        // Sadece oyuncuya aitse orduna ekleyebilir
        btnAddToArmy.interactable = (currentBase.owner == Team.Player);

        // Diğer 2 buton her zaman aktif olabilir
        btnSendNext.interactable = true;
        btnSendCastle.interactable = true;
    }

    public void Cmd_AddToArmy()
{
    if (currentBase == null) return;
    if (playerArmy == null) return;
    if (currentBase.owner != Team.Player) return;

    BasePiyonManager bpm = currentBase.GetComponent<BasePiyonManager>();
    if (bpm != null)
    {
        // tüm wandering piyonları oyuncuya gönder
        bpm.TransferAllToPlayer(transform);
    }

    // kalan sayı sıfırlanır
    currentBase.unitCount = 0;

    UpdateButtons();
}


    public void Cmd_SendToNextVillage()
    {
        PlayerCommander.instance.SendArmyToNextVillage();
    }

    public void Cmd_SendToCastle()
{
    if (currentBase == null) return;

    // Bu köy/kale bize ait mi?
    if (currentBase.owner != Team.Player) return;

    BasePiyonManager bpm = currentBase.GetComponent<BasePiyonManager>();
    if (bpm == null) return;

    // Köyden oyuncu kalesine piyon gönder
    bpm.SendAllToCastle(PlayerCommander.instance.enemyCastle);
}

}
