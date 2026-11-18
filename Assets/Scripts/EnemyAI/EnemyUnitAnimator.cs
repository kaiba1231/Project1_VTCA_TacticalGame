using UnityEngine;

[DefaultExecutionOrder(100)]
public class EnemyUnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private EnemyMoveActionComponent movement;
    [SerializeField] private bool facesLeftByDefault;

    private static readonly int Moving = Animator.StringToHash("Moving");
    private static readonly int MeleePose = Animator.StringToHash("MeleePose");
    private float meleePoseUntil;
    private bool moveFlip;

    private void Awake() => moveFlip = body.flipX;

    public void FaceDirection(float horizontal)
    {
        if (Mathf.Abs(horizontal) > 0.001f)
        {
            moveFlip = (horizontal < 0f) != facesLeftByDefault;
            body.flipX = moveFlip;
        }
    }

    public void PlayMeleeAttack(Vector2 direction)
    {
        meleePoseUntil = ActionDelay.EndTime;
        animator.SetInteger(MeleePose, direction.y >= 0f ? 1 : 2);
        body.flipX = direction.x < 0f;
    }

    private void Update()
    {
        animator.SetBool(Moving, movement.isActiveAndEnabled && movement.IsFollowingPath);
        if (meleePoseUntil == 0f || (Time.time < meleePoseUntil && ActionDelay.EndTime == meleePoseUntil)) return;

        meleePoseUntil = 0f;
        animator.SetInteger(MeleePose, 0);
        body.flipX = moveFlip;
    }
}
