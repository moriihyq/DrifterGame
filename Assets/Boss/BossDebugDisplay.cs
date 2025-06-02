using UnityEngine;
using System.Collections;

using UnityEngine.UI;
// 用于在游戏运行时显示Boss状态信息的调试组件
public class BossDebugDisplay : MonoBehaviour
{
    private BossController bossController;
    private string displayText = "";
    private bool showGUI = true;
    
    void Start()
    {
        bossController = GetComponent<BossController>();
        if (bossController == null)
        {
            Debug.LogError("BossDebugDisplay需要附加在带有BossController的游戏对象上！");
            enabled = false;
        }
    }
    
    void Update()
    {
        // 按下Tab键切换调试显示
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            showGUI = !showGUI;
        }
        
        // 使用反射来获取私有字段的值
        if (bossController != null)
        {
            System.Type type = bossController.GetType();
            bool isActive = (bool)type.GetField("isActive", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(bossController);
            bool isAttacking = (bool)type.GetField("isAttacking", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(bossController);
            bool initialAttackPerformed = (bool)type.GetField("initialAttackPerformed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(bossController);
            float distanceToPlayer = (float)type.GetField("distanceToPlayer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(bossController);
            int currentHealth = (int)type.GetField("currentHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(bossController);
            int maxHealth = (int)type.GetField("maxHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(bossController);
            GameObject player = (GameObject)type.GetField("player", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(bossController);
            
            // 获取玩家位置（如果存在）
            string playerPos = player != null ? player.transform.position.ToString() : "未找到";
            
            // 构建显示文本
            displayText = $"Boss状态监控：\n" +
                          $"激活状态(isActive): {isActive}\n" +
                          $"攻击中(isAttacking): {isAttacking}\n" +
                          $"已执行初始攻击: {initialAttackPerformed}\n" +
                          $"与玩家距离: {distanceToPlayer:F2}\n" +
                          $"生命值: {currentHealth}/{maxHealth}\n" +
                          $"玩家引用: {(player != null ? "有效" : "无效")}\n" +
                          $"玩家位置: {playerPos}\n" +
                          $"Boss位置: {transform.position}\n" +
                          $"当前速度: {GetComponent<Rigidbody2D>()?.linearVelocity.ToString() ?? "N/A"}\n" +
                          $"\n按[Tab]键隐藏此调试信息";
        }
    }
    
    void OnGUI()
    {
        if (showGUI && bossController != null)
        {
            // 在屏幕右上角显示状态信息
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.yellow;
            style.fontSize = 14;
            style.alignment = TextAnchor.UpperRight;
            style.wordWrap = true;
            
            GUI.Label(new Rect(Screen.width - 250, 10, 240, 400), displayText, style);
        }
    }
}
