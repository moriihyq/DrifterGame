using UnityEngine;

// Boss调试类，用于在游戏运行时检查和修复Boss攻击问题
public class BossAttackDebugger : MonoBehaviour
{
    private BossController bossController;
    
    void Start()
    {
        bossController = GetComponent<BossController>();
        if (bossController == null)
        {
            Debug.LogError("BossAttackDebugger需要放在有BossController组件的游戏对象上！");
            enabled = false;
            return;
        }
        
        // 运行时检查玩家碰撞设置
        CheckPlayerColliderSettings();
    }
    
    // 检查玩家碰撞体设置
    private void CheckPlayerColliderSettings()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("<color=red>无法找到带有Player标签的对象！</color>");
            return;
        }
        
        // 检查玩家是否有碰撞体
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider == null)
        {
            Debug.LogError("<color=red>玩家没有Collider2D组件！这会导致Boss无法攻击到玩家。</color>");
            
            // 尝试在子对象中查找
            playerCollider = player.GetComponentInChildren<Collider2D>();
            if (playerCollider != null)
            {
                Debug.Log("<color=yellow>在玩家的子对象上找到了碰撞体，但建议将碰撞体放在主对象上。</color>");
            }
            else
            {
                Debug.LogError("<color=red>玩家及其子对象都没有Collider2D！请添加碰撞体。</color>");
                
                // 自动添加碰撞体
                BoxCollider2D newCollider = player.AddComponent<BoxCollider2D>();
                newCollider.size = new Vector2(1f, 2f);
                newCollider.offset = new Vector2(0f, 1f);
                Debug.Log("<color=green>为玩家自动添加了BoxCollider2D</color>");
            }
        }
        
        // 检查玩家的层设置
        if (player.layer == 0) // Default层
        {
            Debug.LogWarning("<color=yellow>警告：玩家在Default层。建议创建专门的Player层以便正确检测。</color>");
            
            // 检查是否存在Player层
            int playerLayerIndex = LayerMask.NameToLayer("Player");
            if (playerLayerIndex != -1)
            {
                player.layer = playerLayerIndex;
                Debug.Log("<color=green>已将玩家设置为Player层</color>");
                
                // 同时设置所有子对象
                foreach (Transform child in player.transform)
                {
                    child.gameObject.layer = playerLayerIndex;
                }
            }
            else
            {
                Debug.LogError("<color=red>项目中不存在Player层！无法自动设置层。</color>");
            }
        }
        
        // 检查玩家是否有PlayerAttackSystem组件
        PlayerAttackSystem pas = player.GetComponent<PlayerAttackSystem>();
        if (pas == null)
        {
            Debug.LogError("<color=red>玩家没有PlayerAttackSystem组件！Boss无法对其造成伤害。</color>");
            
            // 检查子对象
            pas = player.GetComponentInChildren<PlayerAttackSystem>();
            if (pas != null)
            {
                Debug.Log("<color=yellow>在玩家的子对象上找到了PlayerAttackSystem组件</color>");
            }
        }
        else
        {
            Debug.Log("<color=green>玩家具有正常的PlayerAttackSystem组件</color>");
        }
    }
    
    void OnGUI()
    {
        // 图形化界面已禁用 - 如需重新启用，将下面的return注释掉
        return;
        
        int btnWidth = 180;
        int btnHeight = 30;
        int margin = 10;
        int startY = 50;
        
        // 诊断按钮
        if (GUI.Button(new Rect(Screen.width - btnWidth - margin, startY, btnWidth, btnHeight), "诊断玩家碰撞设置"))
        {
            CheckPlayerColliderSettings();
        }
        
        // 强制检测伤害按钮 (近战)
        if (GUI.Button(new Rect(Screen.width - btnWidth - margin, startY + btnHeight + margin, btnWidth, btnHeight), "Boss近战攻击测试"))
        {
            // 通过反射调用Boss的近战攻击方法
            System.Type bossType = bossController.GetType();
            System.Reflection.MethodInfo method = bossType.GetMethod("ApplyMeleeAttackDamage", 
                                                                    System.Reflection.BindingFlags.NonPublic | 
                                                                    System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                method.Invoke(bossController, null);
                Debug.Log("<color=cyan>已强制执行Boss近战攻击判定</color>");
            }
        }
        
        // 强制检测伤害按钮 (远程)
        if (GUI.Button(new Rect(Screen.width - btnWidth - margin, startY + (btnHeight + margin) * 2, btnWidth, btnHeight), "Boss远程攻击测试"))
        {
            // 通过反射调用Boss的远程攻击方法  
            System.Type bossType = bossController.GetType();
            System.Reflection.MethodInfo method = bossType.GetMethod("ApplyRangedAttackDamage", 
                                                                    System.Reflection.BindingFlags.NonPublic | 
                                                                    System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                method.Invoke(bossController, null);
                Debug.Log("<color=cyan>已强制执行Boss远程攻击判定</color>");
            }
        }
        
        // 直接伤害测试按钮
        if (GUI.Button(new Rect(Screen.width - btnWidth - margin, startY + (btnHeight + margin) * 3, btnWidth, btnHeight), "直接伤害玩家 (10点)"))
        {
            bossController.TestDamagePlayer(10);
        }
    }
}
