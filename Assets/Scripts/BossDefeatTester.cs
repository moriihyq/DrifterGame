using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Boss击败测试器 - 用于测试结算画面功能
/// </summary>
public class BossDefeatTester : MonoBehaviour
{
    [Header("测试设置")]
    [Tooltip("测试按键")]
    public KeyCode testKey = KeyCode.V;
    
    [Tooltip("是否启用测试UI")]
    public bool enableTestUI = true;
    
    [Tooltip("是否显示调试信息")]
    public bool showDebugInfo = true;
    
    [Header("组件引用")]
    [SerializeField] private BossController[] bosses;
    [SerializeField] private GameVictoryManager victoryManager;
    
    // UI组件
    private GameObject testUI;
    private Text statusText;
    private Button defeatBossButton;
    private Button testVictoryButton;
    
    private void Start()
    {
        InitializeComponents();
        if (enableTestUI)
        {
            CreateTestUI();
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            TestBossDefeat();
        }
        
        if (enableTestUI)
        {
            UpdateStatusText();
        }
    }
    
    /// <summary>
    /// 初始化组件引用
    /// </summary>
    private void InitializeComponents()
    {
        // 查找Boss控制器
        if (bosses == null || bosses.Length == 0)
        {
            bosses = FindObjectsOfType<BossController>();
        }
        
        // 查找胜利管理器
        if (victoryManager == null)
        {
            victoryManager = FindFirstObjectByType<GameVictoryManager>();
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"<color=cyan>[BossDefeatTester] 测试器已初始化</color>");
            Debug.Log($"<color=cyan>- 找到Boss数量: {bosses.Length}</color>");
            Debug.Log($"<color=cyan>- 胜利管理器: {(victoryManager != null ? "✓" : "✗")}</color>");
            Debug.Log($"<color=cyan>- 测试按键: {testKey}</color>");
        }
    }
    
    /// <summary>
    /// 测试Boss击败
    /// </summary>
    [ContextMenu("测试Boss击败")]
    public void TestBossDefeat()
    {
        if (bosses == null || bosses.Length == 0)
        {
            Debug.LogWarning("[BossDefeatTester] 没有找到Boss进行测试！");
            return;
        }
        
        // 找到第一个活着的Boss
        BossController aliveBoss = null;
        foreach (BossController boss in bosses)
        {
            if (boss != null && !boss.IsDead)
            {
                aliveBoss = boss;
                break;
            }
        }
        
        if (aliveBoss != null)
        {
            // 直接调用Boss的TakeDamage方法来"击败"它
            if (showDebugInfo)
                Debug.Log($"<color=yellow>[BossDefeatTester] 正在测试击败Boss: {aliveBoss.name}</color>");
            
            // 造成足够的伤害来击败Boss
            int maxDamage = 9999; // 足够大的伤害值
            aliveBoss.TakeDamage(maxDamage);
        }
        else
        {
            if (showDebugInfo)
                Debug.Log("<color=orange>[BossDefeatTester] 所有Boss都已被击败！</color>");
        }
    }
    
    /// <summary>
    /// 测试胜利画面
    /// </summary>
    [ContextMenu("测试胜利画面")]
    public void TestVictoryScreen()
    {
        if (victoryManager != null)
        {
            victoryManager.TestVictory();
            if (showDebugInfo)
                Debug.Log("<color=green>[BossDefeatTester] 已触发测试胜利画面</color>");
        }
        else
        {
            Debug.LogWarning("[BossDefeatTester] 找不到GameVictoryManager！");
        }
    }
    
    /// <summary>
    /// 重置所有Boss状态（仅用于测试）
    /// </summary>
    [ContextMenu("重置Boss状态")]
    public void ResetBossStatus()
    {
        foreach (BossController boss in bosses)
        {
            if (boss != null)
            {
                // 重新启用Boss游戏对象
                boss.gameObject.SetActive(true);
                
                // 重新启用碰撞器
                Collider2D collider = boss.GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = true;
                }
                
                // 重新启用Rigidbody2D
                Rigidbody2D rb = boss.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                }
                
                if (showDebugInfo)
                    Debug.Log($"<color=cyan>[BossDefeatTester] 已重置Boss: {boss.name}</color>");
            }
        }
        
        // 重置胜利管理器状态
        if (victoryManager != null)
        {
            victoryManager.HideVictoryScreen();
        }
    }
    
    /// <summary>
    /// 创建测试UI
    /// </summary>
    private void CreateTestUI()
    {
        // 创建Canvas
        GameObject canvasObj = new GameObject("BossDefeatTestUI");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // 创建主面板
        testUI = new GameObject("TestPanel");
        testUI.transform.SetParent(canvasObj.transform, false);
        
        RectTransform panelRect = testUI.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 1);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.pivot = new Vector2(1, 1);
        panelRect.anchoredPosition = new Vector2(-10, -10);
        panelRect.sizeDelta = new Vector2(300, 200);
        
        Image panelImage = testUI.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        
        // 创建状态文本
        GameObject statusTextObj = new GameObject("StatusText");
        statusTextObj.transform.SetParent(testUI.transform, false);
        
        RectTransform statusTextRect = statusTextObj.AddComponent<RectTransform>();
        statusTextRect.anchorMin = new Vector2(0, 1);
        statusTextRect.anchorMax = new Vector2(1, 1);
        statusTextRect.pivot = new Vector2(0.5f, 1);
        statusTextRect.anchoredPosition = new Vector2(0, -10);
        statusTextRect.sizeDelta = new Vector2(-20, 80);
        
        statusText = statusTextObj.AddComponent<Text>();
        statusText.font = GetUIFont();
        statusText.fontSize = 12;
        statusText.color = Color.white;
        statusText.alignment = TextAnchor.UpperLeft;
        
        // 创建击败Boss按钮
        defeatBossButton = CreateTestButton(testUI, "击败Boss (V键)", 
            new Vector2(0, -100), TestBossDefeat);
        
        // 创建测试胜利按钮
        testVictoryButton = CreateTestButton(testUI, "测试胜利画面", 
            new Vector2(0, -140), TestVictoryScreen);
        
        // 创建重置按钮
        CreateTestButton(testUI, "重置Boss状态", 
            new Vector2(0, -180), ResetBossStatus);
    }
    
    /// <summary>
    /// 创建测试按钮
    /// </summary>
    private Button CreateTestButton(GameObject parent, string text, Vector2 position, System.Action onClick)
    {
        GameObject buttonObj = new GameObject($"Button_{text}");
        buttonObj.transform.SetParent(parent.transform, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 1f);
        buttonRect.anchorMax = new Vector2(0.5f, 1f);
        buttonRect.pivot = new Vector2(0.5f, 1f);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(180, 30);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 1f, 0.8f);
        
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(() => onClick?.Invoke());
        
        // 按钮文字
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.font = GetUIFont();
        buttonText.fontSize = 11;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.text = text;
        
        return button;
    }
    
    /// <summary>
    /// 更新状态文本
    /// </summary>
    private void UpdateStatusText()
    {
        if (statusText == null) return;
        
        int aliveBossCount = 0;
        int deadBossCount = 0;
        
        foreach (BossController boss in bosses)
        {
            if (boss != null)
            {
                if (boss.IsDead)
                    deadBossCount++;
                else
                    aliveBossCount++;
            }
        }
        
        string status = $"Boss击败测试工具\n" +
                       $"==================\n" +
                       $"活着的Boss: {aliveBossCount}\n" +
                       $"死亡的Boss: {deadBossCount}\n" +
                       $"胜利管理器: {(victoryManager != null ? "✓" : "✗")}\n" +
                       $"测试按键: {testKey}";
        
        statusText.text = status;
    }
    
    /// <summary>
    /// 设置测试UI的显示状态
    /// </summary>
    public void SetTestUIVisible(bool visible)
    {
        if (testUI != null)
        {
            testUI.SetActive(visible);
        }
    }
    
    /// <summary>
    /// 获取Boss状态信息
    /// </summary>
    public string GetBossStatusInfo()
    {
        if (bosses == null || bosses.Length == 0)
        {
            return "没有找到Boss";
        }
        
        int alive = 0, dead = 0;
        foreach (BossController boss in bosses)
        {
            if (boss != null)
            {
                if (boss.IsDead) dead++;
                else alive++;
            }
        }
        
        return $"活着: {alive}, 死亡: {dead}, 总计: {bosses.Length}";
    }
    
    /// <summary>
    /// 获取UI字体
    /// </summary>
    private Font GetUIFont()
    {
        // 尝试加载CyberpunkCraftpixPixel字体
        Font cyberpunkFont = Resources.Load<Font>("UI_set/10 Font/CyberpunkCraftpixPixel");
        if (cyberpunkFont != null)
        {
            return cyberpunkFont;
        }
        
        // 备用方案：使用LegacyRuntime.ttf
        Font legacyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (legacyFont != null)
        {
            return legacyFont;
        }
        
        // 如果都找不到，返回null（Unity会使用默认字体）
        Debug.LogWarning("[BossDefeatTester] 无法找到可用字体，将使用Unity默认字体");
        return null;
    }
} 