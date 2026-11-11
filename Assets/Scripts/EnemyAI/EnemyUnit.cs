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
    private APComponent apComponent;

    public TurnManager TurnManager => turnManager;
    public int EnemyId => enemyId;
    public int MaxAP => apComponent.MaxAP;
    public int CurrentAP => apComponent.CurrentAP;
    public bool HasEndedTurn => unitTurn.HasEndedTurn;



    // ---------------------- Phuong thuc functions ------------------------ //



    private void Awake()
    {
        unitTurn = GetComponent<UnitTurnComponent>();
        apComponent = GetComponent<APComponent>();
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
        apComponent.ResetAP();
        unitTurn.BeginTurn();
        turnManager.Log("Enemy[" + enemyId + "] START | AP: " + apComponent.CurrentAP);
    }

    public void EndTurn()
    {
        if (HasEndedTurn) return;

        unitTurn.EndTurn();
        turnManager.Log("Enemy[" + enemyId + "] END TURN");
    }

}
