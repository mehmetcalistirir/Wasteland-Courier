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
    public void ReceiveAttack(int count, Team team, Transform attackerKing)
{
    // 🔥 Saldıran kral köye yakın değilse SAVAŞ YOK
    if (Vector2.Distance(attackerKing.position, transform.position) > 0.8f)
        return;

    unitCount -= count;

    if (unitCount <= 0)
    {
        owner = team;
        unitCount = 0;
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

    void StartBattle(int attackerCount, Team attackerTeam)
    {
        BasePiyonManager bpm = GetComponent<BasePiyonManager>();

        int defenderCount = bpm != null ? bpm.GetPiyonCount() : unitCount;

        int kill = Mathf.Min(attackerCount, defenderCount);

        int attackerRemaining = attackerCount - kill;
        int defenderRemaining = defenderCount - kill;

        // Savunma kayıpları
        if (bpm != null)
            bpm.RemovePiyons(kill);
        unitCount = defenderRemaining;

        // Saldıran taraf kayıpları
        if (attackerTeam == Team.Player)
        {
            PlayerCommander.instance.playerArmy.RemovePiyons(kill);
        }
        else
        {
            EnemyCommanderCore.instance.enemyArmy.RemovePiyons(kill);
        }

        // Eğer saldıran kazandıysa köyü ele geçir
        if (attackerRemaining > 0)
        {
            owner = attackerTeam;

            // Kazanan taraf köye kalan ordusunu yerleştirir
            unitCount = attackerRemaining;

            if (bpm != null)
                bpm.SyncTo(attackerRemaining);
        }
    }




    // --- ELE GEÇİRME SİSTEMİ ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        // PLAYER KING tarafsız köye girerse → savaş yok → direkt ele geçir
        if (other.CompareTag("Player") && owner == Team.Neutral)
        {
            owner = Team.Player;
            StartCoroutine(Shake());

            // 🔥 Görev kontrolü (BURAYA DOĞRU!)
            TaskManager.instance.CheckBaseCapture(this);

            return;
        }


        // ENEMY KING tarafsız köye girerse → savaş yok → direkt ele geçir
        if (other.CompareTag("Enemy") && owner == Team.Neutral)
        {
            owner = Team.Enemy;
            StartCoroutine(Shake());
            return;
        }

        // PLAYER ORDUSU rakip köye girerse savaş
        if (other.CompareTag("Player") && owner == Team.Enemy)
        {
            int attackerCount = PlayerCommander.instance.playerArmy.GetCount();
            StartBattle(attackerCount, Team.Player);
            return;
        }

        // ENEMY ORDUSU rakip köye girerse savaş
        if (other.CompareTag("Enemy") && owner == Team.Player)
        {
            int attackerCount = EnemyCommanderCore.instance.enemyArmy.GetCount();
            StartBattle(attackerCount, Team.Enemy);
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
