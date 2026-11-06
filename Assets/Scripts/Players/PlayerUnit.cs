using UnityEngine;

// Gan len root Player co Collider2D.
[RequireComponent(typeof(UnitTurnComponent), typeof(APComponent))]
public class PlayerUnit : MonoBehaviour
{
    [Header("ID - Khong duoc trung")]
    [SerializeField] private int playerId = -1;
    [SerializeField] private bool isActiveUnit;

    private TurnManager turnManager;
    private UnitTurnComponent unitTurn;
    private APComponent ap;

    public int PlayerId => playerId;
    public int MaxAP => ap.MaxAP;
    public int CurrentAP => ap.CurrentAP;
    public bool IsActiveUnit => isActiveUnit;
    public bool HasEndedTurn => unitTurn.HasEndedTurn;
    public bool CanAct => !HasEndedTurn && ap.CurrentAP > 0;



    // ---------------------- Phuong thuc functions ------------------------ //



    private void Awake()
    {
        unitTurn = GetComponent<UnitTurnComponent>();
        ap = GetComponent<APComponent>();
    }


    public void SetPlayerId(int id)
    {
        playerId = id;
    }

    public void SetTurnManager(TurnManager manager)
    {
        turnManager = manager;
    }

    public void SetActiveUnit(bool active)
    {
        isActiveUnit = active;

    }

    public void BeginTurn()
    {
        ap.ResetAP();
        unitTurn.BeginTurn();
        turnManager.Log("Player[" + playerId + "] START | AP: " + ap.CurrentAP);
    }

    public void EndTurn()
    {
        if (HasEndedTurn || turnManager == null || !isActiveUnit) return;

        unitTurn.EndTurn();
        turnManager.Log("Player[" + playerId + "] END TURN | AP left: " + ap.CurrentAP);
        turnManager.OnPlayerEnded(this);
    }

    // De Move / Shoot su dung sau nay. Chua can goi o bai nay.
    public bool TrySpendAP(int amount = 1)
    {
        if (amount <= 0 || !ap.HasEnoughAP(amount) || !CanAct || !isActiveUnit) return false;
        if (turnManager == null || turnManager.CurrentPhase != BattlePhase.PlayerTurn) return false;

        if (!ap.TrySpendAP(amount)) return false;
        turnManager.Log("Player[" + playerId + "] spent " + amount + " AP | AP remaining: " + ap.CurrentAP);

        if (ap.CurrentAP == 0) EndTurn();
        return true;
    }

    // Unity goi khi click vao Collider2D cua Player.
    private void OnMouseDown()
    {
        if (turnManager != null && !isActiveUnit && CanAct)
            turnManager.SelectPlayer(this);
    }

    [ContextMenu("DEBUG: Spend 1 AP")]
    private void DebugSpendAP()
    {
        if (Application.isPlaying)
            TrySpendAP(1);
    }
}
