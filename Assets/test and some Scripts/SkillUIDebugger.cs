using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

public class SkillUIDebugger : MonoBehaviour
{
    [Header("调试设置")]
    [SerializeField] private KeyCode debugKey = KeyCode.F1;
    [SerializeField] private bool showRealTimeDebug = true;
    
    [Header("调试UI")]
    [SerializeField] private Text debugDisplayText;
    
    // 组件引用
    private SkillCooldownUIManager uiManager;
    private MagicBulletSkill magicBulletSkill;
    private PlayerLevelSkillSystem aerialAttackSkill;
    private WindPower windPowerSkill;
    
    void Start()
    {
        FindComponents();
        CreateDebugUI();
        
        Debug.Log("<color=#00FFFF>====== 技能UI调试器启动 ======</color>");
        Debug.Log($"按 {debugKey} 键进行详细诊断");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(debugKey))
        {
            PerformDetailedDiagnosis();
        }
        
        if (showRealTimeDebug)
        {
            UpdateRealTimeDebug();
        }
    }
    
    void FindComponents()
    {
        uiManager = FindObjectOfType<SkillCooldownUIManager>();
        magicBulletSkill = FindObjectOfType<MagicBulletSkill>();
        aerialAttackSkill = FindObjectOfType<PlayerLevelSkillSystem>();
        windPowerSkill = FindObjectOfType<WindPower>();
    }
    
    void PerformDetailedDiagnosis()
    {
        Debug.Log("<color=#FFFF00>====== 开始详细诊断 ======</color>");
        
        // 1. 检查组件存在性
        CheckComponentsExistence();
        
        // 2. 检查UI连接
        CheckUIConnections();
        
        // 3. 检查技能状态
        CheckSkillStates();
        
        // 4. 检查Q技能冷却问题
        DiagnoseQSkillCooldown();
        
        // 5. 检查图标显示问题
        DiagnoseIconIssues();
        
        Debug.Log("<color=#FFFF00>====== 诊断完成 ======</color>");
    }
    
    void CheckComponentsExistence()
    {
        Debug.Log("<color=#00FF00>--- 组件存在性检查 ---</color>");
        Debug.Log($"SkillCooldownUIManager: {(uiManager != null ? "✓" : "✗")}");
        Debug.Log($"MagicBulletSkill: {(magicBulletSkill != null ? "✓" : "✗")}");
        Debug.Log($"PlayerLevelSkillSystem: {(aerialAttackSkill != null ? "✓" : "✗")}");
        Debug.Log($"WindPower: {(windPowerSkill != null ? "✓" : "✗")}");
        
        if (uiManager == null)
        {
            Debug.LogError("❌ 未找到SkillCooldownUIManager！请确保已添加该组件。");
        }
    }
    
    void CheckUIConnections()
    {
        if (uiManager == null) return;
        
        Debug.Log("<color=#00FF00>--- UI连接检查 ---</color>");
        
        var uiManagerType = typeof(SkillCooldownUIManager);
        
        // 检查Q技能UI连接
        CheckUIField(uiManagerType, "qSkillUI", "Q技能UI主对象");
        CheckUIField(uiManagerType, "qSkillCooldownFill", "Q技能冷却进度条");
        CheckUIField(uiManagerType, "qSkillCooldownText", "Q技能文本");
        CheckUIField(uiManagerType, "qSkillIcon", "Q技能图标");
        CheckUIField(uiManagerType, "qSkillBackgroundSprite", "Q技能背景图片");
        
        // 检查X技能UI连接
        CheckUIField(uiManagerType, "xSkillUI", "X技能UI主对象");
        CheckUIField(uiManagerType, "xSkillCooldownFill", "X技能冷却进度条");
        CheckUIField(uiManagerType, "xSkillCooldownText", "X技能文本");
        CheckUIField(uiManagerType, "xSkillIcon", "X技能图标");
        CheckUIField(uiManagerType, "xSkillBackgroundSprite", "X技能背景图片");
        
        // 检查R技能UI连接
        CheckUIField(uiManagerType, "rSkillUI", "R技能UI主对象");
        CheckUIField(uiManagerType, "rSkillCooldownFill", "R技能冷却进度条");
        CheckUIField(uiManagerType, "rSkillCooldownText", "R技能文本");
        CheckUIField(uiManagerType, "rSkillIcon", "R技能图标");
        CheckUIField(uiManagerType, "rSkillBackgroundSprite", "R技能背景图片");
    }
    
    void CheckUIField(System.Type type, string fieldName, string displayName)
    {
        var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            var value = field.GetValue(uiManager);
            Debug.Log($"{displayName}: {(value != null ? "✓ 已连接" : "✗ 未连接")}");
            
            // 特别检查Sprite连接
            if (fieldName.Contains("Sprite") && value != null)
            {
                Sprite sprite = value as Sprite;
                Debug.Log($"  └─ 图片名称: {sprite.name}");
            }
        }
    }
    
    void CheckSkillStates()
    {
        Debug.Log("<color=#00FF00>--- 技能状态检查 ---</color>");
        
        // Q技能状态
        if (magicBulletSkill != null)
        {
            var magicBulletType = typeof(MagicBulletSkill);
            var cooldownTimeField = magicBulletType.GetField("cooldownTime", BindingFlags.NonPublic | BindingFlags.Instance);
            var isOnCooldownField = magicBulletType.GetField("isOnCooldown", BindingFlags.NonPublic | BindingFlags.Instance);
            var currentCooldownTimeField = magicBulletType.GetField("currentCooldownTime", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (cooldownTimeField != null)
            {
                float cooldownTime = (float)cooldownTimeField.GetValue(magicBulletSkill);
                Debug.Log($"Q技能设定冷却时间: {cooldownTime} 秒");
            }
            
            if (isOnCooldownField != null && currentCooldownTimeField != null)
            {
                bool isOnCooldown = (bool)isOnCooldownField.GetValue(magicBulletSkill);
                float currentTime = (float)currentCooldownTimeField.GetValue(magicBulletSkill);
                Debug.Log($"Q技能当前状态: {(isOnCooldown ? "冷却中" : "就绪")}");
                Debug.Log($"Q技能剩余时间: {currentTime} 秒");
            }
        }
        
        // X技能状态
        if (aerialAttackSkill != null)
        {
            Debug.Log($"X技能当前状态: {(aerialAttackSkill.IsAerialAttackOnCooldown ? "冷却中" : "就绪")}");
            Debug.Log($"X技能剩余时间: {aerialAttackSkill.CurrentCooldownTime} 秒");
        }
        
        // R技能状态
        if (windPowerSkill != null)
        {
            Debug.Log($"R技能当前状态: {(windPowerSkill.IsWindPowerOnCooldown ? "冷却中" : "就绪")}");
            Debug.Log($"R技能剩余时间: {windPowerSkill.CurrentCooldownTime} 秒");
        }
    }
    
    void DiagnoseQSkillCooldown()
    {
        Debug.Log("<color=#FF6600>--- Q技能冷却问题诊断 ---</color>");
        
        if (magicBulletSkill == null)
        {
            Debug.LogError("❌ Q技能组件不存在！");
            return;
        }
        
        // 检查冷却时间设置
        var magicBulletType = typeof(MagicBulletSkill);
        var cooldownTimeField = magicBulletType.GetField("cooldownTime", BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (cooldownTimeField != null)
        {
            float cooldownTime = (float)cooldownTimeField.GetValue(magicBulletSkill);
            if (cooldownTime <= 0)
            {
                Debug.LogError($"❌ Q技能冷却时间设置为 {cooldownTime}，应该 > 0");
            }
            else
            {
                Debug.Log($"✓ Q技能冷却时间设置正常: {cooldownTime} 秒");
            }
        }
        
        // 测试Q技能使用
        Debug.Log("💡 建议：请按Q键测试技能，观察控制台输出");
    }
    
    void DiagnoseIconIssues()
    {
        Debug.Log("<color=#FF6600>--- 图标显示问题诊断 ---</color>");
        
        if (uiManager == null) return;
        
        var uiManagerType = typeof(SkillCooldownUIManager);
        
        // 检查背景图片分配
        var qSpriteField = uiManagerType.GetField("qSkillBackgroundSprite", BindingFlags.NonPublic | BindingFlags.Instance);
        var xSpriteField = uiManagerType.GetField("xSkillBackgroundSprite", BindingFlags.NonPublic | BindingFlags.Instance);
        var rSpriteField = uiManagerType.GetField("rSkillBackgroundSprite", BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (qSpriteField != null)
        {
            Sprite qSprite = qSpriteField.GetValue(uiManager) as Sprite;
            Debug.Log($"Q技能背景图: {(qSprite != null ? qSprite.name : "未设置")}");
        }
        
        if (xSpriteField != null)
        {
            Sprite xSprite = xSpriteField.GetValue(uiManager) as Sprite;
            Debug.Log($"X技能背景图: {(xSprite != null ? xSprite.name : "未设置")}");
        }
        
        if (rSpriteField != null)
        {
            Sprite rSprite = rSpriteField.GetValue(uiManager) as Sprite;
            Debug.Log($"R技能背景图: {(rSprite != null ? rSprite.name : "未设置")}");
        }
        
        // 检查实际UI上的图标
        CheckActualUIIcons();
    }
    
    void CheckActualUIIcons()
    {
        if (uiManager == null) return;
        
        var uiManagerType = typeof(SkillCooldownUIManager);
        
        var qIconField = uiManagerType.GetField("qSkillIcon", BindingFlags.NonPublic | BindingFlags.Instance);
        var xIconField = uiManagerType.GetField("xSkillIcon", BindingFlags.NonPublic | BindingFlags.Instance);
        var rIconField = uiManagerType.GetField("rSkillIcon", BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (qIconField != null)
        {
            Image qIcon = qIconField.GetValue(uiManager) as Image;
            if (qIcon != null && qIcon.sprite != null)
            {
                Debug.Log($"Q技能UI实际显示图: {qIcon.sprite.name}");
            }
        }
        
        if (xIconField != null)
        {
            Image xIcon = xIconField.GetValue(uiManager) as Image;
            if (xIcon != null && xIcon.sprite != null)
            {
                Debug.Log($"X技能UI实际显示图: {xIcon.sprite.name}");
            }
        }
        
        if (rIconField != null)
        {
            Image rIcon = rIconField.GetValue(uiManager) as Image;
            if (rIcon != null && rIcon.sprite != null)
            {
                Debug.Log($"R技能UI实际显示图: {rIcon.sprite.name}");
            }
        }
    }
    
    void CreateDebugUI()
    {
        if (debugDisplayText != null) return;
        
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        
        GameObject debugObj = new GameObject("SkillDebugDisplay");
        debugObj.transform.SetParent(canvas.transform, false);
        
        debugDisplayText = debugObj.AddComponent<Text>();
        debugDisplayText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        debugDisplayText.fontSize = 12;
        debugDisplayText.color = Color.cyan;
        
        RectTransform rect = debugObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.anchoredPosition = new Vector2(10, 10);
        rect.sizeDelta = new Vector2(400, 150);
        
        debugDisplayText.alignment = TextAnchor.LowerLeft;
    }
    
    void UpdateRealTimeDebug()
    {
        if (debugDisplayText == null) return;
        
        string info = $"技能UI实时调试 (按{debugKey}详细诊断)\n\n";
        
        // 组件状态
        info += $"组件: UI管理器{(uiManager != null ? "✓" : "✗")} ";
        info += $"Q技能{(magicBulletSkill != null ? "✓" : "✗")} ";
        info += $"X技能{(aerialAttackSkill != null ? "✓" : "✗")} ";
        info += $"R技能{(windPowerSkill != null ? "✓" : "✗")}\n";
        
        // 技能状态
        if (magicBulletSkill != null)
        {
            bool qOnCooldown = GetMagicBulletCooldownStatus();
            float qTime = GetMagicBulletCooldownTime();
            info += $"Q技能: {(qOnCooldown ? $"冷却{qTime:F1}s" : "就绪")}\n";
        }
        
        if (aerialAttackSkill != null)
        {
            info += $"X技能: {(aerialAttackSkill.IsAerialAttackOnCooldown ? $"冷却{aerialAttackSkill.CurrentCooldownTime:F1}s" : "就绪")}\n";
        }
        
        if (windPowerSkill != null)
        {
            info += $"R技能: {(windPowerSkill.IsWindPowerOnCooldown ? $"冷却{windPowerSkill.CurrentCooldownTime:F1}s" : "就绪")}\n";
        }
        
        debugDisplayText.text = info;
    }
    
    bool GetMagicBulletCooldownStatus()
    {
        if (magicBulletSkill == null) return false;
        
        var magicBulletType = typeof(MagicBulletSkill);
        var isOnCooldownField = magicBulletType.GetField("isOnCooldown", BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (isOnCooldownField != null)
        {
            return (bool)isOnCooldownField.GetValue(magicBulletSkill);
        }
        
        return false;
    }
    
    float GetMagicBulletCooldownTime()
    {
        if (magicBulletSkill == null) return 0f;
        
        var magicBulletType = typeof(MagicBulletSkill);
        var currentCooldownTimeField = magicBulletType.GetField("currentCooldownTime", BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (currentCooldownTimeField != null)
        {
            return (float)currentCooldownTimeField.GetValue(magicBulletSkill);
        }
        
        return 0f;
    }
} 