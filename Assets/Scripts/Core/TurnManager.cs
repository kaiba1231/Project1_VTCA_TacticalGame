using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Neu project da co BattlePhase trong GameplayEnums.cs, dung enum da co va xoa enum nay.
public enum BattlePhase
{
    Setup,
    PlayerTurn,
    EnemyTurn
}

// Chi quan ly luot va thu tu ID. Khong quan ly UI, HP hay game over.
public class TurnManager : MonoBehaviour
{
    [Header("Units - Player theo thu tu ID 0, 1, 2...")]
    [SerializeField] private List<PlayerUnit> playerUnits = new List<PlayerUnit>();
    [SerializeField] private List<EnemyUnit> enemyUnits = new List<EnemyUnit>();

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

        if (playerUnits.Count == 0 || enemyUnits.Count == 0)
        {
            Debug.LogError("[TURN] Can it nhat 1 Player va 1 Enemy de test luot!", this);
            return;
        }

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
        CurrentEnemy = null;

        Log("===== ROUND " + RoundNumber + " | PLAYER TURN =====");

        foreach (PlayerUnit player in playerUnits)
        {
            player.BeginTurn();
        }
        SelectFirstAvailablePlayer();
    }

    // PlayerUnit goi khi nguoi choi click len Collider2D.
    public void SelectPlayer(PlayerUnit player)
    {
        if (CurrentPhase != BattlePhase.PlayerTurn) return;
        if (player == null || !player.CanAct) return;
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
        if (CurrentPhase != BattlePhase.PlayerTurn)
        {
            Log("Khong the End Turn: hien tai la " + CurrentPhase);
            return;
        }

        if (SelectedPlayer == null)
        {
            Log("Khong co Player nao dang duoc chon.");
            return;
        }

        SelectedPlayer.EndTurn();
    }

    // Duoc goi khi Player bam End Turn hoac dung het AP.
    public void OnPlayerEnded(PlayerUnit player)
    {
        if (CurrentPhase != BattlePhase.PlayerTurn) return;

        // Chon Player con luot co ID nho nhat.
        // Neu khong con Player nao thi sang Enemy Turn.
        if (!SelectFirstAvailablePlayer())
            StartEnemyTurn();
    }

    private bool SelectFirstAvailablePlayer()
    {
        foreach (PlayerUnit player in playerUnits)
        {
            if (player.CanAct)
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
        Log("===== ROUND " + RoundNumber + " | ENEMY TURN =====");
        StartCoroutine(PreviewEnemyTurns());
    }

    private IEnumerator PreviewEnemyTurns()
    {
        // Hien tai chua co AI: chi cho tung Enemy lan luot nhan luot de test.
        foreach (EnemyUnit enemy in enemyUnits)
        {
            CurrentEnemy = enemy;
            enemy.BeginTurn();
            Log("ACTIVE Enemy dang la [" + enemy.EnemyId + "] " + enemy.name);

            yield return new WaitForSeconds(Mathf.Max(0.1f, enemyPreviewSeconds));

            enemy.EndTurn();
        }

        CurrentEnemy = null;
        Log("All Enemies ended. Next round!");
        StartPlayerTurn();
    }

    public void Log(string message)
    {
        if (showDebugLogs)
            Debug.Log("[TURN] " + message, this);
    }
}
