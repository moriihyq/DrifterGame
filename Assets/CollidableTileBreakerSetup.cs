using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// CollidableTileBreaker 快速设置助手
/// 这个脚本会帮助您快速设置和配置可破坏瓦片系统
/// </summary>
[System.Serializable]
public class CollidableTileBreakerSetup : MonoBehaviour
{
    [Header("快速设置")]
    [SerializeField] private bool autoSetupOnStart = true; // 是否在开始时自动设置
    [SerializeField] private bool createDefaultSettings = true; // 是否创建默认设置
    
    [Header("预设配置")]
    [SerializeField] private TileBreakerPreset preset = TileBreakerPreset.Balanced;
    
    [Header("自定义设置（如果不使用预设）")]
    [SerializeField] private int customTileHitPoints = 1;
    [SerializeField] private float customAttackRange = 2.5f;
    [SerializeField] private int customParticleCount = 8;
    [SerializeField] private Color customParticleColor = new Color(0.6f, 0.4f, 0.2f); // 棕色
    
    public enum TileBreakerPreset
    {
        Fast,       // 快速破坏：1血量，大范围
        Balanced,   // 平衡：1血量，中等范围
        Tough,      // 坚固：2血量，中等范围
        VeryTough,  // 非常坚固：3血量，小范围
        Custom      // 自定义设置
    }
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupTileBreaker();
        }
    }
    
    /// <summary>
    /// 设置瓦片破坏系统
    /// </summary>
    [ContextMenu("设置瓦片破坏系统")]
    public void SetupTileBreaker()
    {
        // 查找或创建 CollidableTileBreaker 组件
        CollidableTileBreaker breaker = GetComponent<CollidableTileBreaker>();
        if (breaker == null)
        {
            breaker = gameObject.AddComponent<CollidableTileBreaker>();
            Debug.Log("[CollidableTileBreakerSetup] 已添加 CollidableTileBreaker 组件");
        }
        
        // 应用预设配置
        ApplyPreset(breaker);
        
        // 自动查找和设置组件
        AutoSetupComponents(breaker);
        
        Debug.Log($"[CollidableTileBreakerSetup] 系统设置完成，使用预设: {preset}");
    }
    
    /// <summary>
    /// 应用预设配置
    /// </summary>
    private void ApplyPreset(CollidableTileBreaker breaker)
    {
        switch (preset)
        {
            case TileBreakerPreset.Fast:
                ApplyFastPreset(breaker);
                break;
            case TileBreakerPreset.Balanced:
                ApplyBalancedPreset(breaker);
                break;
            case TileBreakerPreset.Tough:
                ApplyToughPreset(breaker);
                break;
            case TileBreakerPreset.VeryTough:
                ApplyVeryToughPreset(breaker);
                break;
            case TileBreakerPreset.Custom:
                ApplyCustomPreset(breaker);
                break;
        }
    }
    
    /// <summary>
    /// 快速破坏预设
    /// </summary>
    private void ApplyFastPreset(CollidableTileBreaker breaker)
    {
        breaker.SetTileHitPoints(1);
        breaker.SetAttackRange(3.0f);
        
        // 使用反射设置私有字段（仅用于演示，实际使用中建议添加公共方法）
        SetPrivateField(breaker, "particleCount", 12);
        SetPrivateField(breaker, "particleColor", Color.yellow);
        SetPrivateField(breaker, "particleSpeed", 6f);
        
        Debug.Log("[CollidableTileBreakerSetup] 应用快速破坏预设");
    }
    
    /// <summary>
    /// 平衡预设
    /// </summary>
    private void ApplyBalancedPreset(CollidableTileBreaker breaker)
    {
        breaker.SetTileHitPoints(1);
        breaker.SetAttackRange(2.5f);
        
        SetPrivateField(breaker, "particleCount", 8);
        SetPrivateField(breaker, "particleColor", new Color(0.6f, 0.4f, 0.2f)); // 棕色
        SetPrivateField(breaker, "particleSpeed", 5f);
        
        Debug.Log("[CollidableTileBreakerSetup] 应用平衡预设");
    }
    
    /// <summary>
    /// 坚固预设
    /// </summary>
    private void ApplyToughPreset(CollidableTileBreaker breaker)
    {
        breaker.SetTileHitPoints(2);
        breaker.SetAttackRange(2.0f);
        
        SetPrivateField(breaker, "particleCount", 10);
        SetPrivateField(breaker, "particleColor", Color.gray);
        SetPrivateField(breaker, "particleSpeed", 4f);
        
        Debug.Log("[CollidableTileBreakerSetup] 应用坚固预设");
    }
    
    /// <summary>
    /// 非常坚固预设
    /// </summary>
    private void ApplyVeryToughPreset(CollidableTileBreaker breaker)
    {
        breaker.SetTileHitPoints(3);
        breaker.SetAttackRange(1.8f);
        
        SetPrivateField(breaker, "particleCount", 15);
        SetPrivateField(breaker, "particleColor", new Color(0.4f, 0.4f, 0.4f));
        SetPrivateField(breaker, "particleSpeed", 3f);
        
        Debug.Log("[CollidableTileBreakerSetup] 应用非常坚固预设");
    }
    
    /// <summary>
    /// 自定义预设
    /// </summary>
    private void ApplyCustomPreset(CollidableTileBreaker breaker)
    {
        breaker.SetTileHitPoints(customTileHitPoints);
        breaker.SetAttackRange(customAttackRange);
        
        SetPrivateField(breaker, "particleCount", customParticleCount);
        SetPrivateField(breaker, "particleColor", customParticleColor);
        
        Debug.Log("[CollidableTileBreakerSetup] 应用自定义预设");
    }
    
    /// <summary>
    /// 自动设置组件
    /// </summary>
    private void AutoSetupComponents(CollidableTileBreaker breaker)
    {
        // 确保有音频源
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            Debug.Log("[CollidableTileBreakerSetup] 已添加 AudioSource 组件");
        }
        
        // 如果启用默认设置，创建一些基本配置
        if (createDefaultSettings)
        {
            CreateDefaultBreakSounds(breaker);
        }
    }
    
    /// <summary>
    /// 创建默认破坏音效
    /// </summary>
    private void CreateDefaultBreakSounds(CollidableTileBreaker breaker)
    {
        // 这里可以设置默认的破坏音效
        // 由于需要实际的音频文件，这里只提供框架
        Debug.Log("[CollidableTileBreakerSetup] 提示：请手动添加破坏音效到 breakSounds 数组");
    }
    
    /// <summary>
    /// 使用反射设置私有字段
    /// </summary>
    private void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            field.SetValue(obj, value);
        }
        else
        {
            Debug.LogWarning($"[CollidableTileBreakerSetup] 未找到字段: {fieldName}");
        }
    }
    
    /// <summary>
    /// 验证设置
    /// </summary>
    [ContextMenu("验证当前设置")]
    public void ValidateSetup()
    {
        CollidableTileBreaker breaker = GetComponent<CollidableTileBreaker>();
        if (breaker == null)
        {
            Debug.LogError("[CollidableTileBreakerSetup] 未找到 CollidableTileBreaker 组件！");
            return;
        }
        
        // 检查 Tilemap 设置
        Tilemap[] tilemaps = FindObjectsOfType<Tilemap>();
        bool foundCollideable = false;
        
        foreach (var tilemap in tilemaps)
        {
            if (tilemap.name.ToLower().Contains("collideable") ||
                tilemap.name.ToLower().Contains("collision") ||
                tilemap.name.ToLower().Contains("wall"))
            {
                foundCollideable = true;
                Debug.Log($"[CollidableTileBreakerSetup] ✓ 找到 Collideable Tilemap: {tilemap.name}");
            }
        }
        
        if (!foundCollideable)
        {
            Debug.LogWarning("[CollidableTileBreakerSetup] ⚠ 未找到 Collideable Tilemap！");
        }
        
        // 检查玩家对象
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        
        if (player != null)
        {
            Debug.Log($"[CollidableTileBreakerSetup] ✓ 找到玩家对象: {player.name}");
        }
        else
        {
            Debug.LogWarning("[CollidableTileBreakerSetup] ⚠ 未找到玩家对象！");
        }
        
        // 检查音频源
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            Debug.Log("[CollidableTileBreakerSetup] ✓ AudioSource 组件已配置");
        }
        else
        {
            Debug.LogWarning("[CollidableTileBreakerSetup] ⚠ 缺少 AudioSource 组件");
        }
        
        Debug.Log("[CollidableTileBreakerSetup] 验证完成");
    }
    
    /// <summary>
    /// 重置到默认设置
    /// </summary>
    [ContextMenu("重置到默认设置")]
    public void ResetToDefaults()
    {
        preset = TileBreakerPreset.Balanced;
        customTileHitPoints = 1;
        customAttackRange = 2.5f;
        customParticleCount = 8;
        customParticleColor = new Color(0.6f, 0.4f, 0.2f); // 棕色
        
        CollidableTileBreaker breaker = GetComponent<CollidableTileBreaker>();
        if (breaker != null)
        {
            ApplyBalancedPreset(breaker);
        }
        
        Debug.Log("[CollidableTileBreakerSetup] 已重置到默认设置");
    }
    
    /// <summary>
    /// 创建演示场景
    /// </summary>
    [ContextMenu("创建演示场景")]
    public void CreateDemoScene()
    {
        Debug.Log("[CollidableTileBreakerSetup] 演示场景创建功能");
        Debug.Log("提示：");
        Debug.Log("1. 创建一个 Grid GameObject");
        Debug.Log("2. 在 Grid 下创建 Tilemap (命名为 'Collideable')");
        Debug.Log("3. 添加 TilemapRenderer 和 TilemapCollider2D 组件");
        Debug.Log("4. 在 Tilemap 上绘制一些瓦片");
        Debug.Log("5. 确保有玩家对象并设置正确的标签");
    }
} 