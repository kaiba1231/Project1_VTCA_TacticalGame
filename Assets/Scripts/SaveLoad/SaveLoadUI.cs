using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SaveLoadUI : MonoBehaviour
{
    [SerializeField] private SaveSystem saveSystem;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_Text statusText;

    public string LastMessage { get; private set; }
    public string LastRestoreError { get; private set; }
    [Header("Save slots")]
    [SerializeField] private GameObject slotPanel;
    [SerializeField] private TMP_Text slotTitle;
    [SerializeField] private TMP_Text slotStatus;
    [SerializeField] private Button[] slotButtons;
    [SerializeField] private Button overwriteButton;
    [SerializeField] private Button backButton;

    private SaveSlotInfo[] slots;
    private bool saving;
    private bool busy;
    private int overwriteSlot;
    private SaveData pendingAutoSave;
    private bool autoSaving;

    private void Start()
    {
        if (slotPanel != null) slotPanel.SetActive(false);
        string notification = PersistentSaveData.Instance == null ? null : PersistentSaveData.Instance.TakeNotification();
        if (!string.IsNullOrEmpty(notification)) ShowMessage(notification);
    }

    private void Update()
    {
        bool ready = CanStartRequest();
        if (saveButton != null) saveButton.interactable = ready && CanSaveManually();
        if (loadButton != null) loadButton.interactable = ready && CanOpenLoadMenu();
        if (continueButton != null) continueButton.interactable = ready;
    }

    public void SaveGame()
    {
        if (CanStartRequest() && CanSaveManually()) StartCoroutine(OpenSlots(true));
    }

    public void LoadGame()
    {
        if (CanStartRequest() && CanOpenLoadMenu()) StartCoroutine(OpenSlots(false));
    }

    public void ContinueGame()
    {
        if (CanStartRequest() && gameManager.turnManager == null)
            StartCoroutine(LoadSlot(SaveSystem.AutoSlot));
    }

    private bool CanOpenLoadMenu()
    {
        return gameManager.turnManager == null || gameManager.isPaused;
    }

    private IEnumerator OpenSlots(bool saveMode)
    {
        if (slotPanel == null)
        {
            ShowMessage("Save slots panel is not assigned.");
            yield break;
        }

        saving = saveMode;
        slots = null;
        overwriteSlot = 0;
        busy = true;
        slotPanel.SetActive(true);
        slotTitle.text = saving ? "Save Game" : "Load Game";
        slotStatus.text = "Checking save slots...";
        RefreshSlotButtons();

        string error = null;
        yield return saveSystem.ListSlots((result, message) => { slots = result; error = message; });
        busy = false;
        slotStatus.text = error ?? (saving ? "Choose a slot to save your battle." : "Choose a saved battle.");
        RefreshSlotButtons();
    }

    public void SelectSlot(int slotId)
    {
        if (busy || slots == null || slotId < 1 || slotId > SaveSystem.ManualSlotCount) return;
        if (slotPanel == null || !slotPanel.activeSelf || overwriteSlot != 0) return;
        SaveSlotInfo existing = FindSlot(slotId);
        if (!saving)
        {
            if (existing != null) StartCoroutine(LoadSlot(slotId));
            return;
        }

        if (!CanSaveManually()) return;
        if (existing == null) StartCoroutine(SaveSlot(slotId));
        else
        {
            overwriteSlot = slotId;
            slotStatus.text = $"Replace the saved battle in Slot {slotId}?";
            RefreshSlotButtons();
        }
    }

    public void ConfirmOverwrite()
    {
        if (!busy && overwriteSlot != 0 && CanSaveManually()) StartCoroutine(SaveSlot(overwriteSlot));
    }

    public void CloseSlotPanel()
    {
        if (busy || slotPanel == null) return;
        if (overwriteSlot != 0)
        {
            overwriteSlot = 0;
            slotStatus.text = "Choose a slot to save your battle.";
            RefreshSlotButtons();
        }
        else slotPanel.SetActive(false);
    }

    public bool HandleBack()
    {
        if (slotPanel == null || !slotPanel.activeSelf) return false;
        CloseSlotPanel();
        return true;
    }

    private IEnumerator SaveSlot(int slotId)
    {
        busy = true;
        overwriteSlot = 0;
        slotStatus.text = $"Saving to Slot {slotId}...";
        RefreshSlotButtons();
        string error = null;
        yield return saveSystem.SaveGame(slotId, CaptureBattle(), message => error = message);
        busy = false;
        if (error != null)
        {
            slotStatus.text = error;
            RefreshSlotButtons();
            yield break;
        }
        slotPanel.SetActive(false);
        ShowMessage($"Saved to Slot {slotId}.");
    }

    private IEnumerator LoadSlot(int slotId)
    {
        busy = true;
        if (slotId == SaveSystem.AutoSlot) ShowMessage("Loading autosave...");
        else
        {
            slotStatus.text = $"Loading Slot {slotId}...";
            RefreshSlotButtons();
        }

        SaveData data = null;
        string error = null;
        yield return saveSystem.LoadGame(slotId, (result, message) => { data = result; error = message; });
        if (error == null && gameManager.LoadSavedBattle(data)) yield break;

        busy = false;
        string messageText = error ?? "Could not start loading the saved battle.";
        if (slotId == SaveSystem.AutoSlot) ShowMessage(messageText);
        else
        {
            slotStatus.text = messageText;
            RefreshSlotButtons();
        }
    }

    public void AutoSave()
    {
        if (saveSystem == null || gameManager == null || gameManager.IsGameOver) return;
        pendingAutoSave = CaptureBattle();
        if (!autoSaving) StartCoroutine(WriteAutoSaves());
    }

    private IEnumerator WriteAutoSaves()
    {
        autoSaving = true;
        while (pendingAutoSave != null)
        {
            SaveData data = pendingAutoSave;
            pendingAutoSave = null;
            yield return saveSystem.SaveGame(SaveSystem.AutoSlot, data, error =>
            {
                string message = error == null ? $"Autosaved round {data.roundNumber}." : $"Autosave failed: {error}";
                Debug.Log("[Save] " + message, this);
            });
        }
        autoSaving = false;
    }

    private bool CanStartRequest()
    {
        return !busy && (slotPanel == null || !slotPanel.activeSelf)
            && saveSystem != null && gameManager != null && !gameManager.IsLoading;
    }

    private bool CanSaveManually()
    {
        return gameManager != null && gameManager.isPaused && gameManager.CanPauseGame();
    }

    private SaveSlotInfo FindSlot(int slotId)
    {
        return slots == null ? null : System.Array.Find(slots, slot => slot != null && slot.slotId == slotId);
    }

    private void RefreshSlotButtons()
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int slotId = i + 1;
            SaveSlotInfo info = FindSlot(slotId);
            string details = slots == null ? "Unavailable" : "Empty";
            bool canLoadSlot = false;
            if (info != null)
            {
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(info.scenePath);
                int levelIndex = System.Array.IndexOf(gameManager.gameplaySceneNames, sceneName);
                canLoadSlot = levelIndex >= 0;
                string mapLabel = canLoadSlot ? $"MAP {levelIndex + 1}" : "Removed map";
                details = $"{mapLabel}  |  Round {info.roundNumber}";
                if (System.DateTime.TryParse(info.savedAt, out var time)) details += $"  |  {time:dd/MM/yyyy HH:mm}";
            }
            slotButtons[i].GetComponentInChildren<TMP_Text>().text = $"SLOT {slotId}    •    {details}";
            slotButtons[i].interactable = !busy && overwriteSlot == 0 && slots != null && (saving || canLoadSlot);
        }
        overwriteButton.gameObject.SetActive(overwriteSlot != 0);
        backButton.interactable = !busy;
    }


    private SaveData CaptureBattle()
    {
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
        return data;
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
