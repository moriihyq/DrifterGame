using UnityEngine;

/// <summary>
/// 用于调试Boss动画的工具类
/// 将此脚本附加到Boss对象上，帮助调试动画控制器的问题
/// </summary>
public class BossAnimationDebugger : MonoBehaviour
{
    [Header("动画控制器调试")]
    public bool showDebugInfo = true;
    public KeyCode testAttackKey = KeyCode.Z;
    public KeyCode testShootKey = KeyCode.X;
    public KeyCode testHurtKey = KeyCode.C;
    public KeyCode testDeathKey = KeyCode.V;
    
    private Animator anim;
    
    void Start()
    {
        anim = GetComponent<Animator>();
        Debug.Log("<color=#00FF00>Boss动画调试器已激活</color>");
        
        if (anim == null)
        {
            Debug.LogError("找不到Animator组件!");
            return;
        }
        
        // 打印动画控制器信息
        Debug.Log($"<color=#00FF00>动画控制器: {(anim.runtimeAnimatorController != null ? anim.runtimeAnimatorController.name : "无")}</color>");
        
        // 打印所有参数
        Debug.Log("<color=#00FF00>动画器参数:</color>");
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            Debug.Log($"<color=#00FF00> - {param.name} ({param.type})</color>");
        }
        
        // 打印所有动画片段
        if (anim.runtimeAnimatorController != null)
        {
            Debug.Log("<color=#00FF00>动画片段:</color>");
            foreach (AnimationClip clip in anim.runtimeAnimatorController.animationClips)
            {
                Debug.Log($"<color=#00FF00> - {clip.name} (长度: {clip.length}秒)</color>");
            }
        }
        else
        {
            Debug.LogError("找不到RuntimeAnimatorController!");
        }
    }
    
    void Update()
    {
        if (!showDebugInfo || anim == null) return;
        
        // 使用按键测试各种动画
        if (Input.GetKeyDown(testAttackKey))
        {
            Debug.Log("<color=#FFFF00>测试近战攻击动画</color>");
            TriggerAnimation("Attack");
        }
        
        if (Input.GetKeyDown(testShootKey))
        {
            Debug.Log("<color=#FFFF00>测试远程攻击动画</color>");
            TriggerAnimation("Shoot");
        }
        
        if (Input.GetKeyDown(testHurtKey))
        {
            Debug.Log("<color=#FFFF00>测试受伤动画</color>");
            TriggerAnimation("Hurt");
        }
        
        if (Input.GetKeyDown(testDeathKey))
        {
            Debug.Log("<color=#FFFF00>测试死亡动画</color>");
            SetBoolParameter("IsDead", true);
        }
    }
    
    // 触发一个动画
    private void TriggerAnimation(string triggerName)
    {
        try
        {
            anim.SetTrigger(triggerName);
            Debug.Log($"<color=#00FF00>成功触发动画: {triggerName}</color>");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"触发动画 {triggerName} 时出错: {e.Message}");
        }
    }
    
    // 设置布尔参数
    private void SetBoolParameter(string paramName, bool value)
    {
        try
        {
            anim.SetBool(paramName, value);
            Debug.Log($"<color=#00FF00>成功设置参数 {paramName} = {value}</color>");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"设置参数 {paramName} 时出错: {e.Message}");
        }
    }
    
    // 在编辑器中显示当前状态
    void OnGUI()
    {
        if (!showDebugInfo || anim == null) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("<color=yellow><b>Boss动画调试</b></color>", new GUIStyle { richText = true, fontSize = 16 });
        
        // 显示当前状态信息
        foreach (var param in anim.parameters)
        {
            string value = "未知";
            
            switch (param.type)
            {
                case AnimatorControllerParameterType.Float:
                    value = anim.GetFloat(param.name).ToString("F2");
                    break;
                case AnimatorControllerParameterType.Int:
                    value = anim.GetInteger(param.name).ToString();
                    break;
                case AnimatorControllerParameterType.Bool:
                    value = anim.GetBool(param.name).ToString();
                    break;
                case AnimatorControllerParameterType.Trigger:
                    value = "触发器";
                    break;
            }
            
            GUILayout.Label($"{param.name}: {value}");
        }
        
        // 显示当前播放的动画状态
        GUILayout.Label($"当前状态: {anim.GetCurrentAnimatorStateInfo(0).shortNameHash}");
        
        GUILayout.EndArea();
    }
}
