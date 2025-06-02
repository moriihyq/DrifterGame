using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

/// <summary>
/// 构建问题彻底清理 - 临时移动编辑器脚本解决构建问题
/// </summary>
public class 构建问题彻底清理 : MonoBehaviour
{
    #if UNITY_EDITOR
    
    [MenuItem("Tools/🧹 临时移动编辑器脚本")]
    public static void TemporarilyMoveEditorScripts()
    {
        Debug.Log("<color=cyan>🧹 开始临时移动编辑器相关脚本...</color>");
        
        // 需要临时移动的文件
        string[] editorScriptsToMove = {
            "Assets/Scripts/编译错误检查器.cs",
            "Assets/Scripts/项目构建诊断器.cs",
            "Assets/Scripts/对象持久化错误修复器.cs",
            "Assets/Scripts/强力Prefab修复器.cs",
            "Assets/Scripts/快速错误修复.cs",
            "Assets/Scripts/游戏运行时背景诊断器.cs",
            "Assets/Scripts/背景垂直跟随修复.cs",
            "Assets/Scripts/背景大小调整器.cs",
            "Assets/Scripts/自动背景设置器.cs",
            "Assets/Scripts/背景调试器.cs",
            "Assets/Scripts/背景显示诊断器.cs",
            "Assets/Scripts/背景填充修复器.cs",
            "Assets/Scripts/快速背景跟随修复.cs",
            "Assets/Scripts/强制重新编译.cs",
            "Assets/Scripts/资源损坏修复器.cs",
            "Assets/Scripts/背景跟随修复器.cs"
        };
        
        // 创建临时文件夹
        string tempFolder = "Assets/~TempEditorScripts";
        if (!Directory.Exists(tempFolder))
        {
            Directory.CreateDirectory(tempFolder);
            AssetDatabase.Refresh();
        }
        
        int movedCount = 0;
        foreach (string scriptPath in editorScriptsToMove)
        {
            if (File.Exists(scriptPath))
            {
                string fileName = Path.GetFileName(scriptPath);
                string destPath = Path.Combine(tempFolder, fileName);
                
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
        
        AssetDatabase.Refresh();
        Debug.Log($"<color=green>🎉 完成！移动了 {movedCount} 个脚本到临时文件夹</color>");
        Debug.Log("<color=yellow>💡 现在可以尝试构建项目，构建完成后可以还原这些脚本</color>");
    }
    
    [MenuItem("Tools/🔄 还原编辑器脚本")]
    public static void RestoreEditorScripts()
    {
        Debug.Log("<color=cyan>🔄 开始还原编辑器脚本...</color>");
        
        string tempFolder = "Assets/~TempEditorScripts";
        if (!Directory.Exists(tempFolder))
        {
            Debug.LogWarning("<color=yellow>⚠️ 临时文件夹不存在，没有需要还原的脚本</color>");
            return;
        }
        
        string[] tempFiles = Directory.GetFiles(tempFolder, "*.cs");
        int restoredCount = 0;
        
        foreach (string tempFile in tempFiles)
        {
            string fileName = Path.GetFileName(tempFile);
            string destPath = $"Assets/Scripts/{fileName}";
            string relativeTempPath = "Assets/~TempEditorScripts/" + fileName;
            
            try
            {
                AssetDatabase.MoveAsset(relativeTempPath, destPath);
                Debug.Log($"<color=green>✅ 还原: {relativeTempPath} → {destPath}</color>");
                restoredCount++;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"<color=yellow>⚠️ 无法还原 {relativeTempPath}: {e.Message}</color>");
            }
        }
        
        // 删除临时文件夹
        if (restoredCount == tempFiles.Length)
        {
            Directory.Delete(tempFolder, true);
            File.Delete(tempFolder + ".meta");
        }
        
        AssetDatabase.Refresh();
        Debug.Log($"<color=green>🎉 完成！还原了 {restoredCount} 个脚本</color>");
    }
    
    [MenuItem("Tools/⚡ 快速构建修复")]
    public static void QuickBuildFix()
    {
        Debug.Log("<color=cyan>⚡ 开始快速构建修复...</color>");
        
        // 1. 临时移动编辑器脚本
        TemporarilyMoveEditorScripts();
        
        // 2. 强制刷新
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        
        // 3. 等待编译完成后的提示
        EditorApplication.delayCall += () =>
        {
            Debug.Log("<color=green>✅ 快速修复完成！现在可以尝试构建项目</color>");
            Debug.Log("<color=yellow>💡 构建完成后，记得使用 '还原编辑器脚本' 恢复功能</color>");
        };
    }
    
    #endif
} 