using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitHealthBarUI : MonoBehaviour
{
    [SerializeField] private Image hpFill;
    [SerializeField] private TMP_Text HPText;
    [SerializeField] private HealthComponent health;

    private void Update()
    {
        if (health == null) return;
        if (hpFill != null) hpFill.fillAmount = health.HealthPercent;
        if (HPText != null) HPText.text = health.CurrentHealth.ToString();
    }
}
