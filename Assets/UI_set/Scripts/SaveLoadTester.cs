using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 存档读取测试脚本
/// 测试PlayerAttackSystem的血量保存和读取功能
/// </summary>
public class SaveLoadTester : MonoBehaviour
{
    [Header("测试设置")]
    [SerializeField] private bool enableTesting = true;
    [SerializeField] private int testSlot = 0; // 测试使用的存档槽位
    
    [Header("快捷键设置")]
    [SerializeField] private KeyCode saveKey = KeyCode.F5; // 保存快捷键
    [SerializeField] private KeyCode loadKey = KeyCode.F9; // 读取快捷键
    [SerializeField] private KeyCode damageKey = KeyCode.F1; // 减血测试
    [SerializeField] private KeyCode healKey = KeyCode.F2; // 加血测试
    [SerializeField] private KeyCode moveKey = KeyCode.F3; // 移动位置测试
    
    [Header("UI引用")]
    [SerializeField] private Text statusText; // 状态显示文本
    
    private PlayerAttackSystem playerAttackSystem;
    private SaveManager saveManager;
    private Vector3 originalPosition;
    
    private void Start()
    {
        if (!enableTesting) return;
        
        // 查找组件
        FindComponents();
        
        // 记录初始位置
        if (playerAttackSystem != null)
        {
            originalPosition = playerAttackSystem.transform.position;
        }
        
        // 显示测试说明
        ShowTestInstructions();
    }
    
    private void Update()
    {
        if (!enableTesting) return;
        
        HandleInput();
        UpdateStatusDisplay();
    }
    
    /// <summary>
    /// 查找必要组件
    /// </summary>
    private void FindComponents()
    {
        // 查找PlayerAttackSystem
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerAttackSystem = playerObject.GetComponent<PlayerAttackSystem>();
        }
        
        if (playerAttackSystem == null)
        {
            playerAttackSystem = FindFirstObjectByType<PlayerAttackSystem>();
        }
        
        // 查找SaveManager
        saveManager = SaveManager.Instance;
        if (saveManager == null)
        {
            saveManager = FindFirstObjectByType<SaveManager>();
        }
        
        // 查找状态文本
        if (statusText == null)
        {
            statusText = GetComponentInChildren<Text>();
        }
        
        // 验证组件
        if (playerAttackSystem == null)
        {
            Debug.LogError("[SaveLoadTester] 未找到PlayerAttackSystem组件！");
        }
        
