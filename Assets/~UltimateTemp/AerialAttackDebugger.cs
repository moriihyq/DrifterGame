using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 空中攻击技能调试器 - 用于测试和调试空中攻击技能的所有功能
/// </summary>
public class AerialAttackDebugger : MonoBehaviour
{
    [Header("调试设置")]
    [SerializeField] private bool enableDebugMode = true;
    [SerializeField] private bool showGUI = true;
    [SerializeField] private bool enableDetailedLogs = true;
    
    [Header("组件引用")]
    [SerializeField] private PlayerLevelSkillSystem skillSystem;
    [SerializeField] private PlayerAttackSystem playerAttackSystem;
    
    // GUI样式
    private GUIStyle buttonStyle;
    private GUIStyle labelStyle;
    private bool stylesInitialized = false;
    
    void Start()
    {
        // 自动查找组件
        if (skillSystem == null)
            skillSystem = FindObjectOfType<PlayerLevelSkillSystem>();
            
        if (playerAttackSystem == null)
            playerAttackSystem = FindObjectOfType<PlayerAttackSystem>();
            
        if (enableDetailedLogs)
        {
            Debug.Log("<color=#00FFFF>===== 空中攻击调试器已启动 =====</color>");
            Debug.Log($"<color=#00FFFF>技能系统: {(skillSystem != null ? "找到" : "未找到")}</color>");
            Debug.Log($"<color=#00FFFF>攻击系统: {(playerAttackSystem != null ? "找到" : "未找到")}</color>");
            Debug.Log("<color=#00FFFF>按Tab键切换调试UI显示</color>");
        }
    }
    
