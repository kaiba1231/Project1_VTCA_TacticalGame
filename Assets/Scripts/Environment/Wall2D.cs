using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Obstacle2D : MonoBehaviour
{
    private void Awake()
    {
        if (!CompareTag("Wall"))
            Debug.LogWarning("[Wall] Set Tag to 'Wall': " + name, this);

        Collider2D obstacleCollider = GetComponent<Collider2D>();
        if (obstacleCollider.isTrigger)
            Debug.LogWarning("[Wall] Turn off Is Trigger for " + name, this);
    }
}
