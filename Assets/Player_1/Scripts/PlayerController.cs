using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpForce = 12f;
    public int maxJumpCount = 2;
    public float crouchSpeed = 1.5f;
    public float rollSpeed = 8f;
    public float wallJumpForce = 10f;
    public float climbSpeed = 2f;

    [Header("Health Settings")]
    public int maxHealth = 10;
    public int currentHealth;

    [Header("Spell System (Extensible)")]
    public string spellCategory; // 预留法术类别接口

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;

    private bool isGrounded;
    private bool isCrouching;
    private bool isRolling;
    private bool isClimbing;
    private bool isTouchingWall;
    private int jumpCount;
    private float moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        HandleMovement();
        HandleJump();
        HandleCrouch();
        HandleRoll();
        HandleClimb();
        UpdateAnimator();
    }

    void HandleMovement()
    {
        if (isRolling || isClimbing) return;
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        if (isCrouching) speed = crouchSpeed;
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
        if (moveInput != 0)
            sr.flipX = moveInput < 0;
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isClimbing)
            {
                isClimbing = false;
                rb.gravityScale = 1;
                return;
            }
            if (jumpCount < maxJumpCount)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpCount++;
            }
            else if (isTouchingWall)
            {
                // Wall Jump
                rb.linearVelocity = new Vector2(-moveInput * wallJumpForce, jumpForce);
                jumpCount = 1;
            }
        }
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.S))
            isCrouching = true;
        if (Input.GetKeyUp(KeyCode.S))
            isCrouching = false;
    }

    void HandleRoll()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && isGrounded && !isRolling)
        {
            isRolling = true;
            float rollDir = sr.flipX ? -1 : 1;
            rb.linearVelocity = new Vector2(rollDir * rollSpeed, rb.linearVelocity.y);
            Invoke("EndRoll", 0.5f);
        }
    }
    void EndRoll() { isRolling = false; }

    void HandleClimb()
    {
        if (isTouchingWall && Input.GetKey(KeyCode.W))
        {
            isClimbing = true;
            rb.gravityScale = 0;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, climbSpeed);
        }
        else if (isClimbing && !Input.GetKey(KeyCode.W))
        {
            isClimbing = false;
            rb.gravityScale = 1;
        }
    }

    void UpdateAnimator()
    {
        if (animator == null) return;
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isCrouching", isCrouching);
        animator.SetBool("isRolling", isRolling);
        animator.SetBool("isClimbing", isClimbing);
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
    }

    void FixedUpdate()
    {
        // 检查地面和墙体
        isGrounded = Physics2D.OverlapCircle(transform.position + Vector3.down * 0.1f, 0.2f, LayerMask.GetMask("Ground"));
        isTouchingWall = Physics2D.OverlapCircle(transform.position + Vector3.right * (sr.flipX ? -0.5f : 0.5f), 0.2f, LayerMask.GetMask("Wall"));
        if (isGrounded) jumpCount = 0;
    }

    // 血量相关方法
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0)
        {
            currentHealth = 0;
            Die();
        }
    }
    void Die()
    {
        // 死亡动画和逻辑
        Debug.Log("Player Died");
        if (animator != null)
            animator.SetTrigger("Die");
    }

    // 法术类别接口预留
    public void SetSpellCategory(string category)
    {
        spellCategory = category;
        // TODO: 扩展法术系统
    }
} 