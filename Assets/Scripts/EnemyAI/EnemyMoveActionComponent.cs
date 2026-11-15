using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyUnit), typeof(NavMeshAgent), typeof(APComponent))]
public class EnemyMoveActionComponent : MonoBehaviour
{
    [SerializeField] private float maxMoveDistance = 20f;
    [SerializeField] private int moveAPCost = 1;
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField, Min(0f)] private float positionSpacing = 0.25f;

    private static readonly Dictionary<EnemyMoveActionComponent, Vector3> reservedPositions = new Dictionary<EnemyMoveActionComponent, Vector3>();
    private PlayerUnit reservedTarget;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetReservations()
    {
        reservedPositions.Clear();
    }

    public void ReleasePosition()
    {
        reservedPositions.Remove(this);
        reservedTarget = null;
    }

    private void OnDisable()
    {
        ReleasePosition();
    }

    public void ReserveCurrentPosition(PlayerUnit target)
    {
        reservedPositions[this] = transform.position;
        reservedTarget = target;
    }

    private bool IsPositionAvailable(Vector3 point)
    {
        foreach (var reservation in reservedPositions)
        {
            EnemyMoveActionComponent owner = reservation.Key;
            if (owner == null || owner == this || !owner.isActiveAndEnabled || owner.GetComponent<HealthComponent>().IsDead()) continue;
            float spacing = agent.radius + owner.agent.radius + Mathf.Max(positionSpacing, owner.positionSpacing);
            if (Vector2.Distance(point, reservation.Value) < spacing) return false;
        }
        foreach (EnemyUnit other in enemy.TurnManager.enemyUnits)
        {
            if (other == null || other == enemy || !other.gameObject.activeInHierarchy || other.GetComponent<HealthComponent>().IsDead()) continue;
            NavMeshAgent otherAgent = other.GetComponent<NavMeshAgent>();
            if (otherAgent == null) continue;
            EnemyMoveActionComponent otherMove = other.GetComponent<EnemyMoveActionComponent>();
            float gap = otherMove != null ? Mathf.Max(positionSpacing, otherMove.positionSpacing) : positionSpacing;
            if (Vector2.Distance(point, other.transform.position) < agent.radius + otherAgent.radius + gap) return false;
        }
        return true;
    }

    [SerializeField] private bool showRaycastGizmos = true;
    private Vector3 rayOrigin;
    private Vector3[] rayPoints;
    private bool[] rayBlocked;
    private Vector3[] selectedPath;
    private PlayerUnit debugTarget;

    [SerializeField] private EnemyUnit enemy;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private APComponent apComponent;
    public bool IsMoving { get; private set; }
    public bool MoveFailed { get; private set; }
    private bool waitingForDestinationPath;

    private void Awake()
    {
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    public PlayerUnit FindNearestPlayer()
    {
        PlayerUnit nearest = null;
        float closest = float.MaxValue;
        foreach (PlayerUnit player in enemy.TurnManager.playerUnits)
        {
            if (player == null || player.GetComponent<HealthComponent>().IsDead()) continue;
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance < closest) { closest = distance; nearest = player; }
        }
        debugTarget = nearest;
        return nearest;
    }

    public bool HasClearAttackPosition(PlayerUnit target, float optimalDistance)
    {
        if (target == null) return false;
        float distance = Vector2.Distance(transform.position, target.transform.position);
        return distance <= optimalDistance + agent.stoppingDistance + 0.1f
            && Physics2D.Linecast(transform.position, target.transform.position, obstacleLayers).collider == null
            && IsPositionAvailable(transform.position);
    }

    public bool TryMoveTowardTarget(PlayerUnit target, float optimalDistance)
    {
        TurnManager turns = enemy.TurnManager;
        if (target == null || target.GetComponent<HealthComponent>().IsDead() || turns == null || turns.CurrentPhase != BattlePhase.EnemyTurn || turns.CurrentEnemy != enemy || enemy.HasEndedTurn) return false;
        if (IsMoving || optimalDistance <= 0f || maxMoveDistance <= 0f || !apComponent.HasEnoughAP(moveAPCost) || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return false;

        rayOrigin = target.transform.position;
        rayOrigin.z = agent.nextPosition.z; // Keep 2D path queries on the agent NavMesh plane.
        rayPoints = new Vector3[16];
        rayBlocked = new bool[16];
        selectedPath = null;
        debugTarget = target;
        float shortestPath = float.MaxValue;
        NavMeshPath bestPath = null;
        Vector3 reservedPoint;
        if (reservedTarget == target && reservedPositions.TryGetValue(this, out reservedPoint)
            && Vector2.Distance(rayOrigin, reservedPoint) <= optimalDistance + 0.1f
            && Physics2D.Linecast(rayOrigin, reservedPoint, obstacleLayers).collider == null
            && IsPositionAvailable(reservedPoint))
        {
            NavMeshPath savedPath = new NavMeshPath();
            if (agent.CalculatePath(reservedPoint, savedPath) && savedPath.status == NavMeshPathStatus.PathComplete)
                bestPath = savedPath;
        }
        bool keepReservedPoint = bestPath != null;
        if (!keepReservedPoint) ReleasePosition();
        for (int i = 0; i < 16; i++)
        {
            float angle = i * 22.5f * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            Vector3 point = rayOrigin + direction * optimalDistance;
            rayPoints[i] = point;
            rayBlocked[i] = Physics2D.Linecast(rayOrigin, point, obstacleLayers).collider != null;
            if (rayBlocked[i] || keepReservedPoint) continue;

            NavMeshHit navHit;
            if (!NavMesh.SamplePosition(point, out navHit, 0.2f, agent.areaMask)) continue;
            // Sampling can shift the endpoint; check its range and line of sight again.
            if (Vector2.Distance(rayOrigin, navHit.position) > optimalDistance + 0.1f) continue;
            if (Physics2D.Linecast(rayOrigin, navHit.position, obstacleLayers).collider != null) continue;

            if (!IsPositionAvailable(navHit.position)) continue;
            NavMeshPath path = new NavMeshPath();
            if (!agent.CalculatePath(navHit.position, path) || path.status != NavMeshPathStatus.PathComplete) continue;
            float distance = 0f;
            Vector3[] pathCorners = path.corners;
            for (int j = 1; j < pathCorners.Length; j++)
                distance += Vector3.Distance(pathCorners[j - 1], pathCorners[j]);
            if (pathCorners.Length == 0 || distance >= shortestPath) continue;
            shortestPath = distance;
            bestPath = path;
        }
        if (bestPath == null) return false;

        selectedPath = bestPath.corners;
        Vector3[] corners = bestPath.corners;
        float remaining = maxMoveDistance;
        Vector3 destination = corners[0];
        for (int i = 1; i < corners.Length; i++)
        {
            float length = Vector3.Distance(corners[i - 1], corners[i]);
            if (length >= remaining)
            {
                destination = Vector3.MoveTowards(corners[i - 1], corners[i], remaining);
                break;
            }
            remaining -= length;
            destination = corners[i];
        }
        if (Vector2.Distance(transform.position, destination) < 0.1f) return false;
        NavMeshPath movePath = new NavMeshPath();
        if (!agent.CalculatePath(destination, movePath) || movePath.status != NavMeshPathStatus.PathComplete) return false;
        if (!agent.SetPath(movePath))
        {
            agent.isStopped = true;
            if (!agent.SetDestination(destination))
            {
                agent.isStopped = false;
                return false;
            }
            waitingForDestinationPath = true;
        }
        MoveFailed = false;
        reservedPositions[this] = corners[corners.Length - 1];
        reservedTarget = target;
        IsMoving = true;
        return true;
    }

    private void Update()
    {
        if (!IsMoving || Time.timeScale == 0f || agent.pathPending) return;
        if (waitingForDestinationPath)
        {
            waitingForDestinationPath = false;
            Vector3[] corners = agent.path.corners;
            float distance = 0f;
            for (int i = 1; i < corners.Length; i++)
                distance += Vector3.Distance(corners[i - 1], corners[i]);

            if (!agent.hasPath || agent.pathStatus != NavMeshPathStatus.PathComplete || distance < 0.1f || distance > maxMoveDistance)
            {
                agent.ResetPath();
                agent.isStopped = false;
                IsMoving = false;
                MoveFailed = true;
                ReleasePosition();
                return;
            }
            agent.isStopped = false;
        }
        if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            agent.ResetPath();
            IsMoving = false;
            MoveFailed = true;
            ReleasePosition();
            return;
        }
        if (agent.remainingDistance > agent.stoppingDistance + 0.1f) return;
        agent.ResetPath();
        IsMoving = false;
        apComponent.TrySpendAP(moveAPCost);
    }

    private void OnDrawGizmos()
    {
        if (!showRaycastGizmos || rayPoints == null) return;
        for (int i = 0; i < rayPoints.Length; i++)
        {
            Gizmos.color = rayBlocked[i] ? Color.red : Color.yellow;
            Gizmos.DrawLine(rayOrigin, rayPoints[i]);
            Gizmos.DrawWireSphere(rayPoints[i], 0.15f);
        }
        if (debugTarget != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(debugTarget.transform.position, 0.5f);
        }
        if (selectedPath == null || selectedPath.Length == 0) return;
        Gizmos.color = Color.green;
        for (int i = 1; i < selectedPath.Length; i++)
            Gizmos.DrawLine(selectedPath[i - 1], selectedPath[i]);
        Gizmos.DrawWireSphere(selectedPath[selectedPath.Length - 1], 0.3f);
    }

}
