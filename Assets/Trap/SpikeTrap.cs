using UnityEngine;

/// <summary>
/// 刺陷阱脚本 - 处理刺陷阱的动画事件和伤害检测
/// 解决动画事件"DeactivateSpike"和"ActivateSpike"无接收器的问题
/// 增强对PlayerAttackSystem的支持
/// </summary>
public class SpikeTrap : MonoBehaviour
{
    [Header("刺陷阱设置")]
    [Tooltip("刺陷阱的伤害值")]
    public int damage = 1;
    
    [Tooltip("触发层级掩码，通常包含Player层")]
    public LayerMask targetLayerMask = 1; // 默认为第0层
    
    [Tooltip("是否在刺激活时播放音效")]
    public bool playSound = true;
    
    [Tooltip("刺激活时的音效")]
    public AudioClip spikeActivateSound;
    
    [Tooltip("刺收回时的音效")]
    public AudioClip spikeDeactivateSound;
    
    [Header("伤害设置")]
    [Tooltip("伤害间隔时间（秒），防止连续伤害")]
    public float damageInterval = 0.5f;
    
    [Tooltip("是否显示调试信息")]
    public bool showDebugInfo = true;
    
    private bool spikeActive = false;
    private AudioSource audioSource;
    private Collider2D spikeCollider;
    private Animator animator;
    private float lastDamageTime = 0f; // 上次造成伤害的时间
    
    void Start()
    {
        // 获取组件
        audioSource = GetComponent<AudioSource>();
        spikeCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        
        // 如果没有AudioSource，添加一个
        if (audioSource == null && playSound)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        

    }
    
    /// <summary>
    /// 动画事件：激活刺陷阱
    /// 在动画的适当时机调用，使刺变得危险
    /// </summary>
    public void ActivateSpike()
    {
        spikeActive = true;
        

        
        // 播放激活音效
        if (playSound && audioSource != null && spikeActivateSound != null)
        {
            audioSource.PlayOneShot(spikeActivateSound);
        }
    }
    
    /// <summary>
    /// 动画事件：停用刺陷阱
    /// 在动画的适当时机调用，使刺变得安全
    /// </summary>
    public void DeactivateSpike()
    {
        spikeActive = false;
        

        
        // 播放收回音效
        if (playSound && audioSource != null && spikeDeactivateSound != null)
        {
            audioSource.PlayOneShot(spikeDeactivateSound);
        }
    }
    
    /// <summary>
    /// 检测碰撞并造成伤害
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // 只有当刺激活时才造成伤害
        if (!spikeActive) 
        {

            return;
        }
        
        // 检查碰撞对象是否在目标层级中
        if (((1 << other.gameObject.layer) & targetLayerMask) == 0) 
        {

            return;
        }
        
        // 检查伤害间隔
        if (Time.time - lastDamageTime < damageInterval)
        {

            return;
        }
        
        // 尝试对碰撞对象造成伤害
        bool damageDealt = TryDamageTarget(other.gameObject);
        
        if (damageDealt)
        {
            lastDamageTime = Time.time;
        }
    }
    
    /// <summary>
    /// 持续碰撞检测（用于持续伤害）
    /// </summary>
    void OnTriggerStay2D(Collider2D other)
    {
        // 如果设置了伤害间隔，进行持续伤害检测
        if (damageInterval > 0f && spikeActive)
        {
            OnTriggerEnter2D(other); // 复用进入检测的逻辑
        }
    }
    
    /// <summary>
    /// 尝试对目标造成伤害 - 增强版本，优先使用PlayerAttackSystem
    /// </summary>
    private bool TryDamageTarget(GameObject target)
    {

        
        // 优先尝试PlayerAttackSystem（推荐的伤害系统）
        PlayerAttackSystem playerAttackSystem = target.GetComponent<PlayerAttackSystem>();
        if (playerAttackSystem != null)
        {
            playerAttackSystem.TakeDamage(damage);

            return true;
        }
        
        // 尝试HealthManager（统一血量管理系统）
        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.TakeDamage(damage);

            return true;
        }
        
        // 尝试PlayerController作为备用
        PlayerController playerController = target.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.TakeDamage(damage);

            return true;
        }
        
        // 尝试Enemy组件
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);

            return true;
        }
        
        // 尝试通用的伤害接口
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);

            return true;
        }
        
        // 如果都找不到，静默处理（不输出调试信息）
        return false;
    }
    
    /// <summary>
    /// 手动触发刺陷阱动画
    /// </summary>
    [ContextMenu("触发刺陷阱")]
    public void TriggerSpikeTrap()
    {
        if (animator != null)
        {
            animator.SetTrigger("Trigger"); // 如果动画控制器有触发器参数

        }
        else
        {
            // 如果没有动画器，直接激活
            ActivateSpike();
            // 延迟收回
            Invoke("DeactivateSpike", 1.5f);
        }
    }
    
    /// <summary>
    /// 获取刺的当前状态
    /// </summary>
    public bool IsSpikeActive()
    {
        return spikeActive;
    }
    
    /// <summary>
    /// 强制设置刺的状态
    /// </summary>
    public void SetSpikeState(bool active)
    {
        spikeActive = active;

    }
    
    /// <summary>
    /// 设置伤害值
    /// </summary>
    public void SetDamage(int newDamage)
    {
        damage = newDamage;

    }
    
    /// <summary>
    /// 绘制Gizmos用于调试
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 绘制触发范围
        if (spikeCollider != null)
        {
            Gizmos.color = spikeActive ? Color.red : Color.yellow;
            Gizmos.DrawWireCube(transform.position, spikeCollider.bounds.size);
        }
    }
}

/// <summary>
/// 通用伤害接口
/// 其他可受伤害的对象可以实现此接口
/// </summary>
public interface IDamageable
{
    void TakeDamage(int damage);
} 