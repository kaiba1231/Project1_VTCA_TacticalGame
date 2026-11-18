using UnityEngine;

public class Obstacle2D : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)] private float damageReductionPercent = 30f;
    [SerializeField, Min(0f)] private float coverDistance = 1.5f;

    public float DamageReductionPercent => damageReductionPercent;
    public float CoverDistance => coverDistance;
    public Vector2 Direction => transform.up;
}
