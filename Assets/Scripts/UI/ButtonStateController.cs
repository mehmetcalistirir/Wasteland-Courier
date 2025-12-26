using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
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
    private Selectable selectable; // Button, Dropdown, Toggle vs.

    void Awake()
    {
        image = GetComponent<Image>();
        selectable = GetComponent<Selectable>(); // varsa alır

        
    }

    bool IsInteractable()
    {
        // Button / Dropdown / Toggle vs. varsa onun interactable durumuna bak
        if (selectable != null)
            return selectable.interactable;

        // Hiç Selectable yoksa her zaman etkileşime açık say
        return true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractable() || hoverSprite == null) return;
        image.sprite = hoverSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsInteractable() || normalSprite == null) return;
        image.sprite = normalSprite;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsInteractable() || clickedSprite == null) return;
        image.sprite = clickedSprite;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsInteractable()) return;

        // Hala butonun üzerindeysek hover’a dön
        if (RectTransformUtility.RectangleContainsScreenPoint(
                transform as RectTransform,
                Input.mousePosition))
        {
            if (hoverSprite != null)
                image.sprite = hoverSprite;
        }
        else
        {
            if (normalSprite != null)
                image.sprite = normalSprite;
        }
    }

    void Update()
    {
        // Runtime'da disable edilirse normal sprite'a dön
        if (!IsInteractable() && image != null && normalSprite != null && image.sprite != normalSprite)
        {
            image.sprite = normalSprite;
        }
    }
}
