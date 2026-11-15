using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyUnit), typeof(EnemyMoveActionComponent))]
public class EnemyAIController : MonoBehaviour
{
    [Header("Optimal Attack Distance")]
    [SerializeField, Min(0.1f), InspectorName("Min Value")] private float minAttackDistance = 8f;
    [SerializeField, Min(0.1f), InspectorName("Max Value")] private float maxAttackDistance = 14.4f;
    private float optimalAttackDistance;
    [SerializeField] private NavMeshObstacle blocker;
    [SerializeField] private EnemyAttackActionComponent attack;
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
        float minimum = Mathf.Max(0.1f, minAttackDistance);
        optimalAttackDistance = Random.Range(minimum, Mathf.Max(minimum, maxAttackDistance));
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
                if (attack == null || !attack.CanAttack()) break;
                yield return attack.TryRangedAttack(target, optimalAttackDistance);
                if (this == null) yield break;
                if (!attack.ShotFired) break;
                continue;
            }

            if (!movement.TryMoveTowardTarget(target, optimalAttackDistance)) break;
            while (movement != null && movement.IsMoving) yield return null;
            if (this == null) yield break;
            if (movement.MoveFailed) break;
        }

        agent.enabled = false;
        blocker.enabled = true;
        unitTurn.EndTurn();
    }
}
