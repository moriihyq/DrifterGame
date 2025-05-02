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

    [Header("音频设置")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    private bool isDragging = false;
    private float lastVolume = 0.8f;

    private void Start()
    {
        // 初始化检查
        if (targetSlider == null)
        {
            targetSlider = GetComponent<Slider>();
            if (targetSlider == null)
            {
                Debug.LogError("没有找到Slider组件！");
                return;
            }
        }

        // 找到子对象组件（如果没有手动分配）
        if (fillRect == null)
        {
            Transform fillArea = targetSlider.transform.Find("Fill Area/Fill");
            if (fillArea != null)
                fillRect = fillArea.GetComponent<RectTransform>();
        }

        if (handleRect == null)
        {
            Transform handleArea = targetSlider.transform.Find("Handle Slide Area/Handle");
            if (handleArea != null)
                handleRect = handleArea.GetComponent<RectTransform>();
        }

        // 确保滑动条可交互
        targetSlider.interactable = true;

        // 设置滑动条值变化事件
        targetSlider.onValueChanged.RemoveAllListeners();
        targetSlider.onValueChanged.AddListener(OnSliderValueChanged);

        // 初始化滑动条位置和音量
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

        // 初次更新UI和音量
        UpdateUI(targetSlider.value);
        UpdateVolume(targetSlider.value);
    }

    private void Update()
    {
        // 检测鼠标点击和拖动
        if (Input.GetMouseButtonDown(0))
        {
            // 检查是否点击了滑动条区域
            if (RectTransformUtility.RectangleContainsScreenPoint(targetSlider.GetComponent<RectTransform>(), Input.mousePosition))
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
        // 获取鼠标在滑动条上的位置
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetSlider.GetComponent<RectTransform>(),
            Input.mousePosition,
            null,
            out localPoint
        );

        // 计算滑动条宽度
        float width = targetSlider.GetComponent<RectTransform>().rect.width;
        
        // 计算相对位置 (0-1)
        float position = Mathf.Clamp01((localPoint.x + width / 2) / width);
        
        // 如果滑动条方向是从右到左，反转位置
        if (targetSlider.direction == Slider.Direction.RightToLeft)
            position = 1 - position;

        // 设置滑动条值
        targetSlider.value = position;
    }

    private void OnSliderValueChanged(float value)
    {
        UpdateUI(value);
        UpdateVolume(value);
        
        // 保存音量设置
        lastVolume = value;
        PlayerPrefs.SetFloat("GameVolume", value);
        PlayerPrefs.Save();
    }

    private void UpdateUI(float value)
    {
        // 更新百分比文本
        if (volumeText != null)
        {
            int percentage = Mathf.RoundToInt(value * 100);
            volumeText.text = string.Format(valueFormat, percentage);
        }

        // 手动更新填充区域和手柄位置（如果有需要）
        if (fillRect != null)
        {
            fillRect.anchorMax = new Vector2(value, fillRect.anchorMax.y);
        }

        if (handleRect != null)
        {
            Vector2 anchorMin = handleRect.anchorMin;
            anchorMin.x = value;
            handleRect.anchorMin = anchorMin;
            
            Vector2 anchorMax = handleRect.anchorMax;
            anchorMax.x = value;
            handleRect.anchorMax = anchorMax;
        }
    }

    private void UpdateVolume(float volume)
    {
        // 更新音频源音量
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = volume;
        }
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
            
            Debug.Log("滑动条引用设置成功");
        }
        catch (System.Exception e)
        {
            Debug.LogError("设置引用时出错: " + e.Message);
        }
    }
} 