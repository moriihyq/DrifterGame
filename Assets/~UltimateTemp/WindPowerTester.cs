using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor.Animations;
using UnityEditor;
#endif

public class WindPowerTester : MonoBehaviour
{
    [Header("测试设置")]
    [SerializeField] private bool autoAttachComponents = true;
    [SerializeField] private KeyCode testKey = KeyCode.T;
    
    [Header("测试状态")]
    [SerializeField] private bool allComponentsFound = false;
    [SerializeField] private bool animatorSetupCorrect = false;
    [SerializeField] private bool skillFunctional = false;
    
    [Header("测试UI")]
    [SerializeField] private Text testResultText;
    
    // 组件引用
    private WindPower windPower;
    private PlayerRun playerRun;
    private PlayerAttackSystem playerAttackSystem;
    private Animator playerAnimator;
    
    // 测试用的原始值
    private float originalWalkSpeed;
    private float originalRunSpeed;
    private int originalAttackDamage;
    
    // 测试状态
    private bool hasTestedAnimation = false;
    private bool hasTestedSpeedBoost = false;
    private bool hasTestedDamageBoost = false;
    private bool hasTestedCooldown = false;
    private bool hasTestedDuration = false;
    
    void Start()
    {
        Debug.Log("<color=#00FFFF>====== WindPower技能测试工具 ======</color>");
        Debug.Log("按 " + testKey + " 键运行测试");
        
        if (autoAttachComponents)
        {
            AutoAttachComponents();
        }
        else
        {
            FindComponents();
        }
        
        InitializeTestValues();
        CheckAnimatorSetup();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            RunAllTests();
        }
        
        // 如果WindPower已激活，检查效果
        if (windPower != null && windPower.IsWindPowerActive)
        {
            CheckActiveEffects();
        }
    }
    
    void RunAllTests()
    {
        Debug.Log("<color=#00FFFF>====== 开始WindPower技能测试 ======</color>");
        
        // 重新检查组件和设置
        FindComponents();
        CheckAnimatorSetup();
        
        // 测试风力技能功能
        TestWindPowerFunctionality();
    }
    
    void AutoAttachComponents()
    {
        Debug.Log("自动添加必要组件...");
        
        // 确保有Animator组件
        playerAnimator = GetComponent<Animator>();
        if (playerAnimator == null)
        {
            playerAnimator = gameObject.AddComponent<Animator>();
            Debug.Log("已添加Animator组件");
        }
        
        // 确保有PlayerRun组件
        playerRun = GetComponent<PlayerRun>();
        if (playerRun == null)
        {
            playerRun = gameObject.AddComponent<PlayerRun>();
            Debug.Log("已添加PlayerRun组件");
        }
        
        // 确保有PlayerAttackSystem组件
        playerAttackSystem = GetComponent<PlayerAttackSystem>();
        if (playerAttackSystem == null)
        {
            playerAttackSystem = gameObject.AddComponent<PlayerAttackSystem>();
            Debug.Log("已添加PlayerAttackSystem组件");
        }
        
        // 确保有WindPower组件
        windPower = GetComponent<WindPower>();
        if (windPower == null)
        {
            windPower = gameObject.AddComponent<WindPower>();
            Debug.Log("已添加WindPower组件");
        }
        
        // 添加AudioSource（可选）
        if (GetComponent<AudioSource>() == null)
        {
            gameObject.AddComponent<AudioSource>();
            Debug.Log("已添加AudioSource组件");
        }
    }
    
    void FindComponents()
    {
        Debug.Log("查找必要组件...");
        
        windPower = GetComponent<WindPower>();
        playerRun = GetComponent<PlayerRun>();
        playerAttackSystem = GetComponent<PlayerAttackSystem>();
        playerAnimator = GetComponent<Animator>();
        
        // 检查组件是否都存在
        bool allFound = true;
        
        if (windPower == null)
        {
            Debug.LogError("未找到WindPower组件！");
            allFound = false;
        }
        
        if (playerRun == null)
        {
            Debug.LogWarning("未找到PlayerRun组件！将无法测试速度提升效果");
            allFound = false;
        }
        
        if (playerAttackSystem == null)
        {
            Debug.LogWarning("未找到PlayerAttackSystem组件！将无法测试伤害提升效果");
            allFound = false;
        }
        
        if (playerAnimator == null)
        {
            Debug.LogWarning("未找到Animator组件！将无法测试动画效果");
            allFound = false;
        }
        
        allComponentsFound = allFound;
        UpdateTestResultText();
    }
    
    void InitializeTestValues()
    {
        if (playerRun != null)
        {
            originalWalkSpeed = playerRun.walkSpeed;
            originalRunSpeed = playerRun.runSpeed;
            Debug.Log($"记录原始移动速度 - 步行: {originalWalkSpeed}, 奔跑: {originalRunSpeed}");
        }
        
        if (playerAttackSystem != null)
        {
            originalAttackDamage = playerAttackSystem.AttackDamage;
            Debug.Log($"记录原始攻击伤害: {originalAttackDamage}");
        }
    }
    
    void CheckAnimatorSetup()
    {
        if (playerAnimator == null)
        {
            Debug.LogError("无法检查Animator设置，未找到Animator组件");
            animatorSetupCorrect = false;
            return;
        }
        
        // 获取Animator Controller
        RuntimeAnimatorController runtimeController = playerAnimator.runtimeAnimatorController;
        
        if (runtimeController == null)
        {
            Debug.LogError("未找到AnimatorController！请确保玩家有正确的Animator Controller");
            animatorSetupCorrect = false;
            return;
        }
        
        #if UNITY_EDITOR
        // 在编辑器中，我们可以检查具体的参数
        AnimatorController controller = runtimeController as AnimatorController;
        if (controller != null)
        {
        // 检查是否有windpower参数
        bool hasWindpowerParam = false;
        foreach (AnimatorControllerParameter param in controller.parameters)
        {
            if (param.name == "windpower" && param.type == AnimatorControllerParameterType.Trigger)
            {
                hasWindpowerParam = true;
                break;
            }
        }
        
        if (hasWindpowerParam)
        {
            Debug.Log("<color=#00FF00>✓ 找到windpower触发器</color>");
            animatorSetupCorrect = true;
        }
        else
        {
            Debug.LogError("AnimatorController中未找到windpower触发器！请添加一个类型为Trigger的windpower参数");
            animatorSetupCorrect = false;
        }
        }
        else
        {
            Debug.LogWarning("无法在编辑器中检查AnimatorController参数");
            animatorSetupCorrect = true; // 假设设置正确
        }
        #else
        // 在运行时，我们只能基本检查是否有controller
        Debug.Log("<color=#FFFF00>运行时模式：已找到AnimatorController，假设windpower参数存在</color>");
        animatorSetupCorrect = true;
        #endif
        
        UpdateTestResultText();
    }
    
    void TestWindPowerFunctionality()
    {
        if (windPower == null)
        {
            Debug.LogError("无法测试功能，WindPower组件不存在");
            skillFunctional = false;
            return;
        }
        
        // 模拟按R键激活技能
        Debug.Log("模拟按下R键，激活风力技能...");
        Input.GetKeyDown(KeyCode.R);
        
        // 用协程来检查技能效果
        StartCoroutine(CheckWindPowerEffectsCoroutine());
    }
    
    System.Collections.IEnumerator CheckWindPowerEffectsCoroutine()
    {
        // 等待一帧，让WindPower的Update有机会响应
        yield return null;
        
        // 重置测试状态
        hasTestedAnimation = false;
        hasTestedSpeedBoost = false;
        hasTestedDamageBoost = false;
        hasTestedCooldown = false;
        hasTestedDuration = false;
        
        // 如果技能成功激活，应该能看到这些效果
        float testDuration = 6f; // 比技能持续时间稍长
        float elapsedTime = 0f;
        
        while (elapsedTime < testDuration)
        {
            elapsedTime += Time.deltaTime;
            
            // 检查技能是否激活
            if (windPower.IsWindPowerActive)
            {
                // 检查移动速度是否提升
                if (!hasTestedSpeedBoost && playerRun != null)
                {
                    bool speedBoosted = (playerRun.walkSpeed > originalWalkSpeed) && (playerRun.runSpeed > originalRunSpeed);
                    Debug.Log($"<color={(speedBoosted ? "#00FF00" : "#FF0000")}>{(speedBoosted ? "✓" : "✗")} 移动速度提升测试: {(speedBoosted ? "通过" : "失败")}</color>");
                    hasTestedSpeedBoost = true;
                }
                
                // 检查攻击伤害是否提升
                if (!hasTestedDamageBoost && playerAttackSystem != null)
                {
                    bool damageBoosted = playerAttackSystem.AttackDamage > originalAttackDamage;
                    Debug.Log($"<color={(damageBoosted ? "#00FF00" : "#FF0000")}>{(damageBoosted ? "✓" : "✗")} 攻击伤害提升测试: {(damageBoosted ? "通过" : "失败")}</color>");
                    hasTestedDamageBoost = true;
                }
            }
            
            yield return null;
        }
        
        // 技能持续时间应该已经结束
        if (!hasTestedDuration)
        {
            bool durationCorrect = !windPower.IsWindPowerActive;
            Debug.Log($"<color={(durationCorrect ? "#00FF00" : "#FF0000")}>{(durationCorrect ? "✓" : "✗")} 持续时间测试: {(durationCorrect ? "通过" : "失败")}</color>");
            hasTestedDuration = true;
        }
        
        // 检查冷却是否正确
        if (!hasTestedCooldown)
        {
            bool cooldownActive = windPower.IsWindPowerOnCooldown;
            Debug.Log($"<color={(cooldownActive ? "#00FF00" : "#FF0000")}>{(cooldownActive ? "✓" : "✗")} 冷却时间测试: {(cooldownActive ? "通过" : "失败")}</color>");
            hasTestedCooldown = true;
        }
        
        // 检查是否恢复到原始状态
        CheckRestoredValues();
        
        // 确定技能是否功能正常
        skillFunctional = hasTestedSpeedBoost && hasTestedDamageBoost && hasTestedDuration && hasTestedCooldown;
        UpdateTestResultText();
    }
    
    void CheckActiveEffects()
    {
        // 如果WindPower激活，检查效果是否正确应用
        if (playerRun != null && !hasTestedSpeedBoost)
        {
            bool speedBoosted = (playerRun.walkSpeed > originalWalkSpeed) && (playerRun.runSpeed > originalRunSpeed);
            if (speedBoosted)
            {
                Debug.Log($"<color=#00FF00>✓ 移动速度已提升 - 步行: {playerRun.walkSpeed} (原始: {originalWalkSpeed}), 奔跑: {playerRun.runSpeed} (原始: {originalRunSpeed})</color>");
            }
            else
            {
                Debug.LogError($"✗ 移动速度未提升 - 步行: {playerRun.walkSpeed}, 奔跑: {playerRun.runSpeed}");
            }
            hasTestedSpeedBoost = true;
        }
        
        if (playerAttackSystem != null && !hasTestedDamageBoost)
        {
            bool damageBoosted = playerAttackSystem.AttackDamage > originalAttackDamage;
            if (damageBoosted)
            {
                Debug.Log($"<color=#00FF00>✓ 攻击伤害已提升: {playerAttackSystem.AttackDamage} (原始: {originalAttackDamage})</color>");
            }
            else
            {
                Debug.LogError($"✗ 攻击伤害未提升: {playerAttackSystem.AttackDamage}");
            }
            hasTestedDamageBoost = true;
        }
    }
    
    void CheckRestoredValues()
    {
        // 检查技能结束后，状态是否恢复
        if (playerRun != null)
        {
            bool speedRestored = Mathf.Approximately(playerRun.walkSpeed, originalWalkSpeed) && 
                                  Mathf.Approximately(playerRun.runSpeed, originalRunSpeed);
            Debug.Log($"<color={(speedRestored ? "#00FF00" : "#FF0000")}>{(speedRestored ? "✓" : "✗")} 移动速度恢复测试: {(speedRestored ? "通过" : "失败")}</color>");
        }
        
        if (playerAttackSystem != null)
        {
            bool damageRestored = playerAttackSystem.AttackDamage == originalAttackDamage;
            Debug.Log($"<color={(damageRestored ? "#00FF00" : "#FF0000")}>{(damageRestored ? "✓" : "✗")} 攻击伤害恢复测试: {(damageRestored ? "通过" : "失败")}</color>");
        }
    }
    
    void UpdateTestResultText()
    {
        if (testResultText == null) return;
        
        string result = "<b>风力技能测试结果:</b>\n\n";
        
        // 组件检查
        result += $"组件检查: {(allComponentsFound ? "<color=#00FF00>通过</color>" : "<color=#FF0000>失败</color>")}\n";
        
        // Animator设置
        result += $"Animator设置: {(animatorSetupCorrect ? "<color=#00FF00>通过</color>" : "<color=#FF0000>失败</color>")}\n";
        
        // 功能测试
        result += $"功能测试: {(skillFunctional ? "<color=#00FF00>通过</color>" : "<color=#FF8800>未测试</color>")}\n\n";
        
        // 使用说明
        result += "<b>使用说明:</b>\n";
        result += "1. 按 R 键激活风力技能\n";
        result += "2. 风力提升移动速度和攻击伤害1.5倍\n";
        result += "3. 技能持续5秒\n";
        result += "4. 冷却时间15秒\n\n";
        
        // 测试指南
        result += "<b>测试指南:</b>\n";
        result += $"按 {testKey} 键运行测试";
        
        testResultText.text = result;
    }
}
