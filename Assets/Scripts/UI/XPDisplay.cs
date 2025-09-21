using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class XPDisplay : MonoBehaviour
{
    public PlayerStats playerStats;
    public TextMeshProUGUI LevelText;

    void Update()
    {
        LevelText.text = $"XP: {playerStats.currentXP} / {playerStats.xpToNextLevel}   |   Seviye: {playerStats.level}";
    }
}
