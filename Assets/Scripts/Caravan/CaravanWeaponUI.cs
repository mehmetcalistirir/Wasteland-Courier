using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CaravanWeaponUI : MonoBehaviour
{
    public Transform pistolRoot;
    public Transform rifleRoot;
    public Transform meleeRoot;

    public GameObject weaponButtonPrefab;

    private void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        Clear(pistolRoot);
        Clear(rifleRoot);
        Clear(meleeRoot);

        // Pistols
        CreateButtons(WeaponType.Pistol, pistolRoot);

        // Rifles (MachineGun, Shotgun, Sniper, Bow hepsi Rifle slotunda)
        CreateButtons(WeaponType.MachineGun, rifleRoot);
        CreateButtons(WeaponType.Shotgun, rifleRoot);
        CreateButtons(WeaponType.Sniper, rifleRoot);
        CreateButtons(WeaponType.Bow, rifleRoot);

        // Melee weapons
        CreateButtons(WeaponType.MeeleSword, meleeRoot);
        CreateButtons(WeaponType.ThrowingSpear, meleeRoot);
    }

    private void Clear(Transform parent)
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);
    }

    private void CreateButtons(WeaponType type, Transform root)
    {
        List<WeaponData> list = CaravanInventory.Instance.GetWeapons(type);

        for (int i = 0; i < list.Count; i++)
        {
            WeaponData weapon = list[i];

            GameObject btnGO = Instantiate(weaponButtonPrefab, root);

            // UI text update
            TMP_Text txt = btnGO.GetComponentInChildren<TMP_Text>();
            if (txt != null)
                txt.text = weapon.itemName;

            Button btn = btnGO.GetComponent<Button>();

            int indexCopy = i;
            btn.onClick.AddListener(() =>
            {
                OnWeaponSelected(type, indexCopy);
            });
        }
    }

    private void OnWeaponSelected(WeaponType type, int index)
    {
        // 1) Karavandan silahı çek
        WeaponData selected = CaravanInventory.Instance.TakeWeapon(type, index);

        if (selected == null)
        {
            Debug.LogError("Karavandan silah alınamadı!");
            return;
        }

        // 2) Bu silah hangi slotta kullanılacak?
        WeaponSlotType slotType = WeaponSlotManager.Instance.GetSlotForWeapon(selected);
        int slotIndex = (int)slotType;

        // 3) Oyuncunun üzerindeki aynı slot silahını karavana gönder
        WeaponData oldWeapon = WeaponSlotManager.Instance.slots[slotIndex];

        if (oldWeapon != null)
        {
            CaravanInventory.Instance.StoreWeapon(oldWeapon);
        }

        // 4) Yeni silahı oyuncuya tak
        WeaponSlotManager.Instance.slots[slotIndex] = selected;

        WeaponSlotManager.Instance.slots[slotIndex] = selected;
        WeaponSlotManager.Instance.SwitchSlot(slotIndex);


        // 5) Eğer aktif slot ise handler’ı güncelle
        if (WeaponSlotManager.Instance.activeSlotIndex == slotIndex)
        {
            WeaponSlotManager.Instance.SwitchSlot(slotIndex);
        }

        Debug.Log($"🔄 Swap başarılı! {selected.itemName} oyuncuya takıldı.");

        // 6) UI yenile
        RefreshUI();
    }
}
