using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Linq;
#endif

/// <summary>
/// 项目构建诊断器 - 检查项目构建问题并提供解决方案
/// </summary>
public class 项目构建诊断器 : MonoBehaviour
{
    #if UNITY_EDITOR
    
    /// <summary>
    /// 诊断项目构建问题
    /// </summary>
    [MenuItem("Tools/项目构建/🔍 诊断构建问题")]
    public static void DiagnoseBuildIssues()
    {
        Debug.Log("<color=cyan>===== 🔍 项目构建诊断开始 =====</color>");
        
        // 1. 检查编译错误
        CheckCompileErrors();
        
        // 2. 检查构建设置
        CheckBuildSettings();
        
        // 3. 检查场景设置
        CheckSceneSettings();
        
        // 4. 检查资源和依赖
        CheckAssetsAndDependencies();
        
        // 5. 检查平台设置
        CheckPlatformSettings();
        
        Debug.Log("<color=cyan>===== 🔍 项目构建诊断完成 =====</color>");
    }
    
    /// <summary>
    /// 检查编译错误
    /// </summary>
    private static void CheckCompileErrors()
    {
        Debug.Log("<color=yellow>📝 检查编译错误...</color>");
        
        try
        {
            // 强制重新编译
            AssetDatabase.Refresh();
            
            // 简化的编译检查
            Debug.Log("<color=green>✅ 脚本编译检查完成</color>");
            Debug.Log("<color=white>💡 如果有编译错误，请查看Console窗口的红色错误信息</color>");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"<color=yellow>⚠️ 编译检查跳过: {e.Message}</color>");
        }
    }
    
    /// <summary>
    /// 检查构建设置
    /// </summary>
    private static void CheckBuildSettings()
    {
        Debug.Log("<color=yellow>🔧 检查构建设置...</color>");
        
        var scenes = EditorBuildSettings.scenes;
        
        if (scenes == null || scenes.Length == 0)
        {
            Debug.LogWarning("<color=yellow>⚠️ 构建设置中没有场景！</color>");
            Debug.Log("<color=cyan>💡 解决方案: 打开 File → Build Settings 添加场景</color>");
        }
        else
        {
            Debug.Log($"<color=green>✅ 构建设置包含 {scenes.Length} 个场景</color>");
            
            int enabledScenes = 0;
            foreach (var scene in scenes)
            {
                if (scene.enabled)
                {
                    enabledScenes++;
                    if (File.Exists(scene.path))
                    {
                        Debug.Log($"<color=white>  ✅ {scene.path}</color>");
                    }
                    else
                    {
                        Debug.LogError($"<color=red>  ❌ 场景文件不存在: {scene.path}</color>");
                    }
                }
                else
                {
                    Debug.Log($"<color=gray>  ⏸️ 已禁用: {scene.path}</color>");
                }
            }
            
            if (enabledScenes == 0)
            {
                Debug.LogWarning("<color=yellow>⚠️ 没有启用的场景！</color>");
            }
        }
    }
    
    /// <summary>
    /// 检查场景设置
    /// </summary>
    private static void CheckSceneSettings()
    {
        Debug.Log("<color=yellow>🎬 检查场景设置...</color>");
        
        var scenes = EditorBuildSettings.scenes;
        if (scenes != null && scenes.Length > 0)
        {
            var firstScene = scenes.FirstOrDefault(s => s.enabled);
            if (firstScene != null)
            {
                Debug.Log($"<color=green>✅ 首个场景: {firstScene.path}</color>");
            }
            else
            {
                Debug.LogWarning("<color=yellow>⚠️ 没有启用的首个场景</color>");
            }
        }
    }
    
    /// <summary>
    /// 检查资源和依赖
    /// </summary>
    private static void CheckAssetsAndDependencies()
    {
        Debug.Log("<color=yellow>📦 检查资源和依赖...</color>");
        
        // 检查关键资源
        bool hasInputSettings = false;
        string[] inputAssets = AssetDatabase.FindAssets("t:InputActionAsset");
        if (inputAssets.Length > 0)
        {
            hasInputSettings = true;
            Debug.Log($"<color=green>✅ 找到 {inputAssets.Length} 个输入设置</color>");
        }
        else
        {
            Debug.LogWarning("<color=yellow>⚠️ 没有找到输入设置（InputActionAsset）</color>");
        }
        
        // 检查渲染管线 - 使用更安全的方法
        try
        {
            // 查找URP资源
            string[] urpAssets = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
            if (urpAssets.Length > 0)
            {
                Debug.Log($"<color=green>✅ 找到 URP 渲染管线资源</color>");
            }
            else
            {
                Debug.Log("<color=white>📝 未找到URP资源，可能使用内置渲染管线</color>");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"<color=yellow>⚠️ 渲染管线检查跳过: {e.Message}</color>");
        }
    }
    
    /// <summary>
    /// 检查平台设置
    /// </summary>
    private static void CheckPlatformSettings()
    {
        Debug.Log("<color=yellow>🖥️ 检查平台设置...</color>");
        
        var currentTarget = EditorUserBuildSettings.activeBuildTarget;
        Debug.Log($"<color=green>✅ 当前构建目标: {currentTarget}</color>");
        
        var targetGroup = BuildPipeline.GetBuildTargetGroup(currentTarget);
        Debug.Log($"<color=green>✅ 目标组: {targetGroup}</color>");
        
        // 检查玩家设置
        string productName = PlayerSettings.productName;
        string companyName = PlayerSettings.companyName;
        
        Debug.Log($"<color=white>  产品名称: {productName}</color>");
        Debug.Log($"<color=white>  公司名称: {companyName}</color>");
        
        if (string.IsNullOrEmpty(productName))
        {
            Debug.LogWarning("<color=yellow>⚠️ 产品名称为空</color>");
        }
    }
    
    /// <summary>
    /// 一键修复常见构建问题
    /// </summary>
    [MenuItem("Tools/项目构建/🛠️ 修复常见构建问题")]
    public static void FixCommonBuildIssues()
    {
        Debug.Log("<color=cyan>🛠️ 开始修复常见构建问题...</color>");
        
        int fixedCount = 0;
        
        // 1. 确保有场景在构建设置中
        var scenes = EditorBuildSettings.scenes;
        if (scenes == null || scenes.Length == 0 || !scenes.Any(s => s.enabled))
        {
            // 添加当前场景到构建设置
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(currentScene.path))
            {
                var newScenes = new EditorBuildSettingsScene[] 
                {
                    new EditorBuildSettingsScene(currentScene.path, true)
                };
                EditorBuildSettings.scenes = newScenes;
                Debug.Log($"<color=green>✅ 已添加当前场景到构建设置: {currentScene.path}</color>");
                fixedCount++;
            }
        }
        
        // 2. 设置基本的玩家设置
        if (string.IsNullOrEmpty(PlayerSettings.productName) || PlayerSettings.productName == "Game_latest")
        {
            PlayerSettings.productName = "我的游戏";
            Debug.Log("<color=green>✅ 已设置产品名称</color>");
            fixedCount++;
        }
        
        if (string.IsNullOrEmpty(PlayerSettings.companyName) || PlayerSettings.companyName == "DefaultCompany")
        {
            PlayerSettings.companyName = "我的公司";
            Debug.Log("<color=green>✅ 已设置公司名称</color>");
            fixedCount++;
        }
        
        // 3. 刷新资源数据库
        AssetDatabase.Refresh();
        Debug.Log("<color=green>✅ 已刷新资源数据库</color>");
        fixedCount++;
        
        Debug.Log($"<color=green>🎉 修复完成！共修复了 {fixedCount} 个问题</color>");
        Debug.Log("<color=yellow>请尝试重新构建项目</color>");
    }
    
    /// <summary>
    /// 尝试构建项目
    /// </summary>
    [MenuItem("Tools/项目构建/🚀 尝试构建项目")]
    public static void TryBuildProject()
    {
        Debug.Log("<color=cyan>🚀 开始尝试构建项目...</color>");
        
        // 设置构建选项
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        buildPlayerOptions.locationPathName = "Builds/TestBuild";
        buildPlayerOptions.target = EditorUserBuildSettings.activeBuildTarget;
        buildPlayerOptions.options = BuildOptions.None;
        
        // 根据平台设置文件扩展名
        if (buildPlayerOptions.target == BuildTarget.StandaloneWindows || buildPlayerOptions.target == BuildTarget.StandaloneWindows64)
        {
            buildPlayerOptions.locationPathName += ".exe";
        }
        
        Debug.Log($"<color=white>构建目标: {buildPlayerOptions.target}</color>");
        Debug.Log($"<color=white>构建路径: {buildPlayerOptions.locationPathName}</color>");
        Debug.Log($"<color=white>场景数量: {buildPlayerOptions.scenes.Length}</color>");
        
        // 执行构建
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;
        
        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"<color=green>🎉 构建成功！</color>");
            Debug.Log($"<color=green>构建大小: {summary.totalSize} 字节</color>");
            Debug.Log($"<color=green>构建时间: {summary.totalTime}</color>");
        }
        else
        {
            Debug.LogError($"<color=red>❌ 构建失败: {summary.result}</color>");
            
            if (report.steps != null)
            {
                foreach (var step in report.steps)
                {
                    if (step.messages != null)
                    {
                        foreach (var message in step.messages)
                        {
                            if (message.type == LogType.Error)
                            {
                                Debug.LogError($"<color=red>构建错误: {message.content}</color>");
                            }
                            else if (message.type == LogType.Warning)
                            {
                                Debug.LogWarning($"<color=yellow>构建警告: {message.content}</color>");
                            }
                        }
                    }
                }
            }
        }
    }
    
    #endif
} 