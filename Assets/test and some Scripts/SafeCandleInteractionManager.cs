using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace SafeCandleSystem
{
    /// <summary>
    /// 安全的烛火交互系统 - 完全隔离，不会与现有代码产生冲突
    /// 功能：升级提示、按E键交互、升级效果、自动UI创建
    /// 升级路径：lv1→lv2(踢腿伤害20) → lv3(windpower动画) → lv4(lefttodesign) → lv5(lefttodesign)
    /// </summary>
    public class SafeCandleInteractionManager : MonoBehaviour
    {
        #region 配置常量
        private const string PLAYER_TAG = "Player";
        private const string CANDLE_LAYER_NAME = "CandleLayer";
        private const string UI_CANVAS_NAME = "SafeCandleUI";
        private const KeyCode DEFAULT_INTERACTION_KEY = KeyCode.E;
        private const float DEFAULT_INTERACTION_RANGE = 2.5f;
        private const int KICK_DAMAGE_LEVEL_2 = 20;
        private const string WINDPOWER_TRIGGER = "windpower";
        private const string LEFT_TO_DESIGN_MESSAGE = "lefttodesign";
        #endregion
        
        #region Inspector 配置
        [Header("=== 安全烛火系统配置 ===")]
        [SerializeField] private KeyCode interactionKey = DEFAULT_INTERACTION_KEY;
        [SerializeField] private float interactionRange = DEFAULT_INTERACTION_RANGE;
        [SerializeField] private LayerMask playerLayerMask = 1;
        
        [Header("升级效果配置")]
        [SerializeField] private int kickDamageValue = KICK_DAMAGE_LEVEL_2;
        [SerializeField] private string windpowerAnimationTrigger = WINDPOWER_TRIGGER;
        [SerializeField] private string leftToDesignText = LEFT_TO_DESIGN_MESSAGE;
        
        [Header("烛火预制体（可选）")]
        [SerializeField] private GameObject candlePrefab;
        
        [Header("音效配置")]
        [SerializeField] private AudioClip upgradeSound;
        [SerializeField] private AudioClip interactionSound;
        
        [Header("UI配置")]
        [SerializeField] private Font customUIFont;
        [SerializeField] private Canvas targetCanvas;
        
        [Header("调试设置")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private bool enableDebugKeys = true;
        #endregion
        
        #region 烛火数据结构
        [System.Serializable]
        public class SafeCandleData
        {
            public int requiredPlayerLevel;
            public int targetPlayerLevel;
            public Vector3 worldPosition;
            public bool isCurrentlyActive;
            public string upgradeDescription;
            
            [Header("运行时引用（自动设置）")]
            public GameObject candleGameObject;
            public Light candleLight;
            public GameObject promptUIObject;
            public CandleUpgradeType upgradeType;
            
            public SafeCandleData(int reqLevel, int tarLevel, Vector3 pos, string desc, CandleUpgradeType type)
            {
                requiredPlayerLevel = reqLevel;
                targetPlayerLevel = tarLevel;
                worldPosition = pos;
                upgradeDescription = desc;
                upgradeType = type;
                isCurrentlyActive = true;
            }
        }
        
        public enum CandleUpgradeType
        {
            KickDamageUpgrade,
            WindpowerAnimation,
            LeftToDesignMessage
        }
        #endregion
        
        #region 私有字段
        private List<SafeCandleData> candleDataList = new List<SafeCandleData>();
        private Canvas safeUICanvas;
        private AudioSource safeAudioSource;
        
        // Player组件引用（通过安全方式获取）
        private Transform playerTransformRef;
        private PlayerLevelSkillSystem playerLevelSystemRef;
        private PlayerAttackSystem playerAttackSystemRef;
        private Animator playerAnimatorRef;
        
        // UI组件
        private GameObject mainUpgradePromptUI;
        private Text mainPromptText;
        private GameObject levelDisplayUI;
        private Text levelDisplayText;
        private GameObject debugInfoUI;
        private Text debugInfoText;
        
        // 交互状态
        private SafeCandleData currentNearbyCandle;
        private bool isPlayerNearCandle;
        private bool systemInitialized;
        #endregion
        
        #region Unity 生命周期
        void Awake()
        {
            SafeLog("SafeCandleInteractionManager - 开始初始化");
            InitializeAudioSource();
        }
        
        void Start()
        {
            StartCoroutine(SafeInitializationRoutine());
        }
        
        void Update()
        {
            if (!systemInitialized) return;
            
            UpdatePlayerProximityCheck();
            HandleUserInput();
            UpdateUIDisplay();
            
            if (enableDebugKeys)
            {
                HandleDebugInputs();
            }
        }
        
        void OnDrawGizmos()
        {
            if (!showGizmos) return;
            DrawInteractionGizmos();
        }
        #endregion
        
        #region 安全初始化
        /// <summary>
        /// 安全的初始化协程 - 确保所有组件都正确加载
        /// </summary>
        private IEnumerator SafeInitializationRoutine()
        {
            yield return new WaitForSeconds(0.5f); // 等待其他系统初始化
            
            SafeLog("开始安全初始化流程...");
            
            // Step 1: 安全查找Player组件
            if (!SafeFindPlayerComponents())
            {
                SafeLogError("无法找到Player组件，初始化失败");
                yield break;
            }
            
            // Step 2: 创建安全UI系统
            yield return StartCoroutine(CreateSafeUISystem());
            
            // Step 3: 创建默认烛火
            CreateDefaultCandleConfiguration();
            
            // Step 4: 验证系统完整性
            if (ValidateSystemIntegrity())
            {
                systemInitialized = true;
                SafeLog("SafeCandleInteractionManager - 初始化完成！");
                LogSystemStatus();
            }
            else
            {
                SafeLogError("系统完整性验证失败");
            }
        }
        
        /// <summary>
        /// 安全查找Player相关组件
        /// </summary>
        private bool SafeFindPlayerComponents()
        {
            GameObject playerObject = null;
            
            // 方法1: 通过标签查找
            GameObject taggedPlayer = GameObject.FindGameObjectWithTag(PLAYER_TAG);
            if (taggedPlayer != null)
            {
                playerObject = taggedPlayer;
                SafeLog($"通过标签找到Player: {playerObject.name}");
            }
            
            // 方法2: 通过组件查找（备用方案）
            if (playerObject == null)
            {
                PlayerController playerController = FindObjectOfType<PlayerController>();
                if (playerController != null)
                {
                    playerObject = playerController.gameObject;
                    SafeLog($"通过PlayerController找到Player: {playerObject.name}");
                }
            }
            
            if (playerObject == null)
            {
                SafeLogError("无法找到Player对象！请确保Player对象有正确的标签或组件");
                return false;
            }
            
            // 安全获取所有组件
            playerTransformRef = playerObject.transform;
            playerLevelSystemRef = playerObject.GetComponent<PlayerLevelSkillSystem>();
            playerAttackSystemRef = playerObject.GetComponent<PlayerAttackSystem>();
            playerAnimatorRef = playerObject.GetComponent<Animator>();
            
            // 验证必需组件
            bool hasRequiredComponents = true;
            if (playerLevelSystemRef == null)
            {
                SafeLogWarning("未找到PlayerLevelSkillSystem组件 - 等级功能将不可用");
                hasRequiredComponents = false;
            }
            if (playerAttackSystemRef == null)
            {
                SafeLogWarning("未找到PlayerAttackSystem组件 - 攻击伤害升级将不可用");
                hasRequiredComponents = false;
            }
            if (playerAnimatorRef == null)
            {
                SafeLogWarning("未找到Animator组件 - windpower动画将不可用");
                hasRequiredComponents = false;
            }
            
            return true; // 即使某些组件缺失，也允许系统运行
        }
        
        /// <summary>
        /// 初始化音频源
        /// </summary>
        private void InitializeAudioSource()
        {
            safeAudioSource = GetComponent<AudioSource>();
            if (safeAudioSource == null)
            {
                safeAudioSource = gameObject.AddComponent<AudioSource>();
                safeAudioSource.playOnAwake = false;
                safeAudioSource.volume = 0.7f;
            }
        }
        #endregion
        
        #region UI系统创建
        /// <summary>
        /// 创建安全的UI系统
        /// </summary>
        private IEnumerator CreateSafeUISystem()
        {
            SafeLog("创建安全UI系统...");
            
            // 查找或创建Canvas
            if (targetCanvas != null)
            {
                safeUICanvas = targetCanvas;
            }
            else
            {
                safeUICanvas = FindObjectOfType<Canvas>();
                if (safeUICanvas == null)
                {
                    CreateSafeCanvas();
                }
            }
            
            yield return null; // 等待一帧
            
            // 创建UI组件
            CreateMainPromptUI();
            CreateLevelDisplayUI();
            if (enableDebugLogging)
            {
                CreateDebugInfoUI();
            }
            
            SafeLog("UI系统创建完成");
        }
        
        /// <summary>
        /// 创建安全的Canvas
        /// </summary>
        private void CreateSafeCanvas()
        {
            GameObject canvasObject = new GameObject(UI_CANVAS_NAME);
            safeUICanvas = canvasObject.AddComponent<Canvas>();
            safeUICanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            safeUICanvas.sortingOrder = 100; // 确保在最上层
            
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObject.AddComponent<GraphicRaycaster>();
            SafeLog("创建了安全Canvas");
        }
        
        /// <summary>
        /// 创建主提示UI
        /// </summary>
        private void CreateMainPromptUI()
        {
            mainUpgradePromptUI = new GameObject("SafeCandle_MainPrompt");
            mainUpgradePromptUI.transform.SetParent(safeUICanvas.transform, false);
            
            RectTransform promptRect = mainUpgradePromptUI.AddComponent<RectTransform>();
            promptRect.anchorMin = new Vector2(0.5f, 0.7f);
            promptRect.anchorMax = new Vector2(0.5f, 0.7f);
            promptRect.pivot = new Vector2(0.5f, 0.5f);
            promptRect.sizeDelta = new Vector2(600, 120);
            
            // 背景
            Image promptBg = mainUpgradePromptUI.AddComponent<Image>();
            promptBg.color = new Color(0.05f, 0.05f, 0.05f, 0.9f);
            promptBg.raycastTarget = false;
            
            // 文本
            GameObject textObject = new GameObject("PromptText");
            textObject.transform.SetParent(mainUpgradePromptUI.transform, false);
            
            mainPromptText = textObject.AddComponent<Text>();
            mainPromptText.font = customUIFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            mainPromptText.fontSize = 24;
            mainPromptText.color = Color.yellow;
            mainPromptText.alignment = TextAnchor.MiddleCenter;
            mainPromptText.raycastTarget = false;
            
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);
            
            mainUpgradePromptUI.SetActive(false);
        }
        
        /// <summary>
        /// 创建等级显示UI
        /// </summary>
        private void CreateLevelDisplayUI()
        {
            levelDisplayUI = new GameObject("SafeCandle_LevelDisplay");
            levelDisplayUI.transform.SetParent(safeUICanvas.transform, false);
            
            RectTransform levelRect = levelDisplayUI.AddComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0.02f, 0.92f);
            levelRect.anchorMax = new Vector2(0.02f, 0.92f);
            levelRect.pivot = new Vector2(0f, 1f);
            levelRect.sizeDelta = new Vector2(200, 40);
            
            // 背景
            Image levelBg = levelDisplayUI.AddComponent<Image>();
            levelBg.color = new Color(0, 0, 0, 0.6f);
            levelBg.raycastTarget = false;
            
            levelDisplayText = levelDisplayUI.AddComponent<Text>();
            levelDisplayText.font = customUIFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            levelDisplayText.fontSize = 18;
            levelDisplayText.color = Color.white;
            levelDisplayText.alignment = TextAnchor.MiddleCenter;
            levelDisplayText.raycastTarget = false;
        }
        
        /// <summary>
        /// 创建调试信息UI
        /// </summary>
        private void CreateDebugInfoUI()
        {
            debugInfoUI = new GameObject("SafeCandle_DebugInfo");
            debugInfoUI.transform.SetParent(safeUICanvas.transform, false);
            
            RectTransform debugRect = debugInfoUI.AddComponent<RectTransform>();
            debugRect.anchorMin = new Vector2(0.02f, 0.02f);
            debugRect.anchorMax = new Vector2(0.02f, 0.02f);
            debugRect.pivot = new Vector2(0f, 0f);
            debugRect.sizeDelta = new Vector2(450, 250);
            
            // 背景
            Image debugBg = debugInfoUI.AddComponent<Image>();
            debugBg.color = new Color(0, 0, 0, 0.7f);
            debugBg.raycastTarget = false;
            
            GameObject debugTextObject = new GameObject("DebugText");
            debugTextObject.transform.SetParent(debugInfoUI.transform, false);
            
            debugInfoText = debugTextObject.AddComponent<Text>();
            debugInfoText.font = customUIFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            debugInfoText.fontSize = 12;
            debugInfoText.color = Color.green;
            debugInfoText.alignment = TextAnchor.UpperLeft;
            debugInfoText.raycastTarget = false;
            
            RectTransform debugTextRect = debugTextObject.GetComponent<RectTransform>();
            debugTextRect.anchorMin = Vector2.zero;
            debugTextRect.anchorMax = Vector2.one;
            debugTextRect.offsetMin = new Vector2(8, 8);
            debugTextRect.offsetMax = new Vector2(-8, -8);
        }
        #endregion
        
        #region 烛火创建
        /// <summary>
        /// 创建默认烛火配置
        /// </summary>
        private void CreateDefaultCandleConfiguration()
        {
            SafeLog("创建默认烛火配置...");
            
            // 清空现有数据
            candleDataList.Clear();
            
            // 定义默认烛火配置
            Vector3[] candlePositions = {
                new Vector3(8f, 0f, 0f),   // lv1→lv2
                new Vector3(18f, 0f, 0f),  // lv2→lv3
                new Vector3(28f, 0f, 0f),  // lv3→lv4
                new Vector3(38f, 0f, 0f)   // lv4→lv5
            };
            
            string[] descriptions = {
                "踢腿伤害提升至20",
                "解锁windpower技能",
                "lefttodesign",
                "lefttodesign"
            };
            
            CandleUpgradeType[] upgradeTypes = {
                CandleUpgradeType.KickDamageUpgrade,
                CandleUpgradeType.WindpowerAnimation,
                CandleUpgradeType.LeftToDesignMessage,
                CandleUpgradeType.LeftToDesignMessage
            };
            
            // 创建烛火数据
            for (int i = 0; i < candlePositions.Length; i++)
            {
                SafeCandleData candleData = new SafeCandleData(
                    i + 1, // 需要等级
                    i + 2, // 目标等级
                    candlePositions[i],
                    descriptions[i],
                    upgradeTypes[i]
                );
                
                CreateCandleGameObject(candleData);
                candleDataList.Add(candleData);
            }
            
            SafeLog($"创建了 {candleDataList.Count} 个烛火");
        }
        
        /// <summary>
        /// 创建烛火游戏对象
        /// </summary>
        private void CreateCandleGameObject(SafeCandleData candleData)
        {
            GameObject candleObject;
            
            if (candlePrefab != null)
            {
                candleObject = Instantiate(candlePrefab, candleData.worldPosition, Quaternion.identity);
            }
            else
            {
                // 创建默认烛火外观
                candleObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                candleObject.transform.position = candleData.worldPosition;
                candleObject.transform.localScale = new Vector3(0.4f, 1.2f, 0.4f);
                
                // 设置材质
                Renderer renderer = candleObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material candleMaterial = new Material(Shader.Find("Standard"));
                    candleMaterial.color = new Color(1f, 0.9f, 0.3f); // 暖黄色
                    candleMaterial.SetFloat("_Metallic", 0.2f);
                    candleMaterial.SetFloat("_Smoothness", 0.8f);
                    renderer.material = candleMaterial;
                }
                
                // 移除碰撞体（避免干扰）
                Collider collider = candleObject.GetComponent<Collider>();
                if (collider != null)
                {
                    DestroyImmediate(collider);
                }
            }
            
            candleObject.name = $"SafeCandle_Lv{candleData.requiredPlayerLevel}_to_Lv{candleData.targetPlayerLevel}";
            
            // 添加光效
            Light candleLight = candleObject.GetComponent<Light>();
            if (candleLight == null)
            {
                candleLight = candleObject.AddComponent<Light>();
            }
            candleLight.type = LightType.Point;
            candleLight.color = new Color(1f, 0.8f, 0.4f);
            candleLight.intensity = 1.5f;
            candleLight.range = 4f;
            candleLight.shadows = LightShadows.Soft;
            
            // 设置引用
            candleData.candleGameObject = candleObject;
            candleData.candleLight = candleLight;
            
            // 添加标识组件
            SafeCandleIdentifier identifier = candleObject.AddComponent<SafeCandleIdentifier>();
            identifier.SetCandleData(candleData);
        }
        #endregion
        
        #region 交互逻辑
        /// <summary>
        /// 更新玩家接近检查
        /// </summary>
        private void UpdatePlayerProximityCheck()
        {
            if (playerTransformRef == null) return;
            
            SafeCandleData nearestCandle = null;
            float nearestDistance = float.MaxValue;
            
            foreach (SafeCandleData candle in candleDataList)
            {
                if (!candle.isCurrentlyActive) continue;
                
                float distance = Vector3.Distance(playerTransformRef.position, candle.worldPosition);
                if (distance <= interactionRange && distance < nearestDistance)
                {
                    int currentLevel = GetSafePlayerLevel();
                    if (currentLevel == candle.requiredPlayerLevel)
                    {
                        nearestDistance = distance;
                        nearestCandle = candle;
                    }
                }
            }
            
            bool wasNearCandle = isPlayerNearCandle;
            currentNearbyCandle = nearestCandle;
            isPlayerNearCandle = nearestCandle != null;
            
            // 状态变化时更新UI
            if (isPlayerNearCandle != wasNearCandle)
            {
                if (isPlayerNearCandle)
                {
                    ShowUpgradePrompt(currentNearbyCandle);
                }
                else
                {
                    HideUpgradePrompt();
                }
            }
        }
        
        /// <summary>
        /// 处理用户输入
        /// </summary>
        private void HandleUserInput()
        {
            if (isPlayerNearCandle && currentNearbyCandle != null && Input.GetKeyDown(interactionKey))
            {
                PerformSafeUpgrade(currentNearbyCandle);
            }
        }
        
        /// <summary>
        /// 执行安全升级
        /// </summary>
        private void PerformSafeUpgrade(SafeCandleData candle)
        {
            int currentLevel = GetSafePlayerLevel();
            
            if (currentLevel != candle.requiredPlayerLevel)
            {
                SafeLogWarning($"等级不符合要求。当前等级: {currentLevel}, 需要等级: {candle.requiredPlayerLevel}");
                return;
            }
            
            SafeLog($"执行升级: Lv{candle.requiredPlayerLevel} → Lv{candle.targetPlayerLevel}");
            
            // 设置新等级
            SetSafePlayerLevel(candle.targetPlayerLevel);
            
            // 应用升级效果
            ApplySafeUpgradeEffect(candle);
            
            // 熄灭烛火
            ExtinguishCandle(candle);
            
            // 播放音效
            PlaySafeUpgradeSound();
            
            // 隐藏提示
            HideUpgradePrompt();
            
            SafeLog($"升级完成！新等级: {candle.targetPlayerLevel}");
        }
        
        /// <summary>
        /// 应用安全升级效果
        /// </summary>
        private void ApplySafeUpgradeEffect(SafeCandleData candle)
        {
            switch (candle.upgradeType)
            {
                case CandleUpgradeType.KickDamageUpgrade:
                    ApplyKickDamageUpgrade();
                    break;
                    
                case CandleUpgradeType.WindpowerAnimation:
                    PlayWindpowerAnimation();
                    break;
                    
                case CandleUpgradeType.LeftToDesignMessage:
                    ShowLeftToDesignMessage();
                    break;
            }
        }
        
        /// <summary>
        /// 应用踢腿伤害升级
        /// </summary>
        private void ApplyKickDamageUpgrade()
        {
            if (playerAttackSystemRef != null)
            {
                try
                {
                    playerAttackSystemRef.AttackDamage = kickDamageValue;
                    SafeLog($"踢腿伤害升级成功: {kickDamageValue}");
                }
                catch (System.Exception e)
                {
                    SafeLogError($"设置攻击伤害失败: {e.Message}");
                }
            }
            else
            {
                SafeLogWarning("PlayerAttackSystem组件不存在，无法升级攻击伤害");
            }
        }
        
        /// <summary>
        /// 播放windpower动画
        /// </summary>
        private void PlayWindpowerAnimation()
        {
            if (playerAnimatorRef != null && !string.IsNullOrEmpty(windpowerAnimationTrigger))
            {
                try
                {
                    playerAnimatorRef.SetTrigger(windpowerAnimationTrigger);
                    SafeLog($"windpower动画播放成功: {windpowerAnimationTrigger}");
                }
                catch (System.Exception e)
                {
                    SafeLogError($"播放windpower动画失败: {e.Message}");
                }
            }
            else
            {
                SafeLogWarning("Animator组件不存在或动画触发器未设置，无法播放windpower动画");
            }
        }
        
        /// <summary>
        /// 显示lefttodesign消息
        /// </summary>
        private void ShowLeftToDesignMessage()
        {
            StartCoroutine(ShowSafeMessage(leftToDesignText, 3.5f));
        }
        
        /// <summary>
        /// 安全消息显示协程
        /// </summary>
        private IEnumerator ShowSafeMessage(string message, float duration)
        {
            if (mainPromptText != null && mainUpgradePromptUI != null)
            {
                mainPromptText.text = message;
                mainPromptText.fontSize = 32;
                mainPromptText.color = Color.cyan;
                mainUpgradePromptUI.SetActive(true);
                
                yield return new WaitForSeconds(duration);
                
                mainUpgradePromptUI.SetActive(false);
                mainPromptText.fontSize = 24;
                mainPromptText.color = Color.yellow;
            }
        }
        
        /// <summary>
        /// 熄灭烛火
        /// </summary>
        private void ExtinguishCandle(SafeCandleData candle)
        {
            candle.isCurrentlyActive = false;
            
            if (candle.candleLight != null)
            {
                candle.candleLight.enabled = false;
            }
            
            if (candle.candleGameObject != null)
            {
                Renderer renderer = candle.candleGameObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material extinguishedMaterial = new Material(renderer.material);
                    extinguishedMaterial.color = new Color(0.3f, 0.3f, 0.3f);
                    renderer.material = extinguishedMaterial;
                }
            }
            
            SafeLog($"烛火已熄灭: Lv{candle.requiredPlayerLevel}→Lv{candle.targetPlayerLevel}");
        }
        #endregion
        
        #region UI更新
        /// <summary>
        /// 更新UI显示
        /// </summary>
        private void UpdateUIDisplay()
        {
            // 更新等级显示
            if (levelDisplayText != null)
            {
                int currentLevel = GetSafePlayerLevel();
                levelDisplayText.text = $"等级: {currentLevel}";
            }
            
            // 更新调试信息
            if (enableDebugLogging && debugInfoText != null)
            {
                UpdateDebugDisplay();
            }
        }
        
        /// <summary>
        /// 显示升级提示
        /// </summary>
        private void ShowUpgradePrompt(SafeCandleData candle)
        {
            if (mainPromptText != null && mainUpgradePromptUI != null)
            {
                string promptMessage = $"按 {interactionKey} 键升级\n" +
                                     $"等级 {candle.requiredPlayerLevel} → {candle.targetPlayerLevel}\n" +
                                     $"{candle.upgradeDescription}";
                
                mainPromptText.text = promptMessage;
                mainUpgradePromptUI.SetActive(true);
            }
        }
        
        /// <summary>
        /// 隐藏升级提示
        /// </summary>
        private void HideUpgradePrompt()
        {
            if (mainUpgradePromptUI != null)
            {
                mainUpgradePromptUI.SetActive(false);
            }
        }
        
        /// <summary>
        /// 更新调试显示
        /// </summary>
        private void UpdateDebugDisplay()
        {
            string debugInfo = "=== 安全烛火系统调试 ===\n";
            debugInfo += $"系统状态: {(systemInitialized ? "已初始化" : "初始化中")}\n";
            debugInfo += $"当前等级: {GetSafePlayerLevel()}\n";
            debugInfo += $"玩家位置: {(playerTransformRef ? playerTransformRef.position.ToString("F1") : "N/A")}\n";
            debugInfo += $"靠近烛火: {isPlayerNearCandle}\n";
            debugInfo += $"激活烛火: {GetActiveCandleCount()}/{candleDataList.Count}\n";
            debugInfo += $"交互按键: {interactionKey}\n";
            debugInfo += $"交互距离: {interactionRange:F1}m\n";
            debugInfo += "\n调试快捷键:\n";
            debugInfo += "F1: 强制升级  F2: 重置烛火\n";
            debugInfo += "F3: 切换调试  F4: 系统状态";
            
            debugInfoText.text = debugInfo;
        }
        #endregion
        
        #region 安全辅助方法
        /// <summary>
        /// 安全获取玩家等级
        /// </summary>
        private int GetSafePlayerLevel()
        {
            if (playerLevelSystemRef != null)
            {
                try
                {
                    // 使用反射安全获取private字段
                    var levelField = typeof(PlayerLevelSkillSystem).GetField("playerLevel", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (levelField != null)
                    {
                        return (int)levelField.GetValue(playerLevelSystemRef);
                    }
                }
                catch (System.Exception e)
                {
                    SafeLogError($"获取玩家等级失败: {e.Message}");
                }
            }
            
            return 1; // 默认等级
        }
        
        /// <summary>
        /// 安全设置玩家等级
        /// </summary>
        private void SetSafePlayerLevel(int newLevel)
        {
            if (playerLevelSystemRef != null)
            {
                try
                {
                    // 使用反射安全设置private字段
                    var levelField = typeof(PlayerLevelSkillSystem).GetField("playerLevel", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (levelField != null)
                    {
                        levelField.SetValue(playerLevelSystemRef, newLevel);
                        SafeLog($"玩家等级设置为: {newLevel}");
                    }
                    else
                    {
                        SafeLogError("无法找到playerLevel字段");
                    }
                }
                catch (System.Exception e)
                {
                    SafeLogError($"设置玩家等级失败: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// 获取激活烛火数量
        /// </summary>
        private int GetActiveCandleCount()
        {
            int count = 0;
            foreach (SafeCandleData candle in candleDataList)
            {
                if (candle.isCurrentlyActive) count++;
            }
            return count;
        }
        
        /// <summary>
        /// 播放安全升级音效
        /// </summary>
        private void PlaySafeUpgradeSound()
        {
            if (safeAudioSource != null && upgradeSound != null)
            {
                try
                {
                    safeAudioSource.PlayOneShot(upgradeSound);
                }
                catch (System.Exception e)
                {
                    SafeLogError($"播放升级音效失败: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// 验证系统完整性
        /// </summary>
        private bool ValidateSystemIntegrity()
        {
            bool isValid = true;
            
            if (playerTransformRef == null)
            {
                SafeLogError("Player Transform引用丢失");
                isValid = false;
            }
            
            if (safeUICanvas == null)
            {
                SafeLogError("UI Canvas引用丢失");
                isValid = false;
            }
            
            if (candleDataList.Count == 0)
            {
                SafeLogError("没有创建任何烛火");
                isValid = false;
            }
            
            return isValid;
        }
        #endregion
        
        #region 调试功能
        /// <summary>
        /// 处理调试输入
        /// </summary>
        private void HandleDebugInputs()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                DebugForceUpgrade();
            }
            
            if (Input.GetKeyDown(KeyCode.F2))
            {
                DebugResetAllCandles();
            }
            
            if (Input.GetKeyDown(KeyCode.F3))
            {
                DebugToggleDebugUI();
            }
            
            if (Input.GetKeyDown(KeyCode.F4))
            {
                DebugLogSystemStatus();
            }
        }
        
        /// <summary>
        /// 调试：强制升级
        /// </summary>
        private void DebugForceUpgrade()
        {
            int currentLevel = GetSafePlayerLevel();
            if (currentLevel < 5)
            {
                SetSafePlayerLevel(currentLevel + 1);
                SafeLog($"[调试] 强制升级到等级 {currentLevel + 1}");
            }
            else
            {
                SafeLog("[调试] 已达到最高等级");
            }
        }
        
        /// <summary>
        /// 调试：重置所有烛火
        /// </summary>
        private void DebugResetAllCandles()
        {
            foreach (SafeCandleData candle in candleDataList)
            {
                candle.isCurrentlyActive = true;
                if (candle.candleLight != null)
                {
                    candle.candleLight.enabled = true;
                }
                if (candle.candleGameObject != null)
                {
                    Renderer renderer = candle.candleGameObject.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Material newMaterial = new Material(Shader.Find("Standard"));
                        newMaterial.color = new Color(1f, 0.9f, 0.3f);
                        renderer.material = newMaterial;
                    }
                }
            }
            
            SetSafePlayerLevel(1);
            SafeLog("[调试] 重置所有烛火");
        }
        
        /// <summary>
        /// 调试：切换调试UI
        /// </summary>
        private void DebugToggleDebugUI()
        {
            enableDebugLogging = !enableDebugLogging;
            if (debugInfoUI != null)
            {
                debugInfoUI.SetActive(enableDebugLogging);
            }
            SafeLog($"[调试] 调试UI: {(enableDebugLogging ? "开启" : "关闭")}");
        }
        
        /// <summary>
        /// 调试：记录系统状态
        /// </summary>
        private void DebugLogSystemStatus()
        {
            LogSystemStatus();
        }
        
        /// <summary>
        /// 记录系统状态
        /// </summary>
        private void LogSystemStatus()
        {
            SafeLog("=== 系统状态报告 ===");
            SafeLog($"初始化状态: {systemInitialized}");
            SafeLog($"Player组件: Transform={playerTransformRef != null}, Level={playerLevelSystemRef != null}, Attack={playerAttackSystemRef != null}, Animator={playerAnimatorRef != null}");
            SafeLog($"UI组件: Canvas={safeUICanvas != null}, PromptUI={mainUpgradePromptUI != null}, LevelUI={levelDisplayUI != null}");
            SafeLog($"烛火数量: {candleDataList.Count} (激活: {GetActiveCandleCount()})");
            SafeLog($"当前等级: {GetSafePlayerLevel()}");
            SafeLog("==================");
        }
        
        /// <summary>
        /// 绘制交互范围
        /// </summary>
        private void DrawInteractionGizmos()
        {
            foreach (SafeCandleData candle in candleDataList)
            {
                if (candle.isCurrentlyActive)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(candle.worldPosition, interactionRange);
                    
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(candle.worldPosition, 0.2f);
                }
            }
            
            if (playerTransformRef != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(playerTransformRef.position, interactionRange);
            }
        }
        #endregion
        
        #region 日志系统
        /// <summary>
        /// 安全日志
        /// </summary>
        private void SafeLog(string message)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"<color=cyan>[SafeCandle] {message}</color>");
            }
        }
        
        /// <summary>
        /// 安全警告日志
        /// </summary>
        private void SafeLogWarning(string message)
        {
            if (enableDebugLogging)
            {
                Debug.LogWarning($"<color=yellow>[SafeCandle] {message}</color>");
            }
        }
        
        /// <summary>
        /// 安全错误日志
        /// </summary>
        private void SafeLogError(string message)
        {
            Debug.LogError($"<color=red>[SafeCandle] {message}</color>");
        }
        #endregion
        
        #region 公共接口
        /// <summary>
        /// 运行时添加新烛火（公共API）
        /// </summary>
        public void AddRuntimeCandle(int requiredLevel, int targetLevel, Vector3 position, string description, CandleUpgradeType upgradeType)
        {
            SafeCandleData newCandle = new SafeCandleData(requiredLevel, targetLevel, position, description, upgradeType);
            CreateCandleGameObject(newCandle);
            candleDataList.Add(newCandle);
            SafeLog($"运行时添加烛火: Lv{requiredLevel}→Lv{targetLevel} at {position}");
        }
        
        /// <summary>
        /// 获取系统初始化状态
        /// </summary>
        public bool IsSystemInitialized()
        {
            return systemInitialized;
        }
        
        /// <summary>
        /// 获取当前激活烛火数量
        /// </summary>
        public int GetActiveCandleCountPublic()
        {
            return GetActiveCandleCount();
        }
        #endregion
    }
    
    /// <summary>
    /// 烛火标识符组件 - 用于标识烛火对象
    /// </summary>
    public class SafeCandleIdentifier : MonoBehaviour
    {
        private SafeCandleInteractionManager.SafeCandleData candleData;
        
        public void SetCandleData(SafeCandleInteractionManager.SafeCandleData data)
        {
            candleData = data;
        }
        
        public SafeCandleInteractionManager.SafeCandleData GetCandleData()
        {
            return candleData;
        }
    }
}
