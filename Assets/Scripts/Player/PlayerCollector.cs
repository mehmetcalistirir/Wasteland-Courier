using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCollector : MonoBehaviour
{
    public float collectRange = 1.5f;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, collectRange);
            foreach (var hit in hits)
            {
                Resource res = hit.GetComponent<Resource>();
                if (res != null)
                {
                    res.Collect();
                    break;
                }


            }
        }
    }
}
