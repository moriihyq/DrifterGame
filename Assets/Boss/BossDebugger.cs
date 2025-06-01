using UnityEngine;
using System.Collections;

/// <summary>
/// Boss调试工具，用于测试Boss的激活、追踪和攻击行为
/// </summary>
public class BossDebugger : MonoBehaviour
{
    [Header("调试设置")]
    public bool enableDebug = true;
    public KeyCode activateBossKey = KeyCode.P;
    public KeyCode forceTrackingKey = KeyCode.T;
    public KeyCode forceAttackKey = KeyCode.A;
    public KeyCode showStateKey = KeyCode.S;
    public Color debugLineColor = Color.red;
    
    // 依赖组件
    private BossController bossController;
    private Rigidbody2D rb;
    private Transform meleeAttackPoint;
    private bool isForceTracking = false;
    
    // 调试信息
    private Vector2 lastVelocity;
    private float distanceToPlayer;
    private bool wasKeyPressed = false;
    private bool isActive = false;
    
    void Start()
    {
        // 获取组件引用
        bossController = GetComponent<BossController>();
        rb = GetComponent<Rigidbody2D>();
        
        if (bossController == null)
        {
            Debug.LogError("找不到BossController组件！");
            enabled = false;
            return;
        }
        
        // 获取Boss的私有字段 (使用反射)
        try
        {
            var field = bossController.GetType().GetField("meleeAttackPoint", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
            
            if (field != null)
            {
                meleeAttackPoint = (Transform)field.GetValue(bossController);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"无法访问meleeAttackPoint: {e.Message}");
        }
        
        Debug.Log("<color=#00FF00>【Boss调试器】已初始化。按P激活Boss，按T强制追踪，按A强制攻击，按S显示状态</color>");
    }
    
    void Update()
    {
        if (!enableDebug) return;
        
        // 检测玩家按键
        CheckInputs();
        
        // 获取并显示状态
        UpdateDebugInfo();
        
        // 强制追踪逻辑
        if (isForceTracking)
        {
            ForceTracking();
        }
    }
    
    // 检查调试键输入
    private void CheckInputs()
    {
        // 检查激活Boss的P键
        if (Input.GetKeyDown(activateBossKey))
        {
            wasKeyPressed = true;
            StartCoroutine(CheckActivation());
            Debug.Log("<color=#FF00FF>【Boss调试器】P键被按下，尝试激活Boss</color>");
        }
        
        // 切换强制追踪模式
        if (Input.GetKeyDown(forceTrackingKey))
        {
            isForceTracking = !isForceTracking;
            Debug.Log($"<color=#00FFFF>【Boss调试器】强制追踪模式: {(isForceTracking ? "开启" : "关闭")}</color>");
        }
        
        // 强制执行攻击
        if (Input.GetKeyDown(forceAttackKey))
        {
            ForceAttack();
        }
        
        // 显示当前状态
        if (Input.GetKeyDown(showStateKey))
        {
            ShowCurrentState();
        }
    }
    
    // 检查Boss是否成功激活 (延迟几帧检查)
    private IEnumerator CheckActivation()
    {
        // 等待几帧让BossController处理激活
        yield return new WaitForSeconds(0.1f);
        
        // 检查激活状态
        bool currentActiveState = false;
        
        try
        {
            var field = bossController.GetType().GetField("isActive", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
            
            if (field != null)
            {
                currentActiveState = (bool)field.GetValue(bossController);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"无法访问isActive: {e.Message}");
        }
        
        if (!currentActiveState && wasKeyPressed)
        {
            Debug.LogError("<color=#FF0000>【Boss调试器】警告: P键已按下但Boss未被激活！可能是激活逻辑出现问题。</color>");
            // 尝试手动激活
            Debug.Log("<color=#FF9900>【Boss调试器】尝试通过反射手动激活Boss...</color>");
            TryManualActivation();
        }
        else if (currentActiveState)
        {
            isActive = true;
            Debug.Log("<color=#00FF00>【Boss调试器】Boss成功激活！</color>");
        }
        
        wasKeyPressed = false;
    }
    
    // 更新调试信息
    private void UpdateDebugInfo()
    {
        // 获取与玩家距离
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            distanceToPlayer = player.transform.position.x - transform.position.x;
        }
        
        // 记录速度变化
        if (rb != null)
        {
            if (lastVelocity != rb.linearVelocity)
            {
                if (rb.linearVelocity.magnitude > 0.1f)
                {
                    Debug.Log($"<color=#AAAAFF>【Boss调试器】速度变为: {rb.linearVelocity}</color>");
                }
                lastVelocity = rb.linearVelocity;
            }
        }
    }
    
    // 强制Boss追踪玩家
    private void ForceTracking()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && rb != null)
        {
            float moveDirection = player.transform.position.x > transform.position.x ? 1 : -1;
            rb.linearVelocity = new Vector2(moveDirection * 3f, rb.linearVelocity.y);
            Debug.Log($"<color=#00FFFF>【Boss调试器】强制追踪玩家，位置差: {distanceToPlayer}</color>");
        }
    }
    
    // 强制执行攻击
    private void ForceAttack()
    {
        Debug.Log("<color=#FF9900>【Boss调试器】强制执行攻击</color>");
        
        // 尝试通过反射调用攻击方法
        try
        {
            var method = bossController.GetType().GetMethod("PerformMeleeAttack", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
            
            if (method != null)
            {
                method.Invoke(bossController, null);
                Debug.Log("<color=#00FF00>【Boss调试器】成功执行攻击方法</color>");
            }
            else
            {
                Debug.LogError("<color=#FF0000>【Boss调试器】未找到PerformMeleeAttack方法</color>");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=#FF0000>【Boss调试器】调用攻击方法失败: {e.Message}</color>");
        }
    }
    
    // 尝试手动激活Boss
    private void TryManualActivation()
    {
        try
        {
            // 尝试直接修改isActive字段
            var isActiveField = bossController.GetType().GetField("isActive", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
            
            if (isActiveField != null)
            {
                isActiveField.SetValue(bossController, true);
                Debug.Log("<color=#00FF00>【Boss调试器】已手动设置isActive为true</color>");
            }
            
            // 尝试调用ActivateBoss方法
            var activateMethod = bossController.GetType().GetMethod("ActivateBoss", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
            
            if (activateMethod != null)
            {
                activateMethod.Invoke(bossController, null);
                Debug.Log("<color=#00FF00>【Boss调试器】已手动调用ActivateBoss方法</color>");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=#FF0000>【Boss调试器】手动激活失败: {e.Message}</color>");
        }
    }
    
    // 显示当前状态
    private void ShowCurrentState()
    {
        try
        {
            // 反射获取主要状态变量
            var isActiveField = bossController.GetType().GetField("isActive", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
                
            var isAttackingField = bossController.GetType().GetField("isAttacking", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
                
            var initialAttackPerformedField = bossController.GetType().GetField("initialAttackPerformed", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
                
            var distanceToPlayerField = bossController.GetType().GetField("distanceToPlayer", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
                
            // 获取字段值
            bool bossIsActive = isActiveField != null ? (bool)isActiveField.GetValue(bossController) : false;
            bool bossIsAttacking = isAttackingField != null ? (bool)isAttackingField.GetValue(bossController) : false;
            bool bossInitialAttack = initialAttackPerformedField != null ? 
                (bool)initialAttackPerformedField.GetValue(bossController) : false;
            float bossDistance = distanceToPlayerField != null ? 
                (float)distanceToPlayerField.GetValue(bossController) : 0f;
                
            // 输出状态信息
            Debug.Log("<color=#FFFF00>【Boss状态】</color>");
            Debug.Log($"<color=#FFFF00> - isActive: {bossIsActive}</color>");
            Debug.Log($"<color=#FFFF00> - isAttacking: {bossIsAttacking}</color>");
            Debug.Log($"<color=#FFFF00> - initialAttackPerformed: {bossInitialAttack}</color>");
            Debug.Log($"<color=#FFFF00> - distanceToPlayer: {bossDistance}</color>");
            Debug.Log($"<color=#FFFF00> - velocity: {rb.linearVelocity}</color>");
            
            // 输出玩家信息
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Debug.Log($"<color=#FFFF00> - 玩家位置: {player.transform.position}</color>");
                Debug.Log($"<color=#FFFF00> - Boss位置: {transform.position}</color>");
            }
            else
            {
                Debug.LogError("<color=#FF0000> - 找不到玩家对象！</color>");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=#FF0000>【Boss调试器】显示状态时出错: {e.Message}</color>");
        }
    }
    
    void OnDrawGizmos()
    {
        if (!enableDebug) return;
        
        // 画出到玩家的连线
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Gizmos.color = debugLineColor;
            Gizmos.DrawLine(transform.position, player.transform.position);
        }
        
        // 画出移动方向
        if (rb != null && rb.linearVelocity.magnitude > 0.1f)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, rb.linearVelocity.normalized * 2f);
        }
        
        // 画出攻击范围
        if (meleeAttackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleeAttackPoint.position, 1.5f);
        }
    }
}
