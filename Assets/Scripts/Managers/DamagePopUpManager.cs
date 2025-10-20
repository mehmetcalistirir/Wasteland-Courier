using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance;

    public GameObject popupPrefab;
    public RectTransform popupParent; // MainCanvas
    public Camera mainCam;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (mainCam == null) mainCam = Camera.main;
    }

    public void SpawnPopup(Vector3 worldPosition, int damageAmount)
    {
        if (popupPrefab == null || popupParent == null)
        {
            Debug.LogError("❌ DamagePopupManager: Prefab veya parent eksik!");
            return;
        }

        // World position → screen position
        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPosition + Vector3.up * 1f);

        // Screen position → local UI position
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(popupParent, screenPos, mainCam, out localPos);

        GameObject popup = Instantiate(popupPrefab, popupParent);
        popup.GetComponent<RectTransform>().anchoredPosition = localPos;

        var damageScript = popup.GetComponent<DamagePopup>();
        if (damageScript != null)
            damageScript.Setup(damageAmount);
    }
}
