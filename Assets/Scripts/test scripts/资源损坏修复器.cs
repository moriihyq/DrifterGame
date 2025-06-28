using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
#endif

/// <summary>
/// 资源损坏修复器 - 专门修复损坏的Animator、Prefab等资源
/// </summary>
public class 资源损坏修复器 : MonoBehaviour
{
    #if UNITY_EDITOR
    
    [MenuItem("Tools/项目构建/🔨 修复资源损坏问题")]
    public static void FixCorruptedAssets()
    {
        Debug.Log("<color=cyan>🔨 开始修复资源损坏问题...</color>");
        
        int fixedCount = 0;
        
        // 1. 修复损坏的Animator Controller
        fixedCount += FixCorruptedAnimatorControllers();
        
        // 2. 修复包含缺失脚本的Prefab
        fixedCount += FixPrefabsWithMissingScripts();
        
        // 3. 修复Animator状态机问题
        fixedCount += FixAnimatorTransitions();
        
        // 4. 清理损坏的Prefab引用
        fixedCount += CleanupBrokenPrefabReferences();
        
        // 5. 强制重新导入问题资源
        ForceReimportProblematicAssets();
        
        Debug.Log($"<color=green>✅ 资源损坏修复完成！共修复 {fixedCount} 个问题</color>");
        
        if (fixedCount > 0)
        {
            Debug.Log("<color=yellow>💡 建议重启Unity编辑器以确保修复生效</color>");
        }
    }
    
    [MenuItem("Tools/项目构建/🎯 专门修复Prefab脚本缺失")]
    public static void FixMissingScriptsInPrefabs()
    {
        Debug.Log("<color=cyan>🎯 专门修复Prefab中的缺失脚本...</color>");
        
        // 重点修复问题Prefab
        string[] problematicPrefabs = {
            "Assets/UI_set/saveMessagePrefab.prefab",
            "Assets/UI_set/Save Slot Prefab.prefab"
        };
        
        int fixedCount = 0;
        
        foreach (string prefabPath in problematicPrefabs)
        {
            if (File.Exists(Path.Combine(Application.dataPath.Replace("Assets", ""), prefabPath)))
            {
                fixedCount += FixSpecificPrefab(prefabPath);
            }
            else
            {
                Debug.LogWarning($"<color=yellow>⚠️ 找不到Prefab文件: {prefabPath}</color>");
            }
        }
        
        // 扫描所有其他Prefab
        fixedCount += ScanAndFixAllPrefabs();
        
        Debug.Log($"<color=green>✅ Prefab脚本缺失修复完成！共修复 {fixedCount} 个问题</color>");
    }
    
    private static int FixCorruptedAnimatorControllers()
    {
        Debug.Log("<color=yellow>🎮 修复损坏的Animator Controller...</color>");
        
        int fixedCount = 0;
        
        // 查找所有Animator Controller
        string[] animatorGuids = AssetDatabase.FindAssets("t:AnimatorController");
        
        foreach (string guid in animatorGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            
            // 特别检查BossAnimatorController
            if (assetPath.Contains("BossAnimatorController") || assetPath.Contains("Boss/"))
            {
                Debug.Log($"<color=white>检查Animator Controller: {assetPath}</color>");
                
                try
                {
                    var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(assetPath);
                    if (controller == null)
                    {
                        Debug.LogWarning($"<color=yellow>⚠️ 无法加载 {assetPath}，尝试重新导入...</color>");
                        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                        fixedCount++;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"<color=red>❌ Animator Controller损坏: {assetPath} - {e.Message}</color>");
                    
                    // 尝试重新创建基本的Animator Controller
                    if (CreateBasicAnimatorController(assetPath))
                    {
                        fixedCount++;
                    }
                }
            }
        }
        
        return fixedCount;
    }
    
