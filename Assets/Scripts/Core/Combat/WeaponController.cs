using System.Collections;
using UnityEngine;

public static class WeaponController
{
    private const float ShotInterval = 0.15f;

    public static bool TryFire(Projectile2D prefab, GameObject shooter, Vector2 direction, int damage, int fireRate, int attackAPCost)
    {
        if (shooter == null) return false;
        PlayerUnit player = shooter.GetComponent<PlayerUnit>();
        EnemyUnit enemy = shooter.GetComponent<EnemyUnit>();
        TurnManager turns;
        bool firedByEnemy;

        if (player != null)
        {
            turns = player.TurnManager;
            if (turns == null || turns.CurrentPhase != BattlePhase.PlayerTurn || turns.SelectedPlayer != player || !player.IsActiveUnit || !player.CanAct) return false;
            firedByEnemy = false;
        }
        else if (enemy != null)
        {
            turns = enemy.TurnManager;
            if (turns == null || turns.CurrentPhase != BattlePhase.EnemyTurn || turns.CurrentEnemy != enemy || enemy.HasEndedTurn || enemy.GetComponent<HealthComponent>().IsDead()) return false;
            firedByEnemy = true;
        }
        else return false;

        APComponent ap = shooter.GetComponent<APComponent>();
        Transform owner = shooter.transform;
        if (direction.sqrMagnitude < 0.001f || damage <= 0 || fireRate <= 0 || attackAPCost <= 0 || ap == null) return false;
        if (prefab == null || prefab.ProjectileSpeed <= 0f)
        {
            Debug.LogWarning("[SHOOT] Projectile prefab is missing or its speed is invalid.");
            return false;
        }
        float delay = Mathf.Max(2f, (fireRate - 1) * ShotInterval + 0.01f);
        if (!ActionDelay.TryStart(delay)) return false;
        if (!ap.TrySpendAP(attackAPCost))
        {
            ActionDelay.Cancel();
            return false;
        }
        turns.StartCoroutine(FireBurst(prefab, owner.position, direction, damage, owner, fireRate, firedByEnemy));
        return true;
    }

    private static IEnumerator FireBurst(Projectile2D prefab, Vector3 origin, Vector2 direction, int damage, Transform owner, int fireRate, bool firedByEnemy)
    {
        for (int i = 0; i < fireRate; i++)
        {
            Projectile2D projectile = Object.Instantiate(prefab, origin, Quaternion.identity);
            projectile.Initialize(direction, damage, owner, firedByEnemy);
            if (i < fireRate - 1) yield return new WaitForSeconds(ShotInterval);
        }
    }
}
