using UnityEngine;

public class PanelController : MonoBehaviour
{
    public void Open()
    {
        UIPanelSystem.Instance.OpenPanel(gameObject);
    }

    public void Close()
    {
        UIPanelSystem.Instance.CloseCurrentPanel();
    }
}
