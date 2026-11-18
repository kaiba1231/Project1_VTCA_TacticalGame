using UnityEngine;

public static class CombatResolver
{
    public static void ResolveProjectileHit(HealthComponent target, int damage, Vector2 shotOrigin, Vector2 shotDirection)
    {
        if (target == null) return;
        int damageAfterCover = CalculateCoverDamage(target.transform.position, damage, shotOrigin, shotDirection);
        ApplyDamageAndShowPopup(target, damageAfterCover);
    }

    public static void ApplyDamageAndShowPopup(HealthComponent target, int damage)
    {
        int popupDamage = ApplyDamage(target, damage);
        if (popupDamage <= 0) return;
        DamagePopupUI popup = target.GetComponentInChildren<DamagePopupUI>(true);
        if (popup != null) popup.Show(popupDamage);
    }

    private static int CalculateCoverDamage(Vector2 targetPosition, int damage, Vector2 shotOrigin, Vector2 shotDirection)
    {
        const float maxCoverAngle = 30f; // Stop at the plane of the shot origin so cover behind the shooter does not count.
        float distanceFromShotOrigin = Vector2.Dot(targetPosition - shotOrigin, shotDirection);
        if (distanceFromShotOrigin <= 0f) return damage;

        RaycastHit2D hit = Physics2D.Raycast(targetPosition, -shotDirection, distanceFromShotOrigin, LayerMask.GetMask("Obstacle"));
        if (hit.collider == null || !hit.collider.TryGetComponent(out Obstacle2D cover)) return damage;
        if (hit.distance > cover.CoverDistance) return damage;
        if (Vector2.Angle(shotDirection, cover.Direction) > maxCoverAngle) return damage;

        return Mathf.RoundToInt(damage * (1f - cover.DamageReductionPercent / 100f)); 
    }

    public static int ApplyDamage(HealthComponent target, int damage)
    {
        if (target == null || target.IsDead() || damage <= 0) return 0;
        if (target.IsPlayer && target.GameManager.turnManager.CurrentPhase != BattlePhase.EnemyTurn) return 0;
        return target.TakeDamage(damage);
    }
}
