using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation;
#endif

/// <summary>
/// 编译问题解决 - 解决当前的编译错误问题
/// </summary>
public class 编译问题解决 : MonoBehaviour
{
    #if UNITY_EDITOR
    
    [MenuItem("Tools/🔧 强制重新编译")]
    public static void ForceRecompile()
    {
        Debug.Log("<color=cyan>🔧 强制重新编译所有脚本...</color>");
        
        // 1. 刷新资源数据库
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        
        // 2. 强制重新编译
        CompilationPipeline.RequestScriptCompilation();
        
        Debug.Log("<color=green>✅ 重新编译请求已发送，请等待编译完成</color>");
    }
    
    [MenuItem("Tools/🧹 清理并重新编译")]
    public static void CleanAndRecompile()
    {
        Debug.Log("<color=cyan>🧹 清理并重新编译...</color>");
        
        // 1. 强制刷新所有资源
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        
        // 2. 重新导入所有脚本
        AssetDatabase.ImportAsset("Assets", ImportAssetOptions.ImportRecursive);
        
        // 3. 强制重新编译
        CompilationPipeline.RequestScriptCompilation();
        
        Debug.Log("<color=green>✅ 清理和重新编译完成</color>");
    }
    
    #endif
} 