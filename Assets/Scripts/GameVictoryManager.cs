using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 游戏胜利管理器 - 监控Boss死亡并显示结算画面
/// </summary>
public class GameVictoryManager : MonoBehaviour
{
    [Header("Boss监控设置")]
    [Tooltip("Boss控制器引用（可为空，会自动查找）")]
    public BossController targetBoss;
    
    [Tooltip("监控多个Boss")]
    public BossController[] allBosses;
    
    [Tooltip("是否需要击败所有Boss才算胜利")]
    public bool requireAllBossesDefeated = false;
    
    [Header("场景管理")]
    [Tooltip("主菜单场景名称")]
    public string mainMenuSceneName = "MainMenuScene";
    
    [Tooltip("当前游戏场景名称")]
    public string currentGameSceneName = "5.26地图";
    
    [Header("结算画面设置")]
    [Tooltip("结算UI预制件（如果为空会动态创建）")]
    public GameObject victoryUIPrefab;
    
    [Tooltip("胜利延迟时间（秒）")]
    public float victoryDelay = 2f;
    
    [Tooltip("是否自动保存游戏进度")]
    public bool autoSaveOnVictory = true;
    
    [Header("统计数据")]
    [Tooltip("是否显示游戏统计")]
    public bool showGameStats = true;
    
    [Tooltip("游戏开始时间戳")]
    private float gameStartTime;
    
    [Tooltip("玩家击杀数统计")]
    private int totalKills = 0;
    
    [Tooltip("玩家受伤次数")]
    private int totalDamageTaken = 0;
    
    [Header("UI字体设置")]
    [Tooltip("胜利界面使用的字体")]
    public Font uiFont;
    
    [Header("调试设置")]
    [Tooltip("启用调试信息")]
    public bool enableDebugInfo = true;
    
    // 单例
    private static GameVictoryManager instance;
    public static GameVictoryManager Instance => instance;
    
    // 内部变量
    private bool gameWon = false;
    private List<BossController> defeatedBosses = new List<BossController>();
    private PlayerAttackSystem playerAttackSystem;
    private GameObject victoryUI;
    
    // UI组件引用
    private Canvas victoryCanvas;
    private Text gameTimeText;
    private Text killCountText;
    private Text damageText;
    private Text congratulationText;
    private Button returnToMenuButton;
    
    private void Awake()
    {
        // 检查当前场景是否为游戏场景
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene == "MainMenuScene" || currentScene.Contains("Menu"))
        {
            // 在主菜单场景中不应该存在GameVictoryManager
            Debug.Log("[GameVictoryManager] 在主菜单场景中，销毁GameVictoryManager");
            Destroy(gameObject);
            return;
        }
        
        // 单例实现
        if (instance == null)
        {
            instance = this;
            // 只在游戏场景中保持对象
            // DontDestroyOnLoad(gameObject); // 注释掉，不需要跨场景
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // 记录游戏开始时间
        gameStartTime = Time.time;
        
        Debug.Log($"[GameVictoryManager] 在游戏场景 {currentScene} 中初始化");
    }
    
    private void Start()
    {
        // 再次检查场景，确保我们在正确的场景中
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene == "MainMenuScene" || currentScene.Contains("Menu"))
        {
            Debug.LogWarning("[GameVictoryManager] 检测到在菜单场景中，立即销毁");
            Destroy(gameObject);
            return;
        }
        
