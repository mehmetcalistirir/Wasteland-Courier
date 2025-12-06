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
        // PLAYER KING buff alanına girdi
        if (other.CompareTag("PlayerKing"))
        {
            PlayerMovement2D mov = other.GetComponent<PlayerMovement2D>();
            if (mov != null)
                mov.SetInsideZone(baseCtrl);  // referans ver → hız hesaplaması orada yapılır
        }

        // ENEMY KING buff alanına girdi
        if (other.CompareTag("EnemyKing"))
        {
            EnemyMovementBoost boost = other.GetComponent<EnemyMovementBoost>();
            if (boost != null)
                boost.SetInsideZone(baseCtrl);  // referans ver
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // PLAYER KING buff alanından çıktı
        if (other.CompareTag("PlayerKing"))
        {
            PlayerMovement2D mov = other.GetComponent<PlayerMovement2D>();
            if (mov != null)
                mov.SetInsideZone(null);  // buff kaldır
        }

        // ENEMY KING buff alanından çıktı
        if (other.CompareTag("EnemyKing"))
        {
            EnemyMovementBoost boost = other.GetComponent<EnemyMovementBoost>();
            if (boost != null)
                boost.SetInsideZone(null); // buff kaldır
        }
    }
}
