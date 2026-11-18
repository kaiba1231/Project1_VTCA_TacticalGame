using UnityEngine;

[DisallowMultipleComponent]
public class HealthComponent : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private bool isPlayer;
    [SerializeField, Min(1)] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    private UnitHitFeedback hitFeedback;

    public GameManager GameManager => gameManager;
    public bool IsPlayer => isPlayer;
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public float HealthPercent => (float)currentHealth / maxHealth;
    public bool IsDead() => currentHealth <= 0;

    private void Awake()
    {
        currentHealth = maxHealth;
        hitFeedback = GetComponent<UnitHitFeedback>();
    }

    // Restore khong gay damage/death callback trong luc dung lai tran.
    public void RestoreHealth(int value) => currentHealth = Mathf.Clamp(value, 0, maxHealth);

    public int TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead()) return 0;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        if (IsDead())
        {
            gameManager.CheckBattleResult();
            Destroy(gameObject);
        }
        else if (hitFeedback != null)
        {
            hitFeedback.Play();
        }
        return amount;
    }

    public void Heal(int amount)
    {
        if (!isPlayer || amount <= 0 || IsDead()) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }
}
