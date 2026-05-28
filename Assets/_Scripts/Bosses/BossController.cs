using UnityEngine;
public class BossController : EnemyBase
{
    public override void Spawned()
    {
        base.Spawned();
        if (GameSession.Instance != null)
        {
            GameSession.Instance.RegisterEnemy((int)(Object.Id.Raw));
            Debug.LogWarning($"[BOSS INITIATIVE] Босс {gameObject.name} успешно занял слот в очереди ходов!");
        }
        if (GameSession.Instance == null) {
            Debug.LogError("[BOSS INITIATIVE] Экземпляр GameSession не найден!");
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || IsDead) return;

        int myBossID = (int)Object.Id.Raw;
        if (GameSession.Instance != null && GameSession.Instance.CurrentTurnID != myBossID)
        {
            if (CurrentState != EnemyState.Chase) CurrentState = EnemyState.Chase;
            return;
        }

        FindClosestPlayer();

        base.FixedUpdateNetwork();

        if (CurrentState == EnemyState.Attack)
        {
            FinishBossTurn();
        }
    }

    private void FinishBossTurn()
    {
        // Возвращаем стейт босса в покой
        CurrentState = EnemyState.Chase;

        // Передаем ход обратно игрокам, вызывая метод GameSession
        if (GameSession.Instance != null)
        {
            GameSession.Instance.RPC_RequestEndTurn();
            Debug.LogWarning("[BOSS SYSTEM] Босс успешно атаковал игрока и вернул ход в GameSession!");
        }
    }
}
