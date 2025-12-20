using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image), typeof(Button))]
public class ButtonStateController : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite hoverSprite;
    public Sprite clickedSprite;

    private Image image;
    private Button button;

    void Awake()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();

        image.sprite = normalSprite;
    }

    // 🔒 Button pasifse hiçbir şey yapma
    bool IsInteractable()
    {
        return button != null && button.interactable;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractable()) return;
        image.sprite = hoverSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsInteractable()) return;
        image.sprite = normalSprite;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsInteractable()) return;
        image.sprite = clickedSprite;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsInteractable()) return;

        if (RectTransformUtility.RectangleContainsScreenPoint(
            transform as RectTransform,
            Input.mousePosition))
        {
            image.sprite = hoverSprite;
        }
        else
        {
            image.sprite = normalSprite;
        }
    }

    // 🔁 Button runtime'da disable edilirse sprite resetlensin
    void Update()
    {
        if (!IsInteractable() && image.sprite != normalSprite)
        {
            image.sprite = normalSprite;
        }
    }
}
