using UnityEngine;

// Gan len root Player co Collider2D.
[RequireComponent(typeof(UnitTurnComponent))]
public class PlayerUnit : MonoBehaviour
{
    [Header("AP")]
    [SerializeField] private int maxAP = 2;
    [SerializeField] private int currentAP;

    [Header("Runtime - TurnManager tu gan")]
    [SerializeField] private int playerId = -1;
    [SerializeField] private bool isActiveUnit;

    private TurnManager turnManager;
    private UnitTurnComponent unitTurn;

    public int PlayerId => playerId;
    public int MaxAP => maxAP;
    public int CurrentAP => currentAP;
    public bool IsActiveUnit => isActiveUnit;
    public bool HasEndedTurn => unitTurn.HasEndedTurn;
    public bool CanAct => !HasEndedTurn && currentAP > 0;

    private void Awake()
    {
        unitTurn = GetComponent<UnitTurnComponent>();
    }

    public void SetTurnManager(TurnManager manager)
    {
        turnManager = manager;
    }

    public void SetPlayerId(int id)
    {
        playerId = id;
    }

    public void SetActiveUnit(bool active)
    {
        isActiveUnit = active;
    }

    public void BeginTurn()
    {
        currentAP = maxAP;
        unitTurn.BeginTurn();
        turnManager.Log("Player[" + playerId + "] START | AP: " + currentAP);
    }

    public void EndTurn()
    {
        if (HasEndedTurn || turnManager == null || !isActiveUnit) return;

        unitTurn.EndTurn();
        turnManager.Log("Player[" + playerId + "] END TURN | AP left: " + currentAP);
        turnManager.OnPlayerEnded(this);
    }

    // De Move / Shoot su dung sau nay. Chua can goi o bai nay.
    public bool TrySpendAP(int amount = 1)
    {
        if (amount <= 0 || currentAP < amount || !CanAct || !isActiveUnit) return false;
        if (turnManager == null || turnManager.CurrentPhase != BattlePhase.PlayerTurn) return false;

        currentAP -= amount;
        turnManager.Log("Player[" + playerId + "] spent " + amount + " AP | Left: " + currentAP);

        if (currentAP == 0)
            EndTurn();

        return true;
    }

    // Unity goi khi click vao Collider2D cua Player.
    private void OnMouseDown()
    {
        if (turnManager != null)
            turnManager.SelectPlayer(this);
    }

    [ContextMenu("DEBUG: Spend 1 AP")]
    private void DebugSpendAP()
    {
        if (Application.isPlaying)
            TrySpendAP(1);
    }
}
