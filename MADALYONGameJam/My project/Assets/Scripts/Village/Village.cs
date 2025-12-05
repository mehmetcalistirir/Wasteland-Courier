using UnityEngine;

public class Village : MonoBehaviour
{
    public int uretilenPiyon = 0;     // Köyde biriken piyon miktarı
    public float uretiMPeriyot = 1f;  // Kaç saniyede bir piyon üretilecek?
    
    private bool playerIcinde = false;
    private PlayerPiyon player;

    void Start()
    {
        // Her periyot bir piyon üret
        InvokeRepeating("UretimYap", uretiMPeriyot, uretiMPeriyot);
    }

    void UretimYap()
    {
        uretilenPiyon++;
        Debug.Log("Köyde biriken piyon: " + uretilenPiyon);

        // Oyuncu içindeyse anında ver
        if (playerIcinde && player != null)
        {
            player.PiyonEkle(1);
            uretilenPiyon--;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIcinde = true;
            player = other.GetComponent<PlayerPiyon>();

            // Oyuncu girer girmez biriken tüm piyonları al
            while (uretilenPiyon > 0)
            {
                player.PiyonEkle(1);
                uretilenPiyon--;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIcinde = false;
            player = null;
        }
    }
}
