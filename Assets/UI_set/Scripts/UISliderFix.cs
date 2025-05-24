using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISliderFix : MonoBehaviour
{
    [Header("滑动条设置")]
    [SerializeField] private Slider targetSlider;
    [SerializeField] private RectTransform fillRect;
    [SerializeField] private RectTransform handleRect;
    [SerializeField] private TextMeshProUGUI volumeText;
    [SerializeField] private string valueFormat = "{0}%";

    [Header("滑动条区域")]
    [SerializeField] private RectTransform sliderArea; // 滑动条的可交互区域
    [SerializeField] private RectTransform fillAreaRect; // 填充区域的父对象
    [SerializeField] private RectTransform handleAreaRect; // 把手区域的父对象

    [Header("音频设置")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    private bool isDragging = false;
    private float lastVolume = 0.8f;
    private Canvas parentCanvas;
    private Camera canvasCamera;
    
    // 调试模式
    [SerializeField] private bool debugMode = false;

    private void Start()
    {
        // 初始化组件引用
        InitializeComponents();

        // 确保滑动条可交互
        targetSlider.interactable = true;

        // 设置滑动条值变化事件
        targetSlider.onValueChanged.RemoveAllListeners();
        targetSlider.onValueChanged.AddListener(OnSliderValueChanged);

        // 初始化滑动条位置和音量
        InitializeVolumeValue();

        // 初次更新UI和音量
        ForceUpdateUI(targetSlider.value);
        UpdateVolume(targetSlider.value);
    }

    private void InitializeComponents()
    {
        // 初始化滑动条
        if (targetSlider == null)
        {
            targetSlider = GetComponent<Slider>();
            if (targetSlider == null)
            {
                Debug.LogError("没有找到Slider组件！");
                return;
            }
        }

        // 找到父Canvas和相机
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            if (parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                canvasCamera = parentCanvas.worldCamera;
            }
        }

        // 滑动区域
        if (sliderArea == null)
        {
            sliderArea = targetSlider.GetComponent<RectTransform>();
        }

        // 找到子对象组件（如果没有手动分配）
        if (fillAreaRect == null)
        {
            Transform fillArea = targetSlider.transform.Find("Fill Area");
            if (fillArea != null)
                fillAreaRect = fillArea.GetComponent<RectTransform>();
        }

        if (fillRect == null && fillAreaRect != null)
        {
            Transform fill = fillAreaRect.transform.Find("Fill");
            if (fill != null)
                fillRect = fill.GetComponent<RectTransform>();
        }

        if (handleAreaRect == null)
        {
            Transform handleArea = targetSlider.transform.Find("Handle Slide Area");
            if (handleArea != null)
                handleAreaRect = handleArea.GetComponent<RectTransform>();
        }

        if (handleRect == null && handleAreaRect != null)
        {
            Transform handle = handleAreaRect.transform.Find("Handle");
            if (handle != null)
                handleRect = handle.GetComponent<RectTransform>();
        }

        // 检查组件
        if (debugMode)
        {
            string status = "组件状态：\n";
            status += "滑动条: " + (targetSlider != null ? "已找到" : "未找到") + "\n";
            status += "填充区域父对象: " + (fillAreaRect != null ? "已找到" : "未找到") + "\n";
            status += "填充区域: " + (fillRect != null ? "已找到" : "未找到") + "\n";
            status += "把手区域父对象: " + (handleAreaRect != null ? "已找到" : "未找到") + "\n";
            status += "把手: " + (handleRect != null ? "已找到" : "未找到") + "\n";
            status += "音量文本: " + (volumeText != null ? "已找到" : "未找到");
            Debug.Log(status);
        }
    }

    private void InitializeVolumeValue()
    {
        if (PlayerPrefs.HasKey("GameVolume"))
        {
            lastVolume = PlayerPrefs.GetFloat("GameVolume");
            targetSlider.value = lastVolume;
        }
        else
        {
            targetSlider.value = 0.8f;
            lastVolume = 0.8f;
        }
    }

    private void Update()
    {
        // 检测鼠标点击和拖动
        if (Input.GetMouseButtonDown(0))
        {
            // 检查是否点击了滑动条区域
            if (RectTransformUtility.RectangleContainsScreenPoint(sliderArea, Input.mousePosition, canvasCamera))
            {
                isDragging = true;
                UpdateSliderValueFromMousePosition();
            }
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
        }

        // 如果正在拖动，更新滑动条值
        if (isDragging)
        {
            UpdateSliderValueFromMousePosition();
        }
    }

    private void UpdateSliderValueFromMousePosition()
    {
        if (targetSlider == null || sliderArea == null)
            return;
        
        // 获取鼠标在滑动条上的位置
        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            sliderArea,
            Input.mousePosition,
            canvasCamera,
            out localPoint))
        {
            return; // 转换失败
        }

        // 获取滑动条的实际区域信息
        float sliderLength;
        float normalizedPosition;
        
        // 考虑滑动条的方向
        if (targetSlider.direction == Slider.Direction.LeftToRight || 
            targetSlider.direction == Slider.Direction.RightToLeft)
        {
            // 水平滑动条
            sliderLength = sliderArea.rect.width;
            
            // 计算本地点的归一化位置
            normalizedPosition = (localPoint.x - sliderArea.rect.xMin) / sliderLength;
        }
        else
        {
            // 垂直滑动条
            sliderLength = sliderArea.rect.height;
            normalizedPosition = (localPoint.y - sliderArea.rect.yMin) / sliderLength;
        }
        
        // 限制范围在0-1之间
        normalizedPosition = Mathf.Clamp01(normalizedPosition);
        
        // 根据滑动条方向调整值
        if (targetSlider.direction == Slider.Direction.RightToLeft || 
            targetSlider.direction == Slider.Direction.TopToBottom)
        {
            normalizedPosition = 1 - normalizedPosition;
        }
        
        // 设置滑动条值，确保UI和音量立即更新
        targetSlider.value = normalizedPosition;
        
        // 在拖动过程中直接更新UI和音量，确保实时响应
        ForceUpdateUI(normalizedPosition);
        UpdateVolume(normalizedPosition);
    }

    private void OnSliderValueChanged(float value)
    {
        // 强制更新UI，确保视觉元素与值匹配
        ForceUpdateUI(value);
        UpdateVolume(value);
        
        // 保存音量设置
        lastVolume = value;
        PlayerPrefs.SetFloat("GameVolume", value);
        PlayerPrefs.Save();
        
        if (debugMode)
        {
            Debug.Log("滑动条值改变: " + value + " (" + (value * 100) + "%)");
        }
    }

    private void ForceUpdateUI(float value)
    {
        // 确保先禁用原生更新，然后使用我们的自定义更新
        Slider.SliderEvent originalEvent = targetSlider.onValueChanged;
        targetSlider.onValueChanged = new Slider.SliderEvent();
        
        // 直接设置内部值而不触发事件
        targetSlider.SetValueWithoutNotify(value);
        
        // 更新UI元素
        UpdateUIElements(value);
        
        // 恢复原来的事件
        targetSlider.onValueChanged = originalEvent;
    }

    private void UpdateUIElements(float value)
    {
        // 更新百分比文本
        if (volumeText != null)
        {
            int percentage = Mathf.RoundToInt(value * 100);
            volumeText.text = string.Format(valueFormat, percentage);
            
            // 强制刷新UI，确保文本立即更新
            if (volumeText.gameObject.activeInHierarchy)
            {
                Canvas.ForceUpdateCanvases();
            }
        }

        // 更新滑动条填充图像
        UpdateFillRect(value);

        // 更新把手位置
        UpdateHandleRect(value);
    }

    private void UpdateFillRect(float value)
    {
        if (fillRect != null)
        {
            // 根据滑动条方向设置填充区域
            switch (targetSlider.direction)
            {
                case Slider.Direction.LeftToRight:
                    // 从左到右填充
                    fillRect.anchorMin = new Vector2(0, 0);
                    fillRect.anchorMax = new Vector2(value, 1);
                    fillRect.offsetMin = Vector2.zero;
                    fillRect.offsetMax = Vector2.zero;
                    break;
                    
                case Slider.Direction.RightToLeft:
                    // 从右到左填充
                    fillRect.anchorMin = new Vector2(1 - value, 0);
                    fillRect.anchorMax = new Vector2(1, 1);
                    fillRect.offsetMin = Vector2.zero;
                    fillRect.offsetMax = Vector2.zero;
                    break;
                    
                case Slider.Direction.BottomToTop:
                    // 从下到上填充
                    fillRect.anchorMin = new Vector2(0, 0);
                    fillRect.anchorMax = new Vector2(1, value);
                    fillRect.offsetMin = Vector2.zero;
                    fillRect.offsetMax = Vector2.zero;
                    break;
                    
                case Slider.Direction.TopToBottom:
                    // 从上到下填充
                    fillRect.anchorMin = new Vector2(0, 1 - value);
                    fillRect.anchorMax = new Vector2(1, 1);
                    fillRect.offsetMin = Vector2.zero;
                    fillRect.offsetMax = Vector2.zero;
                    break;
            }
        }
    }

    private void UpdateHandleRect(float value)
    {
        if (handleRect != null)
        {
            // 计算正确的锚点位置
            Vector2 anchorMin, anchorMax;
            
            switch (targetSlider.direction)
            {
                case Slider.Direction.LeftToRight:
                    anchorMin = new Vector2(value, 0.5f);
                    anchorMax = new Vector2(value, 0.5f);
                    break;
                    
                case Slider.Direction.RightToLeft:
                    anchorMin = new Vector2(1 - value, 0.5f);
                    anchorMax = new Vector2(1 - value, 0.5f);
                    break;
                    
                case Slider.Direction.BottomToTop:
                    anchorMin = new Vector2(0.5f, value);
                    anchorMax = new Vector2(0.5f, value);
                    break;
                    
                case Slider.Direction.TopToBottom:
                    anchorMin = new Vector2(0.5f, 1 - value);
                    anchorMax = new Vector2(0.5f, 1 - value);
                    break;
                    
                default:
                    anchorMin = new Vector2(value, 0.5f);
                    anchorMax = new Vector2(value, 0.5f);
                    break;
            }
            
            // 设置把手位置
            handleRect.anchorMin = anchorMin;
            handleRect.anchorMax = anchorMax;
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            handleRect.anchoredPosition = Vector2.zero;
        }
    }

    private void UpdateVolume(float volume)
    {
        // 更新音频源音量，确保立即生效
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = volume;
        }
        
        // 保存音量设置，确保保存最新的值
        lastVolume = volume;
        PlayerPrefs.SetFloat("GameVolume", volume);
        PlayerPrefs.Save();
    }
    
    // 公共方法，允许外部重置并更新UI
    public void ResetAndUpdate()
    {
        ForceUpdateUI(targetSlider.value);
    }
    
    // 该方法允许从编辑器工具中设置引用
    public void SetReferences(object[] references)
    {
        if (references == null || references.Length < 4)
        {
            Debug.LogError("SetReferences: 引用数据不完整");
            return;
        }
        
        try
        {
            targetSlider = references[0] as Slider;
            fillRect = references[1] as RectTransform;
            handleRect = references[2] as RectTransform;
            volumeText = references[3] as TextMeshProUGUI;
            
            if (references.Length > 4)
            {
                musicSource = references[4] as AudioSource;
            }
            
            if (references.Length > 5)
            {
                sfxSource = references[5] as AudioSource;
            }
            
            // 强制更新一次UI
            if (targetSlider != null)
            {
                ForceUpdateUI(targetSlider.value);
            }
            
            Debug.Log("滑动条引用设置成功");
        }
        catch (System.Exception e)
        {
            Debug.LogError("设置引用时出错: " + e.Message);
        }
    }
} 