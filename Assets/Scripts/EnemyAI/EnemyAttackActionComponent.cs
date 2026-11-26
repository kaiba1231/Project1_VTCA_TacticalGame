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
    [SerializeField] private bool useMeleeRaycast;
    [SerializeField] private EnemyUnitAnimator unitAnimator;
    [SerializeField, Min(1)] private int baseDamage = 20;
    [SerializeField, Min(1)] private int fireRate = 1;
    [SerializeField, Min(1)] private int attackAPCost = 1;

    [Header("Arc Aim")]
    [SerializeField, Range(0f, 180f)] private float arcAngle = 45f;
    [SerializeField, Min(0f)] private float scanSpeed = 1f;
    [SerializeField] private Vector2 aimTimeRange = new Vector2(0.1f, 2f); 

    public bool AttackPerformed { get; private set; }

    private void Awake()
    {
        if (unitAnimator == null) unitAnimator = GetComponent<EnemyUnitAnimator>();
    }

    public bool CanAttack()
    {
        if (enemy == null || apComponent == null || movement == null || aim == null || ActionDelay.IsWaiting) return false;
        TurnManager turns = enemy.TurnManager;
        return turns != null && turns.CurrentPhase == BattlePhase.EnemyTurn && turns.CurrentEnemy == enemy
            && !enemy.HasEndedTurn && !enemy.GetComponent<HealthComponent>().IsDead()
            && !movement.IsMoving && apComponent.HasEnoughAP(attackAPCost);
    }

    public IEnumerator TryAttack(PlayerUnit target, float attackDistance)
    {
        AttackPerformed = false;
        if (!CanAttack() || target == null) yield break;
        float reach = useMeleeRaycast ? attackDistance + 2f : attackDistance;
        if (!aim.StartAim(target, arcAngle, scanSpeed, aimTimeRange, reach)) yield break;

        while (aim != null && aim.IsAiming) yield return null;
        if (this == null || aim == null) yield break;
        Vector2 direction = aim.CurrentScanDirection;
        aim.CancelAim();
        if (target == null || target.GetComponent<HealthComponent>().IsDead() || !CanAttack()) yield break;

        AttackPerformed = useMeleeRaycast
            ? TryMeleeAttack(direction, reach)
            : WeaponController.TryFire(projectilePrefab, enemy.gameObject, direction, baseDamage, fireRate, attackAPCost);
        if (AttackPerformed && !useMeleeRaycast && unitAnimator != null)
            unitAnimator.PlayShoot(direction);

        // Wait even when spending the final AP has already ended this enemy's turn.
        while (ActionDelay.IsWaiting) yield return null;
    }

    private bool TryMeleeAttack(Vector2 direction, float reach)
    {
        if (!ActionDelay.TryStart(2f)) return false;
        if (!apComponent.TrySpendAP(attackAPCost))
        {
            ActionDelay.Cancel();
            return false;
        }

        if (unitAnimator != null) unitAnimator.PlayMeleeAttack(direction);
        int layers = LayerMask.GetMask("Default", "Player", "Wall");
        foreach (RaycastHit2D hit in Physics2D.RaycastAll(transform.position, direction, reach, layers))
        {
            if (hit.collider.CompareTag("Wall")) break;
            HealthComponent target = hit.collider.GetComponentInParent<HealthComponent>();
            if (target == null || !target.IsPlayer || target.IsDead()) continue;

            CombatResolver.ApplyDamageAndShowPopup(target, baseDamage);
            break;
        }
        return true;
    }
}
