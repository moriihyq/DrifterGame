using UnityEngine;

/// <summary>
/// 修复Input System冲突导致的玩家移动问题
/// 这个脚本确保玩家移动不受Edgar插件InputHelper的干扰
/// </summary>
public class InputSystemFix : MonoBehaviour
{
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private float debugCheckInterval = 2f;
    
    private float lastDebugTime;
    
    void Start()
    {
        if (showDebugInfo)
        {
            Debug.Log("[InputSystemFix] 输入系统修复已启动");
            CheckInputSystemStatus();
        }
    }
    
    void Update()
    {
        if (showDebugInfo && Time.time - lastDebugTime > debugCheckInterval)
        {
            DebugInputStatus();
            lastDebugTime = Time.time;
        }
    }
    
    void CheckInputSystemStatus()
    {
        Debug.Log("=== 输入系统状态检查 ===");
        
        // 检查Input Manager轴配置
        try
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Debug.Log($"Input Manager轴读取正常 - Horizontal: {horizontal}, Vertical: {vertical}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Input Manager轴读取失败: {e.Message}");
        }
        
        // 检查直接按键输入
        bool aKey = Input.GetKey(KeyCode.A);
        bool dKey = Input.GetKey(KeyCode.D);
        bool wKey = Input.GetKey(KeyCode.W);
        bool sKey = Input.GetKey(KeyCode.S);
        
        Debug.Log($"直接按键检测 - A: {aKey}, D: {dKey}, W: {wKey}, S: {sKey}");
        
        // 检查是否有Edgar InputHelper干扰
        CheckEdgarInputHelper();
        
        Debug.Log("=== 检查完成 ===");
    }
    
    void CheckEdgarInputHelper()
    {
        #if UNITY_EDITOR
        // 在编辑器中检查Edgar InputHelper的存在
        var inputHelperType = System.Type.GetType("Edgar.Unity.Examples.InputHelper, Assembly-CSharp");
        if (inputHelperType != null)
        {
            Debug.LogWarning("[InputSystemFix] 检测到Edgar InputHelper类，可能会干扰输入");
            Debug.Log("[InputSystemFix] 建议解决方案：");
            Debug.Log("1. 禁用新Input System（推荐）");
            Debug.Log("2. 或修改PlayerController使用直接按键检测");
        }
        else
        {
            Debug.Log("[InputSystemFix] 未检测到Edgar InputHelper干扰");
        }
        #endif
    }
    
    void DebugInputStatus()
    {
        if (!showDebugInfo) return;
        
        // 实时显示输入状态
        float horizontal = Input.GetAxisRaw("Horizontal");
        bool aKey = Input.GetKey(KeyCode.A);
        bool dKey = Input.GetKey(KeyCode.D);
        
        if (horizontal != 0 || aKey || dKey)
        {
            Debug.Log($"[输入调试] Horizontal轴: {horizontal}, A键: {aKey}, D键: {dKey}");
        }
    }
    
    /// <summary>
    /// 获取修正后的水平输入
    /// 如果Input Manager的Horizontal轴无效，则直接使用按键检测
    /// </summary>
    public static float GetFixedHorizontalInput()
    {
        // 优先使用Input Manager的轴
        try
        {
            float axisInput = Input.GetAxisRaw("Horizontal");
            if (axisInput != 0) return axisInput;
        }
        catch
        {
            // Input Manager轴无效时使用直接按键检测
        }
        
        // 备用方案：直接按键检测
        float directInput = 0f;
        if (Input.GetKey(KeyCode.A)) directInput -= 1f;
        if (Input.GetKey(KeyCode.D)) directInput += 1f;
        
        return directInput;
    }
    
    /// <summary>
    /// 获取修正后的垂直输入
    /// </summary>
    public static float GetFixedVerticalInput()
    {
        // 优先使用Input Manager的轴
        try
        {
            float axisInput = Input.GetAxisRaw("Vertical");
            if (axisInput != 0) return axisInput;
        }
        catch
        {
            // Input Manager轴无效时使用直接按键检测
        }
        
        // 备用方案：直接按键检测
        float directInput = 0f;
        if (Input.GetKey(KeyCode.S)) directInput -= 1f;
        if (Input.GetKey(KeyCode.W)) directInput += 1f;
        
        return directInput;
    }
}
