using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{

    [SerializeField]
    private Slider HPSlider;

    public void SetMaxHealth(int maxHealth)
    {
    HPSlider.maxValue = maxHealth;
    HPSlider.value = maxHealth;
    }

    public void SetHealth(int currentHealth)
    {
    HPSlider.value = currentHealth;
    }

        
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
