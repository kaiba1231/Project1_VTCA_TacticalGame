using UnityEngine;
using UnityEngine.UI;

// UI forwards the click to the selected unit.
[RequireComponent(typeof(Button))]
public class ShootButtonUI : MonoBehaviour
{
    [SerializeField] private TurnManager turnManager;

    public void OnShootClicked()
    {
        if (turnManager == null || turnManager.CurrentPhase != BattlePhase.PlayerTurn)
            return;

        PlayerUnit player = turnManager.SelectedPlayer;
        if (player == null || !player.CanAct) return;

        PlayerAttackActionComponent attack = player.GetComponent<PlayerAttackActionComponent>();
        if (attack == null)
        {
            Debug.LogWarning("[SHOOT] Player chua gan PlayerAttackActionComponent.", player);
            return;
        }

        attack.TryRangedAttack();
    }
}
