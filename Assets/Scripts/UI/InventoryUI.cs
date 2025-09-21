using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public TextMeshProUGUI stoneText;
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI scrapMetalText;
    public TextMeshProUGUI capacityText;
    public TextMeshProUGUI MeatText;
    public TextMeshProUGUI DeerHideText;
    public TextMeshProUGUI RabbitHideText;
    public TextMeshProUGUI CookedMeatText;
    public TextMeshProUGUI HerbText;

    private PlayerStats stats;

    void Start()
    {
        stats = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (stats == null) return;

        stoneText.text = " : " + stats.GetResourceAmount("Stone");
        woodText.text = " : " + stats.GetResourceAmount("Wood");
        scrapMetalText.text = " : " + stats.GetResourceAmount("scrapMetal");
        MeatText.text = "Et: " + stats.GetResourceAmount("Meat");
        DeerHideText.text = "Geyik Derisi: " + stats.GetResourceAmount("DeerHide");
        RabbitHideText.text = "Tavşan Derisi: " + stats.GetResourceAmount("RabbitHide");
        CookedMeatText.text = "Pişmiş Et: " + stats.GetResourceAmount("CookedMeat");
        HerbText.text = "Şifalı Ot: " + stats.GetResourceAmount("Herb");


        capacityText.text = $" : {stats.GetTotalResourceAmount()}/{stats.inventoryCapacity}";
    }
}