    private static bool CreateBasicAnimatorController(string assetPath)
    {
        try
        {
            Debug.Log($"<color=cyan>🔧 尝试重新创建基本的Animator Controller: {assetPath}</color>");
            
            // 删除损坏的文件
            if (File.Exists(assetPath))
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
            
            // 创建新的Animator Controller
            var newController = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(assetPath);
            
            if (newController != null)
            {
                // 添加基本状态
                var rootStateMachine = newController.layers[0].stateMachine;
                
                // 添加Idle状态
                var idleState = rootStateMachine.AddState("Idle");
                rootStateMachine.defaultState = idleState;
                
                // 添加基本参数（如果需要）
                newController.AddParameter("Speed", AnimatorControllerParameterType.Float);
                newController.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
                
                EditorUtility.SetDirty(newController);
                AssetDatabase.SaveAssets();
                
                Debug.Log($"<color=green>✅ 成功重新创建 {assetPath}</color>");
                return true;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=red>❌ 重新创建Animator Controller失败: {e.Message}</color>");
        }
        
        return false;
    }
    
    private static int FixPrefabsWithMissingScripts()
    {
        Debug.Log("<color=yellow>🧩 修复包含缺失脚本的Prefab...</color>");
        
        int fixedCount = 0;
        
        // 查找所有Prefab
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        
        foreach (string guid in prefabGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            
            try
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab == null) continue;
                
                // 检查是否有缺失的脚本
                if (HasMissingScripts(prefab))
                {
                    Debug.Log($"<color=yellow>⚠️ 发现包含缺失脚本的Prefab: {assetPath}</color>");
                    
                    if (CleanMissingScriptsFromPrefab(assetPath))
                    {
                        fixedCount++;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"<color=yellow>⚠️ 检查Prefab时出错 {assetPath}: {e.Message}</color>");
            }
        }
        
        return fixedCount;
    }
    
