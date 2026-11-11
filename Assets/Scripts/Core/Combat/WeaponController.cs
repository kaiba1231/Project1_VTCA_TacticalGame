using System.Collections;
using UnityEngine;

// Weapon settings are configured per player on AttackActionComponent.
public static class WeaponController
{
    private const float ShotInterval = 0.15f;

    public static bool TryFire(Projectile2D prefab, PlayerUnit player, Vector2 direction, int damage, int fireRate, int attackAPCost)
    {
        if (player == null || direction.sqrMagnitude < 0.001f || damage <= 0 || fireRate <= 0 || attackAPCost <= 0)
            return false;

        if (prefab == null || prefab.ProjectileSpeed <= 0f)
        {
            Debug.LogWarning("[SHOOT] Projectile prefab is missing or its speed is invalid.");
            return false;
        }

        TurnManager turns = player.TurnManager;
        float delay = Mathf.Max(2f, (fireRate - 1) * ShotInterval + 0.01f);
        if (turns == null || turns.CurrentPhase != BattlePhase.PlayerTurn || turns.SelectedPlayer != player || !player.IsActiveUnit || !player.CanAct) return false;
        APComponent ap = player.GetComponent<APComponent>();
        if (ap == null || !ActionDelay.TryStart(delay)) return false;

        if (!ap.TrySpendAP(attackAPCost))
        {
            ActionDelay.Cancel();
            return false;
        }

        turns.StartCoroutine(FireBurst(prefab, player.transform.position, direction, damage, player.transform, fireRate));
        return true;
    }

    private static IEnumerator FireBurst(Projectile2D prefab, Vector3 origin, Vector2 direction, int damage, Transform owner, int fireRate)
    {
        for (int i = 0; i < fireRate; i++)
        {
            Projectile2D projectile = Object.Instantiate(prefab, origin, Quaternion.identity);
            projectile.Initialize(direction, damage, owner);
            if (i < fireRate - 1) yield return new WaitForSeconds(ShotInterval);
        }
    }
}
