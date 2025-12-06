using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    private EnemyCommanderCore core;
    public float attackRange = 1.2f;

    public void Setup(EnemyCommanderCore c)
    {
        core = c;
    }

    public void Tick()
    {
        CheckFightWithPlayer();
    }

    void CheckFightWithPlayer()
    {
        float dist = Vector2.Distance(core.enemyKing.position, core.playerKing.position);

        if (dist < attackRange)
            StartKingBattle();
    }

    void StartKingBattle()
    {
        int enemyCount = core.enemyArmy.GetCount();
        int playerCount = PlayerCommander.instance.GetArmyCount();

        if (playerCount == 0 && enemyCount >= 2)
        {
            GameMode.Instance.LoseGame();
            return;
        }

        if (enemyCount == 0 && playerCount >= 2)
        {
            GameMode.Instance.WinGame();
            return;
        }

        int kill = Mathf.Min(enemyCount, playerCount);

        PlayerCommander.instance.playerArmy.RemovePiyons(kill);
        core.enemyArmy.RemovePiyons(kill);

        enemyCount = core.enemyArmy.GetCount();
        playerCount = PlayerCommander.instance.GetArmyCount();

        if (playerCount == 0 && enemyCount >= 2)
        {
            GameMode.Instance.LoseGame();
            return;
        }

        if (enemyCount == 0 && playerCount >= 2)
        {
            GameMode.Instance.WinGame();
            return;
        }
    }
}
