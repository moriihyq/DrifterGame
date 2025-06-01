using UnityEngine;

public class MovementDiagnostics : MonoBehaviour
{
    [Header("Input Testing")]
    public bool enableDebugging = true;
    
    void Update()
    {
        if (!enableDebugging) return;
        
        // 测试所有可能的输入方法
        TestInputMethods();
        
        // 按F1键显示详细诊断信息
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ShowDetailedDiagnostics();
        }
    }
    
    void TestInputMethods()
    {
        // 测试 Input.GetAxisRaw
        float horizontalRaw = Input.GetAxisRaw("Horizontal");
        float verticalRaw = Input.GetAxisRaw("Vertical");
        
        // 测试直接按键检测
        bool wKey = Input.GetKey(KeyCode.W);
        bool aKey = Input.GetKey(KeyCode.A);
        bool sKey = Input.GetKey(KeyCode.S);
        bool dKey = Input.GetKey(KeyCode.D);
        
        // 只在有输入时显示调试信息
        if (horizontalRaw != 0 || verticalRaw != 0 || wKey || aKey || sKey || dKey)
        {
            Debug.Log($"[MovementDiag] Horizontal Raw: {horizontalRaw}, Vertical Raw: {verticalRaw}");
            Debug.Log($"[MovementDiag] Direct Keys - W: {wKey}, A: {aKey}, S: {sKey}, D: {dKey}");
        }
        
        // 测试新输入系统（如果启用）
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            bool newW = keyboard.wKey.isPressed;
            bool newA = keyboard.aKey.isPressed;
            bool newS = keyboard.sKey.isPressed;
            bool newD = keyboard.dKey.isPressed;
            
            if (newW || newA || newS || newD)
            {
                Debug.Log($"[MovementDiag] New Input System - W: {newW}, A: {newA}, S: {newS}, D: {newD}");
            }
        }
        #endif
    }
    
    void ShowDetailedDiagnostics()
    {
        Debug.Log("=== MOVEMENT DIAGNOSTICS ===");
        
        // 检查Input Manager设置
        Debug.Log($"Input Manager - Horizontal: {Input.GetAxisRaw("Horizontal")}");
        Debug.Log($"Input Manager - Vertical: {Input.GetAxisRaw("Vertical")}");
        
        // 检查PlayerController组件
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            Debug.Log($"PlayerController found: {playerController.name}");
            Rigidbody2D rb = playerController.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Debug.Log($"Rigidbody2D velocity: {rb.linearVelocity}");
                Debug.Log($"Rigidbody2D isKinematic: {rb.isKinematic}");
                Debug.Log($"Rigidbody2D freezeRotation: {rb.freezeRotation}");
            }
        }
        else
        {
            Debug.LogError("PlayerController not found!");
        }
        
        // 检查输入系统设置
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        Debug.Log("New Input System is ENABLED");
        #else
        Debug.Log("Legacy Input Manager is ENABLED");
        #endif
        
        Debug.Log("=== END DIAGNOSTICS ===");
    }
}
