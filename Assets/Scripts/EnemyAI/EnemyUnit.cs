using UnityEngine;

[RequireComponent(typeof(UnitTurnComponent))]
public class EnemyUnit : MonoBehaviour
{
    [Header("AP")]
    [SerializeField] private int maxAP = 2;
    [SerializeField] private int currentAP;

    [Header("Runtime - TurnManager tu gan")]
    [SerializeField] private int enemyId = -1;

    private TurnManager turnManager;
    private UnitTurnComponent unitTurn;

    public int EnemyId => enemyId;
    public int MaxAP => maxAP;
    public int CurrentAP => currentAP;
    public bool HasEndedTurn => unitTurn.HasEndedTurn;

    private void Awake()
    {
        unitTurn = GetComponent<UnitTurnComponent>();
    }

    public void SetTurnManager(TurnManager manager)
    {
        turnManager = manager;
    }

    public void SetEnemyId(int id)
    {
        enemyId = id;
    }

    public void BeginTurn()
    {
        currentAP = maxAP;
        unitTurn.BeginTurn();
        turnManager.Log("Enemy[" + enemyId + "] START | AP: " + currentAP);
    }

    public void EndTurn()
    {
        if (HasEndedTurn) return;

        unitTurn.EndTurn();
        turnManager.Log("Enemy[" + enemyId + "] END TURN");
    }

    // Enemy AI se su dung sau nay. Hien tai Enemy dang duoc mo phong.
    public bool TrySpendAP(int amount = 1)
    {
        if (amount <= 0 || HasEndedTurn || currentAP < amount) return false;
        if (turnManager == null || turnManager.CurrentEnemy != this) return false;

        currentAP -= amount;
        turnManager.Log("Enemy[" + enemyId + "] spent " + amount + " AP | Left: " + currentAP);

        if (currentAP == 0)
            EndTurn();

        return true;
    }
}
