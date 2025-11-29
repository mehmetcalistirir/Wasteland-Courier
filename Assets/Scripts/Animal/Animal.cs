using UnityEngine;
using UnityEngine.UI;


public class Animal : MonoBehaviour
{

    private Animator animator;
    private Vector2 lastMoveDir = Vector2.down; // idle yönü için


    [Header("Animation Direction")]
    public float dirDeadzone = 0.15f;   // çok küçük x/y titreşimlerini yok say
    public float axisHysteresis = 0.10f; // eksenler arası geçişte yapışkanlık (0.05-0.15 iyi)

    // Son seçilen animasyon yönünü tut (sağ/sol/yukarı/aşağı)
    private Vector2 lastAnimDir = Vector2.down;

    public string animalType = "Geyik";
    public int maxHealth = 5;
    public float moveSpeed = 2f;
    public float roamRadius = 5f;
    public float baseDetectionRadius = 4f;
    public float nightDetectionFactor = 0.5f;
    public float fleeSpeedMultiplier = 1.5f;

    public GameObject meatPrefab;
    public GameObject hidePrefab;
    public GameObject hpBarPrefab;

    private int currentHealth;
    private GameObject hpBarInstance;
    private Image hpFillImage;

    private Vector2 roamTarget;
    private AnimalState currentState = AnimalState.Roaming;

    private Transform threatTarget;
    private float detectionRadius;

    private float nightBehaviorTimer;
    private bool isNight;
    

    [Header("Flee Tuning")]
    public float minScareCooldown = 0.5f; // spam önlemek için
    private float lastScareTime = -999f;

    private Vector2? fleeFromPoint = null;   // tehdit noktası (silah sesi/mermi)
    private float tempFleeMultiplier = 1f;
    private Coroutine fleeTimerCo;

    void Start()
    {

        animator = GetComponent<Animator>();
        if (animator == null) Debug.LogError("[Animal] Animator bileşeni bulunamadı!");
        currentHealth = maxHealth;
        detectionRadius = baseDetectionRadius;
        PickNewRoamTarget();

        //deneme

        if (hpBarPrefab != null)
        {
            hpBarInstance = Instantiate(hpBarPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
            hpBarInstance.transform.SetParent(transform, true);

            Transform fill = hpBarInstance.transform.Find("Background/Fill");
            if (fill != null)
                hpFillImage = fill.GetComponent<Image>();
        }

        // Gecelik davranışı tetikleyici
        InvokeRepeating(nameof(UpdateNightBehavior), 0f, 10f);
    }

    void Update()
    {
        DetectThreats();

        switch (currentState)
        {
            case AnimalState.Roaming:
            case AnimalState.NightRoaming:
                Roam();
                break;

            case AnimalState.Fleeing:
                Flee();
                break;

            case AnimalState.Sleeping:
                // sadece uyurken idle'a zorla
                UpdateAnimator(Vector2.zero, 0f);
                break;
        }
    }

    void OnEnable()
    {
        AnimalSoundEmitter.OnSound += OnSoundHeard;
    }

    void OnDisable()
    {
        AnimalSoundEmitter.OnSound -= OnSoundHeard;
    }

    private void OnSoundHeard(SoundEvent s)
    {
        // Uzaksa veya uyuyor ve duyması istenmiyorsa (istersen burada Sleep'e istisna koyabilirsin)
        if (Vector2.Distance(transform.position, s.position) > s.radius) return;

        // Gece/gündüz fark etmeksizin panik
        Scare(s.position, s.fleeDuration, s.speedMultiplier);
    }

    private Vector2 GetAnimCardinalDir(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) return lastAnimDir;

        float ax = Mathf.Abs(dir.x);
        float ay = Mathf.Abs(dir.y);

        // eksen seçimini biraz yapışkan yap
        bool preferX = ax > ay + axisHysteresis;
        bool preferY = ay > ax + axisHysteresis;

        Vector2 target;
        if (preferX || (!preferY && lastAnimDir.y != 0))
            target = new Vector2(dir.x >= 0f ? 1f : -1f, 0f);
        else if (preferY || (!preferX && lastAnimDir.x != 0))
            target = new Vector2(0f, dir.y >= 0f ? 1f : -1f);
        else
            target = (ax >= ay) ? new Vector2(dir.x >= 0f ? 1f : -1f, 0f)
                                : new Vector2(0f, dir.y >= 0f ? 1f : -1f);

        if (Mathf.Abs(dir.x) < dirDeadzone && lastAnimDir.x != 0f) target = lastAnimDir;
        if (Mathf.Abs(dir.y) < dirDeadzone && lastAnimDir.y != 0f) target = lastAnimDir;

        return target;
    }

    public void Scare(Vector2 fromPosition, float duration, float speedMult)
    {
        if (Time.time - lastScareTime < minScareCooldown) return;
        lastScareTime = Time.time;

        fleeFromPoint = fromPosition;
        tempFleeMultiplier = Mathf.Max(speedMult, 1f);
        currentState = AnimalState.Fleeing;

        if (fleeTimerCo != null) StopCoroutine(fleeTimerCo);
        fleeTimerCo = StartCoroutine(StopFleeAfter(duration));
    }

