using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Refs")]
    public Inventory inventory;
    public Transform gridParent;        // 30 slotu bunun altına koy
    public GameObject slotPrefab;

    [Header("Visual")]
    public int slotCount = 30;

    private InventorySlotUI[] slots;

    void Awake()
    {
        if (inventory == null)
            inventory = Inventory.Instance;

        slots = new InventorySlotUI[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            GameObject go = Instantiate(slotPrefab, gridParent); // sadece bir kez oluştur
            InventorySlotUI slot = go.GetComponent<InventorySlotUI>();

            if (slot == null)
            {
                Debug.LogError("Slot prefab içinde InventorySlotUI component eksik!");
                continue;
            }

            slot.Bind(i, this);
            go.SetActive(false); // Başlangıçta gizli
            slots[i] = slot;
            go.SetActive(false); // ✅ Başta gizle
        }
    
}



    void OnEnable()
    {
        if (inventory == null) inventory = Inventory.Instance;
        if (inventory != null) inventory.OnChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        if (inventory != null) inventory.OnChanged -= Refresh;
    }

    public void Refresh()
{
    if (inventory == null || slots == null) return;

    for (int i = 0; i < slots.Length; i++)
    {
        if (slots[i] == null || inventory.slots[i] == null) continue;

        // Eğer envanterde eşya yoksa slotu gizle
        if (inventory.slots[i].data == null)
        {
            slots[i].gameObject.SetActive(false);
        }
        else
        {
            slots[i].gameObject.SetActive(true);
            slots[i].Render(inventory.slots[i]);
        }
    }
}


    public void MoveOrMerge(int fromIndex, int toIndex)
{
    Inventory.Instance.TryMoveOrMerge(fromIndex, toIndex);
    Refresh();
}

}
