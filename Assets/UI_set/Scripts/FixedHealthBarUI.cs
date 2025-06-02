using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FixedHealthBarUI : MonoBehaviour
{
    [Header("血量条组件")]
    [SerializeField] private Image healthBarFill;           // 血量条填充
    [SerializeField] private Image healthBarBackground;     // 血量条背景
    [SerializeField] private Image healthBarFrame;          // 血量条边框
    [SerializeField] private Image healthBarDecoration;     // 装饰图片（可选）
    
    [Header("文本显示")]
    [SerializeField] private TextMeshProUGUI healthText;    // 血量文本显示
    [SerializeField] private bool showHealthText = true;    // 是否显示血量数值
    [SerializeField] private string healthTextFormat = "{0}/{1}"; // 血量文本格式
    
    [Header("血量条设置")]
    [SerializeField] private Gradient healthGradient;       // 血量颜色渐变
    [SerializeField] private float smoothSpeed = 5f;        // 血量条变化平滑速度
    [SerializeField] private bool useGradient = true;       // 是否使用颜色渐变
    [SerializeField] private AnimationCurve damageCurve;    // 受伤时的动画曲线
    
    [Header("特效设置")]
    [SerializeField] private bool enableDamageEffect = true;    // 是否启用受伤特效
    [SerializeField] private float damageEffectDuration = 0.3f; // 受伤特效持续时间
    [SerializeField] private float damageEffectScale = 1.2f;    // 受伤时的缩放倍数
    
    // 私有变量
    private PlayerController playerController;
    private PlayerAttackSystem playerAttackSystem;
    private float targetHealthPercent = 1f;
    private float currentHealthPercent = 1f;
    private float damageEffectTimer = 0f;
    private Vector3 originalScale;
    private int lastHealth;
    
    private void Awake()
    {
        // 记录原始缩放
        originalScale = transform.localScale;
        
        // 设置默认的血量颜色渐变
        if (healthGradient == null || healthGradient.colorKeys.Length == 0)
        {
            healthGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            colorKeys[0].color = new Color(0.8f, 0.2f, 0.2f); // 深红色
            colorKeys[0].time = 0.0f;
            colorKeys[1].color = new Color(0.9f, 0.7f, 0.2f); // 橙黄色
            colorKeys[1].time = 0.5f;
            colorKeys[2].color = new Color(0.2f, 0.8f, 0.2f); // 绿色
            colorKeys[2].time = 1.0f;
            
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0].alpha = 1.0f;
            alphaKeys[0].time = 0.0f;
            alphaKeys[1].alpha = 1.0f;
            alphaKeys[1].time = 1.0f;
            
            healthGradient.SetKeys(colorKeys, alphaKeys);
        }
        
        // 设置默认的受伤动画曲线
        if (damageCurve == null || damageCurve.keys.Length == 0)
        {
            damageCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        }
    }
    
    private void Start()
    {
        // 查找玩家对象
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = GameObject.Find("Player");
            if (player == null)
            {
                PlayerController pc = FindFirstObjectByType<PlayerController>();
                if (pc != null)
                {
                    player = pc.gameObject;
                }
            }
        }
        
        if (player != null)
        {
            // 获取组件引用
            playerController = player.GetComponent<PlayerController>();
            playerAttackSystem = player.GetComponent<PlayerAttackSystem>();
            
            if (playerController != null)
            {
                lastHealth = playerController.currentHealth;
            }
            else if (playerAttackSystem != null)
            {
                lastHealth = playerAttackSystem.GetCurrentHealth();
            }
            
            // 初始更新
            UpdateHealthBar();
            UpdateHealthText();
        }
        else
        {
            Debug.LogError("未找到玩家对象!");
        }
    }
    
    private void Update()
    {
        // 检查血量变化
        CheckHealthChange();
        
        // 平滑更新血量条
        SmoothUpdateHealthBar();
        
        // 处理受伤特效
        HandleDamageEffect();
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
        else
        {
            return;
        }
        
        // 检查血量是否变化
        if (currentHealth != lastHealth)
        {
            // 如果血量减少，触发受伤特效
            if (currentHealth < lastHealth && enableDamageEffect)
            {
                TriggerDamageEffect();
            }
            
            lastHealth = currentHealth;
            targetHealthPercent = (float)currentHealth / maxHealth;
            
            // 更新血量文本
            UpdateHealthText();
        }
    }
    
    private void SmoothUpdateHealthBar()
    {
        // 平滑过渡到目标血量
        currentHealthPercent = Mathf.Lerp(currentHealthPercent, targetHealthPercent, Time.deltaTime * smoothSpeed);
        
        // 更新血量条填充
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
    
    private void UpdateHealthText()
    {
        if (healthText != null && showHealthText)
        {
            int currentHealth = 0;
            int maxHealth = 0;
            
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
            
            healthText.text = string.Format(healthTextFormat, currentHealth, maxHealth);
        }
    }
    
    private void TriggerDamageEffect()
    {
        damageEffectTimer = damageEffectDuration;
    }
    
    private void HandleDamageEffect()
    {
        if (damageEffectTimer > 0)
        {
            damageEffectTimer -= Time.deltaTime;
            
            float normalizedTime = 1f - (damageEffectTimer / damageEffectDuration);
            float curveValue = damageCurve.Evaluate(normalizedTime);
            
            // 应用缩放效果
            float scale = 1f + (damageEffectScale - 1f) * curveValue;
            transform.localScale = originalScale * scale;
            
            // 可选：添加颜色闪烁效果
            if (healthBarFill != null)
            {
                Color baseColor = useGradient ? healthGradient.Evaluate(currentHealthPercent) : healthBarFill.color;
                Color flashColor = Color.Lerp(baseColor, Color.white, curveValue * 0.5f);
                healthBarFill.color = flashColor;
            }
        }
        else
        {
            // 恢复原始缩放
            transform.localScale = originalScale;
        }
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
    
    // 公共方法
    public void SetHealthBarImages(Sprite fillSprite, Sprite backgroundSprite, Sprite frameSprite = null, Sprite decorationSprite = null)
    {
        if (healthBarFill != null && fillSprite != null)
            healthBarFill.sprite = fillSprite;
        
        if (healthBarBackground != null && backgroundSprite != null)
            healthBarBackground.sprite = backgroundSprite;
        
        if (healthBarFrame != null && frameSprite != null)
            healthBarFrame.sprite = frameSprite;
        
        if (healthBarDecoration != null && decorationSprite != null)
            healthBarDecoration.sprite = decorationSprite;
    }
    
    public void ForceUpdateHealthBar()
    {
        UpdateHealthBar();
        UpdateHealthText();
    }
} 