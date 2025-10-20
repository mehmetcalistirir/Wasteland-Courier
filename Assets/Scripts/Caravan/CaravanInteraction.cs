using UnityEngine;
using UnityEngine.InputSystem;

public class CaravanInteraction : MonoBehaviour
{
    private bool isPlayerNearby = false;

    void Update()
    {
        if (isPlayerNearby && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (GameManager.Instance.HasAllFuel())
            {
                GameManager.Instance.LoadNextScene();
            }
            else
            {
                Debug.Log("⛔ Tüm yakıtlar toplanmadan karavana binemezsin!");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerNearby = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerNearby = false;
    }
}
