using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public TurnManager turnManager;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    public PauseMenuUI pauseMenuUI;
    public bool isPaused;

    public string[] gameplaySceneNames = { "MAP_1" };

    public bool isVictory = false;
    public bool isDefeat = false;

    public bool IsGameOver => isVictory || isDefeat;
    public string CurrentLevel => SceneManager.GetActiveScene().name;

    private void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (pauseMenuUI != null) pauseMenuUI.Show(false);
        if (turnManager != null) StartGame();
    }

    private void Update()
    {
        if (pauseMenuUI == null || IsGameOver || !Input.GetKeyDown(KeyCode.Escape)) return;

        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        if (pauseMenuUI == null || IsGameOver) return;
        isPaused = true;
        Time.timeScale = 0f;
        pauseMenuUI.Show(true);
    }

    public void ResumeGame()
    {
        if (IsGameOver) return;
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuUI != null) pauseMenuUI.Show(false);
    }

    public void QuitToMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        ActionDelay.Cancel();
        SceneManager.LoadScene("MainMenu");
    }

    public void StartGame()
    {
        isPaused = false;
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

    public void NewGame()
    {
        LoadLevel(0);
    }

    public void LoadLevel(int levelIndex)
    {
        if (gameplaySceneNames == null || levelIndex < 0 || levelIndex >= gameplaySceneNames.Length)
        {
            Debug.LogWarning("[GAME] Level index is out of range.", this);
            return;
        }

        isPaused = false;
        Time.timeScale = 1f;
        ActionDelay.Cancel();
        SceneManager.LoadScene(gameplaySceneNames[levelIndex]);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
