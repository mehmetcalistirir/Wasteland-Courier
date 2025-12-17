using UnityEngine;


[System.Serializable]
public class MagazineInstance
{
    public MagazineData data;
    public int currentAmmo;

    public bool IsFull
    {
        get
        {
            if (data == null)
            {
                Debug.LogError("MagazineInstance.data NULL!");
                return data != null && currentAmmo >= data.capacity;

            }

            return currentAmmo >= data.capacity;
        }
    }

    public bool IsEmpty
    {
        get
        {
            return currentAmmo <= 0;
        }
    }

    public MagazineInstance(MagazineData data)
    {
        this.data = data;
        currentAmmo = 0;
    }
}

