using UnityEngine;

// Gan len HUD Canvas dang active; PausePanel la child mac dinh inactive.
public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject pausePanel;

    public void Show(bool visible)
    {
        pausePanel.SetActive(visible);
    }

    public void OnResumeClicked()
    {
        gameManager.ResumeGame();
    }

    public void OnQuitToMenuClicked()
    {
        gameManager.QuitToMenu();
    }
}
