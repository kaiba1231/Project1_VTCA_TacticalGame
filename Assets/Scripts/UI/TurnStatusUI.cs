using TMPro;
using UnityEngine;

public class TurnStatusUI : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;

    public void UpdateUI(int roundNumber, BattlePhase phase)
    {
        if (statusText == null) return;
        statusText.text = "Round: " + roundNumber +
                           "\nPhase: " + phase;
    }
}
