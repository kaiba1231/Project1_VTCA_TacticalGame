using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to HealthBarCanvas, a CHILD of the unit's root GameObject.
public class UnitHealthBarUI : MonoBehaviour
{
    [SerializeField] private Image hpFill;
    [SerializeField] private TMP_Text percentText;

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

        if (playerHealth != null)
            percent = playerHealth.HealthPercent;
        else if (enemyHealth != null)
            percent = enemyHealth.HealthPercent;

        if (hpFill != null)
            hpFill.fillAmount = percent;

        if (percentText != null)
            percentText.text = Mathf.RoundToInt(percent * 100f) + "%";
    }
}
