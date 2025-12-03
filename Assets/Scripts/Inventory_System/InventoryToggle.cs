using UnityEngine;
using UnityEngine.InputSystem; // Yeni sistem için şart

public class InventoryToggle : MonoBehaviour
{
    public GameObject inventoryPanel;

    private Keyboard keyboard;

    void Awake()
    {
        keyboard = Keyboard.current;
    }

    void Update()
    {
        
    }
}
