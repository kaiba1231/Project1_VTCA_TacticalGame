using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to HealthBarCanvas, a CHILD of the unit's root GameObject.
public class UnitHealthBarUI : MonoBehaviour
{
    [SerializeField] private Image hpFill;
    [SerializeField] private TMP_Text HPText;

    private PlayerHealthComponent playerHealth;
    private EnemyHealthComponent enemyHealth;

    private void Awake()
    {
        playerHealth = GetComponentInParent<PlayerHealthComponent>();
        enemyHealth = GetComponentInParent<EnemyHealthComponent>();
    }

    private void Update()
    {
        float percent = 0f;
        int currentHP = 0;
        int maxHP = 0;

        if (playerHealth != null) // cho player
        {
            percent = playerHealth.HealthPercent;
            currentHP = playerHealth.CurrentHealth;
            maxHP = playerHealth.MaxHealth;
        }
        else if (enemyHealth != null) // cho enemy
        {
            percent = enemyHealth.HealthPercent;
            currentHP = enemyHealth.CurrentHealth;
            maxHP = enemyHealth.MaxHealth;
        }
        if (hpFill != null)
        {
            hpFill.fillAmount = percent;
        }

        if (HPText != null)
        {
            HPText.text = currentHP.ToString();
        }
    }
}
