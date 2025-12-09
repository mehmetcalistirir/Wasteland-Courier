using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonStateController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite hoverSprite;
    public Sprite clickedSprite;

    private Image image;

    void Awake()
    {
        image = GetComponent<Image>();
        image.sprite = normalSprite;
    }

    // Mouse üstüne gelince
    public void OnPointerEnter(PointerEventData eventData)
    {
        image.sprite = hoverSprite;
    }

    // Mouse ayrılınca
    public void OnPointerExit(PointerEventData eventData)
    {
        image.sprite = normalSprite;
    }

    // Mouse tıklayınca
    public void OnPointerDown(PointerEventData eventData)
    {
        image.sprite = clickedSprite;
    }

    // Mouse bırakınca
    public void OnPointerUp(PointerEventData eventData)
    {
        // Eğer hala üzerindeyse hover'a, değilse normale dön
        if (RectTransformUtility.RectangleContainsScreenPoint(
            GetComponent<RectTransform>(),
            Input.mousePosition))
        {
            image.sprite = hoverSprite;
        }
        else
        {
            image.sprite = normalSprite;
        }
    }
}
