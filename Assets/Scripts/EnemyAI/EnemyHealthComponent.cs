using UnityEngine;

// Attach to the root GameObject of an Enemy Unit (including Boss).
public class EnemyHealthComponent : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public float HealthPercent => (float)currentHealth / maxHealth;

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

        int previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        Debug.Log("[HP] Enemy " + name + ": " + currentHealth + "/" + maxHealth);
        return previousHealth - currentHealth;
    }

    public bool IsDead() => currentHealth <= 0;

    [ContextMenu("DEBUG: Take 20 Damage")]
    private void DebugTakeDamage()
    {
        if (Application.isPlaying) TakeDamage(20);
    }
}
