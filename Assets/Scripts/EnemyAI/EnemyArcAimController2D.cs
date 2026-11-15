using UnityEngine;

public class EnemyArcAimController2D : MonoBehaviour
{
    [SerializeField] private GameObject visual;
    [SerializeField] private LineRenderer scanLine;
    [SerializeField] private SpriteRenderer arcBackground;
    [SerializeField, Min(0.1f)] private float scanLineLength = 8f;

    private PlayerUnit target;
    private float angleWidth;
    private float scanSpeed;
    private float scanPosition;
    private float timeLeft;
    public bool IsAiming { get; private set; }
    public Vector2 CurrentScanDirection { get; private set; }

    public bool StartAim(PlayerUnit player, float angle, float speed, Vector2 timeRange, float attackDistance)
    {
        if (player == null || visual == null || scanLine == null || arcBackground == null || arcBackground.sprite == null) return false;
        target = player;
        scanLineLength = Mathf.Max(0.1f, attackDistance);
        Sprite sprite = arcBackground.sprite;
        float scale = scanLineLength / sprite.bounds.size.x;
        arcBackground.transform.localScale = Vector3.one * scale;
        arcBackground.transform.localPosition = new Vector3(-sprite.bounds.min.x * scale, -sprite.bounds.center.y * scale, 0f);
        Vector2 direction = (Vector2)(player.transform.position - transform.position);
        float aimAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        visual.transform.rotation = Quaternion.Euler(0f, 0f, aimAngle);
        angleWidth = Mathf.Clamp(angle, 0f, 180f);
        scanSpeed = Mathf.Max(0f, speed);
        scanPosition = 0f;
        float minimum = Mathf.Max(0.1f, timeRange.x);
        timeLeft = Random.Range(minimum, Mathf.Max(minimum, timeRange.y));
        visual.SetActive(true);
        IsAiming = true;
        UpdateScanLine();
        return true;
    }

    private void Update()
    {
        if (!IsAiming) return;
        if (target == null || target.GetComponent<HealthComponent>().IsDead())
        {
            CancelAim();
            return;
        }
        scanPosition += scanSpeed * Time.deltaTime;
        UpdateScanLine();
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f) IsAiming = false;
    }

    private void UpdateScanLine()
    {
        float offset = Mathf.Lerp(-angleWidth * 0.5f, angleWidth * 0.5f, Mathf.PingPong(scanPosition, 1f));
        float radians = offset * Mathf.Deg2Rad;
        Vector3 localDirection = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f);
        scanLine.SetPosition(0, Vector3.zero);
        scanLine.SetPosition(1, localDirection * scanLineLength);
        CurrentScanDirection = visual.transform.TransformDirection(localDirection);
    }

    public void CancelAim()
    {
        IsAiming = false;
        if (visual != null) visual.SetActive(false);
    }

    private void OnDisable()
    {
        CancelAim();
    }
}