    private System.Collections.IEnumerator StopFleeAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        fleeFromPoint = null;
        tempFleeMultiplier = 1f;

        // Panik sonrası rutinine dön
        if (isNight) EnterNightMode();
        else currentState = AnimalState.Roaming;

        PickNewRoamTarget();
        fleeTimerCo = null;
    }


    void DetectThreats()
    {
        if (currentState == AnimalState.Sleeping) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player") || hit.CompareTag("Enemy"))
            {
                currentState = AnimalState.Fleeing;
                threatTarget = hit.transform;
                return;
            }
        }

        if (currentState == AnimalState.Fleeing && threatTarget == null)
        {
            if (isNight)
                EnterNightMode(); // tekrar gece davranışına dön
            else
                currentState = AnimalState.Roaming;

            PickNewRoamTarget();
        }
    }

    void Roam()
    {
        float distance = Vector2.Distance(transform.position, roamTarget);
        if (distance < 0.5f)
        {
            PickNewRoamTarget();
        }

        Vector2 dir = (roamTarget - (Vector2)transform.position).normalized;
        MoveWithAnim(dir, moveSpeed);
    }

    void Flee()
    {
        // Öncelik: canlı tehdit -> tehdit hedefi (Player/Enemy)
        // Yoksa: en son duyulan/gelinen tehdit noktası
        Vector2 source;

        if (threatTarget != null)
        {
            source = threatTarget.position;
        }
        else if (fleeFromPoint.HasValue)
        {
            source = fleeFromPoint.Value;
        }
        else
        {
            // Güvenli geri dönüş
            if (isNight) { EnterNightMode(); } else { currentState = AnimalState.Roaming; }
            PickNewRoamTarget();
            UpdateAnimator(Vector2.zero, 0f); // güvenli
            return;
        }

        Vector2 dir = ((Vector2)transform.position - source).normalized;
        float speed = moveSpeed * fleeSpeedMultiplier * tempFleeMultiplier;
        MoveWithAnim(dir, speed);
    }

    private void MoveWithAnim(Vector2 dir, float speed)
    {
        if (dir.sqrMagnitude > 0.0001f)
            transform.Translate(dir * speed * Time.deltaTime);

        Vector2 animDir = GetAnimCardinalDir(dir);     // <-- kritik
        UpdateAnimator(animDir, speed);
    }

    private void UpdateAnimator(Vector2 animDir, float speed)
    {
        if (animator == null) return;

        bool isMovingNow = animDir.sqrMagnitude > 0.0001f && speed > 0.01f;
        animator.SetBool("isMoving", isMovingNow);

        if (isMovingNow)
        {
            animator.SetFloat("MoveX", animDir.x);  // -1,0,1
            animator.SetFloat("MoveY", animDir.y);  // -1,0,1
            lastMoveDir = animDir;
            lastAnimDir = animDir;
        }
        else
        {
            animator.SetFloat("MoveX", lastMoveDir.x);
            animator.SetFloat("MoveY", lastMoveDir.y);
        }
    }

    void PickNewRoamTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * roamRadius;
        roamTarget = (Vector2)transform.position + randomOffset;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (hpFillImage != null)
        {
            float fillValue = Mathf.Clamp01((float)currentHealth / maxHealth);
            hpFillImage.fillAmount = fillValue;
        }

        // ❗ Vurulduysa, saldırı yönünü bilmiyorsak bile rastgele uzağa kaç
        if (!fleeFromPoint.HasValue && threatTarget == null)
        {
            // küçük bir ofsetle bulunduğu yerden ters yöne hedef seç
            Vector2 away = (Vector2)transform.position + Random.insideUnitCircle.normalized * 2f;
            Scare(away, 3f, 2.0f);
        }

        if (currentHealth <= 0)
        {
            DropLoot();
            if (hpBarInstance != null) Destroy(hpBarInstance);
            Destroy(gameObject);
        }
    }

    void DropLoot()
    {
        Instantiate(meatPrefab, transform.position, Quaternion.identity);
        Instantiate(hidePrefab, transform.position + Vector3.right * 0.5f, Quaternion.identity);
    }

    public void SetNight(bool night)
    {
        isNight = night;
        detectionRadius = baseDetectionRadius * (isNight ? nightDetectionFactor : 1f);
        EnterNightMode();
    }

    void EnterNightMode()
    {
        if (!isNight) return;

        // %50 şansla uyur, %50 gezer
        if (Random.value < 0.5f)
            currentState = AnimalState.Sleeping;
        else
            currentState = AnimalState.NightRoaming;

        PickNewRoamTarget();
    }

    void UpdateNightBehavior()
    {
        if (isNight)
            EnterNightMode(); // her 10 saniyede yeniden davranış belirle
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
