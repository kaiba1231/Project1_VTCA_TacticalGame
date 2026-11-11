using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PlayerUnit), typeof(NavMeshAgent))]
public class MoveActionComponent : MonoBehaviour
{
    [SerializeField] private float maxUnitMoveDistance = 5f;

    private PlayerUnit player;
    private NavMeshAgent agent;
    private APComponent apComponent;
    public bool IsMoving { get; private set; }

    private void Awake()
    {
        player = GetComponent<PlayerUnit>();
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
        foreach (Collider2D hit in Physics2D.OverlapPointAll(destination))
        {
            if (hit.CompareTag("Player") || hit.CompareTag("Enemy") || hit.CompareTag("Wall"))
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

        if (agent.SetPath(path))
        {
            IsMoving = true;
        }
    }

    private void CheckArrival()
    {
        if (agent.pathPending) return;

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
