using UnityEngine;

public class PlayerPiyon : MonoBehaviour
{
    public int piyonSayisi = 0;

    public void PiyonEkle(int miktar)
    {
        piyonSayisi += miktar;
        Debug.Log("Player Piyon Sayısı: " + piyonSayisi);
    }
}

