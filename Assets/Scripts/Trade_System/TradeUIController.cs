using UnityEngine;

public class TradeUIController : MonoBehaviour
{
    public NPCTradeInventory currentNPC;

    [Header("UI Refs")]
    public Transform offerContainer;
    public GameObject tradeOfferPrefab;

    public void Open(NPCTradeInventory npc)
    {
        currentNPC = npc;
        gameObject.SetActive(true);
        RefreshUI();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    // ------------------------------------------------
    // 🔄 UI Yenile
    // ------------------------------------------------
    public void RefreshUI()
    {
        if (currentNPC == null)
            return;

        // 1️⃣ Eski kartları temizle
        for (int i = offerContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(offerContainer.GetChild(i).gameObject);
        }

        // 2️⃣ Yeni trade tekliflerini oluştur
        foreach (var recipe in currentNPC.tradeOffers)
        {
            GameObject go = Instantiate(
                tradeOfferPrefab,
                offerContainer
            );

            TradeOfferUI offerUI = go.GetComponent<TradeOfferUI>();
            offerUI.Setup(recipe, this);

            bool canTrade = TradeSystem.Instance.CanTrade(recipe);

            offerUI.tradeButton.interactable = canTrade;

            CanvasGroup cg = go.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.alpha = canTrade ? 1f : 0.5f;
        }

    }

    // ------------------------------------------------
    // 🔴 Button burayı çağırır
    // ------------------------------------------------
    public void OnTradeButtonClicked(TradeRecipe recipe)
    {
        bool success = TradeSystem.Instance.TryTrade(recipe);

        if (success)
            RefreshUI();
    }
}
