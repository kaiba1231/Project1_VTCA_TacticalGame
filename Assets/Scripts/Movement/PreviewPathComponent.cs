using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PlayerUnit), typeof(MoveActionComponent))]
public class PreviewPathComponent : MonoBehaviour
{
    [SerializeField] private LineRenderer line;
    [SerializeField] private Color validColor = Color.cyan;
    [SerializeField] private Color tooFarColor = Color.red;

    private PlayerUnit player;
    private MoveActionComponent moveComponent;
    private NavMeshPath path;


    private void Awake()
    {
        player = GetComponent<PlayerUnit>();
        moveComponent = GetComponent<MoveActionComponent>();
        path = new NavMeshPath();

        // Keo LineRenderer cua PathLine vao Inspector.
        if (line == null)
        {
            Debug.LogError("[PREVIEW] Chua gan LineRenderer cho " + name, this);
            enabled = false;
            return;
        }

        line.positionCount = 0;
    }

    private void Update()
    {
        // An ngay khi Player khong con duoc dieu khien.
        if (!player.IsActiveUnit || !player.CanAct || moveComponent.IsMoving ||
            (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())) //  tránh việc bấm nút UI mà Player cũng nhận lệnh di chuyển.
        {
            line.positionCount = 0;
            return;
        }


        Camera camera = Camera.main;
        Vector3 destination = camera.ScreenToWorldPoint(Input.mousePosition);
        destination.z = 0f;

        float distance;
        if (moveComponent.IsBlocked(destination) || !moveComponent.CalculateMovePath(destination, path, out distance) || distance < 0.1f) // neu con tro chuot trung cac object can thi an preview
        {
            line.positionCount = 0;
            return;
        }

        // Xanh: di duoc. Do: duong vuot qua tam di chuyen.
        Color color = moveComponent.IsMoveDistanceValid(distance) ? validColor : tooFarColor;
        line.startColor = color;
        line.endColor = color;

        Vector3[] corners = path.corners;
        line.positionCount = corners.Length;
        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 point = corners[i];
            line.SetPosition(i, point);
        }
    }

    private void OnDisable()
    {
        if (line != null) line.positionCount = 0;
    }

}
