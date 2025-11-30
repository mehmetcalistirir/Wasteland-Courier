using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CampfireCooking : MonoBehaviour
{
    [Header("Cooking")]
    public float cookTime = 5f;
    public GameObject cookedMeatPrefab;
    public Transform dropPoint;

    [SerializeField] private ItemData meatSO;
    [SerializeField] private ItemData cookedMeatSO;

    [Header("UI")]
    public GameObject progressCanvas;
    public Slider progressBar;

    private bool isPlayerNearby = false;
    private bool isCooking = false;
    private float holdTimer = 0f;

    void Start()
    {
        if (progressCanvas) progressCanvas.SetActive(false);
        if (progressBar) progressBar.value = 0f;
    }

    void Update()
    {
        if (!isPlayerNearby) return;

        bool hasMeat = Inventory.Instance.GetTotalCount(meatSO) > 0;

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
                CookMeat();
                ResetCooking();
            }
        }
        else
        {
            if (isCooking)
                ResetCooking();
        }
    }

    private void CookMeat()
    {
        if (Inventory.Instance.TryConsume(meatSO, 1))
        {
            Vector3 pos = dropPoint ? dropPoint.position
                                    : transform.position + new Vector3(0.4f, 0f, 0f);

            Instantiate(cookedMeatPrefab, pos, Quaternion.identity);
        }
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
