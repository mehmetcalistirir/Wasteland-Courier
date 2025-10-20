using UnityEngine;
using System.Reflection;

public static class InventoryEquipBridge
{
    static void TrySetWeaponData(object playerWeapon, object weaponData)
    {
        if (playerWeapon == null) return;
        var t = playerWeapon.GetType();
        var f = t.GetField("weaponData", BindingFlags.Public | BindingFlags.Instance);
        if (f != null) { f.SetValue(playerWeapon, weaponData); return; }
        var p = t.GetProperty("weaponData", BindingFlags.Public | BindingFlags.Instance);
        if (p != null && p.CanWrite) { p.SetValue(playerWeapon, weaponData, null); return; }
        Debug.LogWarning("[EquipBridge] PlayerWeapon.weaponData bulunamadı (field/property).");
    }

    static bool TrySetAmmo(object playerWeapon, int clip, int reserve)
    {
        if (playerWeapon is IAmmoProvider ip)
        { ip.SetAmmo(clip, reserve); return true; }

        var t = playerWeapon.GetType();
        var m = t.GetMethod("SetAmmo", BindingFlags.Public | BindingFlags.Instance);
        if (m != null)
        {
            m.Invoke(playerWeapon, new object[] { clip, reserve });
            return true;
        }
        Debug.LogWarning("[EquipBridge] SetAmmo yok. IAmmoProvider uygulayın ya da SetAmmo ekleyin.");
        return false;
    }

    static bool TryGetAmmo(object playerWeapon, out int clip, out int reserve)
    {
        clip = reserve = 0;
        if (playerWeapon is IAmmoProvider ip)
        { ip.GetAmmo(out clip, out reserve); return true; }

        var t = playerWeapon.GetType();
        var m = t.GetMethod("GetAmmo", BindingFlags.Public | BindingFlags.Instance);
        if (m != null)
        {
            object[] args = new object[] { 0, 0 };
            m.Invoke(playerWeapon, args);
            clip = (int)args[0]; reserve = (int)args[1];
            return true;
        }
        Debug.LogWarning("[EquipBridge] GetAmmo yok. IAmmoProvider uygulayın ya da GetAmmo ekleyin.");
        return false;
    }

    public static bool EquipWeaponFromSlot(int invIndex, MonoBehaviour playerWeapon)
    {
        var item = Inventory.Instance.slots[invIndex];
        if (item?.data is WeaponItemData w)
        {
            var bp = w.blueprint;
            var payload = item.weapon ?? new InventoryItem.WeaponInstancePayload {
                id = System.Guid.NewGuid().ToString("N"),
                clip = bp.weaponData.clipSize,
                reserve = bp.weaponData.maxAmmoCapacity
            };

            TrySetWeaponData(playerWeapon, bp.weaponData);
            TrySetAmmo(playerWeapon, payload.clip, payload.reserve);
            return true;
        }
        return false;
    }

    public static void SyncBackAmmo(int invIndex, MonoBehaviour playerWeapon)
    {
        var item = Inventory.Instance.slots[invIndex];
        if (item?.weapon != null && TryGetAmmo(playerWeapon, out int clip, out int reserve))
        {
            item.weapon.clip = clip;
            item.weapon.reserve = reserve;

            // ❗ Burada artık event'e dışarıdan Invoke YAPMIYORUZ.
            // Bunun yerine Inventory içindeki yardımcı metodu çağırıyoruz:
            Inventory.Instance.RaiseChanged();
        }
    }
}
