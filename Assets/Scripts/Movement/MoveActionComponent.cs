using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PlayerUnit), typeof(NavMeshAgent))]
public class MoveActionComponent : MonoBehaviour
{
    [SerializeField] private float maxUnitMoveDistance = 5f;

    private PlayerUnit player;
    private NavMeshAgent agent;
    public bool IsMoving { get; private set; }

    private void Start()
    {
        player = GetComponent<PlayerUnit>();
        agent = GetComponent<NavMeshAgent>();

        // Keep the 2D sprite upright on an XY NavMesh.
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

        if (!player.IsActiveUnit || !player.CanAct || !Input.GetMouseButtonDown(0))
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Camera camera = Camera.main;
        if (camera == null) return;

        Vector3 destination = camera.ScreenToWorldPoint(Input.mousePosition);
        destination.z = 0f; // because 2d nen lock vector z axis = 0.

        // Clicking a Player or obstacle is not a Move command.
        foreach (Collider2D hit in Physics2D.OverlapPointAll(destination))
        {
            if (hit.CompareTag("Player") || hit.CompareTag("Enemy") || hit.CompareTag("Wall"))
                return;
        }

        TryMove(destination);
    }

    public void TryMove(Vector3 destination)
    {
        if (IsMoving || !player.IsActiveUnit || !player.CanAct || !agent.isOnNavMesh)
            return;

        if (!NavMesh.SamplePosition(destination, out NavMeshHit hit, 0.1f, agent.areaMask)) // tim diem navmesh gan vi tri clclick
            return;

        NavMeshPath path = new NavMeshPath(); // tao moi navemesh path
        if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete)  // deny đích không thể đến hoàn toàn.
            return;

        float distance = 0f;

        for (int i = 1; i < path.corners.Length; i++) // tinh quang duong thuc te co the vong` qua wall va obstacle.
            distance += Vector2.Distance(path.corners[i - 1], path.corners[i]);

        if (distance < 0.1f || distance > maxUnitMoveDistance)
        {
            Debug.Log("[MOVE] Destination is too close or too far.");
            return;
        }

        if (agent.SetPath(path))
        {
            IsMoving = true;
            Debug.Log("[MOVE] Player " + player.PlayerId + " started moving.");
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
        player.TrySpendAP(1);
        Debug.Log("[MOVE]" + player.PlayerId + " arrived. -1 AP");
    }
}
