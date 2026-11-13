using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Neu project da co BattlePhase trong GameplayEnums.cs, dung enum da co va xoa enum nay.
public enum BattlePhase
{
    Setup,
    PlayerTurn,
    EnemyTurn,
    BattleEnded
}

// Chi quan ly luot va thu tu ID. Khong quan ly UI, HP hay game over.
public class TurnManager : MonoBehaviour
{
    [Header("Units - Player theo thu tu ID 0, 1, 2...")]
    public List<PlayerUnit> playerUnits = new List<PlayerUnit>();
    public List<EnemyUnit> enemyUnits = new List<EnemyUnit>();

    [Header("Turn UI")]
    [SerializeField] private TurnStatusUI turnStatusUI;

    [Header("Settings")]
    [SerializeField] private float enemyPreviewSeconds = 1f;
    [SerializeField] private bool showDebugLogs = true;



    public BattlePhase CurrentPhase { get; private set; } = BattlePhase.Setup;
    public int RoundNumber { get; private set; }
    public PlayerUnit SelectedPlayer { get; private set; }
    public EnemyUnit CurrentEnemy { get; private set; }

    private void Start()
    {
        InitializeUnits();
        StartPlayerTurn();
    }

    private void InitializeUnits()
    {
        InitializePlayers();
        InitializeEnemies();
    }

    private void InitializePlayers()
    {
        for (int i = 0; i < playerUnits.Count; i++)
        {
            playerUnits[i].SetPlayerId(i);
            playerUnits[i].SetTurnManager(this);
            playerUnits[i].SetActiveUnit(false);
        }
    }

    private void InitializeEnemies()
    {
        // ****** LUU Y PHAI KIEM TRA LAI ENEMY TRONG SCENE XEM ENEMY NAO CO BI TRUNG ID HAY KHONG *********//
        enemyUnits.RemoveAll(enemy => enemy == null);

        // Kiem tra Enemy ID bi trung.
        List<int> usedIds = new List<int>();

        foreach (EnemyUnit enemy in enemyUnits)
        {
            int id = enemy.EnemyId;

            if (id < 0 || usedIds.Contains(id))
            {
                Debug.LogError("Enemy ID khong hop le hoac bi trung: " + enemy.name, enemy);

                return;
            }

            usedIds.Add(id);

        }

        // Sort Enemy theo ID da gan trong Inspector.
        enemyUnits.Sort((a, b) =>
            a.EnemyId.CompareTo(b.EnemyId));

        foreach (EnemyUnit enemy in enemyUnits)
        {
            enemy.SetTurnManager(this);

            Log($"EnemyId={enemy.EnemyId} -> {enemy.name}");
        }
    }




    // ------------- function cua player --------------------- //


    private void StartPlayerTurn()
    {
        RoundNumber++;
        CurrentPhase = BattlePhase.PlayerTurn;
        UpdateTurnUI();
        CurrentEnemy = null;

        Log("===== ROUND " + RoundNumber + " | PLAYER TURN =====");

        foreach (PlayerUnit player in playerUnits)
        {
            if (player != null && !player.GetComponent<PlayerHealthComponent>().IsDead())
                player.GetComponent<UnitTurnComponent>().BeginTurn();
        }
        SelectFirstAvailablePlayer();
    }

    // PlayerUnit goi khi nguoi choi click len Collider2D.
    public void SelectPlayer(PlayerUnit player)
    {
        if (ActionDelay.IsWaiting) return;
        if (SelectedPlayer != null)  // Không cho đổi Player khi unit hiện tại đang di chuyển
        {
            MoveActionComponent unitMove = SelectedPlayer.GetComponent<MoveActionComponent>();

            if (unitMove != null && unitMove.IsMoving)
                return;
        }

        if (CurrentPhase != BattlePhase.PlayerTurn) return;
        if (player == null || !player.CanAct || !playerUnits.Contains(player)) return;
        if (SelectedPlayer == player) return;

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

    // Button UI chi can goi ham nay.
    public void EndSelectedPlayerTurn()
    {
        if (ActionDelay.IsWaiting) return;
        if (CurrentPhase != BattlePhase.PlayerTurn || SelectedPlayer == null || !SelectedPlayer.CanAct)
            return;

        MoveActionComponent unitMove = SelectedPlayer.GetComponent<MoveActionComponent>(); // bo sung de ngan khong cho end turn khi unit dang di chuyen
        if (unitMove != null && unitMove.IsMoving)
        {
            Debug.Log("[TURN] Cannot End Turn khi unit is moving!");
            return;
        }

        SelectedPlayer.GetComponent<UnitTurnComponent>().EndTurn();
    }

    // UnitTurnComponent goi khi Player bam End Turn hoac dung het AP.
    public void OnPlayerEnded(PlayerUnit player)
    {
        if (CurrentPhase != BattlePhase.PlayerTurn) return;

        if (ActionDelay.IsWaiting)
        {
            StartCoroutine(AdvanceAfterActionDelay());
            return;
        }
        AdvanceAfterPlayerEnded();
    }

    private IEnumerator AdvanceAfterActionDelay()
    {
        while (ActionDelay.IsWaiting) yield return null;
        if (CurrentPhase == BattlePhase.PlayerTurn) AdvanceAfterPlayerEnded();
    }

    private void AdvanceAfterPlayerEnded()
    {
        // Chon Player con luot co ID nho nhat.
        // Neu khong con Player nao thi sang Enemy Turn.
        if (!SelectFirstAvailablePlayer())
            StartEnemyTurn();
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


    // ------------- function cua enemy --------------------- //
    private void StartEnemyTurn()
    {
        SetSelectedPlayer(null);
        CurrentPhase = BattlePhase.EnemyTurn;
        UpdateTurnUI();
        Log("===== ROUND " + RoundNumber + " | ENEMY TURN =====");
        StartCoroutine(PreviewEnemyTurns());
    }

    private IEnumerator PreviewEnemyTurns()
    {
        // Hien tai chua co AI: chi cho tung Enemy lan luot nhan luot de test.
        foreach (EnemyUnit enemy in enemyUnits)
        {
            if (enemy == null) continue;
            EnemyHealthComponent health = enemy.GetComponent<EnemyHealthComponent>();
            if (health != null && health.IsDead()) continue;
            CurrentEnemy = enemy;
            UnitTurnComponent unitTurn = enemy.GetComponent<UnitTurnComponent>();
            unitTurn.BeginTurn();
            Log("ACTIVE Enemy dang la [" + enemy.EnemyId + "] " + enemy.name);

            yield return new WaitForSeconds(Mathf.Max(0.1f, enemyPreviewSeconds));

            unitTurn.EndTurn();
        }

        CurrentEnemy = null;
        Log("All Enemies ended. Next round!");
        StartPlayerTurn();
    }

    public void UpdateTurnUI()
    {
        if (turnStatusUI != null) turnStatusUI.UpdateUI(RoundNumber, CurrentPhase);
    }

    public void Log(string message)
    {
        if (showDebugLogs)
            Debug.Log("[TURN] " + message, this);
    }
}
