using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CostRowUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text text;

    public void Setup(Sprite sprite, string itemName, int owned, int required)
    {
        Debug.Log($"[COST ROW] sprite={(sprite ? sprite.name : "NULL")}");
        icon.sprite = sprite;

        text.text = $"{itemName}  {owned}/{required}";
        text.color = owned >= required ? Color.white : Color.red;
    }
}
