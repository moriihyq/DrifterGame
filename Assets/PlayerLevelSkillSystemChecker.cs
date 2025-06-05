using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PlayerLevelSkillSystemChecker : MonoBehaviour
{
    [Header("检查结果")]
    [SerializeField] private bool playerLevelSkillSystemFound = false;
    [SerializeField] private bool animatorFound = false;
    [SerializeField] private bool aerialAttackTriggerExists = false;
    [SerializeField] private bool audioSourceFound = false;
    [SerializeField] private bool skillCooldownUIFound = false;
    [SerializeField] private bool cooldownTextFound = false;
    [SerializeField] private bool cooldownFillImageFound = false;
    [SerializeField] private bool levelRequirementTextFound = false;
    
    [Header("自动修复选项")]
    [SerializeField] private bool autoCreateMissingComponents = true;
    [SerializeField] private bool autoCreateUI = true;
    
    void Start()
    {
        PerformSystemCheck();
        
        if (autoCreateMissingComponents)
        {
            AutoCreateMissingComponents();
        }
        
        if (autoCreateUI)
        {
            AutoCreateUI();
        }
    }
    
    [ContextMenu("执行系统检查")]
    public void PerformSystemCheck()
    {
        Debug.Log("=== 玩家等级技能系统检查开始 ===");
        
        // 检查PlayerLevelSkillSystem组件
        PlayerLevelSkillSystem skillSystem = GetComponent<PlayerLevelSkillSystem>();
        playerLevelSkillSystemFound = skillSystem != null;
        LogCheck("PlayerLevelSkillSystem组件", playerLevelSkillSystemFound);
        
        // 检查Animator组件
        Animator animator = GetComponent<Animator>();
        animatorFound = animator != null;
        LogCheck("Animator组件", animatorFound);
        
        // 检查Aerial_Attack触发器
        if (animatorFound && animator.runtimeAnimatorController != null)
        {
            aerialAttackTriggerExists = HasAnimatorParameter(animator, "Aerial_Attack");
            LogCheck("Aerial_Attack动画触发器", aerialAttackTriggerExists);
        }
        else
        {
            aerialAttackTriggerExists = false;
            LogCheck("Aerial_Attack动画触发器", false, "需要先设置Animator Controller");
        }
        
        // 检查AudioSource组件
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSourceFound = audioSource != null;
        LogCheck("AudioSource组件", audioSourceFound);
        
        // 检查UI元素
        CheckUIElements();
        
        Debug.Log("=== 系统检查完成 ===");
        PrintSummary();
    }
    
    void CheckUIElements()
    {
        // 检查技能冷却UI
        GameObject skillUI = GameObject.Find("SkillCooldownUI");
        skillCooldownUIFound = skillUI != null;
        LogCheck("SkillCooldownUI", skillCooldownUIFound);
        
        // 检查冷却时间文本
        if (skillCooldownUIFound)
        {
            Transform cooldownTextTransform = skillUI.transform.Find("CooldownText");
            cooldownTextFound = cooldownTextTransform != null && cooldownTextTransform.GetComponent<Text>() != null;
            LogCheck("CooldownText", cooldownTextFound);
            
            // 检查冷却填充图像
            Transform cooldownFillTransform = skillUI.transform.Find("CooldownFill");
            cooldownFillImageFound = cooldownFillTransform != null && cooldownFillTransform.GetComponent<Image>() != null;
            LogCheck("CooldownFill", cooldownFillImageFound);
        }
        else
        {
            cooldownTextFound = false;
            cooldownFillImageFound = false;
        }
        
        // 检查等级要求文本
        GameObject levelReqText = GameObject.Find("LevelRequirementText");
        levelRequirementTextFound = levelReqText != null && levelReqText.GetComponent<Text>() != null;
        LogCheck("LevelRequirementText", levelRequirementTextFound);
    }
    
    void AutoCreateMissingComponents()
    {
        // 自动添加PlayerLevelSkillSystem
        if (!playerLevelSkillSystemFound)
        {
            gameObject.AddComponent<PlayerLevelSkillSystem>();
            Debug.Log("✅ 自动添加了PlayerLevelSkillSystem组件");
        }
        
        // 自动添加AudioSource
        if (!audioSourceFound)
        {
            gameObject.AddComponent<AudioSource>();
            Debug.Log("✅ 自动添加了AudioSource组件");
        }
    }
    
    void AutoCreateUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("❌ 场景中没有找到Canvas，无法创建UI元素");
            return;
        }
        
        // 自动创建SkillCooldownUI
        if (!skillCooldownUIFound)
        {
            CreateSkillCooldownUI(canvas);
        }
        
        // 自动创建LevelRequirementText
        if (!levelRequirementTextFound)
        {
            CreateLevelRequirementText(canvas);
        }
    }
    
    void CreateSkillCooldownUI(Canvas canvas)
    {
        // 创建主面板
        GameObject skillUI = new GameObject("SkillCooldownUI");
        skillUI.transform.SetParent(canvas.transform, false);
        
        Image panelImage = skillUI.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f);
        
        RectTransform panelRect = skillUI.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 1);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.anchoredPosition = new Vector2(-80, -40);
        panelRect.sizeDelta = new Vector2(150, 80);
        
        // 创建冷却时间文本
        GameObject cooldownTextObj = new GameObject("CooldownText");
        cooldownTextObj.transform.SetParent(skillUI.transform, false);
        
        Text cooldownText = cooldownTextObj.AddComponent<Text>();
        cooldownText.text = "空中攻击: 0.0s";
        cooldownText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        cooldownText.fontSize = 12;
        cooldownText.color = Color.white;
        cooldownText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = cooldownTextObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = new Vector2(0, -25);
        
        // 创建冷却填充图像
        GameObject cooldownFillObj = new GameObject("CooldownFill");
        cooldownFillObj.transform.SetParent(skillUI.transform, false);
        
        Image cooldownFill = cooldownFillObj.AddComponent<Image>();
        cooldownFill.color = new Color(1, 0.5f, 0, 0.8f);
        cooldownFill.type = Image.Type.Filled;
        cooldownFill.fillMethod = Image.FillMethod.Radial360;
        
        RectTransform fillRect = cooldownFillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0.1f, 0.3f);
        fillRect.anchorMax = new Vector2(0.9f, 0.9f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        skillUI.SetActive(false); // 默认隐藏
        
        Debug.Log("✅ 自动创建了SkillCooldownUI");
    }
    
    void CreateLevelRequirementText(Canvas canvas)
    {
        GameObject levelReqTextObj = new GameObject("LevelRequirementText");
        levelReqTextObj.transform.SetParent(canvas.transform, false);
        
        Text levelReqText = levelReqTextObj.AddComponent<Text>();
        levelReqText.text = "需要等级3解锁空中攻击";
        levelReqText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        levelReqText.fontSize = 16;
        levelReqText.color = Color.yellow;
        levelReqText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = levelReqTextObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.7f);
        textRect.anchorMax = new Vector2(0.5f, 0.7f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(300, 30);
        
        levelReqTextObj.SetActive(false); // 默认隐藏
        
        Debug.Log("✅ 自动创建了LevelRequirementText");
    }
    
    bool HasAnimatorParameter(Animator animator, string paramName)
    {
        if (animator.runtimeAnimatorController == null) return false;
        
        AnimatorControllerParameter[] parameters = animator.parameters;
        foreach (AnimatorControllerParameter param in parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
    
    void LogCheck(string itemName, bool found, string additionalInfo = "")
    {
        string status = found ? "✅" : "❌";
        string message = $"{status} {itemName}: {(found ? "已找到" : "未找到")}";
        
        if (!string.IsNullOrEmpty(additionalInfo))
        {
            message += $" ({additionalInfo})";
        }
        
        Debug.Log(message);
    }
    
    void PrintSummary()
    {
        int totalChecks = 8;
        int passedChecks = 0;
        
        if (playerLevelSkillSystemFound) passedChecks++;
        if (animatorFound) passedChecks++;
        if (aerialAttackTriggerExists) passedChecks++;
        if (audioSourceFound) passedChecks++;
        if (skillCooldownUIFound) passedChecks++;
        if (cooldownTextFound) passedChecks++;
        if (cooldownFillImageFound) passedChecks++;
        if (levelRequirementTextFound) passedChecks++;
        
        float percentage = (float)passedChecks / totalChecks * 100f;
        
        Debug.Log($"=== 检查总结: {passedChecks}/{totalChecks} 项通过 ({percentage:F1}%) ===");
        
        if (passedChecks == totalChecks)
        {
            Debug.Log("🎉 系统配置完整！可以正常使用空中攻击技能系统！");
        }
        else
        {
            Debug.Log("⚠️  系统配置不完整，请参考PlayerLevelSkillSystemGuide.txt完成设置");
            
            if (!aerialAttackTriggerExists)
            {
                Debug.Log("🔧 需要手动设置：在Animator Controller中添加'Aerial_Attack'触发器参数");
            }
        }
    }
    
    [ContextMenu("连接UI引用")]
    public void ConnectUIReferences()
    {
        PlayerLevelSkillSystem skillSystem = GetComponent<PlayerLevelSkillSystem>();
        if (skillSystem == null)
        {
            Debug.LogWarning("❌ 未找到PlayerLevelSkillSystem组件");
            return;
        }
        
        // 使用反射来设置私有字段（仅用于开发阶段）
        var skillSystemType = typeof(PlayerLevelSkillSystem);
        
        // 连接SkillCooldownUI
        GameObject skillUI = GameObject.Find("SkillCooldownUI");
        if (skillUI != null)
        {
            var skillCooldownUIField = skillSystemType.GetField("skillCooldownUI", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            skillCooldownUIField?.SetValue(skillSystem, skillUI);
            
            // 连接CooldownText
            Transform cooldownTextTransform = skillUI.transform.Find("CooldownText");
            if (cooldownTextTransform != null)
            {
                var cooldownTextField = skillSystemType.GetField("cooldownText", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                cooldownTextField?.SetValue(skillSystem, cooldownTextTransform.GetComponent<Text>());
            }
            
            // 连接CooldownFillImage
            Transform cooldownFillTransform = skillUI.transform.Find("CooldownFill");
            if (cooldownFillTransform != null)
            {
                var cooldownFillImageField = skillSystemType.GetField("cooldownFillImage", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                cooldownFillImageField?.SetValue(skillSystem, cooldownFillTransform.GetComponent<Image>());
            }
        }
        
        // 连接LevelRequirementText
        GameObject levelReqText = GameObject.Find("LevelRequirementText");
        if (levelReqText != null)
        {
            var levelRequirementTextField = skillSystemType.GetField("levelRequirementText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            levelRequirementTextField?.SetValue(skillSystem, levelReqText.GetComponent<Text>());
        }
        
        Debug.Log("✅ UI引用连接完成！");
    }
}
