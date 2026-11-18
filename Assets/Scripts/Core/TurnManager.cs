using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattlePhase
{
    Setup,
    PlayerTurn,
    EnemyTurn,
    BattleEnded
}

public class TurnManager : MonoBehaviour
{
    [Header("Units - Player theo thu tu ID 0, 1, 2...")]
    public List<PlayerUnit> playerUnits = new List<PlayerUnit>();
    public List<EnemyUnit> enemyUnits = new List<EnemyUnit>();

    [SerializeField] private TurnStatusUI turnStatusUI;
    [SerializeField] private bool showDebugLogs = true;

    public BattlePhase CurrentPhase { get; private set; } = BattlePhase.Setup;
    public int RoundNumber { get; private set; }
    public PlayerUnit SelectedPlayer { get; private set; }
    public EnemyUnit CurrentEnemy { get; private set; }

    public bool InitializeBattle(out string error)
    {
        error = null;
        if (playerUnits.Count == 0 || enemyUnits.Count == 0)
        { error = "Battle roster is empty."; return false; }

        foreach (PlayerUnit player in playerUnits)
            if (player == null)
            { error = "Player roster contains a missing reference."; return false; }

        var enemyIds = new HashSet<int>();
        foreach (EnemyUnit enemy in enemyUnits)
        {
            if (enemy == null)
            { error = "Enemy roster contains a missing reference."; return false; }
            if (enemy.EnemyId < 0 || !enemyIds.Add(enemy.EnemyId))
            { error = "Enemy turn IDs are invalid."; return false; }
        }

        for (int i = 0; i < playerUnits.Count; i++)
        {
            playerUnits[i].SetPlayerId(i);
            playerUnits[i].SetTurnManager(this);
            playerUnits[i].SetActiveUnit(false);
        }
        enemyUnits.Sort((a, b) => a.EnemyId.CompareTo(b.EnemyId));
        foreach (EnemyUnit enemy in enemyUnits)
        {
            enemy.SetTurnManager(this);
            Log("EnemyId=" + enemy.EnemyId + " -> " + enemy.name);
        }
        RoundNumber = 0;
        CurrentPhase = BattlePhase.Setup;
        return true;
    }

    public void BeginNewBattle()
    {
        RoundNumber = 0;
        StartPlayerTurn();
    }

    private void StartPlayerTurn()
    {
        RoundNumber++;
        CurrentPhase = BattlePhase.PlayerTurn;
        CurrentEnemy = null;
        UpdateTurnUI();
        Log("===== ROUND " + RoundNumber + " | PLAYER TURN =====");

        foreach (PlayerUnit player in playerUnits)
        {
            if (player != null && !player.GetComponent<HealthComponent>().IsDead())
                player.GetComponent<UnitTurnComponent>().BeginTurn();
        }
        SelectFirstAvailablePlayer();
    }

    public void SelectPlayer(PlayerUnit player)
    {
        if (ActionDelay.IsWaiting || IsSelectedPlayerMoving()) return;
        if (CurrentPhase != BattlePhase.PlayerTurn) return;
        if (player == null || !player.CanAct || !playerUnits.Contains(player) || SelectedPlayer == player) return;
        SetSelectedPlayer(player);
    }

    private void SetSelectedPlayer(PlayerUnit player)
    {
        if (SelectedPlayer != null) SelectedPlayer.SetActiveUnit(false);
        SelectedPlayer = player;
        if (SelectedPlayer != null)
        {
            SelectedPlayer.SetActiveUnit(true);
            Log("SELECT Player[" + SelectedPlayer.PlayerId + "] " + SelectedPlayer.name);
        }
    }

    public void EndSelectedPlayerTurn()
    {
        if (ActionDelay.IsWaiting || CurrentPhase != BattlePhase.PlayerTurn) return;
        if (SelectedPlayer == null || !SelectedPlayer.CanAct) return;
        if (IsSelectedPlayerMoving())
        {
            Log("Cannot End Turn while the selected Player is moving.");
            return;
        }
        SelectedPlayer.GetComponent<UnitTurnComponent>().EndTurn();
    }

    private bool IsSelectedPlayerMoving()
    {
        if (SelectedPlayer == null) return false;
        PlayerMoveActionComponent movement = SelectedPlayer.GetComponent<PlayerMoveActionComponent>();
        return movement != null && movement.IsMoving;
    }

    // UnitTurnComponent goi khi Player End Turn hoac het AP.
    public void OnPlayerEnded(PlayerUnit player)
    {
        if (CurrentPhase != BattlePhase.PlayerTurn) return;
        if (ActionDelay.IsWaiting)
            StartCoroutine(AdvanceAfterActionDelay());
        else AdvanceAfterPlayerEnded();
    }

    private IEnumerator AdvanceAfterActionDelay()
    {
        while (ActionDelay.IsWaiting) yield return null;
        if (CurrentPhase == BattlePhase.PlayerTurn) AdvanceAfterPlayerEnded();
    }

    private void AdvanceAfterPlayerEnded()
    {
        if (!SelectFirstAvailablePlayer()) StartEnemyTurn();
    }

    private bool SelectFirstAvailablePlayer()
    {
        foreach (PlayerUnit player in playerUnits)
        {
            if (player != null && player.CanAct)
            {
                SetSelectedPlayer(player);
                return true;
            }
        }
        SetSelectedPlayer(null);
        return false;
    }

    private void StartEnemyTurn()
    {
        SetSelectedPlayer(null);
        CurrentPhase = BattlePhase.EnemyTurn;
        UpdateTurnUI();
        Log("===== ROUND " + RoundNumber + " | ENEMY TURN =====");
        StartCoroutine(RunEnemyTurns());
    }

    private IEnumerator RunEnemyTurns()
    {
        foreach (EnemyUnit enemy in enemyUnits)
        {
            if (enemy == null) continue;
            HealthComponent health = enemy.GetComponent<HealthComponent>();
            if (health != null && health.IsDead()) continue;

            CurrentEnemy = enemy;
            UnitTurnComponent unitTurn = enemy.GetComponent<UnitTurnComponent>();
            unitTurn.BeginTurn();
            Log("ACTIVE Enemy [" + enemy.EnemyId + "] " + enemy.name);

            EnemyAIController ai = enemy.GetComponent<EnemyAIController>();
            if (ai != null) yield return StartCoroutine(ai.TakeTurn());
            else unitTurn.EndTurn();
        }
        CurrentEnemy = null;
        Log("All Enemies ended. Next round!");
        StartPlayerTurn();
    }

    // Chi gan trang thai luot; SaveLoadUI phu trach du lieu tung unit.
    public void PrepareForRestore()
    {
        CurrentPhase = BattlePhase.Setup;
        CurrentEnemy = null;
        SetSelectedPlayer(null);
    }

    public void RestoreTurnState(int roundNumber, PlayerUnit selectedPlayer)
    {
        RoundNumber = roundNumber;
        CurrentPhase = BattlePhase.PlayerTurn;
        CurrentEnemy = null;
        SetSelectedPlayer(selectedPlayer);
        UpdateTurnUI();
    }

    public void UpdateTurnUI()
    {
        if (turnStatusUI != null) turnStatusUI.UpdateUI(RoundNumber, CurrentPhase);
    }

    public void Log(string message)
    {
        if (showDebugLogs) Debug.Log("[TURN] " + message, this);
    }
}
