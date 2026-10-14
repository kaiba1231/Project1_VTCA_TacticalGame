using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{   
    [SerializeField]
    private int maxHP = 100;
    public int playerlevel = 3;
    public int playercurrency = 500;    

    public int currentHP;
    [SerializeField]
    private HealthBarUI healthBarUI;
    [SerializeField]
    private GameManager gameManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        currentHP = maxHP;
    }

    void Start()
    {   
        if (healthBarUI != null)    healthBarUI.SetMaxHealth(maxHP);

        Debug.Log("Player start");
        Debug.Log("Playergold:" + playercurrency);
        Debug.Log("bat dau di chuyen:");

    }

    private void Die()
    {   
        Debug.Log("Nhan vat die");
        if (gameManager != null)    gameManager.GameOver();
        gameObject.SetActive(false);
    }
    
    public void TakeDamage(int damage)
    {
        if (damage <= 0 || currentHP == 0)    return;
        currentHP = Mathf.Max(currentHP - damage, 0);

        // Debug.Log("Player Health: " + currentHealth);
        if (healthBarUI != null)  healthBarUI.SetHealth(currentHP);
        if (currentHP == 0) Die();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
