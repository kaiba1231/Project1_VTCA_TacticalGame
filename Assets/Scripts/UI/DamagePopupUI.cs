using TMPro;
using UnityEngine;

// Attach to the unit's UI_UnitCanvas and assign its DamagePopup TMP child.
public class DamagePopupUI : MonoBehaviour
{
    [SerializeField] private TMP_Text popupText;
    private float displayDuration = 1f;
    private float riseSpeed = 5f;
    private float timeLeft;
    private Vector2 startPosition;

    private void Awake()
    {
        if (popupText != null) startPosition = popupText.rectTransform.anchoredPosition;
    }

    public void Show(int damage)
    {
        if (popupText == null || damage <= 0) return;
        popupText.rectTransform.anchoredPosition = startPosition;
        popupText.text = damage.ToString();
        timeLeft = displayDuration;
        popupText.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (popupText == null || !popupText.gameObject.activeSelf) return;
        popupText.rectTransform.position += Vector3.up * riseSpeed * Time.deltaTime;
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            popupText.gameObject.SetActive(false);
            popupText.rectTransform.anchoredPosition = startPosition;
        }
    }
}
