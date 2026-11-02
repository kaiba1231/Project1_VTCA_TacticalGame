using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Giu trang thai luot cua tung Player/Enemy.
[DisallowMultipleComponent]
public class UnitTurnComponent : MonoBehaviour
{
    [SerializeField] private bool hasEndedTurn = true;

    public bool HasEndedTurn => hasEndedTurn; // cac script khac kiem tra trang thai hasendturn cua unit

    public void BeginTurn()
    {
        hasEndedTurn = false;
    }

    public void EndTurn()
    {
        hasEndedTurn = true;
    }
}
