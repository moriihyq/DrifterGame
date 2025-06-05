using UnityEngine;
using UnityEngine.UI;

public class SkillCooldownUITester : MonoBehaviour
{
    [Header("测试设置")]
    [SerializeField] private KeyCode testAllSkillsKey = KeyCode.T;
    [SerializeField] private bool showDebugInfo = true;
    
    [Header("状态显示")]
    [SerializeField] private Text debugText;
    
    // 技能组件引用
    private MagicBulletSkill magicBulletSkill;
    private PlayerLevelSkillSystem aerialAttackSkill;
    private WindPower windPowerSkill;
    private SkillCooldownUIManager uiManager;
    
    // 测试状态
    private bool hasTested = false;
    
    void Start()
    {
        FindComponents();
        
        if (showDebugInfo)
        {
            Debug.Log("<color=#00FFFF>====== 技能冷却UI测试器 ======</color>");
            Debug.Log($"按 {testAllSkillsKey} 键测试所有技能冷却UI");
            Debug.Log("或者直接按 Q、X、R 键分别测试各个技能");
        }
        
        CreateDebugUI();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(testAllSkillsKey))
        {
            TestAllSkills();
        }
        
        UpdateDebugDisplay();
    }
    
    void FindComponents()
    {
        // 查找技能组件
        magicBulletSkill = FindObjectOfType<MagicBulletSkill>();
        aerialAttackSkill = FindObjectOfType<PlayerLevelSkillSystem>();
        windPowerSkill = FindObjectOfType<WindPower>();
        uiManager = FindObjectOfType<SkillCooldownUIManager>();
        
        // 检查组件状态
        if (showDebugInfo)
        {
            Debug.Log($"MagicBulletSkill: {(magicBulletSkill != null ? "✓" : "✗")}");
            Debug.Log($"PlayerLevelSkillSystem: {(aerialAttackSkill != null ? "✓" : "✗")}");
            Debug.Log($"WindPower: {(windPowerSkill != null ? "✓" : "✗")}");
            Debug.Log($"SkillCooldownUIManager: {(uiManager != null ? "✓" : "✗")}");
        }
    }
    
    void TestAllSkills()
    {
        Debug.Log("<color=#FFFF00>开始测试所有技能冷却UI...</color>");
        hasTested = true;
        
        // 模拟按键触发技能
        StartCoroutine(TestSkillSequence());
    }
    
    System.Collections.IEnumerator TestSkillSequence()
    {
        // 测试Q键技能
        Debug.Log("测试Q键 - 魔法子弹技能");
        SimulateKeyPress(KeyCode.Q);
        yield return new WaitForSeconds(1f);
        
        // 测试X键技能
        Debug.Log("测试X键 - 空中攻击技能");
        SimulateKeyPress(KeyCode.X);
        yield return new WaitForSeconds(1f);
        
        // 测试R键技能
        Debug.Log("测试R键 - 风力技能");
        SimulateKeyPress(KeyCode.R);
        yield return new WaitForSeconds(1f);
        
        Debug.Log("<color=#00FF00>技能测试完成！观察屏幕右上角的冷却UI</color>");
    }
    
    void SimulateKeyPress(KeyCode key)
    {
        // 这个方法提醒用户手动按键，因为无法直接模拟Input.GetKeyDown
        Debug.Log($"<color=#FFFF00>请手动按 {key} 键来测试对应技能</color>");
    }
    
    void CreateDebugUI()
    {
        if (debugText != null) return;
        
        // 查找或创建调试文本
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        
        GameObject debugObj = new GameObject("DebugText");
        debugObj.transform.SetParent(canvas.transform, false);
        
        debugText = debugObj.AddComponent<Text>();
        debugText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        debugText.fontSize = 14;
        debugText.color = Color.yellow;
        
        RectTransform rect = debugObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(10, -10);
        rect.sizeDelta = new Vector2(300, 200);
        
        debugText.alignment = TextAnchor.UpperLeft;
    }
    
    void UpdateDebugDisplay()
    {
        if (debugText == null || !showDebugInfo) return;
        
        string info = "技能冷却UI测试器\n\n";
        
        // 组件状态
        info += "组件检查:\n";
        info += $"MagicBulletSkill: {(magicBulletSkill != null ? "✓" : "✗")}\n";
        info += $"PlayerLevelSkillSystem: {(aerialAttackSkill != null ? "✓" : "✗")}\n";
        info += $"WindPower: {(windPowerSkill != null ? "✓" : "✗")}\n";
        info += $"SkillCooldownUIManager: {(uiManager != null ? "✓" : "✗")}\n\n";
        
        // 技能状态
        info += "技能状态:\n";
        
        // Q键技能状态
        if (magicBulletSkill != null)
        {
            bool qOnCooldown = GetMagicBulletCooldownStatus();
            info += $"Q键技能: {(qOnCooldown ? "冷却中" : "就绪")}\n";
        }
        else
        {
            info += "Q键技能: 未找到\n";
        }
        
        // X键技能状态
        if (aerialAttackSkill != null)
        {
            info += $"X键技能: {(aerialAttackSkill.IsAerialAttackOnCooldown ? "冷却中" : "就绪")}\n";
        }
        else
        {
            info += "X键技能: 未找到\n";
        }
        
        // R键技能状态
        if (windPowerSkill != null)
        {
            info += $"R键技能: {(windPowerSkill.IsWindPowerOnCooldown ? "冷却中" : "就绪")}\n";
        }
        else
        {
            info += "R键技能: 未找到\n";
        }
        
        info += $"\n按 {testAllSkillsKey} 键开始测试";
        info += "\n或按 Q、X、R 键分别测试";
        
        debugText.text = info;
    }
    
    bool GetMagicBulletCooldownStatus()
    {
        if (magicBulletSkill == null) return false;
        
        // 通过反射获取私有字段
        var magicBulletType = typeof(MagicBulletSkill);
        var isOnCooldownField = magicBulletType.GetField("isOnCooldown", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (isOnCooldownField != null)
        {
            return (bool)isOnCooldownField.GetValue(magicBulletSkill);
        }
        
        return false;
    }
    
    [ContextMenu("检查UI连接")]
    public void CheckUIConnections()
    {
        if (uiManager == null)
        {
            Debug.LogError("未找到SkillCooldownUIManager组件");
            return;
        }
        
        Debug.Log("检查UI连接...");
        
        // 使用反射检查UI组件连接状态
        var uiManagerType = typeof(SkillCooldownUIManager);
        
        CheckUIField(uiManagerType, "qSkillUI", "Q技能UI");
        CheckUIField(uiManagerType, "xSkillUI", "X技能UI");
        CheckUIField(uiManagerType, "rSkillUI", "R技能UI");
        
        Debug.Log("UI连接检查完成");
    }
    
    void CheckUIField(System.Type type, string fieldName, string displayName)
    {
        var field = type.GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (field != null)
        {
            var value = field.GetValue(uiManager);
            Debug.Log($"{displayName}: {(value != null ? "✓ 已连接" : "✗ 未连接")}");
        }
    }
} 