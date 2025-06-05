using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 复活点管理器 - 管理游戏中的所有复活点
/// 负责玩家复活时选择最近的复活点
/// </summary>
public class RespawnManager : MonoBehaviour
{
    [Header("复活设置")]
    [Tooltip("玩家复活时的无敌时间（秒）")]
    public float invincibilityTime = 2f;
    
    [Tooltip("复活时的特效预制件")]
    public GameObject respawnEffect;
    
    [Tooltip("复活音效")]
    public AudioClip respawnSound;
    
    [Tooltip("地图下边界Y坐标，玩家掉落到此坐标以下会复活")]
    public float mapBottomBoundary = -10f;
    
    [Tooltip("默认复活点位置（当没有激活的复活点时使用）")]
    public Vector3 defaultRespawnPosition = Vector3.zero;
    
    private static RespawnManager instance;
    public static RespawnManager Instance => instance;
    
    private List<RespawnPoint> allRespawnPoints = new List<RespawnPoint>();
    private RespawnPoint lastActivatedRespawnPoint;
    private PlayerController player;
    private AudioSource audioSource;
    
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private int totalRespawnPoints = 0;
    [SerializeField] private int activatedRespawnPoints = 0;
    
    void Awake()
    {
        // 单例模式
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // 获取或添加AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }
    
    void Start()
    {
        // 查找玩家
        FindPlayer();
        
        // 注册所有复活点
        RegisterAllRespawnPoints();
    }
    
    void Update()
    {
        // 检查玩家是否掉落出边界
        CheckPlayerBounds();
    }
    
    /// <summary>
    /// 查找玩家对象
    /// </summary>
    void FindPlayer()
    {
        if (player == null)
        {
            // 首先尝试通过PlayerController找到玩家
            player = FindFirstObjectByType<PlayerController>();
            
            // 如果没找到PlayerController，尝试通过PlayerAttackSystem找到玩家
            if (player == null)
            {
                PlayerAttackSystem attackSystem = FindFirstObjectByType<PlayerAttackSystem>();
                if (attackSystem != null)
                {
                    // 检查PlayerAttackSystem的GameObject是否有PlayerController组件
                    player = attackSystem.GetComponent<PlayerController>();
                    
                    // 如果没有，则检查是否有"Player"标签
                    if (player == null && attackSystem.CompareTag("Player"))
                    {
                        // 创建一个临时的PlayerController引用，用于位置操作
                        // 这里我们用Transform来代替PlayerController进行位置管理
                        if (showDebugInfo)
                        {
                            Debug.Log("[RespawnManager] 找到PlayerAttackSystem但没有PlayerController，将直接操作Transform");
                        }
                    }
                }
            }
            
            if (player == null)
            {
                Debug.LogWarning("[RespawnManager] 找不到PlayerController组件！尝试通过Player标签查找...");
                
                // 最后尝试通过标签查找
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.GetComponent<PlayerController>();
                    if (player == null && showDebugInfo)
                    {
                        Debug.LogWarning("[RespawnManager] 找到带Player标签的对象，但没有PlayerController组件");
                    }
                }
            }
            
            if (player != null && showDebugInfo)
            {
                Debug.Log("[RespawnManager] 找到玩家：" + player.name);
            }
            else if (showDebugInfo)
            {
                Debug.LogError("[RespawnManager] 无法找到玩家对象！请确保玩家有PlayerController组件或Player标签");
            }
        }
    }
    
    /// <summary>
    /// 注册场景中的所有复活点
    /// </summary>
    void RegisterAllRespawnPoints()
    {
        RespawnPoint[] foundPoints = FindObjectsByType<RespawnPoint>(FindObjectsSortMode.None);
        
        foreach (var point in foundPoints)
        {
            RegisterRespawnPoint(point);
        }
        
        UpdateDebugInfo();
        
        if (showDebugInfo)
        {
            Debug.Log($"[RespawnManager] 注册了 {allRespawnPoints.Count} 个复活点");
        }
    }
    
    /// <summary>
    /// 注册单个复活点
    /// </summary>
    public void RegisterRespawnPoint(RespawnPoint respawnPoint)
    {
        if (!allRespawnPoints.Contains(respawnPoint))
        {
            allRespawnPoints.Add(respawnPoint);
            UpdateDebugInfo();
            
            if (showDebugInfo)
            {
                Debug.Log($"[RespawnManager] 注册复活点：{respawnPoint.name}");
            }
        }
    }
    
    /// <summary>
    /// 注销复活点
    /// </summary>
    public void UnregisterRespawnPoint(RespawnPoint respawnPoint)
    {
        if (allRespawnPoints.Remove(respawnPoint))
        {
            UpdateDebugInfo();
            
            if (showDebugInfo)
            {
                Debug.Log($"[RespawnManager] 注销复活点：{respawnPoint.name}");
            }
        }
    }
    
    /// <summary>
    /// 激活复活点
    /// </summary>
    public void ActivateRespawnPoint(RespawnPoint respawnPoint)
    {
        if (respawnPoint.IsActivated) return;
        
        respawnPoint.Activate();
        lastActivatedRespawnPoint = respawnPoint;
        UpdateDebugInfo();
        
        if (showDebugInfo)
        {
            Debug.Log($"[RespawnManager] 激活复活点：{respawnPoint.name}");
        }
    }
    
    /// <summary>
    /// 获取最近的已激活复活点
    /// </summary>
    public RespawnPoint GetNearestActivatedRespawnPoint(Vector3 fromPosition)
    {
        var activatedPoints = allRespawnPoints.Where(p => p.IsActivated).ToList();
        
        if (activatedPoints.Count == 0)
        {
            if (showDebugInfo)
            {
                Debug.LogWarning("[RespawnManager] 没有已激活的复活点！");
            }
            return null;
        }
        
        // 找到最近的复活点
        RespawnPoint nearestPoint = activatedPoints
            .OrderBy(p => Vector3.Distance(fromPosition, p.transform.position))
            .First();
            
        if (showDebugInfo)
        {
            float distance = Vector3.Distance(fromPosition, nearestPoint.transform.position);
            Debug.Log($"[RespawnManager] 最近的复活点：{nearestPoint.name}，距离：{distance:F2}");
        }
        
        return nearestPoint;
    }
    
    /// <summary>
    /// 复活玩家
    /// </summary>
    public void RespawnPlayer()
    {
        // 获取玩家对象 - 支持多种方式
        Transform playerTransform = GetPlayerTransform();
        if (playerTransform == null) 
        {
            Debug.LogError("[RespawnManager] 无法找到玩家对象进行复活！");
            return;
        }
        
        Vector3 respawnPosition = GetRespawnPosition();
        
        // 移动玩家到复活点
        playerTransform.position = respawnPosition;
        
        // 恢复玩家生命值 - 支持多种血量系统
        RestorePlayerHealth();
        
        // 重置玩家状态
        Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
        }
        
        // 播放复活特效
        if (respawnEffect != null)
        {
            Instantiate(respawnEffect, respawnPosition, Quaternion.identity);
        }
        
        // 播放复活音效
        if (respawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(respawnSound);
        }
        
        // 给予无敌时间
        if (invincibilityTime > 0)
        {
            StartCoroutine(ProvideInvincibilityForTransform(playerTransform));
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[RespawnManager] 玩家复活在：{respawnPosition}");
        }
    }
    
    /// <summary>
    /// 获取玩家Transform - 支持多种玩家组件
    /// </summary>
    private Transform GetPlayerTransform()
    {
        // 首先尝试通过已缓存的PlayerController
        if (player != null)
        {
            return player.transform;
        }
        
        // 尝试通过PlayerAttackSystem找到玩家
        PlayerAttackSystem attackSystem = FindFirstObjectByType<PlayerAttackSystem>();
        if (attackSystem != null)
        {
            return attackSystem.transform;
        }
        
        // 尝试通过Player标签找到玩家
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            return playerObj.transform;
        }
        
        // 最后尝试重新查找PlayerController
        FindPlayer();
        return player != null ? player.transform : null;
    }
    
    /// <summary>
    /// 恢复玩家生命值 - 兼容多种血量系统
    /// </summary>
    private void RestorePlayerHealth()
    {
        // 尝试恢复PlayerController的血量
        if (player != null)
        {
            player.currentHealth = player.maxHealth;
        }
        
        // 尝试恢复PlayerAttackSystem的血量
        PlayerAttackSystem attackSystem = FindFirstObjectByType<PlayerAttackSystem>();
        if (attackSystem != null)
        {
            attackSystem.SetHealth(attackSystem.GetMaxHealth());
            if (showDebugInfo)
            {
                Debug.Log($"[RespawnManager] 恢复PlayerAttackSystem血量到：{attackSystem.GetMaxHealth()}");
            }
        }
        
        // 如果有其他血量系统，可以在这里添加
    }
    
    /// <summary>
    /// 获取复活位置
    /// </summary>
    Vector3 GetRespawnPosition()
    {
        Transform playerTransform = GetPlayerTransform();
        Vector3 currentPosition = playerTransform != null ? playerTransform.position : defaultRespawnPosition;
        
        // 优先使用最近的已激活复活点
        RespawnPoint nearestPoint = GetNearestActivatedRespawnPoint(currentPosition);
        
        if (nearestPoint != null)
        {
            return nearestPoint.GetRespawnPosition();
        }
        
        // 如果没有激活的复活点，使用默认位置
        if (showDebugInfo)
        {
            Debug.LogWarning("[RespawnManager] 使用默认复活位置");
        }
        
        return defaultRespawnPosition;
    }
    
    /// <summary>
    /// 检查玩家是否超出地图边界
    /// </summary>
    void CheckPlayerBounds()
    {
        Transform playerTransform = GetPlayerTransform();
        if (playerTransform == null) return;
        
        // 检查是否掉落到地图下方
        if (playerTransform.position.y < mapBottomBoundary)
        {
            if (showDebugInfo)
            {
                Debug.Log("[RespawnManager] 玩家掉落出地图边界，触发复活");
            }
            
            RespawnPlayer();
        }
    }
    
    /// <summary>
    /// 提供无敌时间
    /// </summary>
    System.Collections.IEnumerator ProvideInvincibility()
    {
        if (player != null)
        {
            yield return StartCoroutine(ProvideInvincibilityForTransform(player.transform));
        }
        else
        {
            yield return new WaitForSeconds(invincibilityTime);
        }
    }
    
    /// <summary>
    /// 为指定Transform提供无敌时间
    /// </summary>
    System.Collections.IEnumerator ProvideInvincibilityForTransform(Transform playerTransform)
    {
        // 这里可以添加无敌状态的视觉效果，比如闪烁
        SpriteRenderer playerSprite = playerTransform.GetComponent<SpriteRenderer>();
        
        if (playerSprite != null)
        {
            float blinkInterval = 0.1f;
            float elapsed = 0f;
            Color originalColor = playerSprite.color;
            
            while (elapsed < invincibilityTime)
            {
                playerSprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f); // 半透明
                yield return new WaitForSeconds(blinkInterval);
                
                playerSprite.color = originalColor; // 恢复
                yield return new WaitForSeconds(blinkInterval);
                
                elapsed += blinkInterval * 2;
            }
            
            // 确保最终恢复正常颜色
            playerSprite.color = originalColor;
        }
        else
        {
            yield return new WaitForSeconds(invincibilityTime);
        }
        
        if (showDebugInfo)
        {
            Debug.Log("[RespawnManager] 无敌时间结束");
        }
    }
    
    /// <summary>
    /// 更新调试信息
    /// </summary>
    void UpdateDebugInfo()
    {
        totalRespawnPoints = allRespawnPoints.Count;
        activatedRespawnPoints = allRespawnPoints.Count(p => p.IsActivated);
    }
    
    /// <summary>
    /// 手动触发玩家复活（用于调试或其他逻辑调用）
    /// </summary>
    public void TriggerRespawn()
    {
        RespawnPlayer();
    }
    
    /// <summary>
    /// 设置地图边界
    /// </summary>
    public void SetMapBottomBoundary(float boundary)
    {
        mapBottomBoundary = boundary;
    }
    
    /// <summary>
    /// 获取复活点统计信息
    /// </summary>
    public string GetRespawnPointStats()
    {
        return $"复活点统计：总共 {totalRespawnPoints} 个，已激活 {activatedRespawnPoints} 个";
    }
} 