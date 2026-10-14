using UnityEngine;
using TMPro;

public class CoinCounterUI : MonoBehaviour
{   
    [SerializeField]
    private TextMeshProUGUI coinText;

    public void SetCoinCount(int coinCount)
    {
        coinText.text = "Coins: " + coinCount;
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
