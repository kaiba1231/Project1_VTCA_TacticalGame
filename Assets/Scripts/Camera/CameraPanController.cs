using UnityEngine;

[RequireComponent(typeof(Camera))]
[DefaultExecutionOrder(-100)]
public class CameraPanController : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 20f;

    private void Update()
    {
        Vector3 direction = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) direction += Vector3.up;
        if (Input.GetKey(KeyCode.S)) direction += Vector3.down;
        if (Input.GetKey(KeyCode.A)) direction += Vector3.left;
        if (Input.GetKey(KeyCode.D)) direction += Vector3.right;

        if (direction != Vector3.zero)
            transform.position += direction.normalized * (moveSpeed * Time.deltaTime);
    }
}
