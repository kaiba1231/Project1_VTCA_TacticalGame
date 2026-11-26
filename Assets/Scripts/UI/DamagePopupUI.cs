using TMPro;
using UnityEngine;

// Attach to the unit's UI_UnitCanvas and assign its DamagePopup TMP child.
public class DamagePopupUI : MonoBehaviour
{
    [SerializeField] private TMP_Text popupText;
    [SerializeField] private TMP_Text blockText;
    private float displayDuration = 1f;
    private float riseSpeed = 5f;
    private float timeLeft;
    private float blockHideTime;
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

    public void ShowBlock()
    {
        if (blockText == null) return;
        blockHideTime = Time.time + 0.2f;
        blockText.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (blockText != null && blockText.gameObject.activeSelf && Time.time >= blockHideTime)
            blockText.gameObject.SetActive(false);

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
