using UnityEngine;

[DisallowMultipleComponent]
public class APComponent : MonoBehaviour
{
    [SerializeField] private int maxAP = 2;
    [SerializeField] private int currentAP;

    public int MaxAP => maxAP;
    public int CurrentAP => currentAP;

    public void ResetAP() => currentAP = maxAP;

    public bool HasEnoughAP(int amount)
    {
        if (amount <= 0 || currentAP < amount)
        {
            return false;
        }

        return true;
    }
    public bool TrySpendAP(int amount)
    {
        if (!HasEnoughAP(amount)) return false;
        currentAP -= amount;
        PlayerUnit player = GetComponent<PlayerUnit>();
        EnemyUnit enemy = GetComponent<EnemyUnit>();

        if (player != null && player.TurnManager != null)
            player.TurnManager.Log("Player[" + player.PlayerId + "] spent " + amount + " AP | AP remaining: " + currentAP);

        if (currentAP == 0)
        {
            if (player != null) player.EndTurn();
            else if (enemy != null) enemy.EndTurn();
        }
        return true;
    }

    [ContextMenu("DEBUG: Spend 1 AP")]
    private void DebugSpendAP()
    {
        if (!Application.isPlaying) return;

        PlayerUnit player = GetComponent<PlayerUnit>();
        if (player != null && (player.TurnManager == null || player.TurnManager.CurrentPhase != BattlePhase.PlayerTurn || !player.IsActiveUnit || !player.CanAct))
            return;

        EnemyUnit enemy = GetComponent<EnemyUnit>();
        if (enemy != null && (enemy.TurnManager == null || enemy.TurnManager.CurrentPhase != BattlePhase.EnemyTurn || enemy.TurnManager.CurrentEnemy != enemy || enemy.HasEndedTurn))
            return;

        TrySpendAP(1);
    }
}