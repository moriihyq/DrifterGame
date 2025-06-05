using UnityEngine;

/// <summary>
/// 复活点脚本 - 使用trap动画实现复活点功能
/// 玩家通过E键与复活点交互，复活点从静止到播放动画
/// </summary>
public class RespawnPoint : MonoBehaviour
{
    [Header("复活点设置")]
    [Tooltip("复活点的唯一ID")]
    public string respawnPointID = "";
    
    [Tooltip("复活偏移量（相对于复活点位置）")]
    public Vector3 respawnOffset = Vector3.zero;
    
    [Tooltip("玩家需要多接近才能交互（距离）")]
    public float interactionRange = 2f;
    
    [Tooltip("激活时播放的音效")]
    public AudioClip activationSound;
    
    [Tooltip("激活时的特效预制件")]
    public GameObject activationEffect;
    
    [Header("动画设置")]
    [Tooltip("复活点的动画控制器")]
    public Animator animator;
    
    [Tooltip("激活动画的触发器名称")]
    public string activationTrigger = "Activate";
    
    [Tooltip("是否循环播放动画")]
    public bool loopAnimation = true;
    
    [Header("交互提示")]
    [Tooltip("交互提示UI预制件")]
    public GameObject interactionPrompt;
    
    [Tooltip("提示文本内容")]
    public string promptText = "按 E 键激活复活点";
    
    private bool isActivated = false;
    private bool playerInRange = false;
    private PlayerController player;
    private AudioSource audioSource;
    private GameObject currentPrompt;
    private SpriteRenderer spriteRenderer;
    
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private float distanceToPlayer = 0f;
    
    public bool IsActivated => isActivated;
    
    void Awake()
    {
        // 如果没有设置ID，使用GameObject名称
        if (string.IsNullOrEmpty(respawnPointID))
        {
            respawnPointID = gameObject.name;
        }
        
        // 获取组件
        if (animator == null)
            animator = GetComponent<Animator>();
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 确保有碰撞体用于检测玩家
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            CircleCollider2D newCollider = gameObject.AddComponent<CircleCollider2D>();
            newCollider.isTrigger = true;
            newCollider.radius = interactionRange;
        }
        else
        {
            collider.isTrigger = true;
        }
    }
    
    void Start()
    {
        // 注册到复活点管理器
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.RegisterRespawnPoint(this);
        }
        else
        {
            Debug.LogWarning("[RespawnPoint] 找不到RespawnManager！");
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[RespawnPoint] 复活点 {respawnPointID} 已初始化");
        }
    }
    
    void Update()
    {
        // 更新调试信息
        UpdateDebugInfo();
        
        // 处理玩家交互
        HandlePlayerInteraction();
    }
    
    /// <summary>
    /// 更新调试信息
    /// </summary>
    void UpdateDebugInfo()
    {
        if (player != null)
        {
            distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        }
    }
    
    /// <summary>
    /// 处理玩家交互
    /// </summary>
    void HandlePlayerInteraction()
    {
        // 如果已经激活，不需要处理交互
        if (isActivated) return;
        
        // 检查玩家是否在范围内且按下E键
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ActivateRespawnPoint();
        }
    }
    
    /// <summary>
    /// 激活复活点
    /// </summary>
    public void ActivateRespawnPoint()
    {
        if (isActivated) return;
        
        isActivated = true;
        
        // 播放激活动画
        if (animator != null && !string.IsNullOrEmpty(activationTrigger))
        {
            animator.SetTrigger(activationTrigger);
            
            if (loopAnimation)
            {
                // 设置循环播放
                animator.SetBool("IsActive", true);
            }
        }
        
        // 播放激活音效
        if (activationSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(activationSound);
        }
        
        // 播放激活特效
        if (activationEffect != null)
        {
            Instantiate(activationEffect, transform.position, Quaternion.identity);
        }
        
        // 隐藏交互提示
        HideInteractionPrompt();
        
        // 通知复活点管理器
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.ActivateRespawnPoint(this);
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[RespawnPoint] 复活点 {respawnPointID} 已激活！");
        }
    }
    
    /// <summary>
    /// 外部调用的激活方法（用于管理器）
    /// </summary>
    public void Activate()
    {
        ActivateRespawnPoint();
    }
    
    /// <summary>
    /// 获取复活位置
    /// </summary>
    public Vector3 GetRespawnPosition()
    {
        return transform.position + respawnOffset;
    }
    
    /// <summary>
    /// 显示交互提示
    /// </summary>
    void ShowInteractionPrompt()
    {
        if (isActivated) return;
        
        if (interactionPrompt != null && currentPrompt == null)
        {
            Vector3 promptPosition = transform.position + Vector3.up * 1.5f;
            currentPrompt = Instantiate(interactionPrompt, promptPosition, Quaternion.identity);
            
            // 设置提示文本
            TMPro.TextMeshPro textComponent = currentPrompt.GetComponentInChildren<TMPro.TextMeshPro>();
            if (textComponent != null)
            {
                textComponent.text = promptText;
            }
            
            // 让提示跟随复活点
            currentPrompt.transform.SetParent(transform);
        }
    }
    
    /// <summary>
    /// 隐藏交互提示
    /// </summary>
    void HideInteractionPrompt()
    {
        if (currentPrompt != null)
        {
            Destroy(currentPrompt);
            currentPrompt = null;
        }
    }
    
    /// <summary>
    /// 玩家进入触发区域
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                playerInRange = true;
                ShowInteractionPrompt();
                
                if (showDebugInfo && !isActivated)
                {
                    Debug.Log($"[RespawnPoint] 玩家进入复活点 {respawnPointID} 交互范围");
                }
            }
        }
    }
    
    /// <summary>
    /// 玩家离开触发区域
    /// </summary>
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
            HideInteractionPrompt();
            
            if (showDebugInfo && !isActivated)
            {
                Debug.Log($"[RespawnPoint] 玩家离开复活点 {respawnPointID} 交互范围");
            }
        }
    }
    
    /// <summary>
    /// 重置复活点状态（用于测试或特殊需求）
    /// </summary>
    public void ResetRespawnPoint()
    {
        isActivated = false;
        
        if (animator != null)
        {
            animator.SetBool("IsActive", false);
            animator.ResetTrigger(activationTrigger);
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[RespawnPoint] 复活点 {respawnPointID} 已重置");
        }
    }
    
    /// <summary>
    /// 设置复活点ID
    /// </summary>
    public void SetRespawnPointID(string id)
    {
        respawnPointID = id;
    }
    
    /// <summary>
    /// 获取到玩家的距离
    /// </summary>
    public float GetDistanceToPlayer()
    {
        if (player != null)
        {
            return Vector3.Distance(transform.position, player.transform.position);
        }
        return float.MaxValue;
    }
    
    void OnDestroy()
    {
        // 从管理器中注销
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.UnregisterRespawnPoint(this);
        }
        
        // 清理提示UI
        HideInteractionPrompt();
    }
    
    /// <summary>
    /// 绘制交互范围的Gizmos
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // 绘制交互范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // 绘制复活位置
        Gizmos.color = isActivated ? Color.green : Color.red;
        Vector3 respawnPos = GetRespawnPosition();
        Gizmos.DrawWireCube(respawnPos, Vector3.one * 0.5f);
        Gizmos.DrawLine(transform.position, respawnPos);
        
        // 绘制状态信息
        Gizmos.color = Color.white;
        if (Application.isPlaying && showDebugInfo)
        {
            // 可以在Scene视图中显示调试信息
        }
    }
} 