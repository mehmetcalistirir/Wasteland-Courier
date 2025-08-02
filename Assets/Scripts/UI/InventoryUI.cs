using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public TextMeshProUGUI stoneText;
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI scrapMetalText;
    public TextMeshProUGUI capacityText;

    private PlayerStats stats;

    void Start()
    {
        stats = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (stats == null) return;

        stoneText.text = "Taş: " + stats.GetResourceAmount("Stone");
        woodText.text = "Odun: " + stats.GetResourceAmount("Wood");
        scrapMetalText.text = "Hurda Metal: " + stats.GetResourceAmount("scrapMetal");

        capacityText.text = $"Kapasite: {stats.GetTotalResourceAmount()}/{stats.inventoryCapacity}";
    }
}
