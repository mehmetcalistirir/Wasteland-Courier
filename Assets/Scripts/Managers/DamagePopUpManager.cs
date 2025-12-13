using UnityEngine;
using TMPro;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance;

    public GameObject popupPrefab;
    public RectTransform popupParent; // DamagePopups
    public Camera mainCam;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (mainCam == null)
            mainCam = Camera.main;
    }

    public void SpawnPopup(Vector3 worldPos, int damage)
    {
        if (!popupPrefab || !popupParent) return;

        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos + Vector3.up * 1f);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            popupParent,
            screenPos,
            mainCam,               // ⚠️ Screen Space - Overlay MANTIĞI
            out Vector2 localPos
        );

        GameObject popup = Instantiate(popupPrefab, popupParent);
        popup.GetComponent<RectTransform>().anchoredPosition = localPos;

        popup.GetComponent<DamagePopup>()?.Setup(damage);
    }
}