        if (saveManager == null)
        {
            Debug.LogError("[SaveLoadTester] 未找到SaveManager组件！");
        }
    }
    
    /// <summary>
    /// 处理输入
    /// </summary>
    private void HandleInput()
    {
        // 保存游戏
        if (Input.GetKeyDown(saveKey))
        {
            TestSave();
        }
        
        // 读取游戏
        if (Input.GetKeyDown(loadKey))
        {
            TestLoad();
        }
        
        // 减血测试
        if (Input.GetKeyDown(damageKey))
        {
            TestDamage();
        }
        
        // 加血测试
        if (Input.GetKeyDown(healKey))
        {
            TestHeal();
        }
        
        // 移动位置测试
        if (Input.GetKeyDown(moveKey))
        {
            TestMove();
        }
    }
    
    /// <summary>
    /// 测试保存功能
    /// </summary>
    private void TestSave()
    {
        if (saveManager == null)
        {
            Debug.LogError("SaveManager未找到，无法测试保存");
            return;
        }
        
        if (playerAttackSystem != null)
        {
            Debug.Log($"[测试保存] 当前血量: {playerAttackSystem.Health}/{playerAttackSystem.MaxHealth}");
            Debug.Log($"[测试保存] 当前位置: {playerAttackSystem.transform.position}");
            
            // 显示私有字段状态（通过反射检查）
            Debug.Log($"[字段检查] currentHealth 字段是否存在: {CheckFieldExists(playerAttackSystem, "currentHealth")}");
            Debug.Log($"[字段检查] health 字段是否存在: {CheckFieldExists(playerAttackSystem, "health")}");
            Debug.Log($"[字段检查] maxHealth 字段是否存在: {CheckFieldExists(playerAttackSystem, "maxHealth")}");
        }
        
        saveManager.SaveGame(testSlot);
        Debug.Log($"已保存到槽位 {testSlot}");
    }
    
    /// <summary>
    /// 检查字段是否存在
    /// </summary>
    private bool CheckFieldExists(object obj, string fieldName)
    {
        var field = obj.GetType().GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        return field != null;
    }
    
    /// <summary>
    /// 测试读取功能
    /// </summary>
    private void TestLoad()
    {
        if (saveManager == null)
        {
            Debug.LogError("SaveManager未找到，无法测试读取");
            return;
        }
        
        Debug.Log($"正在从槽位 {testSlot} 读取存档...");
        
        if (playerAttackSystem != null)
        {
            Debug.Log($"[读取前] 血量: {playerAttackSystem.Health}/{playerAttackSystem.MaxHealth}");
            Debug.Log($"[读取前] 位置: {playerAttackSystem.transform.position}");
        }
        
        saveManager.LoadGameInCurrentScene(testSlot);
        
        // 等待一帧后显示读取结果
        StartCoroutine(ShowLoadResult());
    }
    
    private System.Collections.IEnumerator ShowLoadResult()
    {
        yield return new WaitForEndOfFrame();
        
        if (playerAttackSystem != null)
        {
            Debug.Log($"[读取后] 血量: {playerAttackSystem.Health}/{playerAttackSystem.MaxHealth}");
            Debug.Log($"[读取后] 位置: {playerAttackSystem.transform.position}");
        }
    }
    
    /// <summary>
    /// 测试减血
    /// </summary>
    private void TestDamage()
    {
        if (playerAttackSystem == null) return;
        
        int damage = 25;
        int oldHealth = playerAttackSystem.Health;
        playerAttackSystem.TakeDamage(damage);
        
        Debug.Log($"[测试减血] {oldHealth} → {playerAttackSystem.Health} (-{damage})");
    }
    
    /// <summary>
    /// 测试加血
    /// </summary>
    private void TestHeal()
    {
        if (playerAttackSystem == null) return;
        
        int healAmount = 20;
        int oldHealth = playerAttackSystem.Health;
        playerAttackSystem.Heal(healAmount);
        
        Debug.Log($"[测试加血] {oldHealth} → {playerAttackSystem.Health} (+{healAmount})");
    }
    
    /// <summary>
    /// 测试移动位置
    /// </summary>
    private void TestMove()
    {
        if (playerAttackSystem == null) return;
        
        Vector3 oldPos = playerAttackSystem.transform.position;
        Vector3 newPos = originalPosition + new Vector3(Random.Range(-5f, 5f), Random.Range(-2f, 2f), 0);
        playerAttackSystem.transform.position = newPos;
        
        Debug.Log($"[测试移动] {oldPos} → {newPos}");
    }
    
    /// <summary>
    /// 更新状态显示
    /// </summary>
    private void UpdateStatusDisplay()
    {
        if (statusText == null || playerAttackSystem == null) return;
        
        statusText.text = $"玩家血量: {playerAttackSystem.Health}/{playerAttackSystem.MaxHealth}\n" +
                         $"位置: {playerAttackSystem.transform.position:F1}\n" +
                         $"存档槽位: {testSlot}";
    }
    
    /// <summary>
    /// 显示测试说明
    /// </summary>
    private void ShowTestInstructions()
    {
        Debug.Log("=== 存档读取测试脚本激活 ===");
        Debug.Log($"[{saveKey}] 保存游戏到槽位 {testSlot}");
        Debug.Log($"[{loadKey}] 从槽位 {testSlot} 读取游戏");
        Debug.Log($"[{damageKey}] 减血测试 (25点)");
        Debug.Log($"[{healKey}] 加血测试 (20点)");
        Debug.Log($"[{moveKey}] 随机移动位置");
        Debug.Log("==========================");
    }
    
    /// <summary>
    /// 在屏幕上显示测试信息
    /// </summary>
    private void OnGUI()
    {
        if (!enableTesting) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 200));
        GUILayout.Label("=== 存档系统测试面板 ===");
        
        if (playerAttackSystem != null)
        {
            GUILayout.Label($"当前血量: {playerAttackSystem.Health}/{playerAttackSystem.MaxHealth}");
            GUILayout.Label($"当前位置: {playerAttackSystem.transform.position:F1}");
        }
        else
        {
            GUILayout.Label("未找到PlayerAttackSystem!");
        }
        
        GUILayout.Label($"测试槽位: {testSlot}");
        
        GUILayout.Space(10);
        GUILayout.Label($"[{saveKey}] 保存  [{loadKey}] 读取");
        GUILayout.Label($"[{damageKey}] 减血  [{healKey}] 加血  [{moveKey}] 移动");
        
        if (saveManager == null)
        {
            GUILayout.Label("警告: 未找到SaveManager!");
        }
        
        GUILayout.EndArea();
    }
    
    /// <summary>
    /// 设置测试槽位
    /// </summary>
    public void SetTestSlot(int slot)
    {
        testSlot = Mathf.Clamp(slot, 0, 2);
        Debug.Log($"测试槽位设置为: {testSlot}");
    }
    
    /// <summary>
    /// 启用或禁用测试功能
    /// </summary>
    public void SetTestingEnabled(bool enabled)
    {
        enableTesting = enabled;
        
        if (enabled)
        {
            ShowTestInstructions();
        }
    }
} 