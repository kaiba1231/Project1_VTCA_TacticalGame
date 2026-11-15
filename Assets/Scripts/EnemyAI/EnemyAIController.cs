using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyUnit), typeof(EnemyMoveActionComponent))]
public class EnemyAIController : MonoBehaviour
{
    [SerializeField] private float optimalAttackDistance = 8f;
    [SerializeField] private NavMeshObstacle blocker;
    private NavMeshAgent agent;
    private EnemyMoveActionComponent movement;
    private UnitTurnComponent unitTurn;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        movement = GetComponent<EnemyMoveActionComponent>();
        unitTurn = GetComponent<UnitTurnComponent>();
    }

    public IEnumerator TakeTurn()
    {
        blocker.enabled = false;
        yield return null;
        yield return null;
        if (this == null) yield break;
        agent.enabled = true;

        while (!unitTurn.HasEndedTurn)
        {
            PlayerUnit target = movement.FindNearestPlayer();
            if (target == null)
            {
                movement.ReleasePosition();
                break;
            }
            if (movement.HasClearAttackPosition(target, optimalAttackDistance))
            {
                movement.ReserveCurrentPosition(target);
                break;
            }

            if (!movement.TryMoveTowardTarget(target, optimalAttackDistance)) break;
            while (movement != null && movement.IsMoving) yield return null;
            if (this == null) yield break;
        }

        agent.enabled = false;
        blocker.enabled = true;
        unitTurn.EndTurn();
    }
}
