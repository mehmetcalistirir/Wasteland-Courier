using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuildSpot : MonoBehaviour
{
    [Header("Taret Seviyeleri")]
    public TurretLevelData[] levels;

    [Header("İnşa UI")]
    public float buildTime = 2f;
    public Slider progressBar;
    public GameObject progressCanvas;

    private bool isPlayerNearby = false;
    private float holdTimer = 0f;
    private bool isBuilding = false;

    private int currentLevel = 0;
    private GameObject currentTurret;

    private PlayerStats playerStats;

    private void Start()
    {
        if (progressCanvas != null)
            progressCanvas.SetActive(false);

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            playerStats = player.GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (!isPlayerNearby || playerStats == null) return;

        if (Keyboard.current.eKey.isPressed)
        {
            if (currentLevel >= levels.Length)
            {
                Debug.Log("⚠️ Zaten maksimum seviyede.");
                return;
            }

            if (!HasEnoughResources())
            {
                Debug.Log("🚫 Yetersiz kaynak!");
                return;
            }

            if (!isBuilding)
            {
                isBuilding = true;
                if (progressCanvas != null)
                    progressCanvas.SetActive(true);
            }

            holdTimer += Time.deltaTime;
            if (progressBar != null)
                progressBar.value = holdTimer / buildTime;

            if (holdTimer >= buildTime)
                BuildOrUpgradeTurret();
        }
        else if (isBuilding)
        {
            ResetBuild();
        }
    }

    void BuildOrUpgradeTurret()
{
    if (currentLevel >= levels.Length) return;
    TurretLevelData levelData = levels[currentLevel];

    // Kaynak düşür
    if (!Inventory.Instance.TryConsume(levelData.stoneSO, levelData.requiredStone) ||
        !Inventory.Instance.TryConsume(levelData.woodSO, levelData.requiredWood))
    {
        Debug.Log("🚫 Kaynak eksik!");
        return;
    }

    if (currentTurret != null)
        Destroy(currentTurret);

    Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y, -1f);
    currentTurret = Instantiate(levelData.prefab, spawnPos, Quaternion.identity);
    currentLevel++;

    ResetBuild();
    Debug.Log($"✅ Kule seviyesi {currentLevel} oldu!");
}


    void ResetBuild()
    {
        isBuilding = false;
        holdTimer = 0f;

        if (progressBar != null)
            progressBar.value = 0f;

        if (progressCanvas != null)
            progressCanvas.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            isPlayerNearby = true;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            ResetBuild();
        }
    }

    bool HasEnoughResources()
{
    if (currentLevel >= levels.Length || playerStats == null) return false;

    TurretLevelData levelData = levels[currentLevel];

    bool hasResources =
        Inventory.Instance.HasEnough(levelData.stoneSO, levelData.requiredStone) &&
        Inventory.Instance.HasEnough(levelData.woodSO, levelData.requiredWood);

    // Eğer blueprint sistemi Inventory’ye bağlandıysa:
    bool hasBlueprint = string.IsNullOrEmpty(levelData.requiredBlueprintId)
        || Inventory.Instance.HasBlueprint(levelData.requiredBlueprintId);

    if (!hasBlueprint)
        Debug.Log($"🚫 Gerekli taslak yok: {levelData.requiredBlueprintId}");

    return hasResources && hasBlueprint;
}


}
