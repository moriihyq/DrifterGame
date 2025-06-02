using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class 强制重新编译 : MonoBehaviour 
{
    #if UNITY_EDITOR
    [MenuItem("Tools/项目构建/🔄 强制重新编译")]
    public static void ForceRecompile()
    {
        Debug.Log("<color=cyan>🔄 强制重新编译所有脚本...</color>");
        
        // 刷新资源数据库
        AssetDatabase.Refresh();
        
        // 强制重新编译
        EditorUtility.RequestScriptReload();
        
        Debug.Log("<color=green>✅ 重新编译请求已发送</color>");
        Debug.Log("<color=yellow>💡 请等待编译完成...</color>");
    }
    #endif
} 