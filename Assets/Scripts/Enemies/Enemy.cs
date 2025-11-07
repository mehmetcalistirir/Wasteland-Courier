using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using TMPro;

public class Enemy : MonoBehaviour
{

    [Header("Sun Burn (Daytime)")]
    public bool burnsInDay = true;
    public int burnDamagePerTick = 10;
    public float burnTickInterval = 1f;

    private Coroutine burnCo;
    private DayNightCycle dayNightCycle;


    public int maxHealth = 5;
    private int currentHealth;

    private bool isDaytimeCached = false;

    public TextMeshPro damageText;

    public GameObject hpBarPrefab; // Prefab atanacak

    public GameObject damagePopupPrefab;

    private Image hpFillImage; // STATIC KALDIRILDI
    private GameObject hpBarInstance;
    public GameObject goldPrefab; // Inspector'dan atanacak
    public GameObject[] blueprintPrefabs; // Farklı blueprint objeleri atanabilir
    public EnemyType enemyType = EnemyType.Normal;
    public float moveSpeed = 2f;
    public int damageToCaravan = 1;
    private Animator animator;
    private AudioSource audioSource; // YENİ

    [SerializeField] private GameObject ammoMachineGunPrefab;
    [SerializeField] private GameObject ammoPistolPrefab;
    [SerializeField] private GameObject ammoShotgunPrefab;
    [SerializeField] private GameObject ammoSniperPrefab;


    [Header("Audio")]
    [Tooltip("Düşman doğduğunda veya belirli aralıklarla çalınacak sesler.")]
    public List<AudioClip> ambientSounds;
    [Tooltip("Düşman hasar aldığında çalınacak sesler.")]
    public List<AudioClip> hurtSounds;
    [Tooltip("Düşman öldüğünde çalınacak ses.")]
    public AudioClip deathSound; // Genellikle tek bir ölüm sesi olur.

    [Tooltip("Rastgele ortam seslerinin çalınma aralığı (min ve max saniye).")]
    public Vector2 ambientSoundInterval = new Vector2(5f, 15f);

    [Header("Targets")]
    public Transform player;
    public Transform caravan; // Inspector’dan da atanabilir

    [Header("Armored Settings")]
    public float armoredDamageInterval = 1.0f;
    public int armoredCaravanDamage = 1;

    [Header("Exploder Settings")]
    public float explosionRadius = 2f;
    public int explosionDamageToPlayer = 2;
    public int explosionDamageToCaravan = 2;
    public LayerMask explosionHitMask;

    [Header("Knockback")]
    public bool canBeKnockedBack = true;
    public float knockbackRecoveryTime = 0.25f; // geri tepme sonrası hareketin toparlanma süresi

    private Coroutine knockbackCoroutine;

    [Header("Contact Damage (Normal/Fast -> Player)")]
    public float contactDamageInterval = 1.0f;
    public int damageToPlayer = 1;

    [Header("Control")]
    public bool externalMovement = false; // sade varsayılan: false

    [Header("Attack Range Settings")]
    public float damageRangeToPlayer = 5f;
    public float damageRangeToCaravan = 5f;




    private bool isDamagingPlayer = false;
    private Coroutine caravanDamageCo;
    private Coroutine playerDamageCo;

    private Vector2 lastMoveDir = Vector2.down; // başlangıç bakış yönü

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();                // <-- ekle
        if (!animator) Debug.LogError("[Enemy] Animator eksik!");
        if (!audioSource) Debug.LogError("[Enemy] AudioSource eksik!");

    }


    void OnDisable()
    {
        DayNightCycle.OnDayNightChanged -= HandleDayNightChanged;
    }



    private void HandleDayNightChanged(bool isDay)
    {
        if (this == null || !gameObject) return; // 💥 Koruma satırı

        isDaytimeCached = isDay;

        if (!burnsInDay) return;

        if (isDay)
        {
            if (burnCo == null)
                burnCo = StartCoroutine(SunBurnRoutine());
        }
        else
        {
            if (burnCo != null)
            {
                StopCoroutine(burnCo);
                burnCo = null;
            }
        }
    }


    public Transform target;

    void Start()
    {
        // Canı başlat
        currentHealth = maxHealth;

        if (hpBarPrefab != null)
        {
            hpBarInstance = Instantiate(hpBarPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
            hpBarInstance.transform.SetParent(transform); // düşmanı takip etsin
            Transform fill = hpBarInstance.transform.Find("Background/Fill");
            if (fill != null)
                hpFillImage = fill.GetComponent<Image>();
            else
                Debug.LogError("Fill Image bulunamadı! Prefab hiyerarşisini kontrol et.");

            hpFillImage.fillAmount = 1f; // başlangıçta dolu

            DayNightCycle.OnDayNightChanged += HandleDayNightChanged;

            // Eğer oyun başladığında gündüzse hemen yanma başlat
            if (burnsInDay && DayNightCycle.Instance != null && DayNightCycle.Instance.IsDay)
                HandleDayNightChanged(true);

        }


        // Hedefi türe göre ayarla
        if (enemyType == EnemyType.Normal || enemyType == EnemyType.Fast)
        {
            // Inspector'dan atanmadıysa tag ile bul
            if (player == null)
            {
                var go = GameObject.FindGameObjectWithTag("Player");
                if (go != null) player = go.transform;
            }
            target = player;
        }
        else if (enemyType == EnemyType.Armored)
        {
            if (caravan == null)
            {
                var go = GameObject.FindGameObjectWithTag("Caravan");
                if (go != null) caravan = go.transform;
            }
            target = caravan;
        }
        else if (enemyType == EnemyType.Exploder)
        {
            // İstersen player'ı kovalasın:
            if (caravan == null)
            {
                var go = GameObject.FindGameObjectWithTag("Caravan");
                if (go != null) caravan = go.transform;
            }
            target = caravan;
        }

        // (Opsiyonel) ortam sesleri için
        if (ambientSounds != null && ambientSounds.Count > 0 && audioSource != null)
            StartCoroutine(PlayAmbientSounds());

        // --- Mevcut Start içeriğin (OverlapCircleAll vb.) devamı ---
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                if (enemyType == EnemyType.Normal || enemyType == EnemyType.Fast)
                    StartPlayerDamage(hit.transform);
            }
            else if (hit.CompareTag("Caravan"))
            {
                if (enemyType == EnemyType.Armored)
                    StartCaravanDamage(hit.transform);
                else if (enemyType == EnemyType.Exploder)
                    Explode();
            }
        }

        GameObject cycleObj = GameObject.FindObjectOfType<DayNightCycle>()?.gameObject;
        if (cycleObj != null)
            dayNightCycle = cycleObj.GetComponent<DayNightCycle>();

        if (burnsInDay && dayNightCycle != null && IsDayTime())
        {
            burnCo = StartCoroutine(SunBurnRoutine());
        }

    }

    private bool IsDayTime()
    {
        return dayNightCycle != null &&
               dayNightCycle.GetType().GetField("isDay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(dayNightCycle).Equals(true);
    }

    private IEnumerator SunBurnRoutine()
    {
        while (true)
        {
            TakeDamage(burnDamagePerTick);

            if (currentHealth <= 0) yield break; // düşman öldüyse çık
            yield return new WaitForSeconds(burnTickInterval);
        }
    }


    void LateUpdate()
    {
        Vector3 scale = transform.localScale;
        scale.y = Mathf.Abs(scale.y); // her zaman pozitif olsun
        transform.localScale = scale;
    }


    public void ShowDamage(int amount)
    {
        damageText.text = $"-{amount}";
        damageText.color = Color.red;
        damageText.gameObject.SetActive(true);
        StartCoroutine(FadeOutText());
    }

    private System.Collections.IEnumerator FadeOutText()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Color c = damageText.color;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            damageText.color = Color.Lerp(Color.red, Color.white, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        damageText.gameObject.SetActive(false);
    }


    public void ApplyKnockback(Vector2 sourcePosition, float force, float duration)
    {
        if (!canBeKnockedBack) return;

        // Eğer zaten bir knockback coroutine çalışıyorsa tekrar başlatma
        if (knockbackCoroutine != null)
            return;

        Vector2 dir = ((Vector2)transform.position - sourcePosition).normalized;
        knockbackCoroutine = StartCoroutine(KnockbackRoutine(dir, force, duration));
    }

    private IEnumerator KnockbackRoutine(Vector2 direction, float force, float duration)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        float elapsed = 0f;

        if (rb != null)
        {
            bool wasKinematic = rb.isKinematic;
            if (wasKinematic) rb.isKinematic = false;

            // 🔹 Kuvvet uygula (Impulse)
            rb.AddForce(direction * force, ForceMode2D.Impulse);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // 🔹 Hareketi durdur
            rb.linearVelocity = Vector2.zero;

            if (wasKinematic) rb.isKinematic = true;
        }
        else
        {
            // Rigidbody yoksa transform hareketi uygula
            Vector2 startPos = transform.position;
            while (elapsed < duration)
            {
                transform.position = (Vector2)transform.position + direction * (force * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        // 🔹 Küçük bir toparlanma süresi
        yield return new WaitForSeconds(knockbackRecoveryTime);

        knockbackCoroutine = null;
    }


    // Enemy.cs içine
    public void OnSoundHeard(Vector2 soundPosition)
    {
        if (enemyType == EnemyType.Normal || enemyType == EnemyType.Fast)
        {
            // Düşman zaten hedefteyse ve çok yakınsa boşver
            if (target != null && Vector2.Distance(transform.position, target.position) < 1f) return;

            // Yeni hedef pozisyona yönel (örneğin geçici olarak bir boş hedef nokta olabilir)
            StartCoroutine(MoveToSoundPosition(soundPosition));
        }
    }

    private System.Collections.IEnumerator MoveToSoundPosition(Vector2 soundPosition)
    {
        float moveTime = 2f;
        float elapsed = 0f;

        while (elapsed < moveTime)
        {
            Vector2 dir = (soundPosition - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);

            // İstersen burada animasyonlar da ayarla
            animator.SetFloat("MoveX", dir.x);
            animator.SetFloat("MoveY", dir.y);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Hareket sonrası tekrar oyuncuya dön
        if (player != null)
            target = player;
    }


    void Update()
    {
        if (target == null)
        {
            if ((enemyType == EnemyType.Normal || enemyType == EnemyType.Fast) && player != null)
                target = player;
            else if ((enemyType == EnemyType.Armored || enemyType == EnemyType.Exploder) && caravan != null)
                target = caravan;
        }

        if (!externalMovement && target != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, target.position);
            float stopDistance = (enemyType == EnemyType.Normal || enemyType == EnemyType.Fast)
                ? damageRangeToPlayer
                : damageRangeToCaravan;

            Vector2 dir = (target.position - transform.position).normalized;

            if (distanceToTarget > stopDistance)
            {
                Debug.Log("Enemy moving - IsMoving TRUE");

                // 🧭 Hedefe doğru ilerle
                transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);

                // 🎬 Walk animasyonuna geç
                if (!animator.GetBool("IsMoving"))
                    animator.SetBool("IsMoving", true);

                // 🔁 Hedef yönüne dönme (360°)
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
            }
            else
            {
                Debug.Log("Enemy stopped - IsMoving FALSE");

                // 🛑 Idle animasyonuna geç
                if (animator.GetBool("IsMoving"))
                    animator.SetBool("IsMoving", false);
            }
            if (distanceToTarget <= stopDistance)
            {
                if (!animator.GetBool("IsMoving"))
                    animator.SetBool("IsMoving", false);

                // 🧠 Saldırı animasyonunu tetikle (örnek olarak)
                if (enemyType == EnemyType.Normal || enemyType == EnemyType.Fast)
                {
                    if (Time.frameCount % 120 == 0) // her ~1 saniyede bir
                    {
                        animator.SetTrigger("Attack");
                    }
                }
            }

        }


    }






    public void TakeDamage(int amount)
    {

        Debug.Log($"📦 Hasar popup prefab pozisyonu: {transform.position + Vector3.up * 1f}");



        animator.SetTrigger("Hurt");
        currentHealth -= amount;

        PlayRandomSound(hurtSounds);
        // Debug için
        Debug.Log($"Enemy took {amount} damage. Current health: {currentHealth}/{maxHealth}");

        if (hpFillImage != null)
        {
            float fillValue = Mathf.Clamp01((float)currentHealth / maxHealth);
            hpFillImage.fillAmount = fillValue;
            Debug.Log($"Fill amount set to: {fillValue}");
        }

        if (currentHealth <= 0)
        {
            if (enemyType == EnemyType.Exploder)
            {
                Explode();
                return;
            }

            animator.SetTrigger("Die");
            Debug.Log("Enemy should die now!");

            // ALTIN DÜŞÜRME %25 ŞANS
            if (goldPrefab != null && Random.value < 0.25f)
            {
                Instantiate(goldPrefab, transform.position, Quaternion.identity);
                Debug.Log("💰 Düşman altın bıraktı!");
            }

            if (damagePopupPrefab != null)
            {
                Debug.Log("✅ Prefab atandı, instantiate ediliyor.");
                GameObject popup = Instantiate(damagePopupPrefab, transform.position + Vector3.up * 1.0f, Quaternion.identity, transform);
                popup.GetComponent<DamagePopup>().Setup(amount);
            }
            else
            {
                Debug.LogWarning("⚠️ damagePopupPrefab atanmadı!");
            }


            if (blueprintPrefabs.Length > 0 && Random.value < 0.75f)
            {
                int index = Random.Range(0, blueprintPrefabs.Length);
                Instantiate(blueprintPrefabs[index], transform.position, Quaternion.identity);
                Debug.Log("📘 Düşman blueprint düşürdü!");
            }


            if (hpBarInstance != null)
                Destroy(hpBarInstance);
            Destroy(gameObject);
            Die();
        }

        DamagePopupManager.Instance.SpawnPopup(transform.position, amount);



    }

    /*void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (enemyType == EnemyType.Normal || enemyType == EnemyType.Fast)
            {
                StartPlayerDamage(collision.transform);
            }
        }
        else if (collision.CompareTag("Caravan"))
        {
            if (enemyType == EnemyType.Armored)
            {
                StartCaravanDamage(collision.transform);
            }
            else if (enemyType == EnemyType.Exploder)
            {
                Explode();
            }
        }
    }*/


    /* void OnTriggerEnter2D(Collider2D collision)
     {
         // Karavan ile temas
         if (collision.CompareTag("Caravan"))
         {
             if (enemyType == EnemyType.Armored)
             {
                 // Armored: ölme, karavana periyodik hasar vermeye başla
                 StartCaravanDamage(collision.transform);
                 return;
             }
             else if (enemyType == EnemyType.Exploder)
             {
                 // Exploder: patla ve alan hasarı ver
                 Explode();
                 return;
             }
             else
             {
                 // Normal/Fast karavana ulaşırsa istersen yok et veya görmezden gel
                 // Die(); // İSTEMİYORSAN yoruma al
             }
         }

         // Oyuncu ile temas
         if (collision.CompareTag("Player"))
         {
             if (enemyType == EnemyType.Exploder)
             {
                 Explode();
                 return;
             }
             else if (enemyType == EnemyType.Normal || enemyType == EnemyType.Fast)
             {
                 StartPlayerDamage(collision.transform);
             }
         }

     }*/

    /*void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Caravan") && isDamagingCaravan)
        {
            StopCaravanDamage();
        }
        if (collision.CompareTag("Player") && isDamagingPlayer)
        {
            StopPlayerDamage();
        }
    }
    */
    public void StartCaravanDamage(Transform caravanTransform)
    {
        if (!isDamagingCaravan)
            caravanDamageCo = StartCoroutine(DamageCaravanOverTime(caravanTransform));
    }

    public void StopCaravanDamage()
    {
        isDamagingCaravan = false;
        if (caravanDamageCo != null) StopCoroutine(caravanDamageCo);
        caravanDamageCo = null;
    }

    private System.Collections.IEnumerator DamageCaravanOverTime(Transform caravanTransform)
    {
        isDamagingCaravan = true;
        var ch = caravanTransform.GetComponent<CaravanHealth>();

        while (isDamagingCaravan && ch != null)
        {
            ch.TakeDamage(armoredCaravanDamage);
            yield return new WaitForSeconds(armoredDamageInterval);
        }

        isDamagingCaravan = false;
        caravanDamageCo = null;
    }

    public void StartPlayerDamage(Transform playerTransform)
    {
        if (!isDamagingPlayer)
            playerDamageCo = StartCoroutine(DamagePlayerOverTime(playerTransform));
    }

    public void StopPlayerDamage()
    {
        isDamagingPlayer = false;
        if (playerDamageCo != null) StopCoroutine(playerDamageCo);
        playerDamageCo = null;
    }

    private System.Collections.IEnumerator DamagePlayerOverTime(Transform playerTransform)
    {
        isDamagingPlayer = true;
        var ps = playerTransform.GetComponent<PlayerStats>();

        while (isDamagingPlayer && ps != null)
        {
            ps.TakeDamage(damageToPlayer);
            yield return new WaitForSeconds(contactDamageInterval);
        }

        isDamagingPlayer = false;
        playerDamageCo = null;
    }



    private bool isDamagingCaravan = false;




    public void Explode()
    {
        Debug.Log("💥 Exploder patladı!");

        var hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (var hit in hits)
        {
            if (hit == null) continue;

            if (hit.CompareTag("Player"))
            {
                var ps = hit.GetComponent<PlayerStats>();
                if (ps != null) ps.TakeDamage(explosionDamageToPlayer);
            }
            else if (hit.CompareTag("Caravan"))
            {
                var ch = hit.GetComponent<CaravanHealth>();
                if (ch != null) ch.TakeDamage(explosionDamageToCaravan);
            }
        }

        // 📢 Bu satırı mutlaka ekle:
        Die();
    }


    private void Die()
    {
        Debug.Log("💀 Die() çağrıldı!");

        // Bileşenleri devre dışı bırak
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        // Ölüm animasyonu ve sesi
        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        animator.Play("Die");

        // ALTIN bırakma (%25 şans)
        if (goldPrefab != null && Random.value < 0.25f)
            Instantiate(goldPrefab, transform.position, Quaternion.identity);

        // BLUEPRINT bırakma (%75 şans)
        if (blueprintPrefabs.Length > 0 && Random.value < 0.75f)
        {
            int index = Random.Range(0, blueprintPrefabs.Length);
            Instantiate(blueprintPrefabs[index], transform.position, Quaternion.identity);
        }

        // --- 🔫 AMMO bırakma ---
        float dropChance = 0.4f; // %40 olasılıkla mermi düşsün
        float roll = Random.value;
        Debug.Log($"🎲 Ammo drop roll: {roll}");

        if (roll < dropChance)
        {
            List<GameObject> possibleAmmo = new List<GameObject>();

            if (ammoMachineGunPrefab != null) possibleAmmo.Add(ammoMachineGunPrefab);
            if (ammoPistolPrefab != null) possibleAmmo.Add(ammoPistolPrefab);
            if (ammoShotgunPrefab != null) possibleAmmo.Add(ammoShotgunPrefab);
            if (ammoSniperPrefab != null) possibleAmmo.Add(ammoSniperPrefab);

            Debug.Log($"🎯 {possibleAmmo.Count} ammo türü listede.");

            if (possibleAmmo.Count > 0)
            {
                GameObject selectedAmmo = possibleAmmo[Random.Range(0, possibleAmmo.Count)];
                Instantiate(selectedAmmo, transform.position, Quaternion.identity);
                Debug.Log($"🔫 Rastgele mermi düştü: {selectedAmmo.name}");
            }
        }

        // HP bar'ı yok et
        if (hpBarInstance != null) Destroy(hpBarInstance, 2f);

        // Obje'yi yok et
        Destroy(gameObject);
    }




    // YENİ: Rastgele ortam sesi çalan Coroutine
    private System.Collections.IEnumerator PlayAmbientSounds()
    {
        while (true) // Sonsuz döngü
        {
            // Rastgele bir süre bekle
            yield return new WaitForSeconds(Random.Range(ambientSoundInterval.x, ambientSoundInterval.y));

            // Rastgele bir ortam sesi çal
            PlayRandomSound(ambientSounds);
        }
    }

    // YENİ: Verilen listeden rastgele bir ses çalan yardımcı fonksiyon
    private void PlayRandomSound(List<AudioClip> clips)
    {
        if (clips != null && clips.Count > 0 && audioSource != null)
        {
            int index = Random.Range(0, clips.Count);
            audioSource.PlayOneShot(clips[index]);
        }
    }

}