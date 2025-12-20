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

        // Rifles
        CreateButtons(WeaponType.MachineGun, rifleRoot);
        CreateButtons(WeaponType.Shotgun, rifleRoot);
        CreateButtons(WeaponType.Sniper, rifleRoot);
        CreateButtons(WeaponType.Bow, rifleRoot);

        // Melee
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
        List<WeaponItemData> list = CaravanInventory.Instance.GetWeapons(type);

        for (int i = 0; i < list.Count; i++)
        {
            WeaponItemData weaponItem = list[i];

            GameObject btnGO = Instantiate(weaponButtonPrefab, root);

            // UI text
            TMP_Text txt = btnGO.GetComponentInChildren<TMP_Text>();
            if (txt != null)
                txt.text = weaponItem.itemName;

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
        // 1) Karavandan silah ITEM'ini çek
        WeaponItemData selectedItem =
            CaravanInventory.Instance.TakeWeapon(type, index);

        if (selectedItem == null)
        {
            Debug.LogError("Karavandan silah alınamadı!");
            return;
        }

        // 2) Bu silah hangi slotta kullanılacak?
        WeaponSlotType slotType =
            WeaponSlotManager.Instance.GetSlotForWeapon(selectedItem.weaponType);

        int slotIndex = (int)slotType;

        // 3) Oyuncunun üzerindeki silahı karavana gönder
        WeaponItemData oldItem =
            WeaponSlotManager.Instance.GetWeaponItemInSlot(slotIndex);

        if (oldItem != null)
        {
            CaravanInventory.Instance.StoreWeapon(oldItem);
        }

        // 4) Yeni silahı oyuncuya tak
        WeaponSlotManager.Instance.EquipWeaponInSlot(selectedItem, slotIndex);
        WeaponSlotManager.Instance.SwitchSlot(slotIndex);

        Debug.Log($"🔄 Swap başarılı! {selectedItem.itemName} oyuncuya takıldı.");

        // 5) UI yenile
        RefreshUI();
    }
}
