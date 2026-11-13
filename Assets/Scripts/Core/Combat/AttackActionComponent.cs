using UnityEngine;

// Gan script nay len root Player, cung PlayerUnit.
// Kiem tra tan cong va quan ly aim cua unit.
[RequireComponent(typeof(PlayerUnit))]
[DisallowMultipleComponent]
public class AttackActionComponent : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int attackAPCost = 1;
    [SerializeField] private int meleeDamage = 1; // Chua su dung trong v2.
    [SerializeField] private float meleeRange = 1f; // Chua su dung trong v2.

    [Header("Ranged Weapon")]
    [SerializeField] private Projectile2D projectilePrefab;
    [SerializeField, Min(1)] private int baseDamage = 20;
    [SerializeField, Min(1)] private int fireRate = 1;

    [Header("Arc Settings")]
    [SerializeField] private Sprite arcSprite;
    [SerializeField] private float arcRadius = 6f;
    [SerializeField] private float arcAngle = 180f;
    [SerializeField] private float scanSpeed = 1f;
    [SerializeField] private float arcScale = 1f;


    private PlayerUnit player;

    private void Awake()
    {
        player = GetComponent<PlayerUnit>();
    }

    public bool CanAttack()
    {
        if (player == null || ActionDelay.IsWaiting || player.CurrentAP < attackAPCost || player.IsActiveUnit == false || player.CanAct == false)
            return false;

        MoveActionComponent move = GetComponent<MoveActionComponent>();
        if (move != null && move.IsMoving)
            return false;

        return true;
    }

    public void TryMeleeAttack()
    {
        // TODO: Bo sung melee attack va damage sau.
    }

    public bool TryRangedAttack()
    {
        if (!player.CanAct) return false;

        ArcAimController2D currentAim = GetComponent<ArcAimController2D>();
        if (currentAim != null)
        {
            currentAim.CancelAim();
            return true;
        }

        if (!CanAttack())
            return false;

        if (arcSprite == null)
        {
            Debug.LogWarning("[SHOOT] Arc sprite is not assigned.", this);
            return false;
        }

        ArcAimController2D aim = gameObject.AddComponent<ArcAimController2D>();
        aim.Initialize(arcSprite, arcRadius, arcAngle, scanSpeed, arcScale);
        return true;
    }

    public bool ConfirmRangedAttack(Vector2 direction)
    {
        if (!CanAttack()) return false;
        return WeaponController.TryFire(projectilePrefab, player, direction, baseDamage, fireRate, attackAPCost);
    }

    public int GetAttackAPCost()
    {
        return attackAPCost;
    }
}
