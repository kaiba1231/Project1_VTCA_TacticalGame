using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public TurnManager turnManager;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    public bool isVictory = false;
    public bool isDefeat = false;

    public bool IsGameOver => isVictory || isDefeat;
    public string CurrentLevel => SceneManager.GetActiveScene().name;

    private void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (turnManager != null) StartGame();
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        if (turnManager == null)
        {
            Debug.LogError("[GAME] TurnManager has not been assigned.", this);
            return;
        }
        turnManager.gameObject.SetActive(true);
    }

    public int CountLivingPlayers()
    {
        int count = 0;
        foreach (PlayerUnit player in turnManager.playerUnits)
        {
            if (player != null && !player.GetComponent<PlayerHealthComponent>().IsDead()) count++;
        }
        return count;
    }

    public int CountLivingEnemies()
    {
        int count = 0;
        foreach (EnemyUnit enemy in turnManager.enemyUnits)
        {
            if (enemy != null && !enemy.GetComponent<EnemyHealthComponent>().IsDead()) count++;
        }
        return count;
    }

    public void CheckBattleResult()
    {
        if (IsGameOver || turnManager == null) return;

        if (CountLivingPlayers() == 0)
        {
            isDefeat = true;
            Time.timeScale = 0f;
            if (turnManager != null) turnManager.gameObject.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            Debug.Log("[GAME] Defeat!");
        }
        else if (CountLivingEnemies() == 0)
        {
            isVictory = true;
            Time.timeScale = 0f;
            turnManager.gameObject.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(true);
            Debug.Log("[GAME] Victory!");
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
