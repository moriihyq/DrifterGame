using UnityEngine;

// 血瓶设置检查器，自动检查和修复血瓶系统设置
public class HealthPotionSetupChecker : MonoBehaviour
{
    [Header("自动检查设置")]
    public bool autoFixIssues = true; // 是否自动修复问题
    
    void Start()
    {
        CheckHealthPotionSetup();
    }
    
    private void CheckHealthPotionSetup()
    {
        Debug.Log("<color=cyan>=== 血瓶系统设置检查 ===</color>");
        
        // 检查玩家设置
        CheckPlayerSetup();
        
        // 检查场景中的血瓶
        CheckHealthPotions();
        
        Debug.Log("<color=cyan>=== 血瓶系统检查完成 ===</color>");
    }
    
    private void CheckPlayerSetup()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("<color=red>错误: 场景中没有找到带'Player'标签的玩家对象！</color>");
            return;
        }
        
        Debug.Log("<color=green>✓ 找到玩家对象: " + player.name + "</color>");
        
        // 检查PlayerAttackSystem组件
        PlayerAttackSystem pas = player.GetComponent<PlayerAttackSystem>();
        if (pas == null)
        {
            Debug.LogError("<color=red>错误: 玩家对象没有PlayerAttackSystem组件！</color>");
        }
        else
        {
            Debug.Log("<color=green>✓ 玩家具有PlayerAttackSystem组件</color>");
        }
        
        // 检查Animator组件
        Animator anim = player.GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogWarning("<color=yellow>警告: 玩家对象没有Animator组件，无法播放喝药动画</color>");
        }
        else
        {
            Debug.Log("<color=green>✓ 玩家具有Animator组件</color>");
            
            // 检查动画参数
            bool hasDrinkTrigger = false;
            foreach (AnimatorControllerParameter param in anim.parameters)
            {
                if (param.name == "dring-potion" && param.type == AnimatorControllerParameterType.Trigger)
                {
                    hasDrinkTrigger = true;
                    break;
                }
            }
            
            if (hasDrinkTrigger)
            {
                Debug.Log("<color=green>✓ 动画控制器包含'dring-potion'触发器</color>");
            }
            else
            {
                Debug.LogWarning("<color=yellow>警告: 动画控制器缺少'dring-potion'触发器参数</color>");
            }
        }
    }
    
    private void CheckHealthPotions()
    {
        HealthPotion[] potions = FindObjectsOfType<HealthPotion>();
        
        if (potions.Length == 0)
        {
            Debug.LogWarning("<color=yellow>场景中没有找到血瓶对象</color>");
            return;
        }
        
        Debug.Log($"<color=cyan>找到 {potions.Length} 个血瓶对象</color>");
        
        foreach (HealthPotion potion in potions)
        {
            CheckSinglePotion(potion);
        }
    }
    
    private void CheckSinglePotion(HealthPotion potion)
    {
        Debug.Log($"<color=cyan>检查血瓶: {potion.name}</color>");
        
        // 检查碰撞器
        Collider2D collider = potion.GetComponent<Collider2D>();
        if (collider == null)
        {
            Debug.LogWarning($"<color=yellow>警告: 血瓶 {potion.name} 没有碰撞器，HealthPotion脚本会自动添加</color>");
        }
        else if (!collider.isTrigger)
        {
            Debug.LogWarning($"<color=yellow>警告: 血瓶 {potion.name} 的碰撞器不是Trigger</color>");
            if (autoFixIssues)
            {
                collider.isTrigger = true;
                Debug.Log($"<color=green>✓ 已自动修复: 将 {potion.name} 的碰撞器设为Trigger</color>");
            }
        }
        else
        {
            Debug.Log($"<color=green>✓ 血瓶 {potion.name} 碰撞器设置正确</color>");
        }
        
        // 检查SpriteRenderer
        SpriteRenderer sr = potion.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogWarning($"<color=yellow>警告: 血瓶 {potion.name} 没有SpriteRenderer，建议添加以显示血瓶图像</color>");
        }
        else if (sr.sprite == null)
        {
            Debug.LogWarning($"<color=yellow>警告: 血瓶 {potion.name} 的SpriteRenderer没有设置精灵图片</color>");
        }
        else
        {
            Debug.Log($"<color=green>✓ 血瓶 {potion.name} 具有正确的SpriteRenderer设置</color>");
        }
    }
    
    // 在编辑器中添加检查按钮
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 30), "重新检查血瓶系统"))
        {
            CheckHealthPotionSetup();
        }
        
        if (GUI.Button(new Rect(10, 50, 200, 30), "创建测试血瓶"))
        {
            CreateTestPotion();
        }
    }
    
    private void CreateTestPotion()
    {
        // 在玩家附近创建一个测试血瓶
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("无法创建测试血瓶：找不到玩家对象");
            return;
        }
        
        GameObject testPotion = new GameObject("TestHealthPotion");
        testPotion.transform.position = player.transform.position + Vector3.right * 3f;
        
        // 添加组件
        testPotion.AddComponent<HealthPotion>();
        
        SpriteRenderer sr = testPotion.AddComponent<SpriteRenderer>();
        sr.color = Color.red; // 红色方块作为临时图像
        
        // 创建简单的正方形精灵
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.red;
        }
        texture.SetPixels(colors);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        sr.sprite = sprite;
        
        Debug.Log("<color=green>✓ 已在玩家右侧创建测试血瓶</color>");
    }
}
