
using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
[SerializeField]
private TextMeshProUGUI scoreText;

    public void SetScore(int score)
    {
        scoreText.text = "Score: " + score;
    }

}

