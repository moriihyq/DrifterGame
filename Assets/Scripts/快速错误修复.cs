using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

/// <summary>
/// 快速错误修复器 - 专门处理Unity的常见构建错误
/// </summary>
public class 快速错误修复 : MonoBehaviour
{
    #if UNITY_EDITOR
    
    [MenuItem("Tools/项目构建/⚡ 快速修复断言错误")]
    public static void QuickFixAssertionErrors()
    {
        Debug.Log("<color=cyan>⚡ 开始快速修复Unity断言错误...</color>");
        
        // 1. 清理所有空的组件引用
        CleanupMissingComponents();
        
        // 2. 修复DontSaveInEditor问题
        FixDontSaveInEditorIssues();
        
        // 3. 强制刷新和保存
        ForceRefreshProject();
        
        Debug.Log("<color=green>✅ 快速修复完成！</color>");
        Debug.Log("<color=yellow>💡 请尝试重新构建项目</color>");
    }
    
    [MenuItem("Tools/项目构建/🚀 一键解决构建问题")]
    public static void OneClickBuildFix()
    {
        Debug.Log("<color=cyan>🚀 开始一键解决构建问题...</color>");
        
        // 步骤1: 清理编译错误
        Debug.Log("<color=yellow>步骤1: 清理编译错误...</color>");
        CleanupMissingComponents();
        
        // 步骤2: 修复对象引用
        Debug.Log("<color=yellow>步骤2: 修复对象引用...</color>");
        FixDontSaveInEditorIssues();
        
        // 步骤3: 刷新资源
        Debug.Log("<color=yellow>步骤3: 刷新资源...</color>");
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        
        // 步骤4: 重新序列化
        Debug.Log("<color=yellow>步骤4: 重新序列化资源...</color>");
        AssetDatabase.ForceReserializeAssets();
        
        // 步骤5: 保存项目
        Debug.Log("<color=yellow>步骤5: 保存项目...</color>");
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveOpenScenes();
        
        // 步骤6: 强制重新编译
        Debug.Log("<color=yellow>步骤6: 强制重新编译...</color>");
        EditorUtility.RequestScriptReload();
        
        Debug.Log("<color=green>🎉 一键解决完成！</color>");
        Debug.Log("<color=yellow>⏱️ 请等待重新编译完成，然后尝试构建项目</color>");
    }
    
    private static void CleanupMissingComponents()
    {
        Debug.Log("<color=yellow>🧹 清理缺失的组件...</color>");
        
        // 获取当前场景中的所有GameObject
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int cleanedCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj == null) continue;
            
            // 使用SerializedObject来安全地访问组件数组
            SerializedObject serializedObject = new SerializedObject(obj);
            SerializedProperty componentsProperty = serializedObject.FindProperty("m_Component");
            
