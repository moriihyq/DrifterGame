using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // 移动速度
    public float jumpForce = 10f; // 跳跃力度
    public int money = 0;

    private Rigidbody2D rb;
    private bool isGrounded; // 是否在地面上
    private float horizontalInput;

    // 用于地面检测
    public Transform groundCheck; // 在Player下创建一个空物体作为脚底位置
    public LayerMask groundLayer; // 设置哪些层是地面
    public float groundCheckRadius = 0.2f; // 检测范围半径

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // 获取刚体组件
    }

    void Update()
    {
        // 获取水平输入 (-1, 0, 1)
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 地面检测 (画一个看不见的圆圈在脚底，看是否碰到地面层)
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 跳跃 (只有在地面上才能跳)
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); // 使用速度改变更直接
            // 或者使用 AddForce: rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        // (可选) 简单的翻转角色朝向
        Flip();
    }

    void FixedUpdate()
    {
        // 物理相关的移动放在 FixedUpdate 中
        // 使用速度控制左右移动
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    void Flip()
    {
         // 根据移动方向翻转角色 Transform 的 scale.x
        if (horizontalInput > 0)
        {
            transform.localScale = new Vector3(1, 1, 1); // 面向右
        }
        else if (horizontalInput < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); // 面向左
        }
        // 如果 horizontalInput 为 0，则保持当前朝向
    }

    // (可选) 在 Scene 视图中绘制地面检测范围，方便调试
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 检查碰到的物体是否带有 "Coin" 标签
        if (other.gameObject.CompareTag("Coin"))
        {
            // 1. 增加金钱计数
            money++;

            // 2. 在控制台显示消息 (用于测试和调试)
            Debug.Log("金钱+1! 当前总数: " + money);

            // --- 可选：在此处添加更新 UI 显示的代码 ---
            // UpdateMoneyUI(money); // 例如调用一个更新 UI 的函数

            // 3. 销毁碰到的金币 GameObject，使其消失
            Destroy(other.gameObject);
        }
        // 你可以在这里添加更多的 else if 条件来处理与其他类型触发器的碰撞
        // else if (other.gameObject.CompareTag("PowerUp")) { ... }
        // else if (other.gameObject.CompareTag("Hazard")) { ... }
    }
}