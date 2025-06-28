using UnityEngine;
using UnityEngine.UI;

public class SliderHealthBarUI : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private Slider healthSlider; // 血量滑条
    [SerializeField] private Text healthText; // 血量文本（可选）
    [SerializeField] private Image fillImage; // 滑条填充图像
    
    [Header("显示设置")]
    [SerializeField] private bool showHealthText = true; // 是否显示血量文本
    [SerializeField] private bool useColorGradient = true; // 是否使用颜色渐变
    
    [Header("颜色设置")]
    [SerializeField] private Color fullHealthColor = Color.green; // 满血颜色
    [SerializeField] private Color halfHealthColor = Color.yellow; // 半血颜色  
    [SerializeField] private Color lowHealthColor = Color.red; // 低血颜色
    
    [Header("动画设置")]
    [SerializeField] private float animationSpeed = 2f; // 动画速度
    [SerializeField] private bool smoothTransition = true; // 平滑过渡
    
    // 私有变量
    private PlayerAttackSystem playerAttackSystem; // 玩家攻击系统引用
    private float targetHealthPercentage; // 目标血量百分比
    private float currentDisplayedHealth; // 当前显示的血量
    
    private void Start()
    {
        InitializeHealthBar();
    }
    
    private void Update()
    {
        UpdateHealthDisplay();
    }
    
    /// <summary>
    /// 初始化血量条
    /// </summary>
    private void InitializeHealthBar()
    {
        // 查找玩家攻击系统组件
        if (playerAttackSystem == null)
        {
            playerAttackSystem = FindObjectOfType<PlayerAttackSystem>();
            if (playerAttackSystem == null)
            {
                Debug.LogError("[SliderHealthBarUI] 未找到PlayerAttackSystem组件！请确保场景中有玩家对象");
                return;
            }
        }
        
        // 自动获取Slider组件（如果未指定）
        if (healthSlider == null)
        {
            healthSlider = GetComponent<Slider>();
            if (healthSlider == null)
            {
                healthSlider = GetComponentInChildren<Slider>();
                if (healthSlider == null)
                {
                    Debug.LogError("[SliderHealthBarUI] 未找到Slider组件！请确保UI对象上有Slider组件");
                    return;
                }
            }
        }
        
        // 自动获取填充图像
        if (fillImage == null && healthSlider != null)
        {
            fillImage = healthSlider.fillRect?.GetComponent<Image>();
        }
        
        // 自动获取文本组件
        if (healthText == null && showHealthText)
        {
            healthText = GetComponentInChildren<Text>();
        }
        
        // 设置滑条的初始值
        healthSlider.minValue = 0f;
        healthSlider.maxValue = 1f;
        
        // 初始化血量显示
        currentDisplayedHealth = GetCurrentHealthPercentage();
        UpdateSliderValue(currentDisplayedHealth);
        
        Debug.Log("[SliderHealthBarUI] 血量UI初始化完成");
    }
    
    /// <summary>
    /// 更新血量显示
    /// </summary>
    private void UpdateHealthDisplay()
    {
        if (playerAttackSystem == null || healthSlider == null) return;
        
        // 获取目标血量百分比
        targetHealthPercentage = GetCurrentHealthPercentage();
        
        // 平滑过渡或直接更新
        if (smoothTransition)
        {
            // 平滑过渡到目标血量
            currentDisplayedHealth = Mathf.Lerp(currentDisplayedHealth, targetHealthPercentage, 
                                               animationSpeed * Time.deltaTime);
        }
        else
        {
            // 直接更新
            currentDisplayedHealth = targetHealthPercentage;
        }
        
        // 更新滑条值
        UpdateSliderValue(currentDisplayedHealth);
        
        // 更新颜色
        if (useColorGradient && fillImage != null)
        {
            UpdateHealthColor(currentDisplayedHealth);
        }
        
        // 更新文本
        if (showHealthText && healthText != null)
        {
            UpdateHealthText();
        }
    }
    
    /// <summary>
    /// 获取当前血量百分比
    /// </summary>
    /// <returns>血量百分比 (0-1)</returns>
    private float GetCurrentHealthPercentage()
    {
        if (playerAttackSystem == null) return 0f;
        
        int currentHealth = playerAttackSystem.GetCurrentHealth();
        int maxHealth = playerAttackSystem.GetMaxHealth();
        
        if (maxHealth <= 0) return 0f;
        
        return (float)currentHealth / maxHealth;
    }
    
    /// <summary>
    /// 更新滑条数值
    /// </summary>
    /// <param name="value">血量百分比</param>
    private void UpdateSliderValue(float value)
    {
        healthSlider.value = Mathf.Clamp01(value);
    }
    
    /// <summary>
    /// 更新血量颜色
    /// </summary>
    /// <param name="healthPercentage">血量百分比</param>
    private void UpdateHealthColor(float healthPercentage)
    {
        Color targetColor;
        
        if (healthPercentage > 0.6f)
        {
            // 满血到半血：绿色到黄色
            float t = (healthPercentage - 0.6f) / 0.4f;
            targetColor = Color.Lerp(halfHealthColor, fullHealthColor, t);
        }
        else if (healthPercentage > 0.3f)
        {
            // 半血到低血：黄色到红色
            float t = (healthPercentage - 0.3f) / 0.3f;
            targetColor = Color.Lerp(lowHealthColor, halfHealthColor, t);
        }
        else
        {
            // 低血：红色
            targetColor = lowHealthColor;
        }
        
        fillImage.color = targetColor;
    }
    
    /// <summary>
    /// 更新血量文本
    /// </summary>
    private void UpdateHealthText()
    {
        if (playerAttackSystem == null) return;
        
        int currentHealth = playerAttackSystem.GetCurrentHealth();
        int maxHealth = playerAttackSystem.GetMaxHealth();
        
        healthText.text = $"{currentHealth} / {maxHealth}";
    }
    
    /// <summary>
    /// 手动设置PlayerAttackSystem引用
    /// </summary>
    /// <param name="attackSystem">玩家攻击系统</param>
    public void SetPlayerAttackSystem(PlayerAttackSystem attackSystem)
    {
        playerAttackSystem = attackSystem;
        if (healthSlider != null)
        {
            InitializeHealthBar();
        }
    }
    
    /// <summary>
    /// 设置血量条颜色
    /// </summary>
    /// <param name="full">满血颜色</param>
    /// <param name="half">半血颜色</param>
    /// <param name="low">低血颜色</param>
    public void SetHealthColors(Color full, Color half, Color low)
    {
        fullHealthColor = full;
        halfHealthColor = half;
        lowHealthColor = low;
    }
    
    /// <summary>
    /// 启用/禁用平滑过渡
    /// </summary>
    /// <param name="enable">是否启用</param>
    public void SetSmoothTransition(bool enable)
    {
        smoothTransition = enable;
    }
    
    /// <summary>
    /// 设置动画速度
    /// </summary>
    /// <param name="speed">动画速度</param>
    public void SetAnimationSpeed(float speed)
    {
        animationSpeed = Mathf.Max(0.1f, speed);
    }
} 