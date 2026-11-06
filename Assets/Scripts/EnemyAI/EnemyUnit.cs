using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnitTurnComponent), typeof(APComponent))]
public class EnemyUnit : MonoBehaviour
{
    [Header("ID - khong duoc trung")]
    [SerializeField] private int enemyId = -1;

    private TurnManager turnManager;
    private UnitTurnComponent unitTurn;
    private APComponent ap;

    public int EnemyId => enemyId;
    public int MaxAP => ap.MaxAP;
    public int CurrentAP => ap.CurrentAP;
    public bool HasEndedTurn => unitTurn.HasEndedTurn;



    // ---------------------- Phuong thuc functions ------------------------ //



    private void Awake()
    {
        unitTurn = GetComponent<UnitTurnComponent>();
        ap = GetComponent<APComponent>();
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
        ap.ResetAP();
        unitTurn.BeginTurn();
        turnManager.Log("Enemy[" + enemyId + "] START | AP: " + ap.CurrentAP);
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
        if (amount <= 0 || HasEndedTurn || !ap.HasEnoughAP(amount)) return false;
        if (turnManager == null || turnManager.CurrentEnemy != this) return false;

        if (!ap.TrySpendAP(amount)) return false;
        turnManager.Log("Enemy[" + enemyId + "] spent " + amount + " AP | Left: " + ap.CurrentAP);

        if (ap.CurrentAP == 0)
            EndTurn();

        return true;
    }
}
