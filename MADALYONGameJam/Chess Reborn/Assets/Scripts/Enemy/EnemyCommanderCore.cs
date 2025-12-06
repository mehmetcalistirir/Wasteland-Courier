using UnityEngine;
using TMPro;

public class EnemyCommanderCore : MonoBehaviour
{
    public static EnemyCommanderCore instance;

    [Header("References")]
    public EnemyMovement movement;
    public EnemyCombat combat;
    public EnemyAI ai;

    [Header("Core Data")]
    public EnemyArmy enemyArmy;
    public Transform enemyKing;
    public Transform playerKing;

    public BaseController[] villages;
    public BaseController enemyCastle;
    public BaseController playerCastle;

    [Header("UI")]
    public TextMeshPro kingCountText;

    [Header("AI Tick Settings")]
    public float thinkInterval = 1f;  // AI her kaç saniyede bir düşünsün
    private float thinkTimer;
    private float aiTimer = 0f;
    public float ThinkInterval = 1f;

    void Awake()
    {
        instance = this;

        // Null guard
        if (movement != null) movement.Setup(this);
        else Debug.LogError("EnemyCommanderCore: movement referansı atanmadı!");

        if (combat != null) combat.Setup(this);
        else Debug.LogError("EnemyCommanderCore: combat referansı atanmadı!");

        if (ai != null) ai.Setup(this);
        else Debug.LogError("EnemyCommanderCore: ai referansı atanmadı!");
    }

    void Start()
    {
        enemyArmy.transform.SetParent(null); // bağımsız obje olsun
        enemyArmy.transform.position = enemyKing.position; // sadece pozisyon takibi


        thinkTimer = 0.5f;

        // EnemyArmy merkezini enemyKing'e bağla
        if (enemyArmy != null && enemyKing != null)
        {
            enemyArmy.transform.SetParent(null); // artık king pozisyonunu bozmaz

            enemyArmy.transform.localPosition = Vector3.zero;
        }
    }


    void Update()
    {
        if (ai == null) return;
        if (movement == null) return;

        // AI döngüsü
        aiTimer -= Time.deltaTime;
        if (aiTimer <= 0f)
        {
            aiTimer = ThinkInterval;
            ai.Think();
        }

        // Hareket & savaş
        movement.Tick();
        combat.Tick();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerKing"))  // Tag ismini sen nasıl verdiysen ona göre
        {
            GameMode.Instance.KingBattle();
        }
    }


}
