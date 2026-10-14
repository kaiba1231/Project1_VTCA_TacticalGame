using System;
using UnityEngine;
using UnityEngine.Video;

public class PlayerMovementComponent : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 direction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        direction = new(horizontal, vertical);
        if (direction.sqrMagnitude > 1f)
        {
            direction.Normalize();
        }
    }

  
    void FixedUpdate()
    {
        Vector2 targetPosition = rb.position + (moveSpeed *Time.fixedDeltaTime* direction);

        rb.MovePosition(targetPosition);        
    }
    
}
