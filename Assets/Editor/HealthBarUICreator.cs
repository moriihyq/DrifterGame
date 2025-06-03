using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class HealthBarUICreator : EditorWindow
{
    [MenuItem("Tools/创建血量UI")]
    public static void ShowWindow()
    {
        GetWindow<HealthBarUICreator>("血量UI创建器");
    }
    
    private enum UIType
    {
        Basic,      // 基础版本
        Advanced    // 高级版本
    }
    
    private UIType uiType = UIType.Basic;
    private bool includeText = true;
    private bool includeBackground = false;
    private Vector2 position = new Vector2(150, -50);
    private Vector2 size = new Vector2(200, 20);
    
    private void OnGUI()
    {
        GUILayout.Label("血量UI创建器", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        // UI类型选择
        GUILayout.Label("UI类型:");
        uiType = (UIType)EditorGUILayout.EnumPopup("类型", uiType);
        
        GUILayout.Space(10);
        
        // 选项设置
        GUILayout.Label("设置选项:");
        includeText = EditorGUILayout.Toggle("包含血量文本", includeText);
        includeBackground = EditorGUILayout.Toggle("包含背景效果", includeBackground);
        
        GUILayout.Space(10);
        
        // 位置和大小设置
        GUILayout.Label("位置和大小:");
        position = EditorGUILayout.Vector2Field("位置", position);
        size = EditorGUILayout.Vector2Field("大小", size);
        
        GUILayout.Space(20);
        
        // 创建按钮
        if (GUILayout.Button("创建血量UI", GUILayout.Height(30)))
        {
            CreateHealthBarUI();
        }
        
        GUILayout.Space(10);
        
        // 帮助信息
        EditorGUILayout.HelpBox(
            "这个工具会在当前场景中创建一个完整的血量UI系统。\n" +
            "如果场景中没有Canvas，会自动创建一个。\n" +
            "创建的UI会自动连接到PlayerAttackSystem。",
            MessageType.Info);
    }
    
    private void CreateHealthBarUI()
    {
        // 查找或创建Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            Debug.Log("已创建新的Canvas");
        }
        
        // 创建主血量条
        GameObject healthBarObj = CreateSlider(canvas.transform, "HealthBar");
        
        // 创建背景血量条（如果需要）
        GameObject backgroundObj = null;
        if (includeBackground && uiType == UIType.Advanced)
        {
            backgroundObj = CreateSlider(canvas.transform, "BackgroundHealthBar");
            // 设置背景稍微偏移，在主血量条后面
            RectTransform bgRect = backgroundObj.GetComponent<RectTransform>();
            bgRect.anchoredPosition = position;
            bgRect.sizeDelta = size;
            
            // 设置背景颜色
            Slider bgSlider = backgroundObj.GetComponent<Slider>();
            Image bgFill = bgSlider.fillRect.GetComponent<Image>();
            bgFill.color = new Color(1f, 1f, 1f, 0.3f);
        }
        
        // 设置主血量条位置和大小
        RectTransform healthRect = healthBarObj.GetComponent<RectTransform>();
        healthRect.anchoredPosition = position;
        healthRect.sizeDelta = size;
        
        // 添加血量文本
        if (includeText)
        {
            GameObject textObj = new GameObject("HealthText");
            textObj.transform.SetParent(healthBarObj.transform);
            
            Text text = textObj.AddComponent<Text>();
            text.text = "100 / 100";
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
        }
        
        // 添加相应的脚本组件
        if (uiType == UIType.Basic)
        {
            SliderHealthBarUI healthUI = healthBarObj.AddComponent<SliderHealthBarUI>();
            
            // 自动分配组件引用
            Slider slider = healthBarObj.GetComponent<Slider>();
            Text text = includeText ? healthBarObj.GetComponentInChildren<Text>() : null;
            Image fillImage = slider.fillRect.GetComponent<Image>();
            
            // 使用反射设置私有字段（仅用于编辑器工具）
            var healthSliderField = typeof(SliderHealthBarUI).GetField("healthSlider", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            healthSliderField?.SetValue(healthUI, slider);
            
            var healthTextField = typeof(SliderHealthBarUI).GetField("healthText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            healthTextField?.SetValue(healthUI, text);
            
            var fillImageField = typeof(SliderHealthBarUI).GetField("fillImage", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fillImageField?.SetValue(healthUI, fillImage);
            
            Debug.Log("已创建基础版本血量UI");
        }
        else
        {
            SliderHealthBarUIManager healthManager = healthBarObj.AddComponent<SliderHealthBarUIManager>();
            
            // 添加CanvasGroup组件用于淡入淡出效果
            CanvasGroup canvasGroup = healthBarObj.AddComponent<CanvasGroup>();
            
            // 自动分配组件引用
            Slider slider = healthBarObj.GetComponent<Slider>();
            Slider backgroundSlider = backgroundObj?.GetComponent<Slider>();
            Text text = includeText ? healthBarObj.GetComponentInChildren<Text>() : null;
            Image fillImage = slider.fillRect.GetComponent<Image>();
            Image backgroundImage = backgroundSlider?.fillRect.GetComponent<Image>();
            
            // 使用反射设置私有字段
            var fields = typeof(SliderHealthBarUIManager).GetFields(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            foreach (var field in fields)
            {
                switch (field.Name)
                {
                    case "healthSlider":
                        field.SetValue(healthManager, slider);
                        break;
                    case "backgroundSlider":
                        field.SetValue(healthManager, backgroundSlider);
                        break;
                    case "healthText":
                        field.SetValue(healthManager, text);
                        break;
                    case "fillImage":
                        field.SetValue(healthManager, fillImage);
                        break;
                    case "backgroundImage":
                        field.SetValue(healthManager, backgroundImage);
                        break;
                    case "canvasGroup":
                        field.SetValue(healthManager, canvasGroup);
                        break;
                }
            }
            
            Debug.Log("已创建高级版本血量UI管理器");
        }
        
        // 自定义Slider外观
        CustomizeSliderAppearance(healthBarObj);
        if (backgroundObj != null)
        {
            CustomizeSliderAppearance(backgroundObj);
        }
        
        // 选中创建的对象
        Selection.activeGameObject = healthBarObj;
        
        Debug.Log($"血量UI创建完成！位置: {position}, 大小: {size}");
    }
    
    private GameObject CreateSlider(Transform parent, string name)
    {
        // 创建Slider对象
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(parent);
        
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.interactable = false; // 玩家不应该手动拖拽
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        
        // 设置RectTransform
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 1); // 左上角
        sliderRect.anchorMax = new Vector2(0, 1); // 左上角
        sliderRect.pivot = new Vector2(0, 1);     // 左上角为锚点
        
        // 创建Background
        GameObject backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(sliderObj.transform);
        
        Image backgroundImg = backgroundObj.AddComponent<Image>();
        backgroundImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // 深灰色背景
        
        RectTransform backgroundRect = backgroundObj.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.sizeDelta = Vector2.zero;
        backgroundRect.anchoredPosition = Vector2.zero;
        
        // 创建Fill Area
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform);
        
        RectTransform fillAreaRect = fillAreaObj.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;
        fillAreaRect.anchoredPosition = Vector2.zero;
        
        // 创建Fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform);
        
        Image fillImg = fillObj.AddComponent<Image>();
        fillImg.color = Color.green; // 默认绿色
        
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;
        
        // 设置Slider的引用
        slider.fillRect = fillRect;
        
        return sliderObj;
    }
    
    private void CustomizeSliderAppearance(GameObject sliderObj)
    {
        Slider slider = sliderObj.GetComponent<Slider>();
        
        // 设置背景样式
        Transform background = sliderObj.transform.Find("Background");
        if (background != null)
        {
            Image bgImage = background.GetComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        }
        
        // 设置填充样式
        if (slider.fillRect != null)
        {
            Image fillImage = slider.fillRect.GetComponent<Image>();
            fillImage.color = Color.green;
        }
    }
} 