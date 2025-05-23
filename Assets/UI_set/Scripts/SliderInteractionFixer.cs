using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(Slider))]
public class SliderInteractionFixer : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private Slider slider;
    private RectTransform sliderRect;
    private bool isDragging = false;
    
    [Header("调试选项")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private bool blockChildInteraction = true;
    
    private void Awake()
    {
        slider = GetComponent<Slider>();
        sliderRect = GetComponent<RectTransform>();
        
        // 确保滑动条可交互
        slider.interactable = true;
        
        // 禁用子对象的射线检测，防止它们阻挡滑动条的交互
        if (blockChildInteraction)
        {
            DisableChildRaycastTargets();
        }
        
        // 确保滑动条本身可以接收射线检测
        var sliderImage = GetComponent<Image>();
        if (sliderImage == null)
        {
            sliderImage = gameObject.AddComponent<Image>();
            sliderImage.color = new Color(0, 0, 0, 0); // 透明背景
        }
        sliderImage.raycastTarget = true;
    }
    
    private void DisableChildRaycastTargets()
    {
        // 获取所有子对象的图形组件
        Graphic[] childGraphics = GetComponentsInChildren<Graphic>(true);
        
        foreach (var graphic in childGraphics)
        {
            // 跳过滑动条本身的图形组件
            if (graphic.gameObject == gameObject)
                continue;
                
            // 如果是把手（Handle），保持可交互
            if (graphic.name.ToLower().Contains("handle"))
            {
                graphic.raycastTarget = true;
                continue;
            }
            
            // 其他子对象禁用射线检测
            graphic.raycastTarget = false;
            
            if (debugMode)
            {
                Debug.Log($"禁用了 {graphic.name} 的射线检测");
            }
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!slider.interactable)
            return;
            
        isDragging = true;
        UpdateSliderValue(eventData);
        
        if (debugMode)
        {
            Debug.Log("开始拖动滑动条");
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || !slider.interactable)
            return;
            
        UpdateSliderValue(eventData);
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        
        if (debugMode)
        {
            Debug.Log("停止拖动滑动条");
        }
    }
    
    private void UpdateSliderValue(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            sliderRect, 
            eventData.position, 
            eventData.pressEventCamera, 
            out localPoint))
        {
            return;
        }
        
        float normalizedValue = 0f;
        
        switch (slider.direction)
        {
            case Slider.Direction.LeftToRight:
                normalizedValue = Mathf.InverseLerp(sliderRect.rect.xMin, sliderRect.rect.xMax, localPoint.x);
                break;
                
            case Slider.Direction.RightToLeft:
                normalizedValue = 1f - Mathf.InverseLerp(sliderRect.rect.xMin, sliderRect.rect.xMax, localPoint.x);
                break;
                
            case Slider.Direction.BottomToTop:
                normalizedValue = Mathf.InverseLerp(sliderRect.rect.yMin, sliderRect.rect.yMax, localPoint.y);
                break;
                
            case Slider.Direction.TopToBottom:
                normalizedValue = 1f - Mathf.InverseLerp(sliderRect.rect.yMin, sliderRect.rect.yMax, localPoint.y);
                break;
        }
        
        // 应用滑动条的最小值和最大值
        float actualValue = Mathf.Lerp(slider.minValue, slider.maxValue, normalizedValue);
        slider.value = actualValue;
        
        if (debugMode)
        {
            Debug.Log($"滑动条值更新: {slider.value} (归一化: {normalizedValue})");
        }
    }
    
    // 在编辑器中重新启用子对象的射线检测
    private void OnValidate()
    {
        if (!Application.isPlaying && blockChildInteraction)
        {
            // 在编辑器中提示
            Debug.Log("SliderInteractionFixer: 运行时将自动禁用子对象的射线检测以防止交互问题");
        }
    }
    
    // 公共方法：手动刷新射线检测设置
    public void RefreshRaycastSettings()
    {
        if (blockChildInteraction)
        {
            DisableChildRaycastTargets();
        }
    }
} 