using UnityEngine;

[DefaultExecutionOrder(100)]
public class EnemyUnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private EnemyMoveActionComponent movement;
    [SerializeField] private bool facesLeftByDefault;
    [SerializeField] private PolygonCollider2D facingCollider;

    private static readonly int Moving = Animator.StringToHash("Moving");
    private static readonly int Shoot = Animator.StringToHash("Base Layer.Shoot");
    private static readonly int MeleePose = Animator.StringToHash("MeleePose");
    private float meleePoseUntil;
    private bool moveFlip;

    private Vector2[] facingPath;
    private Vector2[] flippedFacingPath;

    private void Awake()
    {
        moveFlip = body.flipX;
        if (facingCollider == null) return;

        facingPath = facingCollider.GetPath(0);
        flippedFacingPath = new Vector2[facingPath.Length];
        for (int i = 0; i < facingPath.Length; i++)
        {
            Vector2 point = facingPath[facingPath.Length - 1 - i];
            flippedFacingPath[i] = new Vector2(-point.x, point.y);
        }

        if (moveFlip) facingCollider.SetPath(0, flippedFacingPath);
    }

    private void SetFacing(bool flipped)
    {
        if (body.flipX == flipped) return;

        body.flipX = flipped;
        if (facingCollider != null)
            facingCollider.SetPath(0, flipped ? flippedFacingPath : facingPath);
    }

    public void FaceDirection(float horizontal)
    {
        if (Mathf.Abs(horizontal) > 0.001f)
        {
            moveFlip = (horizontal < 0f) != facesLeftByDefault;
            SetFacing(moveFlip);
        }
    }

    public void PlayMeleeAttack(Vector2 direction)
    {
        meleePoseUntil = ActionDelay.EndTime;
        animator.SetInteger(MeleePose, direction.y >= 0f ? 1 : 2);
        SetFacing(direction.x < 0f);
    }

    public void PlayShoot(Vector2 direction)
    {
        FaceDirection(direction.x);

        if (animator.HasState(0, Shoot))
            animator.Play(Shoot, 0, 0f);
    }

    private void Update()
    {
        animator.SetBool(Moving, movement.isActiveAndEnabled && movement.IsFollowingPath);
        if (meleePoseUntil == 0f || (Time.time < meleePoseUntil && ActionDelay.EndTime == meleePoseUntil)) return;

        meleePoseUntil = 0f;
        animator.SetInteger(MeleePose, 0);
        SetFacing(moveFlip);
    }
}
