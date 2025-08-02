using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController Instance;
    public GameObject craftMessage;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowCraftMessage()
    {
        if (craftMessage != null)
            craftMessage.SetActive(true);
    }
}
