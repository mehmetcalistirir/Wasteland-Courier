using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class PlayerPiyon : MonoBehaviour
{
    public GameObject piyonPrefab;

    public float baseRadius = 1.5f;   // 1. halkanın yarıçapı
    public float radiusStep = 1.0f;   // Her yeni halka ne kadar genişlesin?
    public TextMeshPro countText;

    private List<GameObject> piyonListesi = new List<GameObject>();

    public void PiyonEkle(int miktar)
    {
        for (int i = 0; i < miktar; i++)
        {
            GameObject p = Instantiate(piyonPrefab, transform.position, Quaternion.identity);
            piyonListesi.Add(p);
        }
    }

    void Update()
    {
        PiyonlariYorungelerdeDondur();
        if (countText != null)
            countText.text = GetCount().ToString();
    }

    void PiyonlariYorungelerdeDondur()
    {
        for (int i = 0; i < piyonListesi.Count; i++)
        {
            int ring, indexInRing, ringSize;
            HesaplaRingBilgisi(i, out ring, out indexInRing, out ringSize);

            float radius = baseRadius + ring * radiusStep;
            float angle = (float)indexInRing / ringSize * Mathf.PI * 2f;

            Vector3 hedefPoz = transform.position +
                new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;

            piyonListesi[i].transform.position =
                Vector3.Lerp(piyonListesi[i].transform.position, hedefPoz, Time.deltaTime * 5f);
        }
    }

        public void RemovePiyons(int amount)
    {
        amount = Mathf.Min(amount, piyonListesi.Count);

        for (int i = 0; i < amount; i++)
        {
            Destroy(piyonListesi[0]);
            piyonListesi.RemoveAt(0);
        }
    }
    

    void HesaplaRingBilgisi(int index, out int ring, out int indexInRing, out int ringSize)
    {
        ring = 0;
        ringSize = 8;   // İlk halka = 8 piyon
        int count = index;

        while (count >= ringSize)
        {
            count -= ringSize;
            ring++;
            ringSize *= 2; // her halka bir öncekinin 2 katı
        }

        indexInRing = count;
    }

    public int GetCount()
{
    return piyonListesi.Count;
}

public GameObject[] ExtractAll()
{
    GameObject[] arr = piyonListesi.ToArray();
    piyonListesi.Clear();
    return arr;
}

}
