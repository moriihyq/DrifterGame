using UnityEngine;

public class PlayerRun : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
        if (moveInput != 0)
            sr.flipX = moveInput < 0;
    }
} 