using UnityEngine;
using System.Collections;

public class EnemySunBurn : MonoBehaviour
{
    public bool burnsInDay = true;
    public int burnDamagePerTick = 10;
    public float burnTickInterval = 1f;

    private Coroutine burnCo;
    private EnemyHealth health;

    void Awake()
    {
        health = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        DayNightCycle.OnDayNightChanged += OnDayChanged;
    }

    private void OnDisable()
    {
        DayNightCycle.OnDayNightChanged -= OnDayChanged;
    }

    private void OnDayChanged(bool isDay)
    {
        if (!burnsInDay) return;

        if (isDay)
            burnCo = StartCoroutine(Burn());
        else if (burnCo != null)
            StopCoroutine(burnCo);
    }

    IEnumerator Burn()
    {
        while (true)
        {
            health.TakeDamage(burnDamagePerTick);
            yield return new WaitForSeconds(burnTickInterval);
        }
    }
}