    void Update()
    {
        if (!enableDebugMode) return;
        
        // 切换调试UI显示
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            showGUI = !showGUI;
            Debug.Log($"<color=#00FFFF>调试UI: {(showGUI ? "显示" : "隐藏")}</color>");
        }
    }
    
    void OnGUI()
    {
        if (!enableDebugMode || !showGUI || skillSystem == null) return;
        
        InitializeStyles();
        
        // 设置GUI区域
        float panelWidth = 350f;
        float panelHeight = 400f;
        float margin = 10f;
        
        // 背景面板
        GUI.Box(new Rect(margin, margin, panelWidth, panelHeight), "空中攻击技能调试器", labelStyle);
        
        // 按钮尺寸
        float btnWidth = panelWidth - 40f;
        float btnHeight = 30f;
        float startY = 50f;
        float spacing = 35f;
        
        int buttonIndex = 0;
        
        // 等级控制区域
        GUI.Label(new Rect(20f, startY + buttonIndex * spacing, btnWidth, 20f), 
            $"当前等级: {skillSystem.PlayerLevel}", labelStyle);
        buttonIndex++;
        
        if (GUI.Button(new Rect(20f, startY + buttonIndex * spacing, btnWidth / 2 - 5f, btnHeight), 
            "设置等级为3"))
        {
            skillSystem.PlayerLevel = 3;
            Debug.Log("<color=#00FF00>玩家等级设置为3级</color>");
        }
        
        if (GUI.Button(new Rect(20f + btnWidth / 2 + 5f, startY + buttonIndex * spacing, btnWidth / 2 - 5f, btnHeight), 
            "等级+1"))
        {
            skillSystem.PlayerLevel++;
            Debug.Log($"<color=#00FF00>玩家等级提升到{skillSystem.PlayerLevel}级</color>");
        }
        buttonIndex++;
        
        // 技能状态显示
        GUI.Label(new Rect(20f, startY + buttonIndex * spacing, btnWidth, 20f), 
            $"技能冷却: {(skillSystem.IsAerialAttackOnCooldown ? $"{skillSystem.CurrentCooldownTime:F1}s" : "就绪")}", 
            labelStyle);
        buttonIndex++;
        
        // 冷却控制
        if (GUI.Button(new Rect(20f, startY + buttonIndex * spacing, btnWidth, btnHeight), 
            "重置冷却时间"))
        {
            // 通过反射重置冷却时间
            ResetCooldown();
        }
        buttonIndex++;
        
        // 技能测试按钮
        GUI.color = skillSystem.CanUseAerialAttack() ? Color.green : Color.red;
        if (GUI.Button(new Rect(20f, startY + buttonIndex * spacing, btnWidth, btnHeight), 
            "测试空中攻击技能"))
        {
            TestAerialAttack();
        }
        GUI.color = Color.white;
        buttonIndex++;
        
        // 敌人信息显示
        buttonIndex++;
        GUI.Label(new Rect(20f, startY + buttonIndex * spacing, btnWidth, 20f), 
            "=== 敌人信息 ===", labelStyle);
        buttonIndex++;
        
        // 查找并显示敌人信息
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        BossController[] bosses = FindObjectsOfType<BossController>();
        
        GUI.Label(new Rect(20f, startY + buttonIndex * spacing, btnWidth, 20f), 
            $"普通敌人数量: {enemies.Length}", labelStyle);
        buttonIndex++;
        
        GUI.Label(new Rect(20f, startY + buttonIndex * spacing, btnWidth, 20f), 
            $"Boss数量: {bosses.Length}", labelStyle);
        buttonIndex++;
        
        // 敌人距离信息
        if (enemies.Length > 0)
        {
            foreach (Enemy enemy in enemies)
            {
                if (enemy != null)
                {
                    float distance = Vector2.Distance(skillSystem.transform.position, enemy.transform.position);
                    GUI.Label(new Rect(20f, startY + buttonIndex * spacing, btnWidth, 20f), 
                        $"{enemy.name}: {distance:F1}m", labelStyle);
                    buttonIndex++;
                }
            }
        }
        
        if (bosses.Length > 0)
        {
            foreach (BossController boss in bosses)
            {
                if (boss != null)
                {
                    float distance = Vector2.Distance(skillSystem.transform.position, boss.transform.position);
                    GUI.Label(new Rect(20f, startY + buttonIndex * spacing, btnWidth, 20f), 
                        $"{boss.name}: {distance:F1}m", labelStyle);
                    buttonIndex++;
                }
            }
        }
    }
    
    void InitializeStyles()
    {
        if (stylesInitialized) return;
        
        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 12;
        
        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 12;
        labelStyle.normal.textColor = Color.white;
        
        stylesInitialized = true;
    }
    
    void TestAerialAttack()
    {
        if (skillSystem == null)
        {
            Debug.LogError("<color=#FF0000>找不到PlayerLevelSkillSystem组件!</color>");
            return;
        }
        
        Debug.Log("<color=#FFFF00>===== 手动测试空中攻击技能 =====</color>");
        
        // 使用反射调用私有方法来测试
        System.Type type = skillSystem.GetType();
        System.Reflection.MethodInfo tryUseMethod = type.GetMethod("TryUseAerialAttack", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (tryUseMethod != null)
        {
            tryUseMethod.Invoke(skillSystem, null);
        }
        else
        {
            Debug.LogError("<color=#FF0000>无法找到TryUseAerialAttack方法!</color>");
        }
    }
    
    void ResetCooldown()
    {
        if (skillSystem == null) return;
        
        // 使用反射重置冷却时间
        System.Type type = skillSystem.GetType();
        
        var isOnCooldownField = type.GetField("isAerialAttackOnCooldown", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var currentCooldownField = type.GetField("currentCooldownTime", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (isOnCooldownField != null && currentCooldownField != null)
        {
            isOnCooldownField.SetValue(skillSystem, false);
            currentCooldownField.SetValue(skillSystem, 0f);
            
            Debug.Log("<color=#00FF00>技能冷却时间已重置!</color>");
        }
        else
        {
            Debug.LogError("<color=#FF0000>无法重置冷却时间，找不到相关字段!</color>");
        }
    }
    
    // 提供一个快速设置方法
    [System.Obsolete("仅用于调试")]
    public void QuickSetupForTesting()
    {
        if (skillSystem != null)
        {
            skillSystem.PlayerLevel = 3;
            ResetCooldown();
            Debug.Log("<color=#00FF00>快速设置完成：等级3，冷却时间重置</color>");
        }
    }
    
    // 在Scene视图中绘制额外的调试信息
    void OnDrawGizmosSelected()
    {
        if (!enableDebugMode || skillSystem == null) return;
        
        // 绘制玩家到所有敌人的连线
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        BossController[] bosses = FindObjectsOfType<BossController>();
        
        // 绘制到敌人的距离线
        Gizmos.color = Color.yellow;
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null)
            {
                Gizmos.DrawLine(transform.position, enemy.transform.position);
                
                // 在中点显示距离
                Vector3 midPoint = (transform.position + enemy.transform.position) / 2f;
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                
                #if UNITY_EDITOR
                UnityEditor.Handles.color = Color.yellow;
                UnityEditor.Handles.Label(midPoint, $"{distance:F1}m");
                #endif
            }
        }
        
        // 绘制到Boss的距离线
        Gizmos.color = Color.red;
        foreach (BossController boss in bosses)
        {
            if (boss != null)
            {
                Gizmos.DrawLine(transform.position, boss.transform.position);
                
                Vector3 midPoint = (transform.position + boss.transform.position) / 2f;
                float distance = Vector2.Distance(transform.position, boss.transform.position);
                
                #if UNITY_EDITOR
                UnityEditor.Handles.color = Color.red;
                UnityEditor.Handles.Label(midPoint, $"Boss: {distance:F1}m");
                #endif
            }
        }
    }
}
