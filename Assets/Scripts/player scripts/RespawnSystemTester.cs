using UnityEngine;

/// <summary>
/// 复活系统测试器 - 用于测试复活点系统的各项功能
/// </summary>
public class RespawnSystemTester : MonoBehaviour
{
    [Header("测试设置")]
    [Tooltip("是否在开始时自动查找玩家")]
    public bool autoFindPlayer = true;
    
    [Tooltip("测试用的玩家对象")]
    public GameObject testPlayer;
    
    [Header("测试按键")]
    [Tooltip("测试玩家死亡的按键")]
    public KeyCode testDeathKey = KeyCode.K;
    
    [Tooltip("测试强制复活的按键")]
    public KeyCode testRespawnKey = KeyCode.R;
    
    [Tooltip("测试掉落边界的按键")]
    public KeyCode testFallKey = KeyCode.F;
    
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private string playerStatus = "未找到";
    [SerializeField] private string managerStatus = "未找到";
    [SerializeField] private int respawnPointCount = 0;
    [SerializeField] private int activatedRespawnPoints = 0;
    
    private PlayerAttackSystem playerAttackSystem;
    private PlayerController playerController;
    
    void Start()
    {
        if (autoFindPlayer)
        {
            FindTestPlayer();
        }
        
        UpdateDebugInfo();
    }
    
    void Update()
    {
        // 更新调试信息
        UpdateDebugInfo();
        
        // 处理测试按键
        HandleTestInput();
    }
    
    /// <summary>
    /// 查找测试用的玩家
    /// </summary>
    void FindTestPlayer()
    {
        if (testPlayer == null)
        {
            // 尝试通过PlayerAttackSystem找到玩家
            playerAttackSystem = FindFirstObjectByType<PlayerAttackSystem>();
            if (playerAttackSystem != null)
            {
                testPlayer = playerAttackSystem.gameObject;
            }
            else
            {
                // 尝试通过PlayerController找到玩家
                playerController = FindFirstObjectByType<PlayerController>();
                if (playerController != null)
                {
                    testPlayer = playerController.gameObject;
                }
                else
                {
                    // 尝试通过标签找到玩家
                    GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                    if (playerObj != null)
                    {
                        testPlayer = playerObj;
                    }
                }
            }
        }
        
        // 获取玩家组件
        if (testPlayer != null)
        {
            playerAttackSystem = testPlayer.GetComponent<PlayerAttackSystem>();
            playerController = testPlayer.GetComponent<PlayerController>();
        }
    }
    
    /// <summary>
    /// 处理测试输入
    /// </summary>
    void HandleTestInput()
    {
        // 测试玩家死亡
        if (Input.GetKeyDown(testDeathKey))
        {
            TestPlayerDeath();
        }
        
        // 测试强制复活
        if (Input.GetKeyDown(testRespawnKey))
        {
            TestForceRespawn();
        }
        
        // 测试掉落边界
        if (Input.GetKeyDown(testFallKey))
        {
            TestFallOutOfBounds();
        }
    }
    
    /// <summary>
    /// 测试玩家死亡
    /// </summary>
    public void TestPlayerDeath()
    {
        if (playerAttackSystem != null)
        {
            int currentHealth = playerAttackSystem.GetCurrentHealth();
            playerAttackSystem.TakeDamage(currentHealth); // 造成致命伤害
            
            if (showDebugInfo)
            {
                Debug.Log($"<color=red>[RespawnSystemTester] 测试玩家死亡，造成 {currentHealth} 点伤害</color>");
            }
        }
        else if (playerController != null)
        {
            playerController.TakeDamage(playerController.currentHealth); // 造成致命伤害
            
            if (showDebugInfo)
            {
                Debug.Log($"<color=red>[RespawnSystemTester] 测试玩家死亡（通过PlayerController）</color>");
            }
        }
        else
        {
            Debug.LogWarning("[RespawnSystemTester] 找不到玩家组件进行死亡测试！");
        }
    }
    
