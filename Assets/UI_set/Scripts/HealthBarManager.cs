using UnityEngine;

/// <summary>
/// 血量条管理器
/// 负责连接PlayerAttackSystem和HealthBarUI，实现血量同步显示
/// </summary>
public class HealthBarManager : MonoBehaviour
{
    [Header("组件引用")]
    [SerializeField] private HealthBarUI healthBarUI; // 血量条UI组件
    [SerializeField] private PlayerAttackSystem playerAttackSystem; // 玩家攻击系统组件
    
    [Header("更新设置")]
    [SerializeField] private bool autoFindPlayer = true; // 自动查找玩家
    [SerializeField] private float updateInterval = 0.1f; // 更新间隔（秒）
    [SerializeField] private bool enableDebugLog = false; // 启用调试日志
    
    [Header("UI设置")]
    [SerializeField] private bool showHealthText = true; // 显示血量数字
    [SerializeField] private bool animateHealthChanges = true; // 血量变化动画
    
    // 私有变量
    private float lastUpdateTime;
    private int lastKnownHealth = -1;
    private int lastKnownMaxHealth = -1;
    private bool isInitialized = false;
    
    // 单例模式（可选）
    public static HealthBarManager Instance { get; private set; }
    
    private void Awake()
    {
        // 单例设置
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 可选：跨场景保持
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        InitializeComponents();
    }
    
    private void Start()
    {
        SetupHealthBarEvents();
        InitializeHealthDisplay();
    }
    
    private void Update()
    {
        // 定期更新血量显示
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateHealthDisplay();
            lastUpdateTime = Time.time;
        }
    }
    
    /// <summary>
    /// 初始化组件引用
    /// </summary>
    private void InitializeComponents()
    {
        // 自动查找HealthBarUI组件
        if (healthBarUI == null)
        {
            healthBarUI = FindObjectOfType<HealthBarUI>();
            if (healthBarUI == null)
            {
                healthBarUI = GetComponentInChildren<HealthBarUI>();
            }
        }
        
        // 自动查找PlayerAttackSystem组件
        if (autoFindPlayer && playerAttackSystem == null)
        {
            // 先尝试通过标签查找
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerAttackSystem = playerObject.GetComponent<PlayerAttackSystem>();
            }
            
            // 如果通过标签找不到，尝试直接查找组件
            if (playerAttackSystem == null)
            {
                playerAttackSystem = FindObjectOfType<PlayerAttackSystem>();
            }
        }
        
        // 验证组件
        ValidateComponents();
    }
    
    /// <summary>
    /// 验证必要组件是否存在
    /// </summary>
    private void ValidateComponents()
    {
        bool healthBarUIValid = healthBarUI != null;
        bool playerAttackSystemValid = playerAttackSystem != null;
        
        if (!healthBarUIValid)
        {
            Debug.LogError("[HealthBarManager] 未找到HealthBarUI组件！请确保场景中有血量条UI。");
        }
        
        if (!playerAttackSystemValid)
        {
            Debug.LogError("[HealthBarManager] 未找到PlayerAttackSystem组件！请确保玩家对象有PlayerAttackSystem组件。");
        }
        
        // 只有两个组件都有效时才认为初始化完成
        bool wasInitialized = isInitialized;
        isInitialized = healthBarUIValid && playerAttackSystemValid;
        
        if (enableDebugLog)
        {
            Debug.Log($"[HealthBarManager] 组件验证完成。HealthBarUI: {healthBarUIValid}, PlayerAttackSystem: {playerAttackSystemValid}, 已初始化: {isInitialized}");
            
            if (wasInitialized != isInitialized)
            {
                Debug.Log($"[HealthBarManager] 初始化状态变化: {wasInitialized} → {isInitialized}");
            }
        }
    }
    
    /// <summary>
    /// 设置血量条事件
    /// </summary>
    private void SetupHealthBarEvents()
    {
        if (healthBarUI == null) return;
        
        // 订阅血量条事件
        healthBarUI.OnHealthCritical += OnHealthCritical;
        healthBarUI.OnHealthEmpty += OnHealthEmpty;
        healthBarUI.OnHealthPercentageChanged += OnHealthPercentageChanged;
    }
    
    /// <summary>
    /// 初始化血量显示
    /// </summary>
    private void InitializeHealthDisplay()
    {
        if (!isInitialized) return;
        
        // 初始显示当前血量
        UpdateHealthDisplay(true);
        
        if (enableDebugLog)
        {
            Debug.Log("[HealthBarManager] 血量显示初始化完成。");
        }
    }
    
    /// <summary>
    /// 更新血量显示
    /// </summary>
    /// <param name="forceUpdate">强制更新</param>
    public void UpdateHealthDisplay(bool forceUpdate = false)
    {
        if (!isInitialized) return;
        
        // 添加空引用检查
        if (playerAttackSystem == null)
        {
            if (enableDebugLog)
                Debug.LogWarning("[HealthBarManager] PlayerAttackSystem为null，跳过血量更新");
            return;
        }
        
        if (healthBarUI == null)
        {
            if (enableDebugLog)
                Debug.LogWarning("[HealthBarManager] HealthBarUI为null，跳过血量更新");
            return;
        }
        
        int currentHealth = playerAttackSystem.Health;
        int maxHealth = playerAttackSystem.MaxHealth;
        
        // 检查是否需要更新
        bool needsUpdate = forceUpdate || 
                          currentHealth != lastKnownHealth || 
                          maxHealth != lastKnownMaxHealth;
        
        if (needsUpdate)
        {
            // 更新血量条
            healthBarUI.SetHealth(currentHealth, maxHealth, animateHealthChanges);
            
            // 记录当前值
            lastKnownHealth = currentHealth;
            lastKnownMaxHealth = maxHealth;
            
            if (enableDebugLog)
            {
                Debug.Log($"[HealthBarManager] 血量更新: {currentHealth}/{maxHealth} ({(float)currentHealth / maxHealth * 100:F1}%)");
            }
        }
    }
    
    /// <summary>
    /// 手动设置PlayerAttackSystem引用
    /// </summary>
    /// <param name="playerSystem">玩家攻击系统组件</param>
    public void SetPlayerAttackSystem(PlayerAttackSystem playerSystem)
    {
        playerAttackSystem = playerSystem;
        ValidateComponents();
        
        if (isInitialized)
        {
            InitializeHealthDisplay();
        }
    }
    
    /// <summary>
    /// 手动设置HealthBarUI引用
    /// </summary>
    /// <param name="healthBar">血量条UI组件</param>
    public void SetHealthBarUI(HealthBarUI healthBar)
    {
        healthBarUI = healthBar;
        ValidateComponents();
        SetupHealthBarEvents();
        
        if (isInitialized)
        {
            InitializeHealthDisplay();
        }
    }
    
    /// <summary>
    /// 获取当前血量百分比
    /// </summary>
    /// <returns>血量百分比 (0-1)</returns>
    public float GetHealthPercentage()
    {
        if (!isInitialized || playerAttackSystem == null) return 0f;
        
        return (float)playerAttackSystem.Health / playerAttackSystem.MaxHealth;
    }
    
    /// <summary>
    /// 检查是否为危险血量
    /// </summary>
    /// <returns>是否危险</returns>
    public bool IsHealthCritical()
    {
        return GetHealthPercentage() <= 0.25f;
    }
    
    /// <summary>
    /// 重置血量条显示
    /// </summary>
    public void ResetHealthBar()
    {
        if (healthBarUI != null)
        {
            healthBarUI.ResetToFull();
        }
        lastKnownHealth = -1;
        lastKnownMaxHealth = -1;
    }
    
    /// <summary>
    /// 启用或禁用血量变化动画
    /// </summary>
    /// <param name="enabled">是否启用</param>
    public void SetAnimationEnabled(bool enabled)
    {
        animateHealthChanges = enabled;
        if (healthBarUI != null)
        {
            healthBarUI.SetAnimationEnabled(enabled);
        }
    }
    
    /// <summary>
    /// 设置更新间隔
    /// </summary>
    /// <param name="interval">更新间隔（秒）</param>
    public void SetUpdateInterval(float interval)
    {
        updateInterval = Mathf.Max(0.01f, interval);
    }
    
    // 事件处理方法
    private void OnHealthCritical()
    {
        if (enableDebugLog)
        {
            Debug.LogWarning("[HealthBarManager] 玩家血量危险！");
        }
        
        // 可以在这里添加其他危险血量时的逻辑
        // 例如：播放警告音效、显示警告文本等
    }
    
    private void OnHealthEmpty()
    {
        if (enableDebugLog)
        {
            Debug.LogError("[HealthBarManager] 玩家血量为空！");
        }
        
        // 可以在这里添加玩家死亡时的UI逻辑
        // 例如：显示死亡界面、重置游戏等
    }
    
    private void OnHealthPercentageChanged(float percentage)
    {
        if (enableDebugLog)
        {
            Debug.Log($"[HealthBarManager] 血量百分比变化: {percentage * 100:F1}%");
        }
        
        // 可以在这里添加血量变化时的其他逻辑
        // 例如：更新其他UI元素、触发特效等
    }
    
    private void OnDestroy()
    {
        // 清理事件订阅
        if (healthBarUI != null)
        {
            healthBarUI.OnHealthCritical -= OnHealthCritical;
            healthBarUI.OnHealthEmpty -= OnHealthEmpty;
            healthBarUI.OnHealthPercentageChanged -= OnHealthPercentageChanged;
        }
        
        // 清理单例引用
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    // 编辑器调试方法
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        
        // 在编辑器中自动查找组件
        if (autoFindPlayer && playerAttackSystem == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerAttackSystem = playerObject.GetComponent<PlayerAttackSystem>();
            }
        }
        
        if (healthBarUI == null)
        {
            healthBarUI = FindObjectOfType<HealthBarUI>();
        }
    }
    
    /// <summary>
    /// 强制重新初始化并连接PlayerAttackSystem
    /// 用于存档加载后确保血量条正确同步
    /// </summary>
    public void ForceReconnectPlayerSystem()
    {
        Debug.Log("[HealthBarManager] 强制重新连接PlayerAttackSystem");
        
        // 重新查找PlayerAttackSystem
        playerAttackSystem = null;
        InitializeComponents();
        
        if (isInitialized)
        {
            InitializeHealthDisplay();
            Debug.Log("[HealthBarManager] 重新连接成功");
        }
        else
        {
            Debug.LogWarning("[HealthBarManager] 重新连接失败");
        }
    }
    
    /// <summary>
    /// 获取当前连接的PlayerAttackSystem
    /// </summary>
    public PlayerAttackSystem GetConnectedPlayerSystem()
    {
        return playerAttackSystem;
    }
} 