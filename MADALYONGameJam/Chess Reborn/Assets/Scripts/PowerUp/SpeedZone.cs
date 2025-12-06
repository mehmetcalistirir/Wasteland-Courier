using UnityEngine;

public class SpeedZone : MonoBehaviour
{
    private BaseController baseCtrl;

    void Awake()
    {
        baseCtrl = GetComponentInParent<BaseController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // PLAYER buff alanına girdi
        if (other.CompareTag("Player"))
        {
            PlayerMovement2D mov = other.GetComponent<PlayerMovement2D>();
            if (mov != null)
                mov.SetInsideZone(baseCtrl); // sadece referans ver
        }

        // ENEMY buff alanına girdi
        if (other.CompareTag("Enemy"))
        {
            EnemyMovementBoost boost = other.GetComponent<EnemyMovementBoost>();
            if (boost != null)
                boost.SetInsideZone(baseCtrl); // sadece referans ver
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // PLAYER buff alanından çıktı
        if (other.CompareTag("Player"))
        {
            PlayerMovement2D mov = other.GetComponent<PlayerMovement2D>();
            if (mov != null)
                mov.SetInsideZone(null);  // referansı kaldır
        }

        // ENEMY buff alanından çıktı
        if (other.CompareTag("Enemy"))
        {
            EnemyMovementBoost boost = other.GetComponent<EnemyMovementBoost>();
            if (boost != null)
                boost.SetInsideZone(null); // referansı kaldır
        }
    }
}
