using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{   
    [SerializeField]
    private GameScore gameScore;
   
    private PlayerInventory playerInventory;

   
   
   
    private void Awake()
    {
    playerInventory = GetComponent<PlayerInventory>();
    }


    void OnCollisionEnter2D(Collision2D collision) // cái này dừng lại stop object khi va chạm collision
    //void OnTriggerEnter2D(Collider2D other) cái này trigger cho đi xuyên qua, bat trigger trong editor inspector collider
    {
        Debug.Log("Phat hien va cham collision voi obstacle." + collision.gameObject.name);
    }

    void OnTriggerEnter2D(Collider2D coin)
    {   
        if (coin.CompareTag("Coin") && playerInventory != null)
        {   
            if (coin.gameObject.name.Contains("Silver"))
            {
                playerInventory.AddCoin(1);
                gameScore.AddScore(3);
                Destroy(coin.gameObject);
            }
            else if (coin.gameObject.name.Contains("Premium"))
            {
                playerInventory.AddCoin(1);
                gameScore.AddScore(5);
                Destroy(coin.gameObject);
            }
            else 
            {
                playerInventory.AddCoin(1);
                gameScore.AddScore(1);
                Destroy(coin.gameObject);
            }

          
            Debug.Log("Đã lụm" + coin.gameObject.name);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
