using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 编译检查器 - 运行时检查项目编译状态
/// </summary>
public class 编译检查器 : MonoBehaviour
{
    [Header("检查设置")]
    [SerializeField] private bool autoCheckOnStart = true;
    [SerializeField] private bool showDetailedInfo = true;
    
    void Start()
    {
        if (autoCheckOnStart)
        {
            检查编译状态();
        }
    }
    
    [ContextMenu("检查编译状态")]
    public void 检查编译状态()
    {
        Debug.Log("🔍 开始检查项目编译状态...");
        
        // 如果能运行到这里，说明至少主要脚本没有编译错误
        Debug.Log("✅ 基本编译检查通过 - 脚本可以正常运行");
        
        // 检查关键Player组件
        检查Player组件();
        
        // 检查敌人系统
        检查敌人系统();
        
        // 检查技能系统
        检查技能系统();
        
        Debug.Log("📊 编译状态检查完成！");
    }
    
    private void 检查Player组件()
    {
        Debug.Log("🎮 检查Player相关组件...");
        
        var playerObjects = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log($"找到 {playerObjects.Length} 个Player对象");
        
        foreach (var player in playerObjects)
        {
            // 检查PlayerAttackSystem
            var attackSystem = player.GetComponent<PlayerAttackSystem>();
            if (attackSystem != null)
            {
                Debug.Log($"✅ {player.name} - PlayerAttackSystem正常");
            }
            else
            {
                Debug.LogWarning($"⚠️ {player.name} - 缺少PlayerAttackSystem组件");
            }
            
            // 检查PlayerController
            var controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                Debug.Log($"✅ {player.name} - PlayerController正常");
            }
            else
            {
                Debug.LogWarning($"⚠️ {player.name} - 缺少PlayerController组件");
            }
            
            // 检查其他组件
            var rigidbody = player.GetComponent<Rigidbody2D>();
            var animator = player.GetComponent<Animator>();
            var spriteRenderer = player.GetComponent<SpriteRenderer>();
            
            Debug.Log($"📋 {player.name} 组件状态:");
            Debug.Log($"   - Rigidbody2D: {(rigidbody ? "✅" : "❌")}");
            Debug.Log($"   - Animator: {(animator ? "✅" : "❌")}");
            Debug.Log($"   - SpriteRenderer: {(spriteRenderer ? "✅" : "❌")}");
        }
    }
    
    private void 检查敌人系统()
    {
        Debug.Log("👹 检查敌人系统...");
        
        // 检查Enemy类是否正常
        var enemies = FindObjectsOfType<Enemy>();
        Debug.Log($"找到 {enemies.Length} 个Enemy对象");
        
        // 检查BossController类是否正常
        var bosses = FindObjectsOfType<BossController>();
        Debug.Log($"找到 {bosses.Length} 个Boss对象");
        
        // 检查减速效果组件
        var slowEffects = FindObjectsOfType<EnemySlowEffect>();
        Debug.Log($"找到 {slowEffects.Length} 个EnemySlowEffect组件");
        
        var bossSlowEffects = FindObjectsOfType<BossSlowEffect>();
        Debug.Log($"找到 {bossSlowEffects.Length} 个BossSlowEffect组件");
    }
    
    private void 检查技能系统()
    {
        Debug.Log("⚡ 检查技能系统...");
        
        var skillSystems = FindObjectsOfType<PlayerLevelSkillSystem>();
        Debug.Log($"找到 {skillSystems.Length} 个PlayerLevelSkillSystem组件");
        
        foreach (var skill in skillSystems)
        {
            Debug.Log($"📊 {skill.gameObject.name} 技能系统状态:");
            Debug.Log($"   - 当前等级: {skill.PlayerLevel}");
            Debug.Log($"   - 技能是否可用: {skill.CanUseAerialAttack()}");
            Debug.Log($"   - 冷却状态: {(skill.IsAerialAttackOnCooldown ? $"冷却中 {skill.CurrentCooldownTime:F1}s" : "就绪")}");
        }
    }
    
    void Update()
    {
        // 按F12键手动触发检查
        if (Input.GetKeyDown(KeyCode.F12))
        {
            检查编译状态();
        }
    }
} 