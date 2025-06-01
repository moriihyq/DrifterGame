using UnityEngine;
using System;

public class HealthManager : MonoBehaviour
{
    // 单例模式
    private static HealthManager instance;
    public static HealthManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<HealthManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("HealthManager");
                    instance = go.AddComponent<HealthManager>();
                }
            }
            return instance;
        }
    }
    
    [Header("玩家引用")]
    [SerializeField] private GameObject player;
    private PlayerController playerController;
    private PlayerAttackSystem playerAttackSystem;
    
    [Header("UI引用")]
    [SerializeField] private HealthBarUI followHealthBar;        // 跟随玩家的血量条
    [SerializeField] private FixedHealthBarUI fixedHealthBar;    // 固定在屏幕上的血量条
    
    [Header("血量设置")]
    [SerializeField] private int maxHealthOverride = -1;          // 覆盖最大血量（-1表示使用原始值）
    
    // 事件
    public static event Action<int, int> OnHealthChanged;        // 当前血量，最大血量
    public static event Action<int> OnDamaged;                   // 受到的伤害值
    public static event Action<int> OnHealed;                     // 恢复的血量值
    public static event Action OnPlayerDeath;                     // 玩家死亡事件
    
    private int currentHealth;
    private int maxHealth;
    
    private void Awake()
    {
        // 确保单例
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        
        // 可选：使管理器在场景切换时不被销毁
        // DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        InitializePlayer();
    }
    
    private void InitializePlayer()
    {
        // 查找玩家
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = GameObject.Find("Player");
                if (player == null)
                {
                    PlayerController pc = FindObjectOfType<PlayerController>();
                    if (pc != null)
                    {
                        player = pc.gameObject;
                    }
                }
            }
        }
        
        if (player != null)
        {
            // 获取组件
            playerController = player.GetComponent<PlayerController>();
            playerAttackSystem = player.GetComponent<PlayerAttackSystem>();
            
            // 初始化血量
            if (playerController != null)
            {
                maxHealth = maxHealthOverride > 0 ? maxHealthOverride : playerController.maxHealth;
                currentHealth = playerController.currentHealth;
                
                // 如果需要覆盖最大血量
                if (maxHealthOverride > 0)
                {
                    playerController.maxHealth = maxHealthOverride;
                    playerController.currentHealth = maxHealthOverride;
                    currentHealth = maxHealthOverride;
                }
            }
            else if (playerAttackSystem != null)
            {
                maxHealth = maxHealthOverride > 0 ? maxHealthOverride : playerAttackSystem.GetMaxHealth();
                currentHealth = playerAttackSystem.GetCurrentHealth();
                
                // 如果需要覆盖最大血量
                if (maxHealthOverride > 0)
                {
                    playerAttackSystem.SetHealth(maxHealthOverride);
                    currentHealth = maxHealthOverride;
                }
            }
            
            // 触发初始事件
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        else
        {
            Debug.LogError("HealthManager: 未找到玩家对象!");
        }
    }
    
    // 受到伤害
    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;
        
        int previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        // 更新玩家组件
        UpdatePlayerHealth();
        
        // 触发事件
        OnDamaged?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // 检查死亡
        if (currentHealth <= 0 && previousHealth > 0)
        {
            OnPlayerDeath?.Invoke();
        }
        
        Debug.Log($"玩家受到 {damage} 点伤害，当前血量：{currentHealth}/{maxHealth}");
    }
    
    // 恢复血量
    public void Heal(int amount)
    {
        if (amount <= 0 || currentHealth <= 0) return;
        
        int previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        
        // 更新玩家组件
        UpdatePlayerHealth();
        
        // 触发事件
        if (currentHealth > previousHealth)
        {
            OnHealed?.Invoke(currentHealth - previousHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        
        Debug.Log($"玩家恢复 {amount} 点血量，当前血量：{currentHealth}/{maxHealth}");
    }
    
    // 设置血量
    public void SetHealth(int newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        UpdatePlayerHealth();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (currentHealth <= 0)
        {
            OnPlayerDeath?.Invoke();
        }
    }
    
    // 设置最大血量
    public void SetMaxHealth(int newMaxHealth, bool healToFull = false)
    {
        maxHealth = Mathf.Max(1, newMaxHealth);
        
        if (healToFull)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }
        
        UpdatePlayerHealth();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    // 更新玩家组件的血量
    private void UpdatePlayerHealth()
    {
        if (playerController != null)
        {
            playerController.currentHealth = currentHealth;
            playerController.maxHealth = maxHealth;
        }
        else if (playerAttackSystem != null)
        {
            playerAttackSystem.SetHealth(currentHealth);
        }
    }
    
    // 获取当前血量
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    // 获取最大血量
    public int GetMaxHealth()
    {
        return maxHealth;
    }
    
    // 获取血量百分比
    public float GetHealthPercentage()
    {
        return maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
    }
    
    // 检查玩家是否存活
    public bool IsAlive()
    {
        return currentHealth > 0;
    }
    
    // 重置血量到满血
    public void ResetHealth()
    {
        SetHealth(maxHealth);
    }
    
    // 设置UI引用
    public void SetHealthBarUI(HealthBarUI healthBar)
    {
        followHealthBar = healthBar;
    }
    
    public void SetFixedHealthBarUI(FixedHealthBarUI healthBar)
    {
        fixedHealthBar = healthBar;
    }
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
} 