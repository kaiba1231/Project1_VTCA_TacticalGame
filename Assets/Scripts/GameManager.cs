using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public TurnManager turnManager;
    public SaveLoadUI saveLoadUI;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    public PauseMenuUI pauseMenuUI;
    public bool isPaused;

    public string[] gameplaySceneNames = { "MAP_1" };

    public bool isVictory = false;
    public bool isDefeat = false;

    public bool IsLoading { get; private set; }

    public bool IsGameOver => isVictory || isDefeat;
    public string CurrentLevel => SceneManager.GetActiveScene().name;

    private void Awake()
    {
        IsLoading = turnManager != null;
    }

    private IEnumerator Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (pauseMenuUI != null) pauseMenuUI.Show(false);
        if (turnManager == null) yield break;
        Time.timeScale = 1f;
        ActionDelay.Cancel();
        turnManager.gameObject.SetActive(true);
        if (saveLoadUI == null)
        {
            FailRestore("SaveLoadUI has not been assigned.");
            yield break;
        }
        if (!turnManager.InitializeBattle(out string error))
        {
            FailRestore(error ?? "Invalid Player roster.");
            yield break;
        }
        // Cho Start cua cac movement component hoan tat; input van bi khoa.
        yield return null;
        SaveData pending = PersistentSaveData.Instance == null ? null : PersistentSaveData.Instance.TakePendingLoad();
        if (pending != null)
        {
            if (pending.scenePath != SceneManager.GetActiveScene().path)
            {
                FailRestore("Loaded scene does not match the saved map.");
                yield break;
            }
            yield return saveLoadUI.RestoreBattle(pending);
            if (!string.IsNullOrEmpty(saveLoadUI.LastRestoreError))
            {
                FailRestore(saveLoadUI.LastRestoreError);
                yield break;
            }
            IsLoading = false;
            isPaused = false;
            saveLoadUI.ShowMessage("Saved battle restored.");
            Debug.Log("[SAVE] Restored round " + turnManager.RoundNumber, this);
        }
        else StartGame();
    }

    private void FailRestore(string message)
    {
        Debug.LogWarning("[SAVE] " + message, this);
        if (PersistentSaveData.Instance != null)
        {
            PersistentSaveData.Instance.ClearPendingLoad();
            PersistentSaveData.Instance.SetNotification("Cannot restore battle: " + message);
        }
        Time.timeScale = 1f;
        ActionDelay.Cancel();
        SceneManager.LoadScene("MainMenu");
    }

    public bool LoadSavedBattle(SaveData data)
    {
        if (IsLoading || data == null || PersistentSaveData.Instance == null) return false;
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(data.scenePath);
        if (System.Array.IndexOf(gameplaySceneNames, sceneName) < 0 || !Application.CanStreamedLevelBeLoaded(data.scenePath)) return false;
        if (!PersistentSaveData.Instance.QueueLoad(data)) return false;
        IsLoading = true;
        isPaused = false;
        Time.timeScale = 1f;
        ActionDelay.Cancel();
        try { SceneManager.LoadScene(data.scenePath); return true; }
        catch (System.Exception exception)
        {
            PersistentSaveData.Instance.ClearPendingLoad();
            IsLoading = false;
            Debug.LogWarning("[SAVE] " + exception.Message, this);
            return false;
        }
    }

    private void Update()
    {
        if (IsLoading || pauseMenuUI == null || IsGameOver || !Input.GetKeyDown(KeyCode.Escape)) return;

        if (isPaused && saveLoadUI != null && saveLoadUI.HandleBack()) return;
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public bool CanPauseGame()
    {
        if (IsLoading || pauseMenuUI == null || IsGameOver || ActionDelay.IsWaiting) return false;
        if (turnManager == null || turnManager.CurrentPhase != BattlePhase.PlayerTurn) return false;
        if (turnManager.SelectedPlayer == null || turnManager.SelectedPlayer.HasEndedTurn) return false;
        foreach (PlayerUnit player in turnManager.playerUnits)
        {
            if (player != null && player.GetComponent<PlayerMoveActionComponent>().IsMoving) return false;
        }
        return true;
    }

    public void PauseGame()
    {
        if (!CanPauseGame()) return;
        isPaused = true;
        Time.timeScale = 0f;
        pauseMenuUI.Show(true);
    }

    public void ResumeGame()
    {
        if (IsLoading || IsGameOver) return;
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuUI != null) pauseMenuUI.Show(false);
    }

    public void QuitToMenu()
    {
        if (IsLoading) return;
        if (PersistentSaveData.Instance != null) PersistentSaveData.Instance.ClearPendingLoad();
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
        IsLoading = false;
        turnManager.BeginNewBattle();
    }

    public int CountLivingPlayers()
    {
        int count = 0;
        foreach (PlayerUnit player in turnManager.playerUnits)
        {
            if (player != null && !player.GetComponent<HealthComponent>().IsDead()) count++;
        }
        return count;
    }

    public int CountLivingEnemies()
    {
        int count = 0;
        foreach (EnemyUnit enemy in turnManager.enemyUnits)
        {
            if (enemy != null && !enemy.GetComponent<HealthComponent>().IsDead()) count++;
        }
        return count;
    }

    public void CheckBattleResult()
    {
        if (IsLoading || IsGameOver || turnManager == null) return;

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
        if (IsLoading) return;
        if (gameplaySceneNames == null || levelIndex < 0 || levelIndex >= gameplaySceneNames.Length)
        {
            Debug.LogWarning("[GAME] Level index is out of range.", this);
            return;
        }

        isPaused = false;
        Time.timeScale = 1f;
        ActionDelay.Cancel();
        if (PersistentSaveData.Instance != null) PersistentSaveData.Instance.ClearPendingLoad();
        IsLoading = true;
        SceneManager.LoadScene(gameplaySceneNames[levelIndex]);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartGame()
    {
        if (IsLoading) return;
        if (PersistentSaveData.Instance != null) PersistentSaveData.Instance.ClearPendingLoad();
        IsLoading = true;
        isPaused = false;
        ActionDelay.Cancel();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
