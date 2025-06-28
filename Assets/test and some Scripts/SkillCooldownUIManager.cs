using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkillCooldownUIManager : MonoBehaviour
{
    [Header("Q键技能 - 魔法子弹")]
    [SerializeField] private GameObject qSkillUI;
    [SerializeField] private Image qSkillCooldownFill;
    [SerializeField] private Text qSkillCooldownText;
    [SerializeField] private Image qSkillIcon;
    
    [Header("X键技能 - 空中攻击")]
    [SerializeField] private GameObject xSkillUI;
    [SerializeField] private Image xSkillCooldownFill;
    [SerializeField] private Text xSkillCooldownText;
    [SerializeField] private Image xSkillIcon;
    
    [Header("R键技能 - 风力")]
    [SerializeField] private GameObject rSkillUI;
    [SerializeField] private Image rSkillCooldownFill;
    [SerializeField] private Text rSkillCooldownText;
    [SerializeField] private Image rSkillIcon;
    
    [Header("技能背景图片")]
    [SerializeField] private Sprite qSkillBackgroundSprite;
    [SerializeField] private Sprite xSkillBackgroundSprite;
    [SerializeField] private Sprite rSkillBackgroundSprite;
    
    [Header("显示设置")]
    [SerializeField] private bool showReadyText = true;
    [SerializeField] private string readyText = "Q";
    [SerializeField] private string xReadyText = "X";
    [SerializeField] private string rReadyText = "R";
    
    [Header("冷却遮盖效果设置")]
    [SerializeField] private Color cooldownMaskColor = new Color(0.2f, 0.2f, 0.2f, 0.8f); // 深灰色遮盖
    [SerializeField] private bool smoothRotation = true; // 平滑旋转效果
    
    // 技能组件引用
    private MagicBulletSkill magicBulletSkill;
    private PlayerLevelSkillSystem aerialAttackSkill;
    private WindPower windPowerSkill;
    
    // 冷却时间常量
    private const float MAGIC_BULLET_COOLDOWN = 30f;
    private const float AERIAL_ATTACK_COOLDOWN = 20f;
    private const float WIND_POWER_COOLDOWN = 15f;
    
    void Start()
    {
        // 查找技能组件
        FindSkillComponents();
        
        // 初始化UI
        InitializeUI();
    }
    
    void Update()
    {
        // 更新各个技能的UI
        UpdateQSkillUI();
        UpdateXSkillUI();
        UpdateRSkillUI();
    }
    
    void FindSkillComponents()
    {
        // 查找玩家对象上的技能组件
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("SkillCooldownUIManager: 未找到Player标签的对象，尝试查找第一个相关组件");
            magicBulletSkill = FindObjectOfType<MagicBulletSkill>();
            aerialAttackSkill = FindObjectOfType<PlayerLevelSkillSystem>();
            windPowerSkill = FindObjectOfType<WindPower>();
        }
        else
        {
            magicBulletSkill = player.GetComponent<MagicBulletSkill>();
            aerialAttackSkill = player.GetComponent<PlayerLevelSkillSystem>();
            windPowerSkill = player.GetComponent<WindPower>();
        }
        
        // 检查组件是否找到
        if (magicBulletSkill == null)
            Debug.LogWarning("SkillCooldownUIManager: 未找到MagicBulletSkill组件");
        if (aerialAttackSkill == null)
            Debug.LogWarning("SkillCooldownUIManager: 未找到PlayerLevelSkillSystem组件");
        if (windPowerSkill == null)
            Debug.LogWarning("SkillCooldownUIManager: 未找到WindPower组件");
    }
    
    void InitializeUI()
    {
        // 设置技能图标背景
        if (qSkillIcon != null && qSkillBackgroundSprite != null)
            qSkillIcon.sprite = qSkillBackgroundSprite;
        if (xSkillIcon != null && xSkillBackgroundSprite != null)
            xSkillIcon.sprite = xSkillBackgroundSprite;
        if (rSkillIcon != null && rSkillBackgroundSprite != null)
            rSkillIcon.sprite = rSkillBackgroundSprite;
            
        // 显示所有技能UI（始终可见）
        SetSkillUIActive(qSkillUI, true);
        SetSkillUIActive(xSkillUI, true);
        SetSkillUIActive(rSkillUI, true);
        
        // 设置冷却遮盖的初始属性
        SetupCooldownMask(qSkillCooldownFill);
        SetupCooldownMask(xSkillCooldownFill);
        SetupCooldownMask(rSkillCooldownFill);
        
        // 初始化为就绪状态
        SetSkillReadyState(qSkillCooldownFill, qSkillCooldownText, readyText);
        SetSkillReadyState(xSkillCooldownFill, xSkillCooldownText, xReadyText);
        SetSkillReadyState(rSkillCooldownFill, rSkillCooldownText, rReadyText);
    }
    
    void SetupCooldownMask(Image maskImage)
    {
        if (maskImage == null) return;
        
        // 设置为Radial360填充方式
        maskImage.type = Image.Type.Filled;
        maskImage.fillMethod = Image.FillMethod.Radial360;
        maskImage.fillOrigin = (int)Image.Origin360.Top; // 从顶部开始
        maskImage.fillClockwise = true; // 顺时针方向
        
        // 设置遮盖颜色
        maskImage.color = cooldownMaskColor;
        
        // 初始为不可见
        maskImage.fillAmount = 0f;
        
        Debug.Log($"设置冷却遮盖: 颜色={cooldownMaskColor}, 填充方式=Radial360");
    }
    
    void UpdateQSkillUI()
    {
        if (magicBulletSkill == null) return;
        
        // 通过反射获取私有字段（因为MagicBulletSkill没有公共属性）
        var magicBulletType = typeof(MagicBulletSkill);
        var isOnCooldownField = magicBulletType.GetField("isOnCooldown", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var currentCooldownTimeField = magicBulletType.GetField("currentCooldownTime", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (isOnCooldownField != null && currentCooldownTimeField != null)
        {
            bool isOnCooldown = (bool)isOnCooldownField.GetValue(magicBulletSkill);
            float currentCooldownTime = (float)currentCooldownTimeField.GetValue(magicBulletSkill);
            
            if (isOnCooldown)
            {
                // 确保currentCooldownTime大于0
                if (currentCooldownTime > 0)
                {
                    UpdateSkillCooldownUI(qSkillCooldownFill, qSkillCooldownText, 
                        currentCooldownTime, MAGIC_BULLET_COOLDOWN, "Q", true);
                }
                else
                {
                    SetSkillReadyState(qSkillCooldownFill, qSkillCooldownText, readyText);
                }
            }
            else
            {
                SetSkillReadyState(qSkillCooldownFill, qSkillCooldownText, readyText);
            }
        }
        else
        {
            SetSkillReadyState(qSkillCooldownFill, qSkillCooldownText, readyText);
        }
    }
    
    void UpdateXSkillUI()
    {
        if (aerialAttackSkill == null) return;
        
        // 使用公共属性
        if (aerialAttackSkill.IsAerialAttackOnCooldown)
        {
            UpdateSkillCooldownUI(xSkillCooldownFill, xSkillCooldownText, 
                aerialAttackSkill.CurrentCooldownTime, AERIAL_ATTACK_COOLDOWN, "X", true);
        }
        else
        {
            SetSkillReadyState(xSkillCooldownFill, xSkillCooldownText, xReadyText);
        }
    }
    
    void UpdateRSkillUI()
    {
        if (windPowerSkill == null) return;
        
        // 使用公共属性
        if (windPowerSkill.IsWindPowerOnCooldown)
        {
            UpdateSkillCooldownUI(rSkillCooldownFill, rSkillCooldownText, 
                windPowerSkill.CurrentCooldownTime, WIND_POWER_COOLDOWN, "R", true);
        }
        else
        {
            SetSkillReadyState(rSkillCooldownFill, rSkillCooldownText, rReadyText);
        }
    }
    
    void UpdateSkillCooldownUI(Image fillImage, Text cooldownText, 
        float currentTime, float maxTime, string keyName, bool startCooldown)
    {
        // 更新圆形遮盖（从满圆慢慢变小）
        if (fillImage != null)
        {
            // 如果是开始冷却，立即设置为满圆
            if (startCooldown && fillImage.fillAmount < 0.1f)
            {
                fillImage.fillAmount = 1f;
            }
            
            // 计算剩余时间的比例（1表示满圆遮盖，0表示无遮盖）
            float fillAmount = currentTime / maxTime;
            
            if (smoothRotation)
            {
                // 平滑过渡效果
                fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, fillAmount, Time.deltaTime * 5f);
            }
            else
            {
                // 直接设置
                fillImage.fillAmount = fillAmount;
            }
            
            // 设置遮盖颜色（确保可见）
            fillImage.color = cooldownMaskColor;
        }
        
        // 更新冷却时间文本
        if (cooldownText != null)
        {
            // 确保currentTime不为负数并向上取整
            currentTime = Mathf.Max(0f, currentTime);
            cooldownText.text = Mathf.Ceil(currentTime).ToString();
            cooldownText.color = Color.white;
        }
    }
    
    void SetSkillReadyState(Image fillImage, Text cooldownText, string keyText)
    {
        // 清空遮盖（技能就绪时无遮盖）
        if (fillImage != null)
        {
            if (smoothRotation)
            {
                // 平滑消失
                fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, 0f, Time.deltaTime * 8f);
            }
            else
            {
                fillImage.fillAmount = 0f;
            }
        }
        
        // 就绪状态下不显示任何文字
        if (cooldownText != null)
        {
            cooldownText.text = "";
        }
    }
    
    void SetSkillUIActive(GameObject skillUI, bool active)
    {
        if (skillUI != null)
        {
            skillUI.SetActive(active);
        }
    }
    
    // 手动连接UI元素的方法（在Inspector中调用）
    [ContextMenu("自动查找UI元素")]
    public void AutoFindUIElements()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("场景中没有找到Canvas");
            return;
        }
        
        // 尝试查找Q技能UI
        Transform qSkillTransform = canvas.transform.Find("QSkillUI");
        if (qSkillTransform != null)
        {
            qSkillUI = qSkillTransform.gameObject;
            qSkillCooldownFill = qSkillTransform.Find("CooldownFill")?.GetComponent<Image>();
            qSkillCooldownText = qSkillTransform.Find("CooldownText")?.GetComponent<Text>();
            qSkillIcon = qSkillTransform.Find("SkillIcon")?.GetComponent<Image>();
        }
        
        // 尝试查找X技能UI
        Transform xSkillTransform = canvas.transform.Find("XSkillUI");
        if (xSkillTransform != null)
        {
            xSkillUI = xSkillTransform.gameObject;
            xSkillCooldownFill = xSkillTransform.Find("CooldownFill")?.GetComponent<Image>();
            xSkillCooldownText = xSkillTransform.Find("CooldownText")?.GetComponent<Text>();
            xSkillIcon = xSkillTransform.Find("SkillIcon")?.GetComponent<Image>();
        }
        
        // 尝试查找R技能UI
        Transform rSkillTransform = canvas.transform.Find("RSkillUI");
        if (rSkillTransform != null)
        {
            rSkillUI = rSkillTransform.gameObject;
            rSkillCooldownFill = rSkillTransform.Find("CooldownFill")?.GetComponent<Image>();
            rSkillCooldownText = rSkillTransform.Find("CooldownText")?.GetComponent<Text>();
            rSkillIcon = rSkillTransform.Find("SkillIcon")?.GetComponent<Image>();
        }
        
        Debug.Log("UI元素查找完成");
    }
    
    [ContextMenu("重新设置冷却遮盖")]
    public void ResetCooldownMasks()
    {
        SetupCooldownMask(qSkillCooldownFill);
        SetupCooldownMask(xSkillCooldownFill);
        SetupCooldownMask(rSkillCooldownFill);
        Debug.Log("冷却遮盖重新设置完成");
    }
} 