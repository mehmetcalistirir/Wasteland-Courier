// SoundEmitter.cs (yeni bir script)
using UnityEngine;

public static class SoundEmitter
{
    public static void EmitSound(Vector2 position, float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius);
        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.OnSoundHeard(position);
            }
        }
    }
}
