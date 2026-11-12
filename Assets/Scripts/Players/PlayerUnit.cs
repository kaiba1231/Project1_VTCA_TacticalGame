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
    private APComponent apComponent;
    private PlayerHealthComponent health;

    public TurnManager TurnManager => turnManager;
    public int PlayerId => playerId;
    public int MaxAP => apComponent.MaxAP;
    public int CurrentAP => apComponent.CurrentAP;
    public bool IsActiveUnit => isActiveUnit;
    public bool HasEndedTurn => unitTurn.HasEndedTurn;
    public bool CanAct => turnManager != null && turnManager.isActiveAndEnabled && health != null && !health.IsDead() && !HasEndedTurn && apComponent.CurrentAP > 0;



    // ---------------------- Phuong thuc functions ------------------------ //



    private void Awake()
    {
        unitTurn = GetComponent<UnitTurnComponent>();
        apComponent = GetComponent<APComponent>();
        health = GetComponent<PlayerHealthComponent>();
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
        apComponent.ResetAP();
        unitTurn.BeginTurn();
        turnManager.Log("Player[" + playerId + "] START | AP: " + apComponent.CurrentAP);
    }

    public void EndTurn()
    {
        if (HasEndedTurn || turnManager == null || !isActiveUnit) return;

        unitTurn.EndTurn();
        turnManager.Log("Player[" + playerId + "] END TURN | AP left: " + apComponent.CurrentAP);
        turnManager.OnPlayerEnded(this);
    }

    // Unity goi khi click vao Collider2D cua Player.
    private void OnMouseDown()
    {
        if (turnManager != null && !isActiveUnit && CanAct)
            turnManager.SelectPlayer(this);
    }


}


