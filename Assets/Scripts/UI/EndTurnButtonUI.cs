using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class EndTurnButtonUI : MonoBehaviour
{
    [SerializeField] private TurnManager turnManager;

    private Button button;


    // ---------------------- Phuong thuc functions ------------------------ //


    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        if (turnManager == null)
            turnManager = FindAnyObjectByType<TurnManager>();

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
