using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;

public class SliderDebugTool : EditorWindow
{
    private GameObject selectedSlider;
    private Vector2 scrollPos;
    private bool showRaycastTargets = true;
    private bool showHierarchy = true;
    private bool autoFix = false;
    
    [MenuItem("Tools/UI调试/滑动条交互诊断工具")]
    public static void ShowWindow()
    {
        GetWindow<SliderDebugTool>("滑动条诊断");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("滑动条交互问题诊断工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // 选择滑动条
        EditorGUI.BeginChangeCheck();
        selectedSlider = EditorGUILayout.ObjectField("选择滑动条:", selectedSlider, typeof(GameObject), true) as GameObject;
        if (EditorGUI.EndChangeCheck())
        {
            if (selectedSlider != null && selectedSlider.GetComponent<Slider>() == null)
            {
                Debug.LogWarning("选择的对象不包含Slider组件！");
                selectedSlider = null;
            }
        }
        
        if (selectedSlider == null)
        {
            EditorGUILayout.HelpBox("请选择一个包含Slider组件的GameObject", MessageType.Info);
            return;
        }
        
        EditorGUILayout.Space();
        
        // 显示选项
        showRaycastTargets = EditorGUILayout.Toggle("显示Raycast Target状态", showRaycastTargets);
        showHierarchy = EditorGUILayout.Toggle("显示层级结构", showHierarchy);
        autoFix = EditorGUILayout.Toggle("自动修复", autoFix);
        
        EditorGUILayout.Space();
        
        // 诊断按钮
        if (GUILayout.Button("开始诊断", GUILayout.Height(30)))
        {
            DiagnoseSlider();
        }
        
        EditorGUILayout.Space();
        
        // 快速修复按钮
        EditorGUILayout.LabelField("快速修复选项:", EditorStyles.boldLabel);
        
        if (GUILayout.Button("应用SliderInteractionFixer脚本"))
        {
            ApplySliderInteractionFixer();
        }
        
        if (GUILayout.Button("修复Raycast Target设置"))
        {
            FixRaycastTargets();
        }
        
        if (GUILayout.Button("清理遮挡元素"))
        {
            CleanBlockingElements();
        }
        
        // 显示诊断结果
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        if (showHierarchy && selectedSlider != null)
        {
            EditorGUILayout.LabelField("层级结构:", EditorStyles.boldLabel);
            ShowHierarchy(selectedSlider.transform, 0);
        }
        EditorGUILayout.EndScrollView();
    }
    
    private void DiagnoseSlider()
    {
        if (selectedSlider == null) return;
        
        Debug.Log("=== 滑动条诊断开始 ===");
        
        Slider slider = selectedSlider.GetComponent<Slider>();
        
        // 检查基本设置
        Debug.Log($"滑动条可交互: {slider.interactable}");
        Debug.Log($"滑动条方向: {slider.direction}");
        Debug.Log($"滑动条值范围: {slider.minValue} - {slider.maxValue}");
        Debug.Log($"当前值: {slider.value}");
        
        // 检查Canvas设置
        Canvas canvas = selectedSlider.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"Canvas渲染模式: {canvas.renderMode}");
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                Debug.Log($"Canvas相机: {(canvas.worldCamera != null ? canvas.worldCamera.name : "未设置")}");
            }
        }
        
        // 检查GraphicRaycaster
        GraphicRaycaster raycaster = selectedSlider.GetComponentInParent<GraphicRaycaster>();
        if (raycaster == null)
        {
            Debug.LogError("未找到GraphicRaycaster组件！这会导致UI无法接收输入。");
        }
        
        // 检查EventSystem
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            Debug.LogError("场景中没有EventSystem！UI输入将无法工作。");
        }
        
        // 检查所有子对象的Raycast Target
        List<GameObject> blockingObjects = new List<GameObject>();
        Graphic[] graphics = selectedSlider.GetComponentsInChildren<Graphic>(true);
        
        foreach (var graphic in graphics)
        {
            if (graphic.raycastTarget && graphic.gameObject != selectedSlider)
            {
                // 检查是否是必要的组件
                if (!graphic.name.ToLower().Contains("handle"))
                {
                    blockingObjects.Add(graphic.gameObject);
                    Debug.LogWarning($"潜在遮挡对象: {GetPath(graphic.transform)} (Raycast Target = true)");
                }
            }
        }
        
        if (blockingObjects.Count > 0)
        {
            Debug.LogWarning($"发现 {blockingObjects.Count} 个可能遮挡滑动条交互的对象");
            
            if (autoFix)
            {
                foreach (var obj in blockingObjects)
                {
                    Graphic g = obj.GetComponent<Graphic>();
                    if (g != null)
                    {
                        g.raycastTarget = false;
                        Debug.Log($"已禁用 {obj.name} 的Raycast Target");
                    }
                }
                EditorUtility.SetDirty(selectedSlider);
            }
        }
        else
        {
            Debug.Log("未发现明显的遮挡问题");
        }
        
        // 检查滑动条本身的Image组件
        Image sliderImage = selectedSlider.GetComponent<Image>();
        if (sliderImage == null)
        {
            Debug.LogWarning("滑动条GameObject没有Image组件，这可能导致无法接收点击");
            if (autoFix)
            {
                sliderImage = selectedSlider.AddComponent<Image>();
                sliderImage.color = new Color(0, 0, 0, 0.01f); // 几乎透明
                sliderImage.raycastTarget = true;
                Debug.Log("已添加透明Image组件");
                EditorUtility.SetDirty(selectedSlider);
            }
        }
        else if (!sliderImage.raycastTarget)
        {
            Debug.LogWarning("滑动条的Image组件Raycast Target为false");
            if (autoFix)
            {
                sliderImage.raycastTarget = true;
                Debug.Log("已启用滑动条的Raycast Target");
                EditorUtility.SetDirty(selectedSlider);
            }
        }
        
        Debug.Log("=== 诊断完成 ===");
    }
    
    private void ApplySliderInteractionFixer()
    {
        if (selectedSlider == null) return;
        
        SliderInteractionFixer fixer = selectedSlider.GetComponent<SliderInteractionFixer>();
        if (fixer == null)
        {
            fixer = selectedSlider.AddComponent<SliderInteractionFixer>();
            Debug.Log("已添加SliderInteractionFixer脚本");
            EditorUtility.SetDirty(selectedSlider);
        }
        else
        {
            Debug.Log("SliderInteractionFixer脚本已存在");
            fixer.RefreshRaycastSettings();
        }
    }
    
    private void FixRaycastTargets()
    {
        if (selectedSlider == null) return;
        
        // 确保滑动条本身可以接收射线
        Image sliderImage = selectedSlider.GetComponent<Image>();
        if (sliderImage == null)
        {
            sliderImage = selectedSlider.AddComponent<Image>();
            sliderImage.color = new Color(0, 0, 0, 0.01f);
        }
        sliderImage.raycastTarget = true;
        
        // 禁用非必要子对象的射线检测
        Graphic[] graphics = selectedSlider.GetComponentsInChildren<Graphic>(true);
        int fixedCount = 0;
        
        foreach (var graphic in graphics)
        {
            if (graphic.gameObject == selectedSlider) continue;
            
            if (!graphic.name.ToLower().Contains("handle") && graphic.raycastTarget)
            {
                graphic.raycastTarget = false;
                fixedCount++;
            }
        }
        
        Debug.Log($"修复了 {fixedCount} 个Raycast Target设置");
        EditorUtility.SetDirty(selectedSlider);
    }
    
    private void CleanBlockingElements()
    {
        if (selectedSlider == null) return;
        
        RectTransform sliderRect = selectedSlider.GetComponent<RectTransform>();
        Canvas canvas = selectedSlider.GetComponentInParent<Canvas>();
        
        if (canvas == null) return;
        
        // 查找所有可能遮挡的UI元素
        Graphic[] allGraphics = canvas.GetComponentsInChildren<Graphic>(true);
        List<GameObject> blockingElements = new List<GameObject>();
        
        foreach (var graphic in allGraphics)
        {
            if (graphic.gameObject == selectedSlider || 
                graphic.transform.IsChildOf(selectedSlider.transform))
                continue;
                
            if (graphic.raycastTarget && IsOverlapping(sliderRect, graphic.rectTransform))
            {
                blockingElements.Add(graphic.gameObject);
            }
        }
        
        if (blockingElements.Count > 0)
        {
            Debug.Log($"发现 {blockingElements.Count} 个可能遮挡滑动条的元素:");
            foreach (var element in blockingElements)
            {
                Debug.Log($"- {GetPath(element.transform)}");
            }
        }
        else
        {
            Debug.Log("未发现遮挡元素");
        }
    }
    
    private bool IsOverlapping(RectTransform rect1, RectTransform rect2)
    {
        if (rect1 == null || rect2 == null) return false;
        
        Vector3[] corners1 = new Vector3[4];
        Vector3[] corners2 = new Vector3[4];
        
        rect1.GetWorldCorners(corners1);
        rect2.GetWorldCorners(corners2);
        
        Rect r1 = new Rect(corners1[0].x, corners1[0].y, 
            corners1[2].x - corners1[0].x, corners1[2].y - corners1[0].y);
        Rect r2 = new Rect(corners2[0].x, corners2[0].y, 
            corners2[2].x - corners2[0].x, corners2[2].y - corners2[0].y);
        
        return r1.Overlaps(r2);
    }
    
    private void ShowHierarchy(Transform transform, int indent)
    {
        EditorGUI.indentLevel = indent;
        
        Graphic graphic = transform.GetComponent<Graphic>();
        string info = transform.name;
        
        if (graphic != null && showRaycastTargets)
        {
            info += $" [Raycast: {(graphic.raycastTarget ? "是" : "否")}]";
        }
        
        if (transform.GetComponent<Slider>() != null)
        {
            info += " [Slider]";
        }
        
        EditorGUILayout.LabelField(info);
        
        foreach (Transform child in transform)
        {
            ShowHierarchy(child, indent + 1);
        }
        
        EditorGUI.indentLevel = 0;
    }
    
    private string GetPath(Transform transform)
    {
        string path = transform.name;
        Transform parent = transform.parent;
        
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
    }
} 