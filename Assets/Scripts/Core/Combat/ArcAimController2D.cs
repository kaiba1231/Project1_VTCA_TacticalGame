using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PlayerUnit))]
public class ArcAimController2D : MonoBehaviour
{
    private PlayerUnit player;
    private PreviewPathComponent preview;
    private MoveActionComponent movement;
    private bool restoreMovement;
    private bool restorePreview;
    private GameObject visual;
    private LineRenderer scanLine;
    private Material lineMaterial;
    private float radius;
    private float angleWidth;
    private float scanSpeed;
    private float aimAngle;
    private float scanPosition;
    private const float scanLineWidth = 0.2f;
    private const float scanLineLength = 8f;



    public Vector2 CurrentScanDirection { get; private set; }

    private void Awake() { player = GetComponent<PlayerUnit>(); }

    public void Initialize(Sprite sprite, float newRadius, float newAngle, float newSpeed, float arcScale)
    {
        movement = GetComponent<MoveActionComponent>();
        restoreMovement = movement != null && movement.enabled;
        if (restoreMovement) movement.enabled = false;

        preview = GetComponent<PreviewPathComponent>();
        restorePreview = preview != null && preview.enabled;
        if (restorePreview) preview.enabled = false;


        radius = Mathf.Max(0.2f, newRadius);
        angleWidth = Mathf.Clamp(newAngle, 0f, 180f);
        scanSpeed = Mathf.Max(0f, newSpeed);

        visual = new GameObject("ArcAimVisual"); // tao arc aim 2d script component gắng vào unit script
        visual.transform.SetParent(transform, false);
        Shader shader = Shader.Find("Sprites/Default");

        GameObject arcbackground = new GameObject("ArcBackground");
        arcbackground.transform.SetParent(visual.transform, false);
        arcbackground.transform.localScale = Vector3.one * arcScale;
        arcbackground.transform.localPosition = new Vector3(-sprite.bounds.min.x * arcScale, -sprite.bounds.center.y * arcScale, 0f);

        SpriteRenderer arc = arcbackground.AddComponent<SpriteRenderer>();
        arc.sprite = sprite;
        arc.color = Color.lightBlue;
        arc.sortingOrder = 99;

        GameObject lineObject = new GameObject("AimLine");
        lineObject.transform.SetParent(visual.transform, false);
        scanLine = lineObject.AddComponent<LineRenderer>();


        lineMaterial = new Material(shader);

        scanLine.sharedMaterial = lineMaterial;
        scanLine.startColor = scanLine.endColor = Color.black;
        scanLine.startWidth = scanLine.endWidth = scanLineWidth;
        scanLine.useWorldSpace = true;
        scanLine.sortingOrder = 100;
        scanLine.positionCount = 2;
    }

    private void Update()
    {
        if (scanLine == null || !player.IsActiveUnit || !player.CanAct)
        {
            CancelAim();
            return;
        }
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            CancelAim();
            return;
        }

        Camera cam = Camera.main;
        if (cam == null) return;
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 origin = transform.position;
        Vector2 toMouse = (Vector2)mouseWorld - origin;
        if (toMouse.sqrMagnitude > 0.001f)
            aimAngle = Mathf.Atan2(toMouse.y, toMouse.x) * Mathf.Rad2Deg;

        visual.transform.rotation = Quaternion.Euler(0f, 0f, aimAngle);
        scanPosition += scanSpeed * Time.deltaTime;
        float offset = Mathf.Lerp(-angleWidth * 0.5f, angleWidth * 0.5f, Mathf.PingPong(scanPosition, 1f));
        float radians = (aimAngle + offset) * Mathf.Deg2Rad;
        CurrentScanDirection = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        scanLine.SetPosition(0, new Vector3(origin.x, origin.y, 0f));
        Vector2 end = origin + CurrentScanDirection * scanLineLength;
        scanLine.SetPosition(1, new Vector3(end.x, end.y, 0f));

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            player.TrySpendAP(1);

            Debug.Log("[ARC] Player " + player.PlayerId + " aim direction: " + CurrentScanDirection);
            CancelAim();
        }
    }

    public void CancelAim() { Destroy(this); }

    private void OnDestroy()
    {
        if (restoreMovement && movement != null) movement.enabled = true;
        if (restorePreview && preview != null) preview.enabled = true;
        if (visual != null) Destroy(visual);
        if (lineMaterial != null) Destroy(lineMaterial);
    }
}
