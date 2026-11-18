using UnityEngine;

[DisallowMultipleComponent]
public class UnitHitFeedback : MonoBehaviour
{
    [SerializeField] private SpriteRenderer body;
    [SerializeField, Min(0.01f)] private float duration = 0.12f;
    [SerializeField, Min(0f)] private float jitterDistance = 0.18f;

    private static readonly Color RedTint = new Color(1f, 0.25f, 0.25f, 1f);
    private Color originalColor;
    private Vector3 originalPosition;
    private float remaining;

    public void Play()
    {
        if (!isActiveAndEnabled || body == null) return;
        if (remaining <= 0f)
        {
            originalColor = body.color;
            originalPosition = body.transform.localPosition;
        }

        remaining = duration;
        ApplyEffect(1f);
    }

    private void Update()
    {
        if (remaining <= 0f) return;
        remaining -= Time.deltaTime;
        if (remaining <= 0f) Restore();
        else ApplyEffect(remaining / duration);
    }

    private void OnDisable()
    {
        if (remaining > 0f) Restore();
    }

    private void ApplyEffect(float strength)
    {
        float jitter = Mathf.Cos((1f - strength) * 4f * Mathf.PI) * jitterDistance * strength;
        body.transform.localPosition = originalPosition + Vector3.right * jitter;
        body.color = Color.Lerp(originalColor, originalColor * RedTint, strength);
    }

    private void Restore()
    {
        remaining = 0f;
        if (body == null) return;
        body.transform.localPosition = originalPosition;
        body.color = originalColor;
    }
}
