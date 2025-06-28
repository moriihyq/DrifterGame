using UnityEngine;

public class SimpleMovementTest : MonoBehaviour
{
    [Header("Simple Movement Test")]
    public float moveSpeed = 5f;
    public bool useDirectKeyDetection = true;
    
    private Rigidbody2D rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
            Debug.LogError("[SimpleMovementTest] No Rigidbody2D found! Please add one to test movement.");
            return;
        }
        
        Debug.Log("[SimpleMovementTest] Starting movement test. Use WASD keys to move.");
        Debug.Log("[SimpleMovementTest] Press F2 to toggle between Input.GetAxisRaw and direct key detection.");
    }
    
    void Update()
    {
        if (rb == null) return;
        
        // Toggle input method
        if (Input.GetKeyDown(KeyCode.F2))
        {
            useDirectKeyDetection = !useDirectKeyDetection;
            Debug.Log($"[SimpleMovementTest] Switched to {(useDirectKeyDetection ? "Direct Key Detection" : "Input.GetAxisRaw")}");
        }
        
        Vector2 movement = Vector2.zero;
        
        if (useDirectKeyDetection)
        {
            // 方法1: 直接检测按键
            if (Input.GetKey(KeyCode.W)) movement.y = 1f;
            if (Input.GetKey(KeyCode.S)) movement.y = -1f;
            if (Input.GetKey(KeyCode.A)) movement.x = -1f;
            if (Input.GetKey(KeyCode.D)) movement.x = 1f;
        }
        else
        {
            // 方法2: 使用Input Manager
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
        }
        
        // 应用移动
        if (movement != Vector2.zero)
        {
            rb.linearVelocity = movement * moveSpeed;
            Debug.Log($"[SimpleMovementTest] Moving: {movement} | Method: {(useDirectKeyDetection ? "Direct Keys" : "Input Manager")} | Velocity: {rb.linearVelocity}");
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
