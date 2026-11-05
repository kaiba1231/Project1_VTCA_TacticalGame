using UnityEngine;

// Attach to the root GameObject of a Player Unit.
public class PlayerHealthComponent : MonoBehaviour
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

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        Debug.Log("[HP] Player " + name + ": " + currentHealth + "/" + maxHealth);
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Debug.Log("[HP] Player " + name + ": " + currentHealth + "/" + maxHealth);
    }

    [ContextMenu("DEBUG: Take 20 Damage")]
    private void DebugTakeDamage()
    {
        if (Application.isPlaying) TakeDamage(20);
    }

    [ContextMenu("DEBUG: Heal 20 HP")]
    private void DebugHeal()
    {
        if (Application.isPlaying) Heal(20);
    }
}
