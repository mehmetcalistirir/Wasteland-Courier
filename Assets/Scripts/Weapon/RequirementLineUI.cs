// RequirementLineUI.cs (TEK TEXT'Lİ VERSİYON)

using UnityEngine;
using TMPro;

public class RequirementLineUI : MonoBehaviour
{
    // Artık sadece tek bir text elemanına ihtiyacımız var.
    public TextMeshProUGUI requirementText;

    // Bu fonksiyon, satırı doğru bilgilerle doldurur ve renklendirir.
    public void Setup(string name, int currentAmount, int requiredAmount)
    {
        if (requirementText == null) return;

        // Yeterli malzememiz var mı?
        bool hasEnough = (currentAmount >= requiredAmount);

        // Metnin rengini belirle.
        // Yeterliyse yeşil, değilse kırmızı.
        // ColorUtility.ToHtmlStringRGB ile renk kodlarını string içine gömebiliriz.
        string colorHex = hasEnough ? "green" : "red";

        // Biçimlendirilmiş metni oluştur: "Barrel: <color=red>0 / 3</color>"
        requirementText.text = $"{name}: <color={colorHex}>{currentAmount} / {requiredAmount}</color>";
    }
}