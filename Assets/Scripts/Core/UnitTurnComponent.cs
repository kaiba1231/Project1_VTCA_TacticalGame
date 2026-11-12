using UnityEngine;

// Bat dau va ket thuc luot cua Player/Enemy tren cung GameObject.
[DisallowMultipleComponent]
public class UnitTurnComponent : MonoBehaviour
{
    [SerializeField] private bool hasEndedTurn = true;

    private APComponent apComponent;
    private PlayerUnit player;
    private EnemyUnit enemy;

    public bool HasEndedTurn => hasEndedTurn;

    private void Awake()
    {
        apComponent = GetComponent<APComponent>();
        player = GetComponent<PlayerUnit>();
        enemy = GetComponent<EnemyUnit>();
    }

    public void BeginTurn()
    {
        apComponent.ResetAP();
        hasEndedTurn = false;

        if (player != null)
            player.TurnManager.Log("Player[" + player.PlayerId + "] START | AP: " + apComponent.CurrentAP);
        else if (enemy != null)
            enemy.TurnManager.Log("Enemy[" + enemy.EnemyId + "] START | AP: " + apComponent.CurrentAP);
    }

    public void EndTurn()
    {
        if (hasEndedTurn) return;

        if (player != null)
        {
            if (player.TurnManager == null || !player.IsActiveUnit) return;

            hasEndedTurn = true;
            player.TurnManager.Log("Player[" + player.PlayerId + "] END TURN | AP left: " + apComponent.CurrentAP);
            player.TurnManager.OnPlayerEnded(player);
        }
        else if (enemy != null)
        {
            hasEndedTurn = true;
            enemy.TurnManager.Log("Enemy[" + enemy.EnemyId + "] END TURN");
        }
    }
}
