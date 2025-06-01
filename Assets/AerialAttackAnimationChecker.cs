using UnityEngine;

/// <summary>
/// 空中攻击动画设置检查器 - 自动检查和设置玩家动画控制器
/// </summary>
public class AerialAttackAnimationChecker : MonoBehaviour
{
    [Header("动画设置")]
    [SerializeField] private string aerialAttackTriggerName = "Aerial_Attack";
    [SerializeField] private bool autoSetupAnimator = true;
    [SerializeField] private bool showDetailedLogs = true;
    
    [Header("组件引用")]
    [SerializeField] private Animator playerAnimator;
    
    private PlayerLevelSkillSystem skillSystem;
    
    void Start()
    {
        // 自动查找组件
        if (playerAnimator == null)
            playerAnimator = GetComponent<Animator>();
            
        skillSystem = GetComponent<PlayerLevelSkillSystem>();
        
        if (autoSetupAnimator)
        {
            CheckAndSetupAnimator();
        }
    }
    
    void CheckAndSetupAnimator()
    {
        if (showDetailedLogs)
        {
            Debug.Log("<color=#00FFFF>===== 空中攻击动画设置检查开始 =====</color>");
        }
        
        // 检查Animator组件
        if (playerAnimator == null)
        {
            Debug.LogError("<color=#FF0000>错误：找不到Animator组件！</color>");
            Debug.LogError("<color=#FF0000>解决方案：请为玩家对象添加Animator组件</color>");
            return;
        }
        
        // 检查Animator Controller
        if (playerAnimator.runtimeAnimatorController == null)
        {
            Debug.LogError("<color=#FF0000>错误：Animator没有设置Controller！</color>");
            Debug.LogError("<color=#FF0000>解决方案：请为Animator设置AnimatorController</color>");
            return;
        }
        
        // 检查触发器参数
        bool hasTrigger = false;
        foreach (AnimatorControllerParameter param in playerAnimator.parameters)
        {
            if (param.name == aerialAttackTriggerName && param.type == AnimatorControllerParameterType.Trigger)
            {
                hasTrigger = true;
                break;
            }
        }
        
        if (hasTrigger)
        {
            Debug.Log($"<color=#00FF00>✓ 找到空中攻击触发器: {aerialAttackTriggerName}</color>");
        }
        else
        {
            Debug.LogWarning($"<color=#FFFF00>警告：未找到触发器参数 '{aerialAttackTriggerName}'</color>");
            Debug.LogWarning("<color=#FFFF00>解决方案：请在AnimatorController中添加名为'Aerial_Attack'的Trigger参数</color>");
        }
        
        // 测试触发器
        if (hasTrigger)
        {
            TestAnimationTrigger();
        }
        
        if (showDetailedLogs)
        {
            Debug.Log("<color=#00FFFF>===== 动画设置检查完成 =====</color>");
        }
    }
    
    void TestAnimationTrigger()
    {
        if (playerAnimator != null)
        {
            Debug.Log($"<color=#FFFF00>测试触发动画参数: {aerialAttackTriggerName}</color>");
            
            try
            {
                // 测试触发器是否工作
                playerAnimator.SetTrigger(aerialAttackTriggerName);
                Debug.Log("<color=#00FF00>✓ 动画触发器测试成功</color>");
                
                // 等待一帧后重置
                StartCoroutine(ResetTriggerAfterFrame());
            }
            catch (System.Exception e)
            {
                Debug.LogError($"<color=#FF0000>动画触发器测试失败: {e.Message}</color>");
            }
        }
    }
    
    System.Collections.IEnumerator ResetTriggerAfterFrame()
    {
        yield return null;
        if (playerAnimator != null)
        {
            playerAnimator.ResetTrigger(aerialAttackTriggerName);
        }
    }
    
    // 手动测试动画的公共方法
    [System.Obsolete("仅用于调试")]
    public void ManualTestAnimation()
    {
        if (playerAnimator != null)
        {
            Debug.Log("<color=#FFFF00>手动测试空中攻击动画</color>");
            playerAnimator.SetTrigger(aerialAttackTriggerName);
        }
        else
        {
            Debug.LogError("<color=#FF0000>无法测试动画：找不到Animator组件</color>");
        }
    }
    
    // 检查特定动画状态是否存在
    public bool HasAnimationState(string stateName)
    {
        if (playerAnimator == null || playerAnimator.runtimeAnimatorController == null)
            return false;
            
        for (int i = 0; i < playerAnimator.layerCount; i++)
        {
            if (playerAnimator.HasState(i, Animator.StringToHash(stateName)))
            {
                return true;
            }
        }
        return false;
    }
    
    // GUI调试界面
    void OnGUI()
    {
        if (!showDetailedLogs) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("空中攻击动画调试", GUI.skin.label);
        
        if (playerAnimator != null)
        {
            GUILayout.Label($"Animator: ✓", GUI.skin.label);
            GUILayout.Label($"Controller: {(playerAnimator.runtimeAnimatorController != null ? "✓" : "✗")}", GUI.skin.label);
            
            if (GUILayout.Button("测试空中攻击动画"))
            {
                ManualTestAnimation();
            }
            
            if (GUILayout.Button("重新检查动画设置"))
            {
                CheckAndSetupAnimator();
            }
        }
        else
        {
            GUILayout.Label("Animator: ✗", GUI.skin.label);
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    // 在Inspector中显示动画信息
    void OnValidate()
    {
        if (playerAnimator == null)
            playerAnimator = GetComponent<Animator>();
    }
}
