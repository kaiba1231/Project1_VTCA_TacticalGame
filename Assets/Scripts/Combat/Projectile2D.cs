using UnityEngine;

public class Projectile2D : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float projectileSpeed = 500f;
    [SerializeField, Min(0.1f)] private float enemyProjectileScale = 3f;
    public float ProjectileSpeed => projectileSpeed;

    private Vector2 direction;
    private Vector2 shotOrigin;
    private int damage;
    private Transform owner;
    private bool firedByEnemy;
    private bool hitCollided;

    public void Initialize(Vector2 aim, int value, Transform shooter, bool enemyShot)
    {
        direction = aim.normalized;
        shotOrigin = transform.position;
        damage = value;
        owner = shooter;
        firedByEnemy = enemyShot;
        if (firedByEnemy) transform.localScale *= enemyProjectileScale;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    private void Update()
    {
        if (!hitCollided) transform.position += (Vector3)(direction * projectileSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hitCollided || (owner != null && (other.transform == owner || other.transform.IsChildOf(owner)))) return;
        if (other.CompareTag("Wall"))
        {
            hitCollided = true;
            if (!firedByEnemy && owner != null)
            {
                DamagePopupUI popup = owner.GetComponentInChildren<DamagePopupUI>();
                if (popup != null) popup.ShowBlock();
            }
            Destroy(gameObject);
            return;
        }

        HealthComponent target = other.GetComponentInParent<HealthComponent>();
        if (target == null || target.IsDead() || target.IsPlayer != firedByEnemy) return;
        hitCollided = true;
        CombatResolver.ResolveProjectileHit(target, damage, shotOrigin, direction);
        Destroy(gameObject);
    }
}
