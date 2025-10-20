using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CampfireCooking : MonoBehaviour
{
    [Header("Cooking")]
    public float cookTime = 5f;                 // Basılı tutma süresi
    public GameObject cookedMeatPrefab;         // Düşecek pickup
    public Transform dropPoint;                 // Opsiyonel, yoksa otomatik offset

    // string yerine ItemData kullan
    [SerializeField] private ItemData meatSO;
    [SerializeField] private ItemData cookedMeatSO;


    [Header("UI")]
    public GameObject progressCanvas;           // Panel (aktif/pasif yapılacak)
    public Slider progressBar;                  // 0..1

    private bool isPlayerNearby = false;
    private bool isCooking = false;
    private float holdTimer = 0f;

    private PlayerStats playerStats;

    void Start()
    {
        var player = GameObject.FindWithTag("Player");
        if (player) playerStats = player.GetComponent<PlayerStats>();

        if (progressCanvas) progressCanvas.SetActive(false);
        if (progressBar) progressBar.value = 0f;
    }

    void Update()
    {
        if (!isPlayerNearby || playerStats == null) return;

        bool hasMeat = playerStats.GetResourceAmount(meatSO) > 0;

        // C'ye basılı tutuluyorsa ve et varsa pişirmeyi başlat/sürdür
        if (hasMeat && Keyboard.current.cKey.isPressed)
        {
            if (!isCooking)
            {
                isCooking = true;
                if (progressCanvas) progressCanvas.SetActive(true);
            }

            holdTimer += Time.deltaTime;
            if (progressBar) progressBar.value = holdTimer / cookTime;

            if (holdTimer >= cookTime)
            {
                FinishCooking();
                ResetCooking();
            }
        }
        else
        {
            // Tuş bırakıldıysa veya et yoksa ilerlemeyi sıfırla
            if (isCooking) ResetCooking();
        }
    }

    private void FinishCooking()
    {
        // Envanterden 1 Meat düş; (RemoveResource bool döndürmüyorsa önce kontrol ettik zaten)
        if (playerStats.GetResourceAmount(meatSO) > 0)
            playerStats.RemoveResource(meatSO, 1);
        else
            return;

        // Pişmiş et prefab'ını düşür
        Vector3 pos = dropPoint ? dropPoint.position
                                : transform.position + new Vector3(0.4f, 0f, 0f);

        if (cookedMeatPrefab)
            Instantiate(cookedMeatPrefab, pos, Quaternion.identity);
        else
            Debug.LogWarning("CookedMeat prefab atanmadı!");
    }

    private void ResetCooking()
    {
        isCooking = false;
        holdTimer = 0f;
        if (progressBar) progressBar.value = 0f;
        if (progressCanvas) progressCanvas.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerNearby = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            ResetCooking();
        }
    }
}
