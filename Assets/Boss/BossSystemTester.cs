using UnityEngine;

[System.Serializable]
public class BossSystemTester : MonoBehaviour
{
    [Header("测试配置")]
    public KeyCode testActivationKey = KeyCode.T;
    public KeyCode testDamageKey = KeyCode.Y;
    public KeyCode testTrackingKey = KeyCode.U;
    
    [Header("引用组件")]
    public BossController bossController;
    public GameObject player;
    
    [Header("测试结果")]
    public bool lastActivationTest = false;
    public bool lastDamageTest = false;
    public bool lastTrackingTest = false;
    
    void Start()
    {
        // 自动查找Boss和玩家
        if (bossController == null)
        {
            bossController = FindObjectOfType<BossController>();
        }
        
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        
        Debug.Log("<color=cyan>Boss系统测试器已初始化</color>");
        Debug.Log("<color=yellow>按键说明:</color>");
        Debug.Log($"<color=white>T键: 测试Boss激活</color>");
        Debug.Log($"<color=white>Y键: 测试Boss伤害</color>");
        Debug.Log($"<color=white>U键: 测试Boss追踪</color>");
    }
    
    void Update()
    {
        // 测试Boss激活
        if (Input.GetKeyDown(testActivationKey))
        {
            TestBossActivation();
        }
        
        // 测试Boss伤害
        if (Input.GetKeyDown(testDamageKey))
        {
            TestBossDamage();
        }
        
        // 测试Boss追踪
        if (Input.GetKeyDown(testTrackingKey))
        {
            TestBossTracking();
        }
    }
    
    public void TestBossActivation()
    {
        Debug.Log("<color=cyan>===== 测试Boss激活 =====</color>");
        
        if (bossController == null)
        {
            Debug.LogError("<color=red>找不到BossController！</color>");
            lastActivationTest = false;
            return;
        }
        
        // 模拟按下P键
        bool wasActive = bossController.IsActive;
        
        // 强制激活Boss（如果还没激活的话）
        if (!bossController.IsActive)
        {
            Debug.Log("<color=yellow>强制激活Boss...</color>");
            bossController.ActivateBoss();
        }
        
        lastActivationTest = bossController.IsActive;
        
        if (lastActivationTest)
        {
            Debug.Log("<color=lime>✓ Boss激活测试成功</color>");
        }
        else
        {
            Debug.LogError("<color=red>✗ Boss激活测试失败</color>");
        }
    }
    
    public void TestBossDamage()
    {
        Debug.Log("<color=cyan>===== 测试Boss伤害系统 =====</color>");
        
        if (bossController == null)
        {
            Debug.LogError("<color=red>找不到BossController！</color>");
            lastDamageTest = false;
            return;
        }
        
        if (player == null)
        {
            Debug.LogError("<color=red>找不到玩家对象！</color>");
            lastDamageTest = false;
            return;
        }
        
        // 获取玩家当前血量
        PlayerAttackSystem playerHealth = player.GetComponent<PlayerAttackSystem>();
        if (playerHealth == null)
        {
            Debug.LogError("<color=red>玩家对象上找不到PlayerAttackSystem组件！</color>");
            lastDamageTest = false;
            return;
        }
        
        int healthBefore = playerHealth.Health;
        Debug.Log($"<color=white>玩家当前血量: {healthBefore}</color>");
        
        // 测试伤害
        bossController.TestDamagePlayer(10);
        
        int healthAfter = playerHealth.Health;
        Debug.Log($"<color=white>伤害后血量: {healthAfter}</color>");
        
        lastDamageTest = (healthAfter < healthBefore);
        
        if (lastDamageTest)
        {
            Debug.Log($"<color=lime>✓ Boss伤害测试成功，造成了 {healthBefore - healthAfter} 点伤害</color>");
        }
        else
        {
            Debug.LogError("<color=red>✗ Boss伤害测试失败，玩家血量没有变化</color>");
        }
    }
    
    public void TestBossTracking()
    {
        Debug.Log("<color=cyan>===== 测试Boss追踪系统 =====</color>");
        
        if (bossController == null)
        {
            Debug.LogError("<color=red>找不到BossController！</color>");
            lastTrackingTest = false;
            return;
        }
        
        if (player == null)
        {
            Debug.LogError("<color=red>找不到玩家对象！</color>");
            lastTrackingTest = false;
            return;
        }
        
        // 确保Boss已激活
        if (!bossController.IsActive)
        {
            bossController.ActivateBoss();
        }
        
        Vector3 bossPosition = bossController.transform.position;
        Vector3 playerPosition = player.transform.position;
        
        Debug.Log($"<color=white>Boss位置: {bossPosition}</color>");
        Debug.Log($"<color=white>玩家位置: {playerPosition}</color>");
        
        float distance = Vector3.Distance(bossPosition, playerPosition);
        Debug.Log($"<color=white>距离: {distance}</color>");
        
        // 等待几帧，然后检查Boss是否移动了
        StartCoroutine(CheckBossMovement(bossPosition));
    }
    
    private System.Collections.IEnumerator CheckBossMovement(Vector3 initialPosition)
    {
        yield return new WaitForSeconds(2f);
        
        Vector3 newPosition = bossController.transform.position;
        float moveDistance = Vector3.Distance(initialPosition, newPosition);
        
        Debug.Log($"<color=white>Boss移动距离: {moveDistance}</color>");
        
        lastTrackingTest = (moveDistance > 0.1f); // 如果移动距离大于0.1，认为追踪正常
        
        if (lastTrackingTest)
        {
            Debug.Log("<color=lime>✓ Boss追踪测试成功，Boss有移动</color>");
        }
        else
        {
            Debug.LogWarning("<color=orange>? Boss追踪测试：Boss没有明显移动，可能已经接近玩家或遇到障碍</color>");
        }
    }
    
    void OnGUI()
    {
        // 在屏幕上显示测试按钮和结果
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        
        GUILayout.Label("<b>Boss系统测试器</b>");
        
        if (GUILayout.Button($"测试Boss激活 [{testActivationKey}]"))
        {
            TestBossActivation();
        }
        
        if (GUILayout.Button($"测试Boss伤害 [{testDamageKey}]"))
        {
            TestBossDamage();
        }
        
        if (GUILayout.Button($"测试Boss追踪 [{testTrackingKey}]"))
        {
            TestBossTracking();
        }
        
        GUILayout.Space(10);
        GUILayout.Label("<b>测试结果:</b>");
        GUILayout.Label($"激活: {(lastActivationTest ? "✓" : "✗")}");
        GUILayout.Label($"伤害: {(lastDamageTest ? "✓" : "✗")}");
        GUILayout.Label($"追踪: {(lastTrackingTest ? "✓" : "?")}");
        
        GUILayout.EndArea();
    }
}
