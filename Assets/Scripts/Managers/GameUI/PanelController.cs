using UnityEngine;

public class PanelController : MonoBehaviour
{
    public void Open()
    {
        
    }

    public void Close()
    {
        UIPanelSystem.Instance.CloseCurrentPanel();
    }
}
