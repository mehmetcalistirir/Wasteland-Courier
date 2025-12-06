using UnityEngine;
using UnityEngine.UI; // Image için gerekli

public class BaseColorChanger : MonoBehaviour
{
    public BaseController baseController;
    public SpriteRenderer spriteRenderer;

    [Header("Colors")]
    public Color playerColor = Color.blue;
    public Color enemyColor = Color.red;
    public Color neutralColor = Color.gray;

    [Header("Neutral Icon (Image)")]
    public SpriteRenderer neutralIcon;   // 👈 Yeni eklenen alan

    void Awake()
    {
        if (baseController == null)
            baseController = GetComponent<BaseController>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        UpdateColor();
        UpdateIcon();
    }

    void UpdateColor()
    {
        switch (baseController.owner)
        {
            case Team.Player:
                spriteRenderer.color = playerColor;
                break;

            case Team.Enemy:
                spriteRenderer.color = enemyColor;
                break;

            case Team.Neutral:
                spriteRenderer.color = neutralColor;
                break;
        }
    }

    void UpdateIcon()
    {
        if (neutralIcon == null) return;

        // Taraf tarafsız ise ikon görünür, değilse gizlenir
        if (baseController.owner == Team.Neutral)
            neutralIcon.gameObject.SetActive(true);
        else
            neutralIcon.gameObject.SetActive(false);
    }
}
