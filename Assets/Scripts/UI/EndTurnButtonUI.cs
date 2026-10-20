using UnityEngine;
using UnityEngine.UI;

// Gan len chinh GameObject Button "EndTurnButton" trong Canvas.
[RequireComponent(typeof(Button))]
public class EndTurnButtonUI : MonoBehaviour
{
    [SerializeField] private TurnManager turnManager;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        if (turnManager == null)
            turnManager = FindFirstObjectByType<TurnManager>();

        button.onClick.AddListener(OnEndTurnClicked);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnEndTurnClicked);
    }

    private void OnEndTurnClicked()
    {
        Debug.Log("[UI] End Turn button clicked");

        if (turnManager != null)
            turnManager.EndSelectedPlayerTurn();
    }
}
