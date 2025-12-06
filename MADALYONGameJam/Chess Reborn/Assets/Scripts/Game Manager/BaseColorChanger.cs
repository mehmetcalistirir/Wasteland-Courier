using UnityEngine;

public class BaseColorChanger : MonoBehaviour
{
    public BaseController baseController; // Köy/kale scripti
    public SpriteRenderer spriteRenderer;

    [Header("Colors")]
    public Color playerColor = Color.blue;
    public Color enemyColor = Color.red;
    public Color neutralColor = Color.gray;

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
}