        Debug.Log($"[GameVictoryManager] 开始在游戏场景 {currentScene} 中初始化管理器");
        InitializeManager();
        StartCoroutine(InitializeWithDelay());
    }
    
    private void InitializeManager()
    {
        // 查找Boss控制器
        if (targetBoss == null && allBosses.Length == 0)
        {
            FindAllBosses();
        }
        
        // 查找玩家攻击系统
        FindPlayerAttackSystem();
        
        // 订阅事件
        SubscribeToEvents();
        
        if (enableDebugInfo)
        {
            Debug.Log($"<color=cyan>[GameVictoryManager] Game Victory Manager initialized</color>");
            Debug.Log($"<color=cyan>- Monitoring Boss count: {(targetBoss != null ? 1 : 0) + allBosses.Length}</color>");
            Debug.Log($"<color=cyan>- Require all bosses defeated: {requireAllBossesDefeated}</color>");
            Debug.Log($"<color=cyan>- Game start time: {gameStartTime}</color>");
        }
    }
    
    private IEnumerator InitializeWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
        
        // 重新检查Boss状态
        CheckBossStatus();
    }
    
    private void FindAllBosses()
    {
        BossController[] foundBosses = FindObjectsOfType<BossController>();
        
        if (foundBosses.Length > 0)
        {
            targetBoss = foundBosses[0]; // 设置主要Boss
            allBosses = foundBosses;
            
            if (enableDebugInfo)
                Debug.Log($"<color=yellow>[GameVictoryManager] Auto-found {foundBosses.Length} Boss(es)</color>");
        }
        else
        {
            Debug.LogWarning("[GameVictoryManager] No Boss controllers found in scene!");
        }
    }
    
    private void FindPlayerAttackSystem()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerAttackSystem = player.GetComponent<PlayerAttackSystem>();
            if (playerAttackSystem == null && enableDebugInfo)
            {
                Debug.LogWarning("[GameVictoryManager] Player doesn't have PlayerAttackSystem component!");
            }
        }
    }
    
    private void SubscribeToEvents()
    {
        // 订阅Boss死亡事件
        BossController.OnBossDefeated += OnBossDefeatedEvent;
        
        // 订阅血量管理器事件（不通过Instance访问以免触发创建）
        HealthManager.OnDamaged += OnPlayerDamaged;
    }
    
    private void OnDestroy()
    {
        // 取消订阅事件
        BossController.OnBossDefeated -= OnBossDefeatedEvent;
        
        // 取消订阅HealthManager事件（不访问Instance）
        HealthManager.OnDamaged -= OnPlayerDamaged;
    }
    
    private void Update()
    {
        if (!gameWon)
        {
            CheckBossStatus();
        }
    }
    
    /// <summary>
    /// Boss死亡事件处理
    /// </summary>
    private void OnBossDefeatedEvent(BossController boss)
    {
        OnBossDefeated(boss);
    }
    
    /// <summary>
    /// Boss被击败时调用（公共方法）
    /// </summary>
    public void OnBossDefeated(BossController boss)
    {
        if (boss == null || defeatedBosses.Contains(boss)) return;
        
        defeatedBosses.Add(boss);
        totalKills++; // 增加击杀数
        
        if (enableDebugInfo)
            Debug.Log($"<color=gold>[GameVictoryManager] Boss {boss.name} defeated!</color>");
        
        // 检查胜利条件
        CheckVictoryCondition();
    }
    
    /// <summary>
    /// 检查Boss状态
    /// </summary>
    private void CheckBossStatus()
    {
        if (gameWon) return;
        
        // 检查主要Boss
        if (targetBoss != null && targetBoss.IsDead)
        {
            OnBossDefeated(targetBoss);
        }
        
        // 检查所有Boss
        foreach (BossController boss in allBosses)
        {
            if (boss != null && boss.IsDead)
            {
                OnBossDefeated(boss);
            }
        }
    }
    
    /// <summary>
    /// 检查胜利条件
    /// </summary>
    private void CheckVictoryCondition()
    {
        if (gameWon) return;
        
        int totalBossCount = GetTotalBossCount();
        
        if (requireAllBossesDefeated)
        {
            if (defeatedBosses.Count >= totalBossCount && totalBossCount > 0)
            {
                TriggerVictory();
            }
        }
        else
        {
            if (defeatedBosses.Count > 0)
            {
                TriggerVictory();
            }
        }
    }
    
    /// <summary>
    /// 获取总Boss数量
    /// </summary>
    private int GetTotalBossCount()
    {
        HashSet<BossController> uniqueBosses = new HashSet<BossController>();
        
        if (targetBoss != null) uniqueBosses.Add(targetBoss);
        
        foreach (BossController boss in allBosses)
        {
            if (boss != null) uniqueBosses.Add(boss);
        }
        
        return uniqueBosses.Count;
    }
    
    /// <summary>
    /// 触发胜利
    /// </summary>
    private void TriggerVictory()
    {
        if (gameWon) return;
        
        gameWon = true;
        
        if (enableDebugInfo)
            Debug.Log($"<color=gold>🎉 VICTORY! 🎉</color>");
        
        // 自动保存
        if (autoSaveOnVictory)
        {
            AutoSaveGame();
        }
        
        // 延迟显示胜利界面
        StartCoroutine(ShowVictoryScreenDelayed());
    }
    
    /// <summary>
    /// 延迟显示胜利画面
    /// </summary>
    private IEnumerator ShowVictoryScreenDelayed()
    {
        yield return new WaitForSeconds(victoryDelay);
        ShowVictoryScreen();
    }
    
    /// <summary>
    /// 显示胜利画面
    /// </summary>
    private void ShowVictoryScreen()
    {
        // 暂停游戏
        Time.timeScale = 0f;
        
        // 创建或显示胜利UI
        if (victoryUIPrefab != null)
        {
            victoryUI = Instantiate(victoryUIPrefab);
        }
        else
        {
            CreateVictoryUI();
        }
        
        // 更新统计数据
        UpdateVictoryStats();
        
        if (enableDebugInfo)
            Debug.Log("<color=gold>[GameVictoryManager] Victory screen displayed</color>");
    }
    
    /// <summary>
    /// 创建胜利UI
    /// </summary>
    private void CreateVictoryUI()
    {
        // 创建Canvas
        GameObject canvasObj = new GameObject("VictoryCanvas");
        victoryCanvas = canvasObj.AddComponent<Canvas>();
        victoryCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        victoryCanvas.sortingOrder = 1000;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // 创建背景遮罩
        GameObject backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(canvasObj.transform, false);
        
        RectTransform backgroundRect = backgroundObj.AddComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        
        Image backgroundImage = backgroundObj.AddComponent<Image>();
        backgroundImage.color = new Color(0, 0, 0, 0.8f);
        
        // 创建主面板
        GameObject panelObj = new GameObject("VictoryPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(800, 500);
        
        // 主面板背景 - 深色半透明
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.05f, 0.15f, 0.95f);
        
        // 创建装饰边框
        CreateDecorativeBorder(panelObj);
        
        // 创建顶部装饰条
        CreateTopDecoration(panelObj);
        
        // 创建标题
        CreateVictoryTitle(panelObj);
        
        // 创建统计信息
        CreateStatsDisplay(panelObj);
        
        // 创建按钮
        CreateButtons(panelObj);
        
        victoryUI = canvasObj;
    }
    
    /// <summary>
    /// 创建胜利标题
    /// </summary>
    private void CreateVictoryTitle(GameObject parent)
    {
        // 主标题背景
        GameObject titleBgObj = new GameObject("TitleBackground");
        titleBgObj.transform.SetParent(parent.transform, false);
        
        RectTransform titleBgRect = titleBgObj.AddComponent<RectTransform>();
        titleBgRect.anchorMin = new Vector2(0.5f, 1f);
        titleBgRect.anchorMax = new Vector2(0.5f, 1f);
        titleBgRect.pivot = new Vector2(0.5f, 1f);
        titleBgRect.anchoredPosition = new Vector2(0, -20);
        titleBgRect.sizeDelta = new Vector2(700, 100);
        
        Image titleBgImage = titleBgObj.AddComponent<Image>();
        titleBgImage.color = new Color(0.8f, 0.6f, 0.1f, 0.3f); // 金色半透明背景
        
        // 主标题文字
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(titleBgObj.transform, false);
        
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = Vector2.zero;
        titleRect.anchorMax = Vector2.one;
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        congratulationText = titleObj.AddComponent<Text>();
        congratulationText.font = GetUIFont();
        congratulationText.fontSize = 42;
        congratulationText.color = new Color(1f, 0.9f, 0.2f); // 金黄色
        congratulationText.alignment = TextAnchor.MiddleCenter;
        congratulationText.text = "🎉 VICTORY! 🎉";
        congratulationText.fontStyle = FontStyle.Bold;
        
        // 添加阴影效果
        CreateTextShadow(titleObj, congratulationText);
    }
    
    /// <summary>
    /// 创建统计显示
    /// </summary>
    private void CreateStatsDisplay(GameObject parent)
    {
        if (!showGameStats) return;
        
        // 统计信息背景面板
        GameObject statsObj = new GameObject("Statistics");
        statsObj.transform.SetParent(parent.transform, false);
        
        RectTransform statsRect = statsObj.AddComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0.5f, 0.5f);
        statsRect.anchorMax = new Vector2(0.5f, 0.5f);
        statsRect.pivot = new Vector2(0.5f, 0.5f);
        statsRect.anchoredPosition = new Vector2(0, -50);
        statsRect.sizeDelta = new Vector2(650, 200);
        
        // 添加统计面板背景
        Image statsImage = statsObj.AddComponent<Image>();
        statsImage.color = new Color(0.1f, 0.15f, 0.3f, 0.6f);
        
        // 统计标题
        CreateStatsTitle(statsObj);
        
        // 统计信息容器
        GameObject statsContainer = new GameObject("StatsContainer");
        statsContainer.transform.SetParent(statsObj.transform, false);
        
        RectTransform containerRect = statsContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0f, 0f);
        containerRect.anchorMax = new Vector2(1f, 0.7f);
        containerRect.offsetMin = new Vector2(40, 20);
        containerRect.offsetMax = new Vector2(-40, -10);
        
        // 添加垂直布局组件
        VerticalLayoutGroup layoutGroup = statsContainer.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 15;
        layoutGroup.childAlignment = TextAnchor.MiddleLeft;
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;
        
        // 创建美化的统计条目
        gameTimeText = CreateStatEntry(statsContainer, "⏱", "Game Time", "--:--");
        killCountText = CreateStatEntry(statsContainer, "👹", "Bosses Defeated", "0");
        damageText = CreateStatEntry(statsContainer, "💔", "Times Damaged", "0");
    }
    
    /// <summary>
    /// 创建统计文本
    /// </summary>
    private Text CreateStatText(GameObject parent, string initialText, float yPos)
    {
        GameObject textObj = new GameObject("StatText");
        textObj.transform.SetParent(parent.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 1f);
        textRect.anchorMax = new Vector2(0.5f, 1f);
        textRect.pivot = new Vector2(0.5f, 1f);
        textRect.anchoredPosition = new Vector2(0, yPos);
        textRect.sizeDelta = new Vector2(500, 30);
        
        Text text = textObj.AddComponent<Text>();
        text.font = GetUIFont();
        text.fontSize = 18;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = initialText;
        
        return text;
    }
    
    /// <summary>
    /// 创建按钮
    /// </summary>
    private void CreateButtons(GameObject parent)
    {
        float buttonY = -450f;
        
        // 返回主菜单按钮（居中显示，改进样式）
        returnToMenuButton = CreateStyledButton(parent, "Return to Main Menu", 
            new Vector2(0, buttonY), ReturnToMainMenu);
    }
    
    /// <summary>
    /// 创建按钮
    /// </summary>
    private Button CreateButton(GameObject parent, string text, Vector2 position, System.Action onClick)
    {
        GameObject buttonObj = new GameObject($"Button_{text}");
        buttonObj.transform.SetParent(parent.transform, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 1f);
        buttonRect.anchorMax = new Vector2(0.5f, 1f);
        buttonRect.pivot = new Vector2(0.5f, 1f);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(120, 40);
        
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
        buttonText.fontSize = 14;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.text = text;
        
        return button;
    }
    
    /// <summary>
    /// 更新胜利统计
    /// </summary>
    private void UpdateVictoryStats()
    {
        if (!showGameStats) return;
        
        // 计算游戏时间
        float gameTime = Time.time - gameStartTime;
        int minutes = (int)(gameTime / 60);
        int seconds = (int)(gameTime % 60);
        
        // 更新UI文本
        if (gameTimeText != null)
            gameTimeText.text = $"{minutes:D2}:{seconds:D2}";
            
        if (killCountText != null)
            killCountText.text = $"{defeatedBosses.Count}";
            
        if (damageText != null)
            damageText.text = $"{totalDamageTaken}";
    }
    
    /// <summary>
    /// 玩家受伤事件处理
    /// </summary>
    private void OnPlayerDamaged(int damage)
    {
        totalDamageTaken++;
    }
    
    /// <summary>
    /// 自动保存游戏
    /// </summary>
    private void AutoSaveGame()
    {
        if (SaveManager.Instance != null)
        {
            // 使用自动保存功能，它会自动选择最近使用的存档槽位
            SaveManager.Instance.AutoSave();
            if (enableDebugInfo)
                Debug.Log("<color=green>[GameVictoryManager] Game progress auto-saved</color>");
        }
    }
    
    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void ReturnToMainMenu()
    {
        if (enableDebugInfo)
            Debug.Log("[GameVictoryManager] Returning to main menu...");
        
        // 开始协程安全地返回主菜单
        StartCoroutine(SafeReturnToMainMenu());
    }
    
    /// <summary>
    /// 安全地返回主菜单
    /// </summary>
    private System.Collections.IEnumerator SafeReturnToMainMenu()
    {
        if (enableDebugInfo)
            Debug.Log("[GameVictoryManager] 开始返回主菜单...");
        
        // 先隐藏胜利界面
        HideVictoryScreen();
        
        // 恢复时间缩放
        Time.timeScale = 1f;
        
        // 简单清理
        CleanupBeforeSceneChange();
        
        // 等待一帧
        yield return null;
        
        if (enableDebugInfo)
            Debug.Log("[GameVictoryManager] 正在加载主菜单场景...");
        
        // 直接加载主菜单场景
        SceneManager.LoadScene(mainMenuSceneName);
    }
    

    
    /// <summary>
    /// 场景切换前的清理工作
    /// </summary>
    private void CleanupBeforeSceneChange()
    {
        try
        {
            // 取消订阅Boss死亡事件
            BossController.OnBossDefeated -= OnBossDefeatedEvent;
            
            // 取消订阅HealthManager事件（不访问Instance以免重新创建）
            HealthManager.OnDamaged -= OnPlayerDamaged;
            
            // 标记游戏已结束，停止更新
            gameWon = true;
            
            if (enableDebugInfo)
                Debug.Log("[GameVictoryManager] 清理完成");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[GameVictoryManager] 清理警告: {e.Message}");
        }
    }
    

    

    
    /// <summary>
    /// 手动触发胜利（用于测试）
    /// </summary>
    [ContextMenu("测试胜利")]
    public void TestVictory()
    {
        TriggerVictory();
    }
    
    /// <summary>
    /// 隐藏胜利界面
    /// </summary>
    public void HideVictoryScreen()
    {
        if (victoryUI != null)
        {
            Destroy(victoryUI);
            victoryUI = null;
        }
        
        // 恢复时间
        Time.timeScale = 1f;
    }
    
    /// <summary>
    /// 获取游戏统计信息
    /// </summary>
    public string GetGameStats()
    {
        float gameTime = Time.time - gameStartTime;
        int minutes = (int)(gameTime / 60);
        int seconds = (int)(gameTime % 60);
        
        return $"Game Time: {minutes:D2}:{seconds:D2}\n" +
               $"Bosses Defeated: {defeatedBosses.Count}\n" +
               $"Times Damaged: {totalDamageTaken}";
    }
    
    /// <summary>
    /// 获取UI字体
    /// </summary>
    private Font GetUIFont()
    {
        // 优先使用指定的UI字体
        if (uiFont != null)
        {
            return uiFont;
        }
        
        // 尝试加载CyberpunkCraftpixPixel字体
        Font cyberpunkFont = Resources.Load<Font>("UI_set/10 Font/CyberpunkCraftpixPixel");
        if (cyberpunkFont != null)
        {
            if (enableDebugInfo)
                Debug.Log("[GameVictoryManager] Using CyberpunkCraftpixPixel font");
            return cyberpunkFont;
        }
        
        // 备用方案：使用LegacyRuntime.ttf
        Font legacyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (legacyFont != null)
        {
            if (enableDebugInfo)
                Debug.Log("[GameVictoryManager] Using LegacyRuntime.ttf font");
            return legacyFont;
        }
        
        // 最后备用：使用Resources中的任意字体
        Font[] allFonts = Resources.FindObjectsOfTypeAll<Font>();
        if (allFonts.Length > 0)
        {
            if (enableDebugInfo)
                Debug.Log($"[GameVictoryManager] Using fallback font: {allFonts[0].name}");
            return allFonts[0];
        }
        
        // 如果都找不到，返回null（Unity会使用默认字体）
        Debug.LogWarning("[GameVictoryManager] Unable to find available font, will use Unity default font");
        return null;
    }
    
    // ==================== 新增美化方法 ====================
    
    /// <summary>
    /// 创建装饰边框
    /// </summary>
    private void CreateDecorativeBorder(GameObject parent)
    {
        // 顶部边框
        CreateBorderLine(parent, new Vector2(0, 240), new Vector2(760, 4), new Color(0.8f, 0.6f, 0.1f, 0.8f));
        // 底部边框
        CreateBorderLine(parent, new Vector2(0, -240), new Vector2(760, 4), new Color(0.8f, 0.6f, 0.1f, 0.8f));
        // 左边框
        CreateBorderLine(parent, new Vector2(-380, 0), new Vector2(4, 480), new Color(0.8f, 0.6f, 0.1f, 0.8f));
        // 右边框
        CreateBorderLine(parent, new Vector2(380, 0), new Vector2(4, 480), new Color(0.8f, 0.6f, 0.1f, 0.8f));
    }
    
    /// <summary>
    /// 创建单条边框线
    /// </summary>
    private void CreateBorderLine(GameObject parent, Vector2 position, Vector2 size, Color color)
    {
        GameObject lineObj = new GameObject("BorderLine");
        lineObj.transform.SetParent(parent.transform, false);
        
        RectTransform lineRect = lineObj.AddComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0.5f, 0.5f);
        lineRect.anchorMax = new Vector2(0.5f, 0.5f);
        lineRect.pivot = new Vector2(0.5f, 0.5f);
        lineRect.anchoredPosition = position;
        lineRect.sizeDelta = size;
        
        Image lineImage = lineObj.AddComponent<Image>();
        lineImage.color = color;
    }
    
    /// <summary>
    /// 创建顶部装饰条
    /// </summary>
    private void CreateTopDecoration(GameObject parent)
    {
        GameObject decorObj = new GameObject("TopDecoration");
        decorObj.transform.SetParent(parent.transform, false);
        
        RectTransform decorRect = decorObj.AddComponent<RectTransform>();
        decorRect.anchorMin = new Vector2(0.5f, 1f);
        decorRect.anchorMax = new Vector2(0.5f, 1f);
        decorRect.pivot = new Vector2(0.5f, 1f);
        decorRect.anchoredPosition = new Vector2(0, 0);
        decorRect.sizeDelta = new Vector2(800, 60);
        
        Image decorImage = decorObj.AddComponent<Image>();
        decorImage.color = new Color(0.2f, 0.25f, 0.4f, 0.7f);
        
        // 添加装饰文字
        GameObject decorTextObj = new GameObject("DecorationText");
        decorTextObj.transform.SetParent(decorObj.transform, false);
        
        RectTransform decorTextRect = decorTextObj.AddComponent<RectTransform>();
        decorTextRect.anchorMin = Vector2.zero;
        decorTextRect.anchorMax = Vector2.one;
        decorTextRect.offsetMin = Vector2.zero;
        decorTextRect.offsetMax = Vector2.zero;
        
        Text decorText = decorTextObj.AddComponent<Text>();
        decorText.font = GetUIFont();
        decorText.fontSize = 16;
        decorText.color = new Color(0.8f, 0.8f, 0.9f, 0.8f);
        decorText.alignment = TextAnchor.MiddleCenter;
        decorText.text = "★ ─────────────────────────────────────────────────────────────────────────────────────── ★";
    }
    
    /// <summary>
    /// 创建统计标题
    /// </summary>
    private void CreateStatsTitle(GameObject parent)
    {
        GameObject titleObj = new GameObject("StatsTitle");
        titleObj.transform.SetParent(parent.transform, false);
        
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -10);
        titleRect.sizeDelta = new Vector2(600, 40);
        
        Text titleText = titleObj.AddComponent<Text>();
        titleText.font = GetUIFont();
        titleText.fontSize = 22;
        titleText.color = new Color(0.9f, 0.9f, 1f, 1f);
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.text = "⚡ MISSION STATISTICS ⚡";
        titleText.fontStyle = FontStyle.Bold;
    }
    
    /// <summary>
    /// 创建美化的统计条目
    /// </summary>
    private Text CreateStatEntry(GameObject parent, string icon, string label, string value)
    {
        GameObject entryObj = new GameObject($"StatEntry_{label}");
        entryObj.transform.SetParent(parent.transform, false);
        
        RectTransform entryRect = entryObj.AddComponent<RectTransform>();
        entryRect.sizeDelta = new Vector2(0, 30);
        
        // 添加水平布局组件
        HorizontalLayoutGroup horizontalLayout = entryObj.AddComponent<HorizontalLayoutGroup>();
        horizontalLayout.spacing = 10;
        horizontalLayout.childAlignment = TextAnchor.MiddleLeft;
        horizontalLayout.childControlHeight = false;
        horizontalLayout.childControlWidth = false;
        horizontalLayout.childForceExpandHeight = false;
        horizontalLayout.childForceExpandWidth = false;
        
        // 图标
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(entryObj.transform, false);
        
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(30, 30);
        
        Text iconText = iconObj.AddComponent<Text>();
        iconText.font = GetUIFont();
        iconText.fontSize = 20;
        iconText.color = new Color(1f, 0.8f, 0.2f, 1f);
        iconText.alignment = TextAnchor.MiddleCenter;
        iconText.text = icon;
        
        // 标签
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(entryObj.transform, false);
        
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(200, 30);
        
        Text labelText = labelObj.AddComponent<Text>();
        labelText.font = GetUIFont();
        labelText.fontSize = 18;
        labelText.color = new Color(0.9f, 0.9f, 1f, 1f);
        labelText.alignment = TextAnchor.MiddleLeft;
        labelText.text = label + ":";
        
        // 数值
        GameObject valueObj = new GameObject("Value");
        valueObj.transform.SetParent(entryObj.transform, false);
        
        RectTransform valueRect = valueObj.AddComponent<RectTransform>();
        valueRect.sizeDelta = new Vector2(150, 30);
        
        Text valueText = valueObj.AddComponent<Text>();
        valueText.font = GetUIFont();
        valueText.fontSize = 18;
        valueText.color = new Color(0.2f, 1f, 0.4f, 1f);
        valueText.alignment = TextAnchor.MiddleRight;
        valueText.text = value;
        valueText.fontStyle = FontStyle.Bold;
        
        return valueText;
    }
    
    /// <summary>
    /// 创建文字阴影效果
    /// </summary>
    private void CreateTextShadow(GameObject parent, Text mainText)
    {
        GameObject shadowObj = new GameObject("TextShadow");
        shadowObj.transform.SetParent(parent.transform, false);
        shadowObj.transform.SetSiblingIndex(0); // 确保阴影在主文字后面
        
        RectTransform shadowRect = shadowObj.AddComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.offsetMin = Vector2.zero;
        shadowRect.offsetMax = Vector2.zero;
        shadowRect.anchoredPosition = new Vector2(3, -3); // 阴影偏移
        
        Text shadowText = shadowObj.AddComponent<Text>();
        shadowText.font = mainText.font;
        shadowText.fontSize = mainText.fontSize;
        shadowText.color = new Color(0, 0, 0, 0.5f); // 半透明黑色阴影
        shadowText.alignment = mainText.alignment;
        shadowText.text = mainText.text;
        shadowText.fontStyle = mainText.fontStyle;
    }
    
    /// <summary>
    /// 创建样式化按钮
    /// </summary>
    private Button CreateStyledButton(GameObject parent, string text, Vector2 position, System.Action onClick)
    {
        GameObject buttonObj = new GameObject($"StyledButton_{text}");
        buttonObj.transform.SetParent(parent.transform, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 1f);
        buttonRect.anchorMax = new Vector2(0.5f, 1f);
        buttonRect.pivot = new Vector2(0.5f, 1f);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(200, 50);
        
        // 按钮背景
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.15f, 0.4f, 0.8f, 0.9f);
        
        // 按钮边框
        CreateButtonBorder(buttonObj);
        
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(() => onClick?.Invoke());
        
        // 设置按钮颜色状态
        var colors = button.colors;
        colors.normalColor = new Color(0.15f, 0.4f, 0.8f, 0.9f);
        colors.highlightedColor = new Color(0.25f, 0.5f, 0.9f, 1f);
        colors.pressedColor = new Color(0.1f, 0.3f, 0.7f, 1f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        button.colors = colors;
        
        // 按钮文字
        GameObject textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.font = GetUIFont();
        buttonText.fontSize = 16;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.text = text;
        buttonText.fontStyle = FontStyle.Bold;
        
        // 添加按钮文字阴影
        CreateTextShadow(textObj, buttonText);
        
        return button;
    }
    
    /// <summary>
    /// 创建按钮边框
    /// </summary>
    private void CreateButtonBorder(GameObject parent)
    {
        // 顶部边框
        CreateBorderLine(parent, new Vector2(0, 23), new Vector2(196, 2), new Color(0.8f, 0.9f, 1f, 0.6f));
        // 底部边框
        CreateBorderLine(parent, new Vector2(0, -23), new Vector2(196, 2), new Color(0.8f, 0.9f, 1f, 0.6f));
        // 左边框
        CreateBorderLine(parent, new Vector2(-98, 0), new Vector2(2, 46), new Color(0.8f, 0.9f, 1f, 0.6f));
        // 右边框
        CreateBorderLine(parent, new Vector2(98, 0), new Vector2(2, 46), new Color(0.8f, 0.9f, 1f, 0.6f));
    }
} 