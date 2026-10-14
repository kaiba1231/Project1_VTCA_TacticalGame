using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject gameOverPanel;

    private bool isGameOver;

    private void Start()
    {
        if (gameOverPanel != null)     gameOverPanel.SetActive(false);
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        if (gameOverPanel != null)       gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager
            .LoadScene(SceneManager
                .GetActiveScene().buildIndex);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
