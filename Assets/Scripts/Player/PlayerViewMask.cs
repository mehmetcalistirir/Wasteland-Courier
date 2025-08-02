using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerViewMask : MonoBehaviour
{
    public Transform maskTransform;

    private Vector2 lastDir = Vector2.right;

    void Update()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current.wKey.isPressed) input += Vector2.up;
        if (Keyboard.current.sKey.isPressed) input += Vector2.down;
        if (Keyboard.current.aKey.isPressed) input += Vector2.left;
        if (Keyboard.current.dKey.isPressed) input += Vector2.right;

        if (input.sqrMagnitude > 0.1f)
            lastDir = input.normalized;

        float angle = Mathf.Atan2(lastDir.y, lastDir.x) * Mathf.Rad2Deg;
        maskTransform.localRotation = Quaternion.Euler(0, 0, angle);
    }
}
