using System.Collections;
using UnityEngine;

public class EnemyAttackActionComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyUnit enemy;
    [SerializeField] private APComponent apComponent;
    [SerializeField] private EnemyMoveActionComponent movement;
    [SerializeField] private EnemyArcAimController2D aim;

    [Header("Weapon")]
    [SerializeField] private Projectile2D projectilePrefab;
    [SerializeField, Min(1)] private int baseDamage = 20;
    [SerializeField, Min(1)] private int fireRate = 1;
    [SerializeField, Min(1)] private int attackAPCost = 1;

    [Header("Arc Aim")]
    [SerializeField, Range(0f, 180f)] private float arcAngle = 45f;
    [SerializeField, Min(0f)] private float scanSpeed = 1f;
    [SerializeField] private Vector2 aimTimeRange = new Vector2(0.1f, 2f); 

    public bool ShotFired { get; private set; }

    public bool CanAttack()
    {
        if (enemy == null || apComponent == null || movement == null || aim == null || ActionDelay.IsWaiting) return false;
        TurnManager turns = enemy.TurnManager;
        return turns != null && turns.CurrentPhase == BattlePhase.EnemyTurn && turns.CurrentEnemy == enemy
            && !enemy.HasEndedTurn && !enemy.GetComponent<HealthComponent>().IsDead()
            && !movement.IsMoving && apComponent.HasEnoughAP(attackAPCost);
    }

    public IEnumerator TryRangedAttack(PlayerUnit target, float attackDistance)
    {
        ShotFired = false;
        if (!CanAttack() || target == null) yield break;
        if (!aim.StartAim(target, arcAngle, scanSpeed, aimTimeRange, attackDistance)) yield break;

        while (aim != null && aim.IsAiming) yield return null;
        if (this == null || aim == null) yield break;
        Vector2 direction = aim.CurrentScanDirection;
        aim.CancelAim();
        if (target == null || target.GetComponent<HealthComponent>().IsDead() || !CanAttack()) yield break;

        ShotFired = WeaponController.TryFire(projectilePrefab, enemy.gameObject, direction, baseDamage, fireRate, attackAPCost);
        // Wait even when spending the final AP has already ended this enemy's turn.
        while (ActionDelay.IsWaiting) yield return null;
    }
}
