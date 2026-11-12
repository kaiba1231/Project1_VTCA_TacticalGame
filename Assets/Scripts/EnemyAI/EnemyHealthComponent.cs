using UnityEngine;

// Attach to the root GameObject of an Enemy Unit (including Boss).
public class EnemyHealthComponent : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField, Min(1)] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    public GameManager GameManager => gameManager;
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public float HealthPercent => (float)currentHealth / maxHealth;
    public bool IsDead() => currentHealth <= 0;


    private void Awake()
    {
        ResetHealth();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }

    public int TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead()) return 0;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        if (IsDead()) gameManager.CheckBattleResult();
        return amount;
    }
}
