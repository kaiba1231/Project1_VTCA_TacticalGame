using UnityEngine;

[DefaultExecutionOrder(100)]
public class PlayerUnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer body;

    private static readonly int Pose = Animator.StringToHash("Pose");
    private bool moveFacingLeft;
    private bool aiming;
    private bool aimFacingLeft;
    private float aimPose;
    private float shotPoseUntil;

    public void SetMoveDirection(float horizontal)
    {
        if (Mathf.Abs(horizontal) > 0.001f)
            moveFacingLeft = horizontal < 0f;
    }

    public void SetAimAngle(float angle)
    {
        aiming = true;
        float absoluteAngle = Mathf.Abs(angle);
        bool side = absoluteAngle <= 45f || absoluteAngle >= 135f;
        aimPose = side ? 0f : Mathf.Sign(angle);
        aimFacingLeft = absoluteAngle >= 135f;
    }

    public void ClearAim() => aiming = false;

    public void HoldShotPose() => shotPoseUntil = ActionDelay.EndTime;

    private void Update()
    {
        bool holdingShotPose = Time.time < shotPoseUntil && ActionDelay.EndTime == shotPoseUntil;
        bool showAimPose = aiming || holdingShotPose;
        animator.SetFloat(Pose, showAimPose ? aimPose : 0f);
        body.flipX = showAimPose ? aimFacingLeft : moveFacingLeft;
    }
}
