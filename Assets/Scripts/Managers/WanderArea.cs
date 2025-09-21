using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class WanderArea : MonoBehaviour
{
    private BoxCollider2D box;

    void Awake() { box = GetComponent<BoxCollider2D>(); }

    public Vector2 GetRandomPoint()
    {
        var center = (Vector2)transform.TransformPoint(box.offset);
        var size   = Vector2.Scale(box.size, (Vector2)transform.lossyScale);
        float rx = Random.Range(-size.x * 0.5f, size.x * 0.5f);
        float ry = Random.Range(-size.y * 0.5f, size.y * 0.5f);
        return center + new Vector2(rx, ry);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        var b = GetComponent<BoxCollider2D>();
        if (!b) return;
        Gizmos.color = Color.cyan;
        var center = (Vector2)transform.TransformPoint(b.offset);
        var size   = Vector2.Scale(b.size, (Vector2)transform.lossyScale);
        Gizmos.DrawWireCube(center, size);
    }
#endif
}
