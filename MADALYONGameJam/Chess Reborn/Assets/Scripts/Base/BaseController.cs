using UnityEngine;
using TMPro;
using System.Collections;

public enum Team { Neutral, Player, Enemy }

public class BaseController : MonoBehaviour
{
    public TextMeshPro countText;
    public Team owner = Team.Neutral;
    public int unitCount = 0;
    public int maxUnits = 20;
    public float productionRate = 0.25f; // saniyede üretilen oran

    private float timer = 0f;
    public bool isCastle = false;
    [Header("Capture Shake Settings")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.1f;


    void Update()
    {
        if (owner != Team.Neutral && unitCount < maxUnits)
        {
            timer += Time.deltaTime;
            if (timer >= 1f / productionRate)
            {
                unitCount++;
                timer = 0f;
            }
        }
    }
    void LateUpdate()
    {
        if (countText != null)
        {
            countText.text = unitCount.ToString();
        }
    }


    // 1v1 kayıp sistemi
    public void ReceiveAttack(int count, Team team)
{
    unitCount -= count;

    // Köy savunması bittiyse takım değiştir
    if (unitCount <= 0)
    {
        owner = team;
        unitCount = 0;

        StartCoroutine(Shake());
        if (team == Team.Player)
            TaskManager.instance.CheckBaseCapture(this);

        if (isCastle)
            GameMode.Instance.CheckCastleWinLose(this);
    }
}


    public void ResolveBattle(int attackerCount, Team attackerTeam)
    {
        // SAVUNMA GÜCÜ = gerçek piyon sayısı
        int defenderCount = unitCount;

        // 1) Karşılıklı öldürme
        int kill = Mathf.Min(attackerCount, defenderCount);

        int attackerRemaining = attackerCount - kill;
        int defenderRemaining = defenderCount - kill;

        // ---- Savunmacı kaybı ----
        unitCount = defenderRemaining;

        // BPM varsa görsel piyonları da azalt
        BasePiyonManager bpm = GetComponent<BasePiyonManager>();
        if (bpm != null)
            bpm.SyncTo(unitCount);
        if (attackerRemaining > 0)
        {
            owner = attackerTeam;
            unitCount = attackerRemaining;

            StartCoroutine(Shake());

            // 🔥 GÖREV KONTROLÜ
            if (attackerTeam == Team.Player)
                TaskManager.instance.CheckBaseCapture(this);
        }
        if (attackerRemaining > 0)
        {
            owner = attackerTeam;

            // 🔥 Sadece attacker PLAYER ise görev kontrolü yapılır
            if (attackerTeam == Team.Player)
                TaskManager.instance.CheckBaseCapture(this);
        }



        // ---- Saldıran kazandı ----
        if (attackerRemaining > 0)
        {
            owner = attackerTeam;
            unitCount = attackerRemaining;
            StartCoroutine(Shake());


            if (bpm != null)
                bpm.SyncTo(unitCount);
        }
        // --- KALE ELE GEÇİRME KONTROLÜ ---
        if (isCastle)
        {
            GameMode.Instance.CheckCastleWinLose(this);
        }

    }

    private void HandleKingBattle(int attackerCount, Team attackerTeam)
{
    int defenderCount = unitCount;

    // 1v1 öldürme
    int kill = Mathf.Min(attackerCount, defenderCount);

    int attackerRemaining = attackerCount - kill;
    int defenderRemaining = defenderCount - kill;

    // Köy savunma kaybı
    unitCount = defenderRemaining;

    // King ordusu piyonlarını yok et
    if (attackerTeam == Team.Player)
        PlayerCommander.instance.playerArmy.RemovePiyons(kill);
    else
        EnemyCommanderCore.instance.enemyArmy.RemovePiyons(kill);

    // --- Eğer saldıran kazandıysa ---
    if (attackerRemaining > 0)
    {
        owner = attackerTeam;
        unitCount = attackerRemaining;
        StartCoroutine(Shake());

        if (attackerTeam == Team.Player)
            TaskManager.instance.CheckBaseCapture(this);
    }

    // Kale kontrolü
    if (isCastle)
        GameMode.Instance.CheckCastleWinLose(this);
}





    // --- ELE GEÇİRME SİSTEMİ ---
private void OnTriggerEnter2D(Collider2D other)
{
    // -------------------------
    // PLAYER KING → Köye girdi
    // -------------------------
    if (other.CompareTag("PlayerKing"))
    {
        int attackerCount = PlayerCommander.instance.GetArmyCount();
        HandleKingBattle(attackerCount, Team.Player);
        return;
    }

    // -------------------------
    // ENEMY KING → Köye girdi
    // -------------------------
    if (other.CompareTag("EnemyKing"))
    {
        int attackerCount = EnemyCommanderCore.instance.enemyArmy.GetCount();
        HandleKingBattle(attackerCount, Team.Enemy);
        return;
    }
}


    IEnumerator Shake()
    {
        Vector3 originalPos = transform.localPosition;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }





}
