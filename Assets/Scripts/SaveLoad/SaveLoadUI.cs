using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class SaveLoadUI : MonoBehaviour
{
    [SerializeField] private SaveSystem saveSystem;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UnityEngine.UI.Button saveButton;
    [SerializeField] private UnityEngine.UI.Button loadButton;
    [SerializeField] private UnityEngine.UI.Button continueButton;
    [SerializeField] private TMP_Text statusText;

    public string LastMessage { get; private set; }
    public string LastRestoreError { get; private set; }
    private bool validSave;
    private bool wasPaused;

    private void Start()
    {
        RefreshSaveButtons();
        string notification = PersistentSaveData.Instance == null ? null : PersistentSaveData.Instance.TakeNotification();
        if (!string.IsNullOrEmpty(notification)) ShowMessage(notification);
    }

    private void Update()
    {
        if (gameManager == null) return;
        bool paused = gameManager.isPaused;
        if (paused && !wasPaused) RefreshSaveButtons();
        wasPaused = paused;
        if (saveButton != null) saveButton.interactable = paused && gameManager.CanPauseGame();
        if (loadButton != null) loadButton.interactable = validSave && !gameManager.IsLoading;
        if (continueButton != null) continueButton.interactable = validSave && !gameManager.IsLoading;
    }

    public void RefreshSaveButtons()
    {
        SaveData data = null;
        string message = "SaveSystem has not been assigned.";
        validSave = saveSystem != null && saveSystem.LoadGame(out data, out message);
        if (loadButton != null) loadButton.interactable = validSave;
        if (continueButton != null) continueButton.interactable = validSave;
        ShowMessage(validSave ? System.IO.Path.GetFileNameWithoutExtension(data.scenePath) + " | Round " + data.roundNumber : message);
    }

    public void SaveGame()
    {
        if (saveSystem == null || gameManager == null || !gameManager.isPaused || !gameManager.CanPauseGame())
        { ShowMessage("Open Pause during an idle Player Turn to save."); return; }

        var turns = gameManager.turnManager;
        var data = new SaveData { scenePath = SceneManager.GetActiveScene().path, roundNumber = turns.RoundNumber };
        foreach (PlayerUnit player in turns.playerUnits)
        {
            if (player == null || player.GetComponent<HealthComponent>().IsDead()) continue;
            data.players.Add(new PlayerSaveData {
                saveId = player.GetComponent<UnitSaveIdentity>().SaveId,
                position = player.transform.position,
                currentHealth = player.GetComponent<HealthComponent>().CurrentHealth,
                currentAP = player.CurrentAP, hasEndedTurn = player.HasEndedTurn });
        }
        foreach (EnemyUnit enemy in turns.enemyUnits)
        {
            if (enemy == null || enemy.GetComponent<HealthComponent>().IsDead()) continue;
            data.enemies.Add(new EnemySaveData {
                saveId = enemy.GetComponent<UnitSaveIdentity>().SaveId,
                position = enemy.transform.position,
                currentHealth = enemy.GetComponent<HealthComponent>().CurrentHealth });
        }
        saveSystem.SaveGame(data, out string message);
        RefreshSaveButtons();
        ShowMessage(message);
    }

    public void LoadGame()
    {
        if (gameManager == null || saveSystem == null || gameManager.IsLoading) return;
        if (!saveSystem.LoadGame(out SaveData data, out string message))
        { RefreshSaveButtons(); ShowMessage(message); return; }
        if (!gameManager.LoadSavedBattle(data)) ShowMessage("Could not start loading the saved battle.");
    }

    public void ShowMessage(string message)
    {
        LastMessage = message;
        if (statusText != null) statusText.text = message ?? "";
    }

    // Scene vua duoc mo lai: unit khong co trong file la unit da chet.
    public IEnumerator RestoreBattle(SaveData data)
    {
        LastRestoreError = null;
        var turns = gameManager.turnManager;
        // File da duoc SaveSystem doc; chi can doi chieu ID voi scene truoc khi ap dung.
        foreach (PlayerSaveData state in data.players)
        {
            if (state == null || !turns.playerUnits.Exists(unit => unit.GetComponent<UnitSaveIdentity>().SaveId == state.saveId))
            { LastRestoreError = "Saved Player ID was not found in this level."; yield break; }
        }
        foreach (EnemySaveData state in data.enemies)
        {
            if (state == null || !turns.enemyUnits.Exists(unit => unit.GetComponent<UnitSaveIdentity>().SaveId == state.saveId))
            { LastRestoreError = "Saved Enemy ID was not found in this level."; yield break; }
        }
        turns.PrepareForRestore();
        var blockers = new List<NavMeshObstacle>();
        foreach (EnemyUnit enemy in turns.enemyUnits)
        {
            enemy.GetComponent<NavMeshAgent>().enabled = false;
            foreach (var obstacle in enemy.GetComponentsInChildren<NavMeshObstacle>())
                if (obstacle.enabled) { obstacle.enabled = false; blockers.Add(obstacle); }
            EnemySaveData state = data.enemies.Find(item => item.saveId == enemy.GetComponent<UnitSaveIdentity>().SaveId);
            if (state == null) { enemy.gameObject.SetActive(false); Destroy(enemy.gameObject); }
            else
            {
                enemy.GetComponent<HealthComponent>().RestoreHealth(state.currentHealth);
                enemy.transform.position = state.position;
            }
        }
        foreach (PlayerUnit player in turns.playerUnits)
        {
            player.GetComponent<NavMeshAgent>().enabled = false;
            PlayerSaveData state = data.players.Find(item => item.saveId == player.GetComponent<UnitSaveIdentity>().SaveId);
            if (state == null) { player.gameObject.SetActive(false); Destroy(player.gameObject); }
            else
            {
                player.GetComponent<HealthComponent>().RestoreHealth(state.currentHealth);
                player.GetComponent<APComponent>().RestoreAP(state.currentAP);
                player.GetComponent<UnitTurnComponent>().RestoreTurnState(state.hasEndedTurn);
                player.transform.position = state.position;
            }
        }
        // Cho Destroy va NavMesh carving cap nhat truoc khi bat lai agent cua Player.
        yield return null;
        turns.playerUnits.RemoveAll(unit => unit == null);
        turns.enemyUnits.RemoveAll(unit => unit == null);
        Physics2D.SyncTransforms();
        float deadline = Time.realtimeSinceStartup + 8f;
        foreach (PlayerUnit player in turns.playerUnits)
        {
            var agent = player.GetComponent<NavMeshAgent>();
            Vector3 savedPosition = player.transform.position;
            // NavMeshPlus nam tren XY; baseOffset cua agent duoc ap theo truc Z.
            Vector3 navPosition = savedPosition + Vector3.forward * agent.baseOffset;
            var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
            NavMeshHit hit;
            while (!NavMesh.SamplePosition(navPosition, out hit, 0.2f, filter))
            {
                if (Time.realtimeSinceStartup >= deadline)
                { LastRestoreError = "Cannot place Player on NavMesh: " + player.name; yield break; }
                yield return null;
            }
            player.transform.position = hit.position;
            agent.enabled = true;
            if (!agent.Warp(hit.position))
            { LastRestoreError = "Cannot restore Player movement: " + player.name; yield break; }
            agent.ResetPath();
            player.transform.position = savedPosition;
        }
        foreach (var obstacle in blockers) if (obstacle != null) obstacle.enabled = true;
        PlayerUnit selected = turns.playerUnits.Find(player => !player.HasEndedTurn && player.CurrentAP > 0);
        turns.RestoreTurnState(data.roundNumber, selected);
    }
}
