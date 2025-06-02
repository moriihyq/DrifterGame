using UnityEngine;

/// <summary>
/// 简化版刺陷阱脚本 - 只处理动画事件
/// 解决 "AnimationEvent 'DeactivateSpike' has no receiver" 错误
/// </summary>
public class SimpleSpikeTrap : MonoBehaviour
{
    [Header("刺陷阱设置")]
    [Tooltip("刺陷阱的伤害值")]
    public int damage = 1;
    
    [Tooltip("触发层级掩码，通常包含Player层")]
    public LayerMask targetLayerMask = 1; // 默认为第0层
    
    private bool spikeActive = false;
    
    /// <summary>
    /// 动画事件：激活刺
    /// </summary>
    public void ActivateSpike()
    {
        spikeActive = true;
    }
    
    /// <summary>
    /// 动画事件：收回刺
    /// </summary>
    public void DeactivateSpike()
    {
        spikeActive = false;
    }
    
    /// <summary>
    /// 获取刺的状态
    /// </summary>
    public bool IsSpikeActive()
    {
        return spikeActive;
    }
    
    /// <summary>
    /// 处理碰撞检测
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // 只有当刺激活时才造成伤害
        if (!spikeActive) return;
        
        // 检查碰撞对象是否在目标层级中
        if (((1 << other.gameObject.layer) & targetLayerMask) == 0) return;
        
        // 尝试对玩家造成伤害
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damage);
        }
    }
    
    void Start()
    {
        // 基本初始化
    }
} 