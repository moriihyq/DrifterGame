using UnityEngine;

public class DebugPlayerDamage : MonoBehaviour
{
    [Header("调试设置")]
    [SerializeField] private bool enableDebug = true;
    [SerializeField] private KeyCode testDamageKey = KeyCode.T; // 按T键测试伤害
    
    private GameObject player;
    private PlayerAttackSystem playerAttackSystem;
    
    void Start()
    {
        if (!enableDebug) return;
        
        // 查找玩家对象的多种方式
        Debug.Log("=== 开始诊断玩家扣血问题 ===");
        
        // 方式1：通过标签查找
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Debug.Log($"✓ 通过标签找到玩家: {player.name}");
            Debug.Log($"  玩家位置: {player.transform.position}");
            Debug.Log($"  玩家标签: {player.tag}");
            Debug.Log($"  玩家层级: {LayerMask.LayerToName(player.layer)}");
        }
        else
        {
            Debug.LogError("✗ 通过标签未找到玩家对象！请确保玩家对象标签设置为'Player'");
            
            // 方式2：通过名字查找
            player = GameObject.Find("Player");
            if (player != null)
            {
                Debug.LogWarning($"⚠ 通过名字找到玩家: {player.name}，但标签可能未正确设置");
            }
        }
        
        // 检查PlayerAttackSystem组件
        if (player != null)
        {
            playerAttackSystem = player.GetComponent<PlayerAttackSystem>();
            if (playerAttackSystem != null)
            {
                Debug.Log($"✓ 找到PlayerAttackSystem组件");
                Debug.Log($"  当前血量: {playerAttackSystem.Health}");
                Debug.Log($"  最大血量: {playerAttackSystem.MaxHealth}");
            }
            else
            {
                Debug.LogError("✗ 玩家对象没有PlayerAttackSystem组件！");
                
                // 检查其他可能的血量组件
                var allComponents = player.GetComponents<MonoBehaviour>();
                Debug.Log($"玩家身上的所有组件:");
                foreach (var comp in allComponents)
                {
                    Debug.Log($"  - {comp.GetType().Name}");
                }
            }
        }
        
        // 检查场景中的敌人
        CheckEnemies();
    }
    
    void Update()
    {
        if (!enableDebug) return;
        
        // 按T键测试伤害
        if (Input.GetKeyDown(testDamageKey))
        {
            TestPlayerDamage();
        }
    }
    
    private void CheckEnemies()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        Debug.Log($"场景中找到 {enemies.Length} 个敌人:");
        
        foreach (Enemy enemy in enemies)
        {
            Debug.Log($"  敌人: {enemy.name}");
            Debug.Log($"    位置: {enemy.transform.position}");
            Debug.Log($"    与玩家距离: {(player != null ? Vector2.Distance(player.transform.position, enemy.transform.position) : -1)}");
            Debug.Log($"    是否激活: {enemy.distanceToPlayer}");
        }
    }
    
    private void TestPlayerDamage()
    {
        if (playerAttackSystem == null)
        {
            Debug.LogError("无法测试伤害：PlayerAttackSystem组件不存在");
            return;
        }
        
        int beforeHealth = playerAttackSystem.Health;
        Debug.Log($"测试伤害前血量: {beforeHealth}");
        
        // 直接调用TakeDamage方法测试
        playerAttackSystem.TakeDamage(10);
        
        int afterHealth = playerAttackSystem.Health;
        Debug.Log($"测试伤害后血量: {afterHealth}");
        
        if (beforeHealth != afterHealth)
        {
            Debug.Log("✓ 玩家TakeDamage方法工作正常");
        }
        else
        {
            Debug.LogError("✗ 玩家TakeDamage方法可能有问题");
        }
    }
    
    // 在编辑器中绘制调试信息
    void OnGUI()
    {
        // 图形化界面已禁用 - 如需重新启用，将下面的return注释掉
        return;
        
        if (!enableDebug) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("=== 玩家扣血调试信息 ===");
        
        if (player != null)
        {
            GUILayout.Label($"玩家位置: {player.transform.position}");
            if (playerAttackSystem != null)
            {
                GUILayout.Label($"血量: {playerAttackSystem.Health}/{playerAttackSystem.MaxHealth}");
            }
        }
        else
        {
            GUILayout.Label("未找到玩家对象！");
        }
        
        GUILayout.Label($"按 {testDamageKey} 键测试伤害");
        
        GUILayout.EndArea();
    }
} 