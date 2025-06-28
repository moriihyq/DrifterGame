using UnityEngine;
using UnityEngine.UI;

public class WindPowerUIManager : MonoBehaviour
{
    [Header("UI元素")]
    [SerializeField] private GameObject windPowerUI;
    [SerializeField] private Text cooldownText;
    [SerializeField] private Image cooldownFillImage;
    [SerializeField] private GameObject activeFX;
    [SerializeField] private Text durationText;
    
    // 风力技能引用
    private WindPower windPower;
    
    void Start()
    {
        // 获取风力技能组件
        windPower = FindObjectOfType<WindPower>();
        
        if (windPower == null)
        {
            Debug.LogError("WindPowerUIManager: 未找到WindPower组件！");
            enabled = false;
            return;
        }
        
        // 尝试自动获取UI元素
        if (windPowerUI == null)
        {
            windPowerUI = GameObject.Find("WindPowerUI");
        }
        
        if (cooldownText == null && windPowerUI != null)
        {
            cooldownText = windPowerUI.transform.Find("CooldownText")?.GetComponent<Text>();
        }
        
        if (cooldownFillImage == null && windPowerUI != null)
        {
            cooldownFillImage = windPowerUI.transform.Find("CooldownFill")?.GetComponent<Image>();
        }
        
        if (activeFX == null && windPowerUI != null)
        {
            activeFX = windPowerUI.transform.Find("ActiveFX")?.gameObject;
        }
        
        if (durationText == null && windPowerUI != null)
        {
            durationText = windPowerUI.transform.Find("DurationText")?.GetComponent<Text>();
        }
        
        // 默认隐藏UI
        HideUI();
    }
    
    void Update()
    {
        UpdateUI();
    }
    
    void UpdateUI()
    {
        if (windPower == null || windPowerUI == null) return;
        
        // 处理技能激活状态
        if (windPower.IsWindPowerActive)
        {
            // 显示技能持续时间
            ShowActiveUI();
            UpdateDurationUI();
        }
        else
        {
            HideActiveUI();
        }
        
        // 处理技能冷却状态
        if (windPower.IsWindPowerOnCooldown)
        {
            ShowCooldownUI();
            UpdateCooldownUI();
        }
        else
        {
            HideCooldownUI();
        }
    }
    
    void UpdateCooldownUI()
    {
        if (cooldownText != null)
        {
            cooldownText.text = $"冷却: {windPower.CurrentCooldownTime:F1}s";
        }
        
        if (cooldownFillImage != null)
        {
            // 假设风力技能的冷却时间是15秒（根据WindPower.cs中的默认值）
            float fillAmount = windPower.CurrentCooldownTime / 15f;
            cooldownFillImage.fillAmount = fillAmount;
        }
    }
    
    void UpdateDurationUI()
    {
        if (durationText != null)
        {
            durationText.text = $"持续: {windPower.CurrentDuration:F1}s";
        }
    }
    
    void ShowUI()
    {
        if (windPowerUI != null)
        {
            windPowerUI.SetActive(true);
        }
    }
    
    void HideUI()
    {
        if (windPowerUI != null)
        {
            windPowerUI.SetActive(false);
        }
    }
    
    void ShowCooldownUI()
    {
        ShowUI();
        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(true);
        }
        
        if (cooldownFillImage != null)
        {
            cooldownFillImage.gameObject.SetActive(true);
        }
    }
    
    void HideCooldownUI()
    {
        if (!windPower.IsWindPowerActive)
        {
            HideUI();
        }
        
        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(false);
        }
        
        if (cooldownFillImage != null)
        {
            cooldownFillImage.gameObject.SetActive(false);
        }
    }
    
    void ShowActiveUI()
    {
        ShowUI();
        if (activeFX != null)
        {
            activeFX.SetActive(true);
        }
        
        if (durationText != null)
        {
            durationText.gameObject.SetActive(true);
        }
    }
    
    void HideActiveUI()
    {
        if (activeFX != null)
        {
            activeFX.SetActive(false);
        }
        
        if (durationText != null)
        {
            durationText.gameObject.SetActive(false);
        }
    }
}
