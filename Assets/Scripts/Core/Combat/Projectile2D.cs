using UnityEngine;

public class Projectile2D : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float projectileSpeed = 500f;
    public float ProjectileSpeed => projectileSpeed;

    private Vector2 direction;
    private int damage;
    private Transform owner;
    private bool hitCollided;

    public void Initialize(Vector2 aim, int value, Transform shooter)
    {
        direction = aim.normalized;
        damage = value;
        owner = shooter;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    private void Update()
    {
        if (hitCollided) return;
        transform.position += (Vector3)(direction * projectileSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hitCollided || (owner != null && (other.transform == owner || other.transform.IsChildOf(owner)))) return;

        bool wall = other.CompareTag("Wall");

        EnemyHealthComponent enemy = other.GetComponentInParent<EnemyHealthComponent>();

        if (!wall && enemy == null) return;

        hitCollided = true;

        if (!wall)
        {
            CombatResolver.ResolveProjectileHit(enemy, damage);
        }

        Destroy(gameObject);
    }

}
