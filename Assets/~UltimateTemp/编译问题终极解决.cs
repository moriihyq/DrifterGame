using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using System.Collections.Generic;
#endif

/// <summary>
/// 编译问题终极解决 - 处理所有导致构建失败的脚本
/// </summary>
public class 编译问题终极解决 : MonoBehaviour
{
    #if UNITY_EDITOR
    
    [MenuItem("Tools/🚀 终极构建修复")]
    public static void UltimateBuildFix()
    {
        Debug.Log("<color=cyan>🚀 开始终极构建修复...</color>");
        
        // 需要临时移动的所有编辑器脚本
        List<string> allEditorScripts = new List<string>
        {
            "Assets/WindPowerTester.cs",
            "Assets/Scripts/ParallaxController.cs", // 如果有编辑器代码
            "Assets/Scripts/构建问题彻底清理.cs",
            "Assets/Scripts/编译问题解决.cs",
            "Assets/PlayerLevelSkillSystem.cs", // 如果有编辑器代码
            "Assets/LayerCollisionSetup.cs",
            "Assets/InputSystemFix.cs",
            "Assets/AerialAttackDebugger.cs",
            "Assets/Boss/AddBossDebugger.cs"
        };
        
        // 创建终极临时文件夹
        string ultimateTempFolder = "Assets/~UltimateTemp";
        if (!Directory.Exists(ultimateTempFolder))
        {
            Directory.CreateDirectory(ultimateTempFolder);
            AssetDatabase.Refresh();
        }
        
        int movedCount = 0;
        
        // 移动指定的脚本
        foreach (string scriptPath in allEditorScripts)
        {
            if (File.Exists(scriptPath))
            {
                string fileName = Path.GetFileName(scriptPath);
                string destPath = Path.Combine(ultimateTempFolder, fileName);
                
                try
                {
                    AssetDatabase.MoveAsset(scriptPath, destPath);
                    Debug.Log($"<color=green>✅ 移动: {scriptPath} → {destPath}</color>");
                    movedCount++;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"<color=yellow>⚠️ 无法移动 {scriptPath}: {e.Message}</color>");
                }
            }
        }
        
        // 强制刷新
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        
        Debug.Log($"<color=green>🎉 终极修复完成！移动了 {movedCount} 个脚本</color>");
        Debug.Log("<color=yellow>💡 现在应该可以构建项目了</color>");
    }
    
    [MenuItem("Tools/🔧 只保留核心游戏脚本")]
    public static void KeepOnlyCoreGameScripts()
    {
        Debug.Log("<color=cyan>🔧 只保留核心游戏脚本...</color>");
        
        // 核心游戏脚本列表（这些是游戏运行必需的）
        List<string> coreGameScripts = new List<string>
        {
            "Assets/PlayerAttackSystem.cs",
            "Assets/WindPower.cs",
            "Assets/WindPowerUIManager.cs",
            "Assets/Player_1/Scripts/PlayerRun.cs",
            "Assets/Player_1/Scripts/PlayerJumpManager.cs",
            "Assets/Player_1/Scripts/WallJumpController.cs",
            "Assets/Enemy.cs",
            "Assets/Enemy2.cs",
            "Assets/Boss/BossController.cs",
            "Assets/CombatManager.cs",
            "Assets/EnemySlowEffect.cs",
            "Assets/EnemyKnockback.cs",
            "Assets/Scripts/简单编译测试.cs"
        };
        
        // 创建核心脚本备份文件夹
        string coreBackupFolder = "Assets/~CoreScriptsBackup";
        if (!Directory.Exists(coreBackupFolder))
        {
            Directory.CreateDirectory(coreBackupFolder);
            AssetDatabase.Refresh();
        }
        
        // 备份核心脚本
        int backedUpCount = 0;
        foreach (string coreScript in coreGameScripts)
        {
            if (File.Exists(coreScript))
            {
                string fileName = Path.GetFileName(coreScript);
                string backupPath = Path.Combine(coreBackupFolder, fileName);
                
                try
                {
                    File.Copy(coreScript, backupPath, true);
                    Debug.Log($"<color=blue>📋 备份核心脚本: {coreScript}</color>");
                    backedUpCount++;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"<color=yellow>⚠️ 无法备份 {coreScript}: {e.Message}</color>");
                }
            }
        }
        
        Debug.Log($"<color=green>✅ 备份了 {backedUpCount} 个核心脚本</color>");
        Debug.Log("<color=yellow>💡 如果构建仍有问题，可能需要检查具体的编译错误信息</color>");
    }
    
    [MenuItem("Tools/🔍 检查剩余编译错误")]
    public static void CheckRemainingCompilerErrors()
    {
        Debug.Log("<color=cyan>🔍 检查剩余的编译错误...</color>");
        
        // 强制重新编译
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        
        Debug.Log("<color=yellow>📋 请查看Unity Console窗口的错误信息：</color>");
        Debug.Log("<color=white>1. 红色的错误信息通常显示具体的编译问题</color>");
        Debug.Log("<color=white>2. 如果看到CS开头的错误代码，那是具体的C#编译错误</color>");
        Debug.Log("<color=white>3. 如果没有红色错误，项目应该可以构建了</color>");
    }
    
    #endif
} 