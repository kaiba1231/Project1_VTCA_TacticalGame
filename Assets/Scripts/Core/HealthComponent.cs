using UnityEngine;

// De san cho Milestone Combat. Chua ket noi den TurnManager, khong co Health UI.
public class HealthComponent : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log("[HEALTH] " + name + " HP: " + currentHealth + "/" + maxHealth);

        // Hien tai CHUA xu ly unit chet / Game Over.
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Debug.Log("[HEALTH] " + name + " HP: " + currentHealth + "/" + maxHealth);
    }
}
