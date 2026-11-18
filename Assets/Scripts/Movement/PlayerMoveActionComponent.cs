using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PlayerUnit), typeof(NavMeshAgent))]
public class PlayerMoveActionComponent : MonoBehaviour
{
    [SerializeField] private float maxUnitMoveDistance = 5f;

    private PlayerUnit player;
    private PlayerUnitAnimator unitAnimator;
    private NavMeshAgent agent;
    private APComponent apComponent;
    private bool waitingForDestinationPath;
    public bool IsMoving { get; private set; }

    private void Awake()
    {
        player = GetComponent<PlayerUnit>();
        unitAnimator = GetComponent<PlayerUnitAnimator>();
        agent = GetComponent<NavMeshAgent>();
        apComponent = GetComponent<APComponent>();
    }

    private void Start()
    {
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        if (IsMoving)
        {
            CheckArrival();
            return;
        }

        if (ActionDelay.IsWaiting || !player.IsActiveUnit || !player.CanAct || !Input.GetMouseButtonDown(0))
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Camera camera = Camera.main;
        if (camera == null) return;

        Vector3 destination = camera.ScreenToWorldPoint(Input.mousePosition);
        destination.z = 0f;

        if (IsBlocked(destination)) return;
        TryMove(destination);
    }

    // Player va preview cung dung chung cach kiem tra vat can.
    public bool IsBlocked(Vector3 destination)
    {
        int obstacleLayer = LayerMask.NameToLayer("Obstacle");
        foreach (Collider2D hit in Physics2D.OverlapPointAll(destination))
        {
            if (hit.gameObject.layer == obstacleLayer || hit.CompareTag("Player") || hit.CompareTag("Enemy") || hit.CompareTag("Wall"))
                return true;
        }
        return false;
    }

    // Tim duong va do chieu dai duong di. Ham nay chua di chuyen unit.
    public bool CalculateMovePath(Vector3 destination, NavMeshPath path, out float distance)
    {
        distance = 0f;
        if (ActionDelay.IsWaiting || player.TurnManager == null || player.TurnManager.CurrentPhase != BattlePhase.PlayerTurn || player.TurnManager.SelectedPlayer != player || IsMoving || !player.IsActiveUnit || !player.CanAct || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
            return false;

        NavMeshHit hit;
        if (!NavMesh.SamplePosition(destination, out hit, 0.1f, agent.areaMask))
            return false;

        if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete)
            return false;

        Vector3[] corners = path.corners;
        for (int i = 1; i < corners.Length; i++)
            distance += Vector2.Distance(corners[i - 1], corners[i]);

        return true;
    }

    public bool IsMoveDistanceValid(float distance)
    {
        return distance >= 0.1f && distance <= maxUnitMoveDistance;
    }

    public void TryMove(Vector3 destination)
    {
        NavMeshPath path = new NavMeshPath();
        float distance;
        if (!CalculateMovePath(destination, path, out distance)) return;

        if (!IsMoveDistanceValid(distance))
        {
            return;
        }

        if (!agent.SetPath(path))
        {
            // Pause movement until the asynchronously calculated path is validated.
            agent.isStopped = true;
            Vector3 endpoint = path.corners[path.corners.Length - 1];
            if (!agent.SetDestination(endpoint))
            {
                agent.isStopped = false;
                return;
            }
            waitingForDestinationPath = true;
        }
        if (!waitingForDestinationPath && unitAnimator != null)
            unitAnimator.SetMoveDirection(destination.x - transform.position.x);

        IsMoving = true;
    }

    private void CheckArrival()
    {
        if (agent.pathPending) return;

        if (waitingForDestinationPath)
        {
            waitingForDestinationPath = false;
            Vector3[] corners = agent.path.corners;
            float distance = 0f;
            for (int i = 1; i < corners.Length; i++)
                distance += Vector2.Distance(corners[i - 1], corners[i]);

            if (!agent.hasPath || agent.pathStatus != NavMeshPathStatus.PathComplete || !IsMoveDistanceValid(distance))
            {
                agent.ResetPath();
                agent.isStopped = false;
                IsMoving = false;
                return;
            }
            if (unitAnimator != null && corners.Length > 0)
                unitAnimator.SetMoveDirection(corners[corners.Length - 1].x - transform.position.x);
            agent.isStopped = false;
        }

        if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            agent.ResetPath();
            IsMoving = false;
            Debug.LogWarning("[MOVE] Path was cancelled.");
            return;
        }

        if (agent.remainingDistance > agent.stoppingDistance + 0.1f)
            return;

        agent.ResetPath();
        IsMoving = false;
        if (player.TurnManager != null && player.TurnManager.CurrentPhase == BattlePhase.PlayerTurn && player.TurnManager.SelectedPlayer == player && player.IsActiveUnit && player.CanAct && apComponent.TrySpendAP(1))
            Debug.Log("[MOVE]" + player.PlayerId + " arrived. -1 AP");
    }
}
