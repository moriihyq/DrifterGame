using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SliderHealthBarUIManager : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private Slider healthSlider; // 血量滑条
    [SerializeField] private Slider backgroundSlider; // 背景滑条（延迟效果）
    [SerializeField] private Text healthText; // 血量文本
    [SerializeField] private Image fillImage; // 滑条填充图像
    [SerializeField] private Image backgroundImage; // 背景图像
    [SerializeField] private CanvasGroup canvasGroup; // 用于淡入淡出效果
    
    [Header("显示设置")]
    [SerializeField] private bool showHealthText = true; // 是否显示血量文本
    [SerializeField] private bool useColorGradient = true; // 是否使用颜色渐变
    [SerializeField] private bool useBackgroundEffect = true; // 是否使用背景延迟效果
    [SerializeField] private bool autoHide = false; // 是否自动隐藏（满血时）
    [SerializeField] private float autoHideDelay = 3f; // 自动隐藏延迟
    
    [Header("颜色设置")]
    [SerializeField] private Color fullHealthColor = Color.green; // 满血颜色
    [SerializeField] private Color halfHealthColor = Color.yellow; // 半血颜色  
    [SerializeField] private Color lowHealthColor = Color.red; // 低血颜色
    [SerializeField] private Color backgroundHealthColor = new Color(1f, 1f, 1f, 0.3f); // 背景颜色
    
    [Header("动画设置")]
    [SerializeField] private float animationSpeed = 2f; // 动画速度
    [SerializeField] private float backgroundDelay = 1f; // 背景延迟时间
    [SerializeField] private bool smoothTransition = true; // 平滑过渡
    [SerializeField] private bool enableDamageFlash = true; // 启用受伤闪烁
    [SerializeField] private float flashDuration = 0.2f; // 闪烁持续时间
    [SerializeField] private Color flashColor = Color.white; // 闪烁颜色
    
    [Header("音效设置")]
    [SerializeField] private AudioSource audioSource; // 音效播放器
    [SerializeField] private AudioClip lowHealthWarning; // 低血警告音效
    [SerializeField] private float lowHealthThreshold = 0.25f; // 低血阈值
    [SerializeField] private float warningInterval = 2f; // 警告音效间隔
    
    // 私有变量
    private PlayerAttackSystem playerAttackSystem; // 玩家攻击系统引用
    private float targetHealthPercentage; // 目标血量百分比
    private float currentDisplayedHealth; // 当前显示的血量
    private float backgroundDisplayedHealth; // 背景显示的血量
    private int lastKnownHealth; // 上次已知血量
    private bool isFlashing; // 是否正在闪烁
    private Coroutine hideCoroutine; // 隐藏协程
    private Coroutine warningCoroutine; // 警告音效协程
    
    private void Start()
    {
        InitializeHealthBar();
    }
    
    private void Update()
    {
        UpdateHealthDisplay();
        CheckHealthChange();
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
                Debug.LogError("[HealthBarUIManager] 未找到PlayerAttackSystem组件！");
                return;
            }
        }
        
        // 自动获取组件
        SetupUIComponents();
        
        // 设置滑条的初始值
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
        }
        
        if (backgroundSlider != null)
        {
            backgroundSlider.minValue = 0f;
            backgroundSlider.maxValue = 1f;
        }
        
        // 初始化血量显示
        float initialHealth = GetCurrentHealthPercentage();
        currentDisplayedHealth = initialHealth;
        backgroundDisplayedHealth = initialHealth;
        lastKnownHealth = playerAttackSystem.GetCurrentHealth();
        
        UpdateSliderValue(initialHealth);
        if (backgroundSlider != null)
        {
            backgroundSlider.value = initialHealth;
        }
        
        // 设置初始可见性
        if (autoHide && initialHealth >= 1f)
        {
            SetUIVisibility(false);
        }
        
        Debug.Log("[HealthBarUIManager] 血量UI管理器初始化完成");
    }
    
    /// <summary>
    /// 设置UI组件
    /// </summary>
    private void SetupUIComponents()
    {
        // 自动获取Slider组件
        if (healthSlider == null)
        {
            healthSlider = GetComponent<Slider>();
            if (healthSlider == null)
            {
                healthSlider = GetComponentInChildren<Slider>();
            }
        }
        
        // 自动获取CanvasGroup
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
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
        
        // 自动获取音频源
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && lowHealthWarning != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
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
            currentDisplayedHealth = Mathf.Lerp(currentDisplayedHealth, targetHealthPercentage, 
                                               animationSpeed * Time.deltaTime);
        }
        else
        {
            currentDisplayedHealth = targetHealthPercentage;
        }
        
        // 更新主滑条
        UpdateSliderValue(currentDisplayedHealth);
        
        // 更新背景滑条（延迟效果）
        if (useBackgroundEffect && backgroundSlider != null)
        {
            UpdateBackgroundSlider();
        }
        
        // 更新颜色
        if (useColorGradient && fillImage != null && !isFlashing)
        {
            UpdateHealthColor(currentDisplayedHealth);
        }
        
        // 更新文本
        if (showHealthText && healthText != null)
        {
            UpdateHealthText();
        }
        
        // 检查低血警告
        CheckLowHealthWarning();
    }
    
    /// <summary>
    /// 检查血量变化
    /// </summary>
    private void CheckHealthChange()
    {
        if (playerAttackSystem == null) return;
        
        int currentHealth = playerAttackSystem.GetCurrentHealth();
        
        // 检测血量变化
        if (currentHealth != lastKnownHealth)
        {
            bool isDamage = currentHealth < lastKnownHealth;
            
            // 显示UI（如果设置了自动隐藏）
            if (autoHide)
            {
                ShowUI();
                RestartHideTimer();
            }
            
            // 受伤闪烁效果
            if (isDamage && enableDamageFlash)
            {
                StartDamageFlash();
            }
            
            lastKnownHealth = currentHealth;
        }
    }
    
    /// <summary>
    /// 更新背景滑条
    /// </summary>
    private void UpdateBackgroundSlider()
    {
        if (backgroundDisplayedHealth > currentDisplayedHealth)
        {
            // 血量降低时，背景滑条延迟下降
            backgroundDisplayedHealth = Mathf.Lerp(backgroundDisplayedHealth, currentDisplayedHealth, 
                                                  (animationSpeed / backgroundDelay) * Time.deltaTime);
        }
        else
        {
            // 血量恢复时，背景滑条立即跟上
            backgroundDisplayedHealth = currentDisplayedHealth;
        }
        
        backgroundSlider.value = backgroundDisplayedHealth;
        
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundHealthColor;
        }
    }
    
    /// <summary>
    /// 开始受伤闪烁效果
    /// </summary>
    private void StartDamageFlash()
    {
        if (isFlashing) return;
        StartCoroutine(DamageFlashCoroutine());
    }
    
    /// <summary>
    /// 受伤闪烁协程
    /// </summary>
    private IEnumerator DamageFlashCoroutine()
    {
        isFlashing = true;
        
        if (fillImage != null)
        {
            Color originalColor = fillImage.color;
            
            // 闪烁效果
            float elapsed = 0f;
            while (elapsed < flashDuration)
            {
                float t = Mathf.PingPong(elapsed * 8f, 1f); // 快速闪烁
                fillImage.color = Color.Lerp(originalColor, flashColor, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // 恢复原色
            fillImage.color = originalColor;
        }
        
        isFlashing = false;
    }
    
    /// <summary>
    /// 检查低血警告
    /// </summary>
    private void CheckLowHealthWarning()
    {
        if (currentDisplayedHealth <= lowHealthThreshold)
        {
            if (warningCoroutine == null)
            {
                warningCoroutine = StartCoroutine(LowHealthWarningCoroutine());
            }
        }
        else
        {
            if (warningCoroutine != null)
            {
                StopCoroutine(warningCoroutine);
                warningCoroutine = null;
            }
        }
    }
    
    /// <summary>
    /// 低血警告协程
    /// </summary>
    private IEnumerator LowHealthWarningCoroutine()
    {
        while (currentDisplayedHealth <= lowHealthThreshold)
        {
            // 播放警告音效
            if (audioSource != null && lowHealthWarning != null)
            {
                audioSource.PlayOneShot(lowHealthWarning);
            }
            
            yield return new WaitForSeconds(warningInterval);
        }
        
        warningCoroutine = null;
    }
    
    /// <summary>
    /// 显示UI
    /// </summary>
    public void ShowUI()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
    
    /// <summary>
    /// 隐藏UI
    /// </summary>
    public void HideUI()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
    
    /// <summary>
    /// 设置UI可见性
    /// </summary>
    private void SetUIVisibility(bool visible)
    {
        if (visible)
        {
            ShowUI();
        }
        else
        {
            HideUI();
        }
    }
    
    /// <summary>
    /// 重新开始隐藏计时器
    /// </summary>
    private void RestartHideTimer()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        hideCoroutine = StartCoroutine(HideUIAfterDelay());
    }
    
    /// <summary>
    /// 延迟隐藏UI协程
    /// </summary>
    private IEnumerator HideUIAfterDelay()
    {
        yield return new WaitForSeconds(autoHideDelay);
        
        // 只有在满血时才隐藏
        if (GetCurrentHealthPercentage() >= 1f)
        {
            HideUI();
        }
        
        hideCoroutine = null;
    }
    
    /// <summary>
    /// 获取当前血量百分比
    /// </summary>
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
    private void UpdateSliderValue(float value)
    {
        healthSlider.value = Mathf.Clamp01(value);
    }
    
    /// <summary>
    /// 更新血量颜色
    /// </summary>
    private void UpdateHealthColor(float healthPercentage)
    {
        Color targetColor;
        
        if (healthPercentage > 0.6f)
        {
            float t = (healthPercentage - 0.6f) / 0.4f;
            targetColor = Color.Lerp(halfHealthColor, fullHealthColor, t);
        }
        else if (healthPercentage > 0.3f)
        {
            float t = (healthPercentage - 0.3f) / 0.3f;
            targetColor = Color.Lerp(lowHealthColor, halfHealthColor, t);
        }
        else
        {
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
} 