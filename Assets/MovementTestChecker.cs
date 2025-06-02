using UnityEngine;

/// <summary>
/// 快速测试脚本，用于验证玩家移动修复是否生效
/// 使用方法：将此脚本添加到任意GameObject上，运行游戏并按WAD键观察Console输出
/// </summary>
public class MovementTestChecker : MonoBehaviour
{
    [Header("测试设置")]
    [SerializeField] private bool enableTesting = true;
    [SerializeField] private float testInterval = 0.5f;
    
    private float lastTestTime;
    
    void Update()
    {
        if (!enableTesting) return;
        
        if (Time.time - lastTestTime > testInterval)
        {
            TestMovementInput();
            lastTestTime = Time.time;
        }
    }
    
    void TestMovementInput()
    {
        // 测试修复后的输入
        float horizontal = InputSystemFix.GetFixedHorizontalInput();
        float vertical = InputSystemFix.GetFixedVerticalInput();
        
        // 测试原始输入（用于对比）
        float originalHorizontal = 0f;
        try
        {
            originalHorizontal = Input.GetAxisRaw("Horizontal");
        }
        catch
        {
            originalHorizontal = -999f; // 标记为无效
        }
        
        // 测试直接按键
        bool aKey = Input.GetKey(KeyCode.A);
        bool dKey = Input.GetKey(KeyCode.D);
        bool wKey = Input.GetKey(KeyCode.W);
        bool sKey = Input.GetKey(KeyCode.S);
        
        // 只有在按键时才输出，避免刷屏
        if (horizontal != 0 || vertical != 0 || aKey || dKey || wKey || sKey)
        {
            Debug.Log($"[移动测试] 修复输入 H:{horizontal:F1} V:{vertical:F1} | 原始H:{originalHorizontal:F1} | 按键 A:{aKey} D:{dKey} W:{wKey} S:{sKey}");
            
            // 检查修复是否生效
            if (horizontal == 0 && (aKey || dKey))
            {
                Debug.LogWarning("[移动测试] 检测到输入冲突！按键有效但修复输入无效");
            }
            else if (horizontal != 0 && (aKey || dKey))
            {
                Debug.Log("[移动测试] ✓ 输入修复正常工作");
            }
        }
    }
    
    [ContextMenu("执行完整测试")]
    public void RunFullTest()
    {
        Debug.Log("=== 开始完整移动测试 ===");
        
        Debug.Log("请按以下顺序测试按键，观察Console输出：");
        Debug.Log("1. 按住A键（向左移动）");
        Debug.Log("2. 按住D键（向右移动）");
        Debug.Log("3. 按住W键（向上/攀爬）");
        Debug.Log("4. 按住S键（向下/蹲下）");
        Debug.Log("5. 同时按AD键（应该抵消）");
        Debug.Log("6. 同时按WS键（应该抵消）");
        
        Debug.Log("=== 如果所有按键都能正确检测，说明修复成功！ ===");
    }
}
