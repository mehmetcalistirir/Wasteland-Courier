using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class CampfireInteraction : MonoBehaviour
{
    [SerializeField] private ItemData meatSO;
    [SerializeField] private ItemData cookedMeatSO;

    [Header("Cooking")]
    public GameObject cookedMeatPrefab;
    public Transform dropPoint;
    public float cookDuration = 5f;
    public AudioSource sfx;
    public AudioClip sizzleClip;

    [Header("UI")]
    public GameObject cookPromptPanel;
    public TextMeshProUGUI cookPromptText;
    public Slider cookProgress;

    private bool isPlayerNearby = false;
    private bool isHolding = false;
    private float holdTimer = 0f;

    void Start()
    {
        if (cookPromptPanel != null) cookPromptPanel.SetActive(false);
        if (cookProgress != null) cookProgress.value = 0f;
    }

    void Update()
    {
        if (!isPlayerNearby)
        {
            HidePrompt();
            return;
        }

        bool hasMeat = Inventory.Instance.GetTotalCount(meatSO) > 0;

        if (cookPromptPanel) cookPromptPanel.SetActive(hasMeat);
        if (cookPromptText) cookPromptText.text = hasMeat ? "C'ye basılı tut: Eti Pişir" : "Et yok";

        if (!hasMeat)
        {
            ResetHold();
            return;
        }

        if (Keyboard.current.cKey.isPressed)
        {
            isHolding = true;
            holdTimer += Time.deltaTime;

            if (cookProgress != null)
                cookProgress.value = holdTimer / cookDuration;

            if (holdTimer >= cookDuration)
            {
                CookAndDrop();
                ResetHold();
            }
        }
        else
        {
            if (isHolding)
                ResetHold();
        }
    }

    private void CookAndDrop()
    {
        if (!Inventory.Instance.TryConsume(meatSO, 1))
        {
            Debug.Log("🥩 Et yok, pişirme iptal.");
            return;
        }

        if (sfx && sizzleClip)
            sfx.PlayOneShot(sizzleClip);

        Vector3 pos = dropPoint
            ? dropPoint.position
            : transform.position + new Vector3(0.4f, 0, 0);

        Instantiate(cookedMeatPrefab, pos, Quaternion.identity);
    }

    private void ResetHold()
    {
        isHolding = false;
        holdTimer = 0f;

        if (cookProgress != null)
            cookProgress.value = 0f;
    }

    private void HidePrompt()
    {
        if (cookPromptPanel) cookPromptPanel.SetActive(false);
        ResetHold();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            HidePrompt();
        }
    }
}
