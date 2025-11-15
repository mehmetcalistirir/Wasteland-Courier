using UnityEngine;
using System.Collections;

public class BurnZone : MonoBehaviour
{
    public int damagePerSecond = 5;
    public float duration = 5f;

    private float tickTimer;

private void OnTriggerStay2D(Collider2D other)
{
    if (other.CompareTag("Enemy"))
    {
        tickTimer += Time.deltaTime;
        if (tickTimer >= 1f)
        {
            tickTimer = 0f;
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
                enemy.TakeDamage(damagePerSecond);
        }
    }
}

}
