public static class CombatResolver
{
    public static void ResolveProjectileHit(EnemyHealthComponent enemy, int damage)
    {
        if (enemy == null) return;

        int actualDamage = ApplyEnemyDamage(enemy, damage);
        if (actualDamage <= 0) return;

        DamagePopupUI popup = enemy.GetComponentInChildren<DamagePopupUI>(true);
        if (popup != null) popup.Show(actualDamage);
    }

    public static int ApplyEnemyDamage(EnemyHealthComponent enemy, int damage)
    {
        if (enemy == null) return 0;
        return enemy.TakeDamage(damage);
    }
}
