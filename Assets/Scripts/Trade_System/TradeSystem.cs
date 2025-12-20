using UnityEngine;

public class TradeSystem : MonoBehaviour
{
    public static TradeSystem Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    // ------------------------------------------------
    // Trade yapılabilir mi?
    // ------------------------------------------------
    public bool CanTrade(TradeRecipe recipe)
    {
        if (recipe == null) return false;

        foreach (var cost in recipe.costs)
        {
            if (!Inventory.Instance.HasEnough(cost.item, cost.amount))
                return false;
        }

        return true;
    }

    // ------------------------------------------------
    // Trade dene
    // ------------------------------------------------
    public bool TryTrade(TradeRecipe recipe)
{
    if (!CanTrade(recipe))
        return false;

    // 🔒 ÖNCE sığıyor mu kontrol et
    if (!Inventory.Instance.CanAdd(
        recipe.resultItem,
        recipe.resultAmount))
    {
        Debug.Log("❌ Envanter dolu → trade iptal");
        return false;
    }

    // 🔥 SONRA tüket
    foreach (var cost in recipe.costs)
    {
        Inventory.Instance.TryConsume(cost.item, cost.amount);
    }

    // 🎁 EN SON ver
    Inventory.Instance.TryAdd(
        recipe.resultItem,
        recipe.resultAmount);

    Debug.Log($"✔ TRADE → {recipe.resultItem.itemName}");
    return true;
}



}
