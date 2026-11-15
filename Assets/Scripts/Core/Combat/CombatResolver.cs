public static class CombatResolver
{
    public static void ResolveProjectileHit(HealthComponent target, int damage)
    {
        if (target == null) return;
        int popupDamage = ApplyDamage(target, damage);
        if (popupDamage <= 0) return;
        DamagePopupUI popup = target.GetComponentInChildren<DamagePopupUI>(true);
        if (popup != null) popup.Show(popupDamage);
    }

    public static int ApplyDamage(HealthComponent target, int damage)
    {
        if (target == null || target.IsDead() || damage <= 0) return 0;
        if (target.IsPlayer && target.GameManager.turnManager.CurrentPhase != BattlePhase.EnemyTurn) return 0;
        return target.TakeDamage(damage);
    }
}
