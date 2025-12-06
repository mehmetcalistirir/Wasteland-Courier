using UnityEngine;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
    private EnemyCommanderCore core;
    private Rigidbody2D rb;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;              // Player ile aynı
    public float straightStepSize = 1f;
    public float diagonalStepSize = 1.4f;
    public float stepCooldown = 0.25f;

    private bool canMove = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Setup(EnemyCommanderCore c)
    {
        core = c;
    }

    public void Tick()
    {
        if (core == null || core.enemyKing == null)
            return;

        MoveTowardTarget();
    }

    // ------------------------------
    // AI hedefi doğrultusunda hareket
    // ------------------------------
    void MoveTowardTarget()
{
    BaseController target = core.ai.currentTargetVillage;
    if (target == null || !canMove)
        return;

    Vector2 diff = target.transform.position - core.enemyKing.position;

    // ❗ 1) Hedefe tamamen ulaştı → köy etkileşimi
    if (diff.magnitude < 0.5f)
    {
        core.ai.OnReachVillage(target);

        // hedefi temizle
        core.ai.currentTargetVillage = null;

        // ✔ AI tekrar hedef seçsin
        core.ai.Think();
        return;
    }

    // ❗ 2) Eğer diff çok küçükse (0,0) → yön bulunamıyor → yeni hedef seç
    if (diff.magnitude < 0.1f)
    {
        core.ai.currentTargetVillage = core.ai.FindNextTarget();
        return;
    }

    // ❗ 3) Normalize edilmiş yön 0,0 dönüyorsa da hedef değiştir
    Vector2 dir = NormalizeDirection(diff);
    if (dir == Vector2.zero)
    {
        core.ai.currentTargetVillage = core.ai.FindNextTarget();
        return;
    }

    // ❗ 4) Normal hareket
    StartCoroutine(MoveOneStep(dir));
}


    // ------------------------------
    // Grid tabanlı 1 adım hareket
    // (PlayerMovement2D ile aynı mantık)
    // ------------------------------
    IEnumerator MoveOneStep(Vector2 direction)
    {
        canMove = false;

        float step = (direction.x != 0 && direction.y != 0)
            ? diagonalStepSize
            : straightStepSize;

        Vector2 startPos = rb.position;
        Vector2 targetPos = startPos + direction * step;

        float t = 0f;
        float duration = step / moveSpeed;

        while (t < duration)
        {
            rb.MovePosition(Vector2.Lerp(startPos, targetPos, t / duration));
            t += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(targetPos);

        yield return new WaitForSeconds(stepCooldown);

        canMove = true;
    }

    // ------------------------------
    // PlayerMovement2D ile aynı normalize sistemi
    // ------------------------------
    Vector2 NormalizeDirection(Vector2 input)
    {
        float x = Mathf.Sign(input.x);
        float y = Mathf.Sign(input.y);

        if (Mathf.Abs(input.x) < 0.4f) x = 0;
        if (Mathf.Abs(input.y) < 0.4f) y = 0;

        return new Vector2(x, y).normalized;
    }
}
