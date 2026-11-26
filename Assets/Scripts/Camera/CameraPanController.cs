using UnityEngine;

[RequireComponent(typeof(Camera))]
[DefaultExecutionOrder(-100)]
public class CameraPanController : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 20f;
    [SerializeField] private TurnManager turnManager;
    [SerializeField, Min(0.01f)] private float focusSmoothTime = 0.25f;

    private bool wasFollowingEnemy;
    private Vector3 focusVelocity;
    private PlayerUnit lastSelectedPlayer;
    private Vector3 playerFocusStart;
    private float playerFocusRemaining;

    private bool IsEnemyTurn => turnManager != null && turnManager.CurrentPhase == BattlePhase.EnemyTurn;

    private void Update()
    {
        if (Time.timeScale == 0f || IsEnemyTurn) return;

        if (FollowSelectedPlayer()) return;

        Vector3 direction = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) direction += Vector3.up;
        if (Input.GetKey(KeyCode.S)) direction += Vector3.down;
        if (Input.GetKey(KeyCode.A)) direction += Vector3.left;
        if (Input.GetKey(KeyCode.D)) direction += Vector3.right;

        if (direction != Vector3.zero)
            transform.position += direction.normalized * (moveSpeed * Time.deltaTime);
    }

    private bool FollowSelectedPlayer()
    {
        PlayerUnit selectedPlayer = turnManager != null && turnManager.CurrentPhase == BattlePhase.PlayerTurn
            ? turnManager.SelectedPlayer
            : null;

        if (selectedPlayer == null)
        {
            playerFocusRemaining = 0f;
            return false;
        }

        if (wasFollowingEnemy || (lastSelectedPlayer != null && lastSelectedPlayer != selectedPlayer))
        {
            wasFollowingEnemy = false;
            focusVelocity = Vector3.zero;
            playerFocusStart = transform.position;
            playerFocusRemaining = focusSmoothTime;
        }

        lastSelectedPlayer = selectedPlayer;

        if (playerFocusRemaining <= 0f) return false;

        playerFocusRemaining = Mathf.Max(0f, playerFocusRemaining - Time.deltaTime);
        float progress = 1f - playerFocusRemaining / focusSmoothTime;
        transform.position = Vector3.Lerp(playerFocusStart, GetFocusPosition(selectedPlayer.transform), Mathf.SmoothStep(0f, 1f, progress));
        return playerFocusRemaining > 0f;
    }

    private void LateUpdate()
    {
        if (Time.timeScale == 0f || !IsEnemyTurn || turnManager.CurrentEnemy == null) return;

        wasFollowingEnemy = true;
        Vector3 target = GetFocusPosition(turnManager.CurrentEnemy.transform);
        transform.position = Vector3.SmoothDamp(transform.position, target, ref focusVelocity, focusSmoothTime);
    }

    private Vector3 GetFocusPosition(Transform target)
    {
        Vector3 position = target.position;
        position.z = transform.position.z;
        return position;
    }
}
