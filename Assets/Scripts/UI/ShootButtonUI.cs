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

        AttackActionComponent attack = player.GetComponent<AttackActionComponent>();
        if (attack == null)
        {
            Debug.LogWarning("[SHOOT] Player chua gan AttackActionComponent.", player);
            return;
        }

        attack.TryRangedAttack();
    }
}
