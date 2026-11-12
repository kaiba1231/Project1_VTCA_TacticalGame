using UnityEngine;

// Attach to the root GameObject of a Player Unit.
public class PlayerHealthComponent : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField, Min(1)] private int maxHealth = 100;
    private int currentHealth;

    public GameManager GameManager => gameManager;
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public float HealthPercent => (float)currentHealth / maxHealth;
    public bool IsDead() => currentHealth <= 0;


    private void Awake()
    {
        ResetMaxHealth();
    }

    public void ResetMaxHealth()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead()) return;
        currentHealth = Mathf.Max(0, currentHealth - amount);

        if (IsDead())
        {
            gameManager.CheckBattleResult();
        }
    }
    public void Heal(int amount)
    {
        if (amount <= 0) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }
}
