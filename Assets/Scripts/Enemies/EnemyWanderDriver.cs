using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyWanderDriver : MonoBehaviour
{
    public WanderArea area;                 // Hangi bölgede gezinecek?
    public float waypointTolerance = 0.15f; // Hedefe varmış sayılacağı mesafe
    public Vector2 idleTimeRange = new Vector2(0.2f, 1.0f); // Noktalar arasında kısa bekleme

    private Enemy enemy;
    private Animator anim;
    private Transform waypoint;     // DÜŞMANIN HEDEFİ
    private bool isIdling = false;
    private float idleTimer = 0f;

    void Awake()
    {
        enemy = GetComponent<Enemy>();
        anim  = GetComponent<Animator>();
    }

    void Start()
    {
        if (area == null)
        {
            Debug.LogWarning("[EnemyWanderDriver] Area atanmamış, yakın çevrede dolanacak.");
        }

        // Waypoint oluştur (area altında konumlamak düzen açısından iyidir)
        GameObject wp = new GameObject($"WanderWP_{name}");
        waypoint = wp.transform;
        waypoint.parent = area ? area.transform : null;

        // İlk rastgele nokta ve hedef ataması
        waypoint.position = GetNextPoint();
        enemy.target = waypoint; // 🔴 Kritik: Enemy artık bu noktaya yürüyecek
    }

    void Update()
    {
        if (enemy == null || waypoint == null) return;

        if (isIdling)
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
            {
                isIdling = false;
                waypoint.position = GetNextPoint();
                // enemy.target zaten waypoint, null olmaz -> Enemy oyuncuya dönmez
            }
            else
            {
                if (anim) anim.SetFloat("Speed", 0f);
            }
            return;
        }

        // Waypoint'e yeterince yaklaştıysak kısa bekle ve yeni nokta seç
        float distSqr = ((Vector2)(transform.position - waypoint.position)).sqrMagnitude;
        if (distSqr <= waypointTolerance * waypointTolerance)
        {
            isIdling = true;
            idleTimer = Random.Range(idleTimeRange.x, idleTimeRange.y);
        }
        // Hareketi Enemy.cs zaten yapıyor (target -> waypoint olduğu için)
    }

    public void Setup(WanderArea areaRef)
{
    if (enemy == null) enemy = GetComponent<Enemy>();
    area = areaRef;

    if (waypoint == null)
    {
        var wp = new GameObject($"WanderWP_{name}");
        waypoint = wp.transform;
        waypoint.parent = area ? area.transform : null;
    }

    waypoint.position = GetNextPoint();
    enemy.target = waypoint; // Hemen waypoint'e kilitlenir (oyuncuya dönmez)
}


    private Vector2 GetNextPoint()
    {
        if (area != null) return area.GetRandomPoint();
        // Area yoksa: bulunduğu nokta etrafında küçük daire
        return (Vector2)transform.position + Random.insideUnitCircle * 2f;
    }

    void OnDestroy()
    {
        if (waypoint != null) Destroy(waypoint.gameObject);
    }
}
