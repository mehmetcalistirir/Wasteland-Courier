using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class CampfireInteraction : MonoBehaviour
{

    // string yerine ItemData kullan
    [SerializeField] private ItemData meatSO;
    [SerializeField] private ItemData cookedMeatSO;

    [Header("Cooking")]
    public GameObject cookedMeatPrefab;
    public Transform dropPoint;
    public float cookDuration = 5f;         // Basılı tutma süresi (sn)
    public AudioSource sfx;
    public AudioClip sizzleClip;

    [Header("UI (optional)")]
    public GameObject cookPromptPanel;      // küçük bir panel
    public TextMeshProUGUI cookPromptText;  // "C'ye basılı tut: Eti Pişir"
    public Slider cookProgress;             // 0..1 ilerleme çubuğu

    private bool isPlayerNearby = false;
    private bool isHoldingToCook = false;
    private float holdTimer = 0f;

    private PlayerStats playerStats;

    void Start()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null) playerStats = player.GetComponent<PlayerStats>();

        // UI başlangıç
        if (cookPromptPanel != null) cookPromptPanel.SetActive(false);
        if (cookProgress != null) cookProgress.value = 0f;
    }

    void Update()
    {
        if (!isPlayerNearby || playerStats == null)
        {
            HidePrompt();
            return;
        }

        bool hasMeat = playerStats.GetResourceAmount(meatSO) > 0;

        // Paneli göster/gizle
        if (cookPromptPanel != null)
            cookPromptPanel.SetActive(hasMeat);

        if (cookPromptText != null)
            cookPromptText.text = hasMeat ? "C'ye basılı tut: Eti Pişir" : "Et yok";

        if (!hasMeat)
        {
            ResetHold();
            return;
        }

        // C tuşu basılı mı?
        if (Keyboard.current.cKey.isPressed)
        {
            // Basılı tutma moduna geç
            isHoldingToCook = true;
            holdTimer += Time.deltaTime;

            if (cookProgress != null)
                cookProgress.value = Mathf.Clamp01(holdTimer / cookDuration);

            // Süre tamamlandıysa pişir ve düşür
            if (holdTimer >= cookDuration)
            {
                CookAndDrop();
                ResetHold(); // bir pişirme tamamlandıktan sonra sıfırla
            }
        }
        else
        {
            // Tuş bırakıldıysa sıfırla
            if (isHoldingToCook) ResetHold();
        }
    }

    private void CookAndDrop()
    {
        // Sesi oynat
        if (sfx != null && sizzleClip != null) sfx.PlayOneShot(sizzleClip);

        // Envanterden 1 Meat düş
        if (playerStats.RemoveResource(meatSO, 1))
        {
            // Pişmiş et prefab'ını düşür
            Vector3 pos = dropPoint != null ? dropPoint.position
                                            : (transform.position + new Vector3(0.4f, 0.0f, 0f));
            if (cookedMeatPrefab != null)
            {
                Instantiate(cookedMeatPrefab, pos, Quaternion.identity);
                Debug.Log("🍗 Pişmiş et hazır ve yere düştü!");
            }
            else
            {
                Debug.LogWarning("cookedMeatPrefab atanmamış!");
            }
        }
        else
        {
            Debug.Log("🥩 Et yok (pişirme iptal).");
        }
    }

    private void ResetHold()
    {
        isHoldingToCook = false;
        holdTimer = 0f;
        if (cookProgress != null) cookProgress.value = 0f;
    }

    private void HidePrompt()
    {
        if (cookPromptPanel != null) cookPromptPanel.SetActive(false);
        ResetHold();
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
            HidePrompt();
        }
    }
}