    /// <summary>
    /// 测试强制复活
    /// </summary>
    public void TestForceRespawn()
    {
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.TriggerRespawn();
            
            if (showDebugInfo)
            {
                Debug.Log("<color=cyan>[RespawnSystemTester] 强制触发复活</color>");
            }
        }
        else
        {
            Debug.LogWarning("[RespawnSystemTester] 找不到RespawnManager进行复活测试！");
        }
    }
    
    /// <summary>
    /// 测试掉落边界
    /// </summary>
    public void TestFallOutOfBounds()
    {
        if (testPlayer != null && RespawnManager.Instance != null)
        {
            // 将玩家移动到边界以下
            Vector3 currentPos = testPlayer.transform.position;
            float boundary = RespawnManager.Instance.mapBottomBoundary;
            testPlayer.transform.position = new Vector3(currentPos.x, boundary - 1f, currentPos.z);
            
            if (showDebugInfo)
            {
                Debug.Log($"<color=orange>[RespawnSystemTester] 测试掉落边界，移动玩家到 Y={boundary - 1f}</color>");
            }
        }
        else
        {
            Debug.LogWarning("[RespawnSystemTester] 找不到玩家或RespawnManager进行边界测试！");
        }
    }
    
    /// <summary>
    /// 更新调试信息
    /// </summary>
    void UpdateDebugInfo()
    {
        // 更新玩家状态
        if (testPlayer != null)
        {
            string healthInfo = "";
            if (playerAttackSystem != null)
            {
                healthInfo = $" 血量:{playerAttackSystem.GetCurrentHealth()}/{playerAttackSystem.GetMaxHealth()}";
            }
            else if (playerController != null)
            {
                healthInfo = $" 血量:{playerController.currentHealth}/{playerController.maxHealth}";
            }
            
            playerStatus = $"已找到 - {testPlayer.name}{healthInfo}";
        }
        else
        {
            playerStatus = "未找到";
        }
        
        // 更新管理器状态
        if (RespawnManager.Instance != null)
        {
            managerStatus = "已找到";
            string stats = RespawnManager.Instance.GetRespawnPointStats();
            string[] parts = stats.Split('，');
            if (parts.Length >= 2)
            {
                // 解析复活点统计信息
                try
                {
                    respawnPointCount = int.Parse(parts[0].Split(' ')[1]);
                    activatedRespawnPoints = int.Parse(parts[1].Split(' ')[1]);
                }
                catch
                {
                    respawnPointCount = 0;
                    activatedRespawnPoints = 0;
                }
            }
        }
        else
        {
            managerStatus = "未找到";
            respawnPointCount = 0;
            activatedRespawnPoints = 0;
        }
    }
    
    /// <summary>
    /// 在Inspector中显示帮助信息
    /// </summary>
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("复活系统测试器", GUI.skin.box);
        GUILayout.Label($"按 {testDeathKey} 键测试玩家死亡");
        GUILayout.Label($"按 {testRespawnKey} 键强制复活");
        GUILayout.Label($"按 {testFallKey} 键测试掉落边界");
        GUILayout.Label($"复活点: {activatedRespawnPoints}/{respawnPointCount}");
        GUILayout.EndArea();
    }
    
    /// <summary>
    /// Inspector右键菜单测试方法
    /// </summary>
    [ContextMenu("测试玩家死亡")]
    void ContextTestDeath()
    {
        TestPlayerDeath();
    }
    
    [ContextMenu("测试强制复活")]
    void ContextTestRespawn()
    {
        TestForceRespawn();
    }
    
    [ContextMenu("测试掉落边界")]
    void ContextTestFall()
    {
        TestFallOutOfBounds();
    }
    
    [ContextMenu("查找玩家")]
    void ContextFindPlayer()
    {
        FindTestPlayer();
        UpdateDebugInfo();
    }
} 