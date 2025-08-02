// WeaponSlotUI.cs (İKONLARI ATAYAN GÜNCELLENMİŞ HALİ)

using UnityEngine;
using System.Collections.Generic;

public class WeaponSlotUI : MonoBehaviour
{
    public static WeaponSlotUI Instance { get; private set; }

    public Transform slotContainer;
    public GameObject slotPrefab;
    
    private List<WeaponSlotButton> slotButtons = new List<WeaponSlotButton>();

    void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    void Start()
    {
        // Önceki cevaptaki "Slot boş" hatasını önlemek için, bu fonksiyonu
        // WeaponSlotManager'ın Awake'i bittikten sonra çağırmak daha güvenlidir.
        // Start() bunun için genellikle yeterlidir.
        InitializeSlots();
    }

    private void InitializeSlots()
    {
        // Önce eskileri temizle
        foreach (Transform child in slotContainer) Destroy(child.gameObject);
        slotButtons.Clear();

        // WeaponSlotManager'dan kuşanılmış silahların listesini al.
        WeaponBlueprint[] equippedBlueprints = WeaponSlotManager.Instance.GetEquippedBlueprints();

        // Bu listedeki her bir silah için bir UI slotu oluştur.
        for (int i = 0; i < equippedBlueprints.Length; i++)
        {
            GameObject slotObject = Instantiate(slotPrefab, slotContainer);
            WeaponSlotButton slotButton = slotObject.GetComponent<WeaponSlotButton>();

            // O slottaki blueprint'i al.
            WeaponBlueprint blueprintInSlot = equippedBlueprints[i];
            
            // Blueprint'ten ikonu çıkar. Eğer blueprint yoksa (slot boşsa), ikon null olacak.
            Sprite weaponIcon = (blueprintInSlot != null) ? blueprintInSlot.weaponIcon : null;
            
            // Butona kendi index'ini ve doğru ikonu vererek kurmasını söyle.
            slotButton.Setup(i, weaponIcon);
            slotButtons.Add(slotButton);
        }
        
        // Başlangıçtaki aktif silahı vurgula
        UpdateHighlight(WeaponSlotManager.Instance.activeSlotIndex);
    }

    // Bir slota tıklandığında bu fonksiyon çağrılır.
    public void OnSlotClicked(int index)
    {
        WeaponSlotManager.Instance.SwitchToSlot(index);
    }

    // Aktif silahın vurgusunu günceller.
    public void UpdateHighlight(int activeIndex)
    {
        for (int i = 0; i < slotButtons.Count; i++)
        {
            slotButtons[i].SetHighlight(i == activeIndex);
        }
    }
}