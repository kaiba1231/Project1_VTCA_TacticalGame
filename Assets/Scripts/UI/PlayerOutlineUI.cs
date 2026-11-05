
using UnityEngine;

[RequireComponent(typeof(PlayerUnit))]
public class PlayerSelectionIndicatorUI : MonoBehaviour
{
    [SerializeField] private SpriteRenderer indicatorRenderer;
    [SerializeField] private TurnManager turnManager;

    [SerializeField] private Color availableColor = Color.yellow;
    [SerializeField] private Color unavailableColor = Color.gray;

    private PlayerUnit player;
    private bool isHovering;

    private void Awake()
    {
        player = GetComponent<PlayerUnit>();
        indicatorRenderer.enabled = false;
    }

    private void OnMouseEnter()
    {
        isHovering = true;
    }

    private void OnMouseExit()
    {
        isHovering = false;
    }

    private void Update()
    {
        if (indicatorRenderer == null || turnManager == null)
            return;

        // Hide indicator trong Enemy Turn
        if (turnManager.CurrentPhase != BattlePhase.PlayerTurn)
        {
            indicatorRenderer.enabled = false;
            return;
        }

        // Player dang Active
        if (player.IsActiveUnit)
        {
            indicatorRenderer.enabled = true;
            indicatorRenderer.color = availableColor;
            return;
        }

        // Khong hover
        if (!isHovering)
        {
            indicatorRenderer.enabled = false;
            return;
        }

        // Hover Player chua Active
        indicatorRenderer.enabled = true;

        if (player.CanAct)
            indicatorRenderer.color = availableColor;
        else
            indicatorRenderer.color = unavailableColor;
    }
}