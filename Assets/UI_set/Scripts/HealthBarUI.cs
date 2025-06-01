using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 血量条UI控制脚本
/// 负责血量条的视觉表现和动画效果
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [Header("血量条组件")]
    [SerializeField] private Slider healthSlider; // 血量条滑块
    [SerializeField] private Image healthFillImage; // 血量填充图像
    [SerializeField] private Image healthBackgroundImage; // 血量条背景图像
    [SerializeField] private Text healthText; // 血量文本显示（可选）
    
    [Header("动画设置")]
    [SerializeField] private float animationSpeed = 2f; // 血量变化动画速度
    [SerializeField] private bool useAnimation = true; // 是否使用动画效果
    
    [Header("颜色设置")]
    [SerializeField] private Color healthyColor = Color.green; // 健康状态颜色
    [SerializeField] private Color damagedColor = Color.yellow; // 受伤状态颜色
    [SerializeField] private Color criticalColor = Color.red; // 危险状态颜色
    [SerializeField] private float criticalThreshold = 0.25f; // 危险状态阈值
    [SerializeField] private float damagedThreshold = 0.5f; // 受伤状态阈值
    
    [Header("特效设置")]
    [SerializeField] private bool enablePulseEffect = true; // 启用脉冲效果
    [SerializeField] private float pulseSpeed = 3f; // 脉冲速度
    [SerializeField] private bool enableShakeEffect = true; // 启用震动效果
    [SerializeField] private float shakeDuration = 0.5f; // 震动持续时间
    [SerializeField] private float shakeIntensity = 5f; // 震动强度
    
    // 私有变量
    private float targetFillAmount;
    private float currentFillAmount;
    private Vector3 originalPosition;
    private bool isShaking = false;
    private float shakeTimer = 0f;
    
    // 事件
    public System.Action<float> OnHealthPercentageChanged;
    public System.Action OnHealthCritical;
    public System.Action OnHealthEmpty;
    
    private void Awake()
    {
        InitializeComponents();
        originalPosition = transform.localPosition;
    }
    
    private void Start()
    {
        // 初始化血量条为满血状态
        SetHealthPercentage(1f, false);
    }
    
    private void Update()
    {
        UpdateHealthBar();
        UpdateEffects();
    }
    
    /// <summary>
    /// 自动查找并初始化组件
    /// </summary>
    private void InitializeComponents()
    {
        // 如果没有手动指定滑块，尝试自动查找
        if (healthSlider == null)
        {
            healthSlider = GetComponentInChildren<Slider>();
        }
        
        // 如果没有手动指定填充图像，尝试从滑块获取
        if (healthFillImage == null && healthSlider != null)
        {
            healthFillImage = healthSlider.fillRect?.GetComponent<Image>();
        }
        
        // 如果没有手动指定背景图像，尝试从滑块获取
        if (healthBackgroundImage == null && healthSlider != null)
        {
            healthBackgroundImage = healthSlider.GetComponent<Image>();
        }
        
        // 如果没有手动指定文本，尝试自动查找
        if (healthText == null)
        {
            healthText = GetComponentInChildren<Text>();
        }
        
        // 初始化滑块设置
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.interactable = false; // 血量条不可交互
        }
    }
    
    /// <summary>
    /// 设置血量百分比
    /// </summary>
    /// <param name="percentage">血量百分比 (0-1)</param>
    /// <param name="animated">是否使用动画</param>
    public void SetHealthPercentage(float percentage, bool animated = true)
    {
        percentage = Mathf.Clamp01(percentage);
        targetFillAmount = percentage;
        
        if (!animated || !useAnimation)
        {
            currentFillAmount = targetFillAmount;
            UpdateSliderValue();
        }
        
        // 触发事件
        OnHealthPercentageChanged?.Invoke(percentage);
        
        // 检查特殊状态
        if (percentage <= 0f)
        {
            OnHealthEmpty?.Invoke();
        }
        else if (percentage <= criticalThreshold)
        {
            OnHealthCritical?.Invoke();
            TriggerShakeEffect();
        }
    }
    
    /// <summary>
    /// 设置血量值
    /// </summary>
    /// <param name="currentHealth">当前血量</param>
    /// <param name="maxHealth">最大血量</param>
    /// <param name="animated">是否使用动画</param>
    public void SetHealth(int currentHealth, int maxHealth, bool animated = true)
    {
        if (maxHealth <= 0) return;
        
        float percentage = (float)currentHealth / maxHealth;
        SetHealthPercentage(percentage, animated);
        
        // 更新文本显示
        UpdateHealthText(currentHealth, maxHealth);
    }
    
    /// <summary>
    /// 更新血量条
    /// </summary>
    private void UpdateHealthBar()
    {
        // 平滑动画
        if (useAnimation && Mathf.Abs(currentFillAmount - targetFillAmount) > 0.01f)
        {
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, animationSpeed * Time.deltaTime);
            UpdateSliderValue();
        }
        else if (!useAnimation)
        {
            currentFillAmount = targetFillAmount;
            UpdateSliderValue();
        }
    }
    
    /// <summary>
    /// 更新滑块数值和颜色
    /// </summary>
    private void UpdateSliderValue()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentFillAmount;
        }
        
        // 更新颜色
        UpdateHealthColor();
    }
    
    /// <summary>
    /// 根据血量百分比更新颜色
    /// </summary>
    private void UpdateHealthColor()
    {
        if (healthFillImage == null) return;
        
        Color targetColor;
        
        if (currentFillAmount <= criticalThreshold)
        {
            targetColor = criticalColor;
        }
        else if (currentFillAmount <= damagedThreshold)
        {
            targetColor = damagedColor;
        }
        else
        {
            targetColor = healthyColor;
        }
        
        healthFillImage.color = targetColor;
    }
    
    /// <summary>
    /// 更新血量文本显示
    /// </summary>
    /// <param name="currentHealth">当前血量</param>
    /// <param name="maxHealth">最大血量</param>
    private void UpdateHealthText(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }
    
    /// <summary>
    /// 更新特效
    /// </summary>
    private void UpdateEffects()
    {
        // 脉冲效果（仅在危险状态下）
        if (enablePulseEffect && currentFillAmount <= criticalThreshold && currentFillAmount > 0f)
        {
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            float scale = 1f + pulse * 0.1f;
            transform.localScale = Vector3.one * scale;
        }
        else
        {
            transform.localScale = Vector3.one;
        }
        
        // 震动效果
        UpdateShakeEffect();
    }
    
    /// <summary>
    /// 触发震动效果
    /// </summary>
    private void TriggerShakeEffect()
    {
        if (enableShakeEffect && !isShaking)
        {
            isShaking = true;
            shakeTimer = shakeDuration;
        }
    }
    
    /// <summary>
    /// 更新震动效果
    /// </summary>
    private void UpdateShakeEffect()
    {
        if (isShaking)
        {
            shakeTimer -= Time.deltaTime;
            
            if (shakeTimer > 0f)
            {
                // 随机震动
                Vector3 shakeOffset = Random.insideUnitCircle * shakeIntensity;
                transform.localPosition = originalPosition + shakeOffset;
            }
            else
            {
                // 震动结束，恢复原位置
                isShaking = false;
                transform.localPosition = originalPosition;
            }
        }
    }
    
    /// <summary>
    /// 重置血量条到满血状态
    /// </summary>
    public void ResetToFull()
    {
        SetHealthPercentage(1f, true);
    }
    
    /// <summary>
    /// 获取当前血量百分比
    /// </summary>
    public float GetCurrentPercentage()
    {
        return currentFillAmount;
    }
    
    /// <summary>
    /// 设置动画速度
    /// </summary>
    public void SetAnimationSpeed(float speed)
    {
        animationSpeed = Mathf.Max(0f, speed);
    }
    
    /// <summary>
    /// 启用或禁用动画
    /// </summary>
    public void SetAnimationEnabled(bool enabled)
    {
        useAnimation = enabled;
    }
} 