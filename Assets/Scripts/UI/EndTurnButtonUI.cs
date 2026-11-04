using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class EndTurnButtonUI : MonoBehaviour
{
    [SerializeField] private TurnManager turnManager;

    [SerializeField] private Button endTurnButton;


    // ---------------------- Phuong thuc functions ------------------------ //


    void Awake()
    {
        if (turnManager == null)
            turnManager = FindAnyObjectByType<TurnManager>();

        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(OnEndTurnClicked);
    }

    void OnDestroy()
    {
        if (endTurnButton != null)
            endTurnButton.onClick.RemoveListener(OnEndTurnClicked);
    }

    private void OnEndTurnClicked()
    {
        Debug.Log("[UI] End Turn button clicked");

        if (turnManager != null)
            turnManager.EndSelectedPlayerTurn();
    }

    void Update()
    {
        if (turnManager == null || endTurnButton == null)
            return;

        bool isPlayerTurn = turnManager.CurrentPhase == BattlePhase.PlayerTurn;

        PlayerUnit player = turnManager.SelectedPlayer;

        if (!isPlayerTurn || player == null || !player.CanAct)
        {
            endTurnButton.interactable = false;
            return;
        }

        MoveActionComponent move = player.GetComponent<MoveActionComponent>();

        endTurnButton.interactable = move == null || !move.IsMoving;
    }
}