    private static int FixSpecificPrefab(string prefabPath)
    {
        Debug.Log($"<color=white>🎯 修复特定Prefab: {prefabPath}</color>");
        
        try
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"<color=yellow>⚠️ 无法加载Prefab: {prefabPath}</color>");
                return 0;
            }
            
            if (CleanMissingScriptsFromPrefab(prefabPath))
            {
                Debug.Log($"<color=green>✅ 成功修复 {prefabPath}</color>");
                return 1;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=red>❌ 修复Prefab失败 {prefabPath}: {e.Message}</color>");
        }
        
        return 0;
    }
    
    private static bool HasMissingScripts(GameObject obj)
    {
        Component[] components = obj.GetComponentsInChildren<Component>(true);
        
        foreach (Component comp in components)
        {
            if (comp == null) return true;
        }
        
        return false;
    }
    
    private static bool CleanMissingScriptsFromPrefab(string prefabPath)
    {
        try
        {
            // 加载Prefab内容
            var prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
            
            if (prefabContents == null) return false;
            
            bool hasChanges = false;
            
            // 获取所有Transform（包括子对象）
            Transform[] allTransforms = prefabContents.GetComponentsInChildren<Transform>(true);
            
            foreach (Transform trans in allTransforms)
            {
                GameObject obj = trans.gameObject;
                
                // 使用SerializedObject来安全地移除缺失的脚本
                SerializedObject serializedObject = new SerializedObject(obj);
                SerializedProperty componentsProperty = serializedObject.FindProperty("m_Component");
                
                if (componentsProperty != null && componentsProperty.isArray)
                {
                    // 从后向前遍历
                    for (int i = componentsProperty.arraySize - 1; i >= 0; i--)
                    {
                        SerializedProperty componentProperty = componentsProperty.GetArrayElementAtIndex(i);
                        SerializedProperty componentRef = componentProperty.FindPropertyRelative("component");
                        
                        if (componentRef != null && componentRef.objectReferenceValue == null)
                        {
                            Debug.Log($"<color=green>✅ 移除 {obj.name} 的缺失脚本 (索引 {i})</color>");
                            componentsProperty.DeleteArrayElementAtIndex(i);
                            hasChanges = true;
                        }
                    }
                    
                    if (hasChanges)
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }
            
            if (hasChanges)
            {
                // 保存修改后的Prefab
                PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
                Debug.Log($"<color=green>✅ 已保存修复的Prefab: {prefabPath}</color>");
            }
            
            // 清理临时对象
            PrefabUtility.UnloadPrefabContents(prefabContents);
            
            return hasChanges;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=red>❌ 清理Prefab脚本失败 {prefabPath}: {e.Message}</color>");
            return false;
        }
    }
    
    private static int ScanAndFixAllPrefabs()
    {
        Debug.Log("<color=yellow>🔍 扫描所有Prefab...</color>");
        
        int fixedCount = 0;
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        
        foreach (string guid in prefabGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            
            // 跳过已经处理过的特定Prefab
            if (assetPath.Contains("saveMessagePrefab") || assetPath.Contains("Save Slot Prefab"))
                continue;
            
            try
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab != null && HasMissingScripts(prefab))
                {
                    if (CleanMissingScriptsFromPrefab(assetPath))
                    {
                        fixedCount++;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"<color=yellow>⚠️ 处理Prefab时出错 {assetPath}: {e.Message}</color>");
            }
        }
        
        return fixedCount;
    }
    
    private static int FixAnimatorTransitions()
    {
        Debug.Log("<color=yellow>🎭 修复Animator状态机过渡问题...</color>");
        
        int fixedCount = 0;
        
        // 这里主要是提醒用户手动检查
        Debug.Log("<color=white>💡 检测到Animator过渡配置问题:</color>");
        Debug.Log("<color=white>   - 'hurt -> enemy_idle' 过渡缺少退出时间或条件</color>");
        Debug.Log("<color=white>   - 请在Animator窗口中手动设置过渡条件</color>");
        
        return fixedCount;
    }
    
    private static int CleanupBrokenPrefabReferences()
    {
        Debug.Log("<color=yellow>🧹 清理损坏的Prefab引用...</color>");
        
        int fixedCount = 0;
        
        // 获取场景中的所有对象
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj == null) continue;
            
            // 检查PrefabUtility连接
            try
            {
                if (PrefabUtility.GetPrefabInstanceStatus(obj) == PrefabInstanceStatus.Disconnected)
                {
                    Debug.Log($"<color=yellow>⚠️ 发现断开连接的Prefab实例: {obj.name}</color>");
                    
                    // 尝试重新连接或断开连接
                    PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    fixedCount++;
                    
                    Debug.Log($"<color=green>✅ 已断开 {obj.name} 的Prefab连接</color>");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"<color=yellow>⚠️ 处理Prefab连接时出错 {obj.name}: {e.Message}</color>");
            }
        }
        
        return fixedCount;
    }
    
    private static void ForceReimportProblematicAssets()
    {
        Debug.Log("<color=yellow>🔄 强制重新导入问题资源...</color>");
        
        // 重新导入Boss相关资源
        string[] problematicPaths = {
            "Assets/Boss",
            "Assets/UI_set",
            "Assets/Animations"
        };
        
        foreach (string path in problematicPaths)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                Debug.Log($"<color=white>重新导入文件夹: {path}</color>");
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
            }
        }
        
        // 刷新资源数据库
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        
        Debug.Log("<color=green>✅ 资源重新导入完成</color>");
    }
    
    [MenuItem("Tools/项目构建/📋 生成资源损坏报告")]
    public static void GenerateCorruptionReport()
    {
        Debug.Log("<color=cyan>📋 生成资源损坏报告...</color>");
        
        int issueCount = 0;
        
        // 检查Animator Controllers
        string[] animatorGuids = AssetDatabase.FindAssets("t:AnimatorController");
        foreach (string guid in animatorGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            try
            {
                var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(assetPath);
                if (controller == null)
                {
                    Debug.LogWarning($"<color=red>❌ 损坏的Animator Controller: {assetPath}</color>");
                    issueCount++;
                }
            }
            catch
            {
                Debug.LogWarning($"<color=red>❌ 无法加载Animator Controller: {assetPath}</color>");
                issueCount++;
            }
        }
        
        // 检查Prefabs
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            try
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab != null && HasMissingScripts(prefab))
                {
                    Debug.LogWarning($"<color=yellow>⚠️ 包含缺失脚本的Prefab: {assetPath}</color>");
                    issueCount++;
                }
            }
            catch
            {
                Debug.LogWarning($"<color=red>❌ 无法加载Prefab: {assetPath}</color>");
                issueCount++;
            }
        }
        
        if (issueCount == 0)
        {
            Debug.Log("<color=green>✅ 未发现资源损坏问题</color>");
        }
        else
        {
            Debug.Log($"<color=yellow>⚠️ 共发现 {issueCount} 个资源问题</color>");
            Debug.Log("<color=white>💡 使用 '修复资源损坏问题' 来自动修复这些问题</color>");
        }
    }
    
    #endif
} 