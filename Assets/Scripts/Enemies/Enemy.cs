using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("Core")]
    public EnemyType enemyType = EnemyType.Normal;
    public float moveSpeed = 2f;
    public bool externalMovement = false;

    [Header("Targets")]
    public Transform player;
    public Transform caravan;
    public Transform target;

    [Header("Attack")]
    public int damageToPlayer = 1;
    public int armoredCaravanDamage = 1;
    public float armoredDamageInterval = 1.0f;
    public float damageRangeToPlayer = 1.2f;
    public float damageRangeToCaravan = 1.2f;

    [Header("Exploder")]
    public float explosionRadius = 2f;
    public int explosionDamageToPlayer = 2;
    public int explosionDamageToCaravan = 2;

    [Header("Knockback")]
    public bool canBeKnockedBack = true;
    public float knockbackRecoveryTime = 0.25f;

    [Header("Audio")]
    public List<AudioClip> ambientSounds;
    public List<AudioClip> hurtSounds;
    public AudioClip deathSound;
    public Vector2 ambientSoundInterval = new Vector2(5f, 15f);

    [Header("Caravan Attack Audio")]
public List<AudioClip> caravanClawAttackSounds;


    private Animator animator;
    private AudioSource audioSource;

    private Coroutine knockbackCoroutine;
    private Coroutine caravanDamageCo;

    private bool isDamagingCaravan = false;
    public bool isAttacking = false;

    // ========================
    // UNITY
    // ========================
    void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (!animator) Debug.LogError("[Enemy] Animator eksik!");
        if (!audioSource) Debug.LogError("[Enemy] AudioSource eksik!");
    }

    void Start()
    {
        ResolveTargets();

        if (ambientSounds != null && ambientSounds.Count > 0)
            StartCoroutine(PlayAmbientSounds());
        ResolveTargets();
    }

    void Update()
    {
         animator.SetBool("IsAttacking", isAttacking);
        if (target == null)
        {
            ResolveTargets();
            if (target == null) return; // hâlâ yoksa bekle
        }
        if (externalMovement || target == null) return;

        float distance = Vector2.Distance(transform.position, target.position);
        float stopDistance = (enemyType == EnemyType.Normal || enemyType == EnemyType.Fast)
            ? damageRangeToPlayer
            : damageRangeToCaravan;

        Vector2 dir = (target.position - transform.position).normalized;

        if (!isAttacking && distance > stopDistance)
        {
            transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);

            animator.SetBool("IsMoving", true);

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }
        else if (!isAttacking)
        {
            animator.SetBool("IsMoving", false);
            StartCoroutine(AttackPlayerRoutine());
        }
    }
    // ========================
// ANIMATION EVENT (CARAVAN ATTACK)
// ========================
public void DealCaravanDamageWithSound()
{
    // Sadece Armored enemy karavana vurur
    if (enemyType != EnemyType.Armored)
        return;

    if (target == null || !target.CompareTag("Caravan"))
        return;

    // 💥 HASAR
    CaravanHealth ch = target.GetComponent<CaravanHealth>();
    if (ch != null)
        ch.TakeDamage(armoredCaravanDamage);

    // 🔊 SES
    if (caravanClawAttackSounds != null && caravanClawAttackSounds.Count > 0)
    {
        int index = Random.Range(0, caravanClawAttackSounds.Count);
        audioSource.PlayOneShot(caravanClawAttackSounds[index]);
    }
    PlayCaravanClawAttackSound();
}
public void PlayCaravanClawAttackSound()
{
    if (caravanClawAttackSounds == null || caravanClawAttackSounds.Count == 0)
        return;

    int index = Random.Range(0, caravanClawAttackSounds.Count);
    audioSource.PlayOneShot(caravanClawAttackSounds[index]);
}




    // ========================
    // TARGET RESOLUTION
    // ========================
    private void ResolveTargets()
    {
        if (enemyType == EnemyType.Normal || enemyType == EnemyType.Fast)
        {
            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player")?.transform;

            target = player;
        }
        else
        {
            if (caravan == null)
                caravan = GameObject.FindGameObjectWithTag("Caravan")?.transform;

            target = caravan;
        }
    }


    // ========================
    // ATTACK
    // ========================
    public IEnumerator AttackPlayerRoutine()
    {
        if (isAttacking) yield break;

        isAttacking = true;
        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(1.5f);

        isAttacking = false;
    }

    // Animator Event
    public void DealDamageToPlayer()
    {
        if (enemyType == EnemyType.Normal || enemyType == EnemyType.Fast)
        {
            var ps = player?.GetComponent<PlayerStats>();
            ps?.TakeDamage(damageToPlayer);
        }
    }

    // ========================
    // CARAVAN DAMAGE
    // ========================
    public void StartCaravanDamage(Transform caravanTransform)
    {
        if (!isDamagingCaravan)
            caravanDamageCo = StartCoroutine(DamageCaravanOverTime(caravanTransform));
    }

    private IEnumerator DamageCaravanOverTime(Transform caravanTransform)
    {
        isDamagingCaravan = true;
        var ch = caravanTransform.GetComponent<CaravanHealth>();

        while (isDamagingCaravan && ch != null)
        {
            ch.TakeDamage(armoredCaravanDamage);
            yield return new WaitForSeconds(armoredDamageInterval);
        }

        isDamagingCaravan = false;
    }

    // ========================
    // KNOCKBACK
    // ========================
    public void ApplyKnockback(Vector2 sourcePosition, float force, float duration)
    {
        if (!canBeKnockedBack || knockbackCoroutine != null) return;

        Vector2 dir = ((Vector2)transform.position - sourcePosition).normalized;
        knockbackCoroutine = StartCoroutine(KnockbackRoutine(dir, force, duration));
    }

    private IEnumerator KnockbackRoutine(Vector2 direction, float force, float duration)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb)
            rb.AddForce(direction * force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(duration);

        if (rb)
            rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(knockbackRecoveryTime);
        knockbackCoroutine = null;
    }

    // ========================
    // EXPLODER
    // ========================
    public void Explode()
    {
        Debug.Log("💥 Exploder patladı!");

        var hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
                hit.GetComponent<PlayerStats>()?.TakeDamage(explosionDamageToPlayer);
            else if (hit.CompareTag("Caravan"))
                hit.GetComponent<CaravanHealth>()?.TakeDamage(explosionDamageToCaravan);
        }
    }

    // ========================
    // DEATH CALLBACK (EnemyHealth çağırır)
    // ========================
    public void OnDeath()
    {
        animator.SetTrigger("Die");
        GetComponent<Collider2D>().enabled = false;

        if (deathSound)
            AudioSource.PlayClipAtPoint(deathSound, transform.position);

        this.enabled = false;
    }

    // ========================
    // AUDIO
    // ========================
    private IEnumerator PlayAmbientSounds()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(
                ambientSoundInterval.x,
                ambientSoundInterval.y));

            PlayRandomSound(ambientSounds);
        }
    }

    private void PlayRandomSound(List<AudioClip> clips)
    {
        if (clips != null && clips.Count > 0 && audioSource != null)
        {
            int index = Random.Range(0, clips.Count);
            audioSource.PlayOneShot(clips[index]);
        }
    }
    public void TakeDamage(int amount)
    {
        EnemyHealth health = GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.TakeDamage(amount);
        }
        else
        {
            Debug.LogWarning("[Enemy] EnemyHealth bulunamadı!");
        }
    }

    public void OnSoundHeard(Vector2 soundPosition)
    {
        if (enemyType == EnemyType.Normal || enemyType == EnemyType.Fast)
        {
            if (target != null && Vector2.Distance(transform.position, target.position) < 1f)
                return;

            StartCoroutine(MoveToSoundPosition(soundPosition));
        }
    }

    private IEnumerator MoveToSoundPosition(Vector2 soundPosition)
    {
        float moveTime = 2f;
        float elapsed = 0f;

        while (elapsed < moveTime)
        {
            Vector2 dir = (soundPosition - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);

            animator.SetFloat("MoveX", dir.x);
            animator.SetFloat("MoveY", dir.y);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (player != null)
            target = player;
    }

    public void StopCaravanDamage()
    {
        isDamagingCaravan = false;

        if (caravanDamageCo != null)
            StopCoroutine(caravanDamageCo);

        caravanDamageCo = null;
    }

}