            if (componentsProperty != null && componentsProperty.isArray)
            {
                // 从后向前遍历，以便安全删除元素
                for (int i = componentsProperty.arraySize - 1; i >= 0; i--)
                {
                    SerializedProperty componentProperty = componentsProperty.GetArrayElementAtIndex(i);
                    SerializedProperty componentRef = componentProperty.FindPropertyRelative("component");
                    
                    if (componentRef != null && componentRef.objectReferenceValue == null)
                    {
                        Debug.Log($"<color=green>✅ 清理 {obj.name} 的空组件引用 (索引 {i})</color>");
                        componentsProperty.DeleteArrayElementAtIndex(i);
                        cleanedCount++;
                    }
                }
                
                serializedObject.ApplyModifiedProperties();
            }
        }
        
        Debug.Log($"<color=green>✅ 共清理了 {cleanedCount} 个空组件引用</color>");
    }
    
    private static void FixDontSaveInEditorIssues()
    {
        Debug.Log("<color=yellow>🔧 修复DontSaveInEditor问题...</color>");
        
        // 查找所有可能导致问题的对象
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int fixedCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj == null) continue;
            
            try
            {
                // 确保对象可以被保存
                if (obj.hideFlags.HasFlag(HideFlags.DontSave) || 
                    obj.hideFlags.HasFlag(HideFlags.DontSaveInEditor))
                {
                    Debug.Log($"<color=yellow>⚠️ 发现不可保存的对象: {obj.name}</color>");
                    
                    // 移除DontSave标志
                    obj.hideFlags = obj.hideFlags & ~HideFlags.DontSave;
                    obj.hideFlags = obj.hideFlags & ~HideFlags.DontSaveInEditor;
                    
                    EditorUtility.SetDirty(obj);
                    fixedCount++;
                    
                    Debug.Log($"<color=green>✅ 已修复 {obj.name} 的保存标志</color>");
                }
                
                // 检查组件的HideFlags
                Component[] components = obj.GetComponents<Component>();
                foreach (Component comp in components)
                {
                    if (comp == null) continue;
                    
                    if (comp.hideFlags.HasFlag(HideFlags.DontSave) || 
                        comp.hideFlags.HasFlag(HideFlags.DontSaveInEditor))
                    {
                        comp.hideFlags = comp.hideFlags & ~HideFlags.DontSave;
                        comp.hideFlags = comp.hideFlags & ~HideFlags.DontSaveInEditor;
                        EditorUtility.SetDirty(comp);
                        fixedCount++;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"<color=yellow>⚠️ 修复对象时出错 {obj.name}: {e.Message}</color>");
            }
        }
        
        Debug.Log($"<color=green>✅ 共修复了 {fixedCount} 个DontSaveInEditor问题</color>");
    }
    
    private static void ForceRefreshProject()
    {
        Debug.Log("<color=yellow>🔄 强制刷新项目...</color>");
        
        // 1. 保存当前场景
        EditorSceneManager.SaveOpenScenes();
        
        // 2. 保存所有资源
        AssetDatabase.SaveAssets();
        
        // 3. 强制刷新资源数据库
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        
        Debug.Log("<color=green>✅ 项目刷新完成</color>");
    }
    
    [MenuItem("Tools/项目构建/🔍 检查问题对象")]
    public static void CheckProblematicObjects()
    {
        Debug.Log("<color=cyan>🔍 检查可能导致构建问题的对象...</color>");
        
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int problemCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj == null) 
            {
                Debug.LogWarning("<color=yellow>⚠️ 发现null GameObject引用</color>");
                problemCount++;
                continue;
            }
            
            // 检查HideFlags
            if (obj.hideFlags.HasFlag(HideFlags.DontSave) || 
                obj.hideFlags.HasFlag(HideFlags.DontSaveInEditor))
            {
                Debug.LogWarning($"<color=yellow>⚠️ 对象 {obj.name} 有DontSave标志</color>");
                problemCount++;
            }
            
            // 检查组件
            Component[] components = obj.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Debug.LogWarning($"<color=yellow>⚠️ 对象 {obj.name} 有空组件 (索引 {i})</color>");
                    problemCount++;
                }
            }
        }
        
        if (problemCount == 0)
        {
            Debug.Log("<color=green>✅ 未发现问题对象</color>");
        }
        else
        {
            Debug.Log($"<color=yellow>⚠️ 发现 {problemCount} 个潜在问题</color>");
            Debug.Log("<color=white>💡 使用 '快速修复断言错误' 来自动修复这些问题</color>");
        }
    }
    
    [MenuItem("Tools/项目构建/💾 安全保存项目")]
    public static void SafeSaveProject()
    {
        Debug.Log("<color=cyan>💾 安全保存项目...</color>");
        
        try
        {
            // 1. 先检查并修复问题
            CleanupMissingComponents();
            FixDontSaveInEditorIssues();
            
            // 2. 保存场景
            bool scenesSaved = EditorSceneManager.SaveOpenScenes();
            Debug.Log($"<color=green>✅ 场景保存: {(scenesSaved ? "成功" : "失败")}</color>");
            
            // 3. 保存资源
            AssetDatabase.SaveAssets();
            Debug.Log("<color=green>✅ 资源保存: 成功</color>");
            
            // 4. 刷新
            AssetDatabase.Refresh();
            Debug.Log("<color=green>✅ 资源刷新: 成功</color>");
            
            Debug.Log("<color=green>🎉 项目安全保存完成！</color>");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=red>❌ 保存项目时出错: {e.Message}</color>");
        }
    }
    
    #endif
} 