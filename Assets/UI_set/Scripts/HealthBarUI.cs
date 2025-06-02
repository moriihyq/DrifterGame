using UnityEngine;
using UnityEngine.UI;
using System;

public class HealthBarUI : MonoBehaviour
{
    [Header("血量条组件")]
    [SerializeField] private Image healthBarFill;         // 血量条填充图片
    [SerializeField] private Image healthBarBackground;   // 血量条背景图片
    [SerializeField] private Image healthBarFrame;        // 血量条边框图片（可选）
    
    [Header("血量条设置")]
    [SerializeField] private Gradient healthGradient;     // 血量颜色渐变
    [SerializeField] private float smoothSpeed = 5f;      // 血量条变化平滑速度
    [SerializeField] private bool useGradient = true;     // 是否使用颜色渐变
    
    [Header("显示设置")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);  // 血量条相对于角色的偏移
    [SerializeField] private bool alwaysVisible = false;              // 是否始终显示
    [SerializeField] private float hideDelay = 3f;                    // 受伤后多久隐藏血量条
    
    [Header("引用")]
    [SerializeField] private GameObject player;           // 玩家对象引用
    private PlayerController playerController;
    private PlayerAttackSystem playerAttackSystem;
    
    // 事件定义
    public event Action OnHealthCritical;
    public event Action OnHealthEmpty;
    public event Action<float> OnHealthPercentageChanged;
    
    private float targetHealthPercent = 1f;
    private float currentHealthPercent = 1f;
    private float hideTimer = 0f;
    private int lastHealth;
    private Canvas canvas;
    private Camera mainCamera;
    private bool animationEnabled = true;
    
    private void Awake()
    {
        // 获取Canvas组件
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("HealthBarUI需要放在Canvas下!");
        }
        
        // 获取主相机
        mainCamera = Camera.main;
        
        // 设置默认的血量颜色渐变
        if (healthGradient == null || healthGradient.colorKeys.Length == 0)
        {
            healthGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            colorKeys[0].color = Color.red;
            colorKeys[0].time = 0.0f;
            colorKeys[1].color = Color.yellow;
            colorKeys[1].time = 0.5f;
            colorKeys[2].color = Color.green;
            colorKeys[2].time = 1.0f;
            
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0].alpha = 1.0f;
            alphaKeys[0].time = 0.0f;
            alphaKeys[1].alpha = 1.0f;
            alphaKeys[1].time = 1.0f;
            
            healthGradient.SetKeys(colorKeys, alphaKeys);
        }
    }
    
    private void Start()
    {
        // 尝试找到玩家对象
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                // 如果没有Player标签，尝试通过名称查找
                player = GameObject.Find("Player");
                if (player == null)
                {
                    // 尝试查找带有PlayerController的对象
                    PlayerController pc = FindFirstObjectByType<PlayerController>();
                    if (pc != null)
                    {
                        player = pc.gameObject;
                    }
                }
            }
        }
        
        if (player != null)
        {
            // 获取PlayerController或PlayerAttackSystem组件
            playerController = player.GetComponent<PlayerController>();
            playerAttackSystem = player.GetComponent<PlayerAttackSystem>();
            
            if (playerController != null)
            {
                lastHealth = playerController.currentHealth;
                UpdateHealthBar();
            }
            else if (playerAttackSystem != null)
            {
                lastHealth = playerAttackSystem.GetCurrentHealth();
                UpdateHealthBar();
            }
            else
            {
                Debug.LogError("未找到PlayerController或PlayerAttackSystem组件!");
            }
        }
        else
        {
            Debug.LogError("未找到玩家对象!");
        }
        
        // 初始隐藏血量条（如果设置为非始终显示）
        if (!alwaysVisible)
        {
            SetHealthBarVisibility(false);
        }
    }
    
    private void Update()
    {
        if (player == null) return;
        
        // 更新血量条位置（跟随玩家）
        UpdatePosition();
        
        // 检查血量变化
        CheckHealthChange();
        
        // 平滑更新血量条
        SmoothUpdateHealthBar();
        
        // 处理隐藏计时器
        HandleHideTimer();
    }
    
    private void UpdatePosition()
    {
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            // 如果是世界空间Canvas，直接设置位置
            transform.position = player.transform.position + offset;
        }
        else if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // 如果是屏幕空间Canvas，需要转换坐标
            Vector3 worldPos = player.transform.position + offset;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
            
            // 转换为Canvas坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                out Vector2 localPos
            );
            
            transform.localPosition = localPos;
        }
    }
    
    private void CheckHealthChange()
    {
        int currentHealth = 0;
        int maxHealth = 0;
        
        // 获取当前血量
        if (playerController != null)
        {
            currentHealth = playerController.currentHealth;
            maxHealth = playerController.maxHealth;
        }
        else if (playerAttackSystem != null)
        {
            currentHealth = playerAttackSystem.GetCurrentHealth();
            maxHealth = playerAttackSystem.GetMaxHealth();
        }
        
        // 检查血量是否变化
        if (currentHealth != lastHealth)
        {
            lastHealth = currentHealth;
            targetHealthPercent = (float)currentHealth / maxHealth;
            
            // 显示血量条
            if (!alwaysVisible)
            {
                SetHealthBarVisibility(true);
                hideTimer = hideDelay;
            }
        }
    }
    
    private void SmoothUpdateHealthBar()
    {
        if (animationEnabled)
    {
        // 平滑过渡到目标血量
        currentHealthPercent = Mathf.Lerp(currentHealthPercent, targetHealthPercent, Time.deltaTime * smoothSpeed);
        }
        else
        {
            // 直接设置到目标血量
            currentHealthPercent = targetHealthPercent;
            }
        
        // 更新血量条视觉效果
        UpdateHealthBarVisual();
    }
    
    private void HandleHideTimer()
    {
        if (!alwaysVisible && hideTimer > 0)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0)
            {
                SetHealthBarVisibility(false);
            }
        }
    }
    
    private void SetHealthBarVisibility(bool visible)
    {
        if (healthBarFill != null)
            healthBarFill.gameObject.SetActive(visible);
        if (healthBarBackground != null)
            healthBarBackground.gameObject.SetActive(visible);
        if (healthBarFrame != null)
            healthBarFrame.gameObject.SetActive(visible);
    }
    
    private void UpdateHealthBar()
    {
        if (playerController != null)
        {
            targetHealthPercent = (float)playerController.currentHealth / playerController.maxHealth;
        }
        else if (playerAttackSystem != null)
        {
            targetHealthPercent = (float)playerAttackSystem.GetCurrentHealth() / playerAttackSystem.GetMaxHealth();
        }
        
        currentHealthPercent = targetHealthPercent;
        
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealthPercent;
            
            if (useGradient && healthGradient != null)
            {
                healthBarFill.color = healthGradient.Evaluate(currentHealthPercent);
            }
        }
    }
    
    // 公共方法，允许外部强制更新血量条
    public void ForceUpdateHealthBar()
    {
        CheckHealthChange();
        UpdateHealthBar();
    }
    
    // 设置玩家引用
    public void SetPlayer(GameObject newPlayer)
    {
        player = newPlayer;
        playerController = player.GetComponent<PlayerController>();
        playerAttackSystem = player.GetComponent<PlayerAttackSystem>();
        UpdateHealthBar();
    }
    
    // 设置血量的方法
    public void SetHealth(int currentHealth, int maxHealth, bool animate = true)
    {
        if (maxHealth <= 0) return;
        
        float newHealthPercent = (float)currentHealth / maxHealth;
        targetHealthPercent = newHealthPercent;
        
        if (!animate)
        {
            currentHealthPercent = targetHealthPercent;
            UpdateHealthBarVisual();
        }
        
        // 触发事件
        OnHealthPercentageChanged?.Invoke(newHealthPercent);
        
        if (newHealthPercent <= 0f)
        {
            OnHealthEmpty?.Invoke();
        }
        else if (newHealthPercent <= 0.25f)
        {
            OnHealthCritical?.Invoke();
        }
        
        // 显示血量条
        if (!alwaysVisible)
        {
            SetHealthBarVisibility(true);
            hideTimer = hideDelay;
        }
    }
    
    // 重置血量条到满血状态
    public void ResetToFull()
    {
        targetHealthPercent = 1f;
        currentHealthPercent = 1f;
        UpdateHealthBarVisual();
        OnHealthPercentageChanged?.Invoke(1f);
    }
    
    // 设置动画启用状态
    public void SetAnimationEnabled(bool enabled)
    {
        animationEnabled = enabled;
    }
    
    // 获取当前血量百分比
    public float GetCurrentPercentage()
    {
        return currentHealthPercent;
    }
    
    // 更新血量条视觉效果
    private void UpdateHealthBarVisual()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealthPercent;
            
            // 更新颜色
            if (useGradient && healthGradient != null)
            {
                healthBarFill.color = healthGradient.Evaluate(currentHealthPercent);
            }
        }
    }
} 