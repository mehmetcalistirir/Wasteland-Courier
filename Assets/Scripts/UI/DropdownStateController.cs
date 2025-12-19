using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropdownStateController : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    private TMP_Dropdown dropdown;
    private Animator anim;

    void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        anim = GetComponent<Animator>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        anim.SetBool("Hover", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        anim.SetBool("Hover", false);
    }

    void Update()
    {
        anim.SetBool("Open", dropdown.IsExpanded);
    }
}
