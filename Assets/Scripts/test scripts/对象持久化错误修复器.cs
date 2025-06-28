using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
#endif

/// <summary>
/// 对象持久化错误修复器 - 修复Unity断言错误：kDontSaveInEditor
/// 这类错误通常由无效的对象引用或已删除的组件造成
/// </summary>
public class 对象持久化错误修复器 : MonoBehaviour
{
    #if UNITY_EDITOR
    
    [MenuItem("Tools/项目构建/🔧 修复对象持久化错误")]
    public static void FixObjectPersistenceErrors()
    {
        Debug.Log("<color=cyan>🔧 开始修复对象持久化错误...</color>");
        
        int fixedCount = 0;
        
        // 1. 清理无效的对象引用
        fixedCount += CleanupInvalidReferences();
        
        // 2. 修复场景中的无效组件
        fixedCount += FixInvalidComponents();
        
        // 3. 清理Prefab中的无效引用
        fixedCount += CleanupPrefabReferences();
        
        // 4. 强制保存所有资源
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveOpenScenes();
        
        Debug.Log($"<color=green>✅ 对象持久化错误修复完成！共修复 {fixedCount} 个问题</color>");
        
        if (fixedCount > 0)
        {
            Debug.Log("<color=yellow>💡 建议重启Unity编辑器以确保修复生效</color>");
        }
    }
    
    [MenuItem("Tools/项目构建/🔍 检查对象持久化问题")]
    public static void CheckObjectPersistenceIssues()
    {
        Debug.Log("<color=cyan>🔍 检查对象持久化问题...</color>");
        
        // 检查当前场景
        CheckCurrentScene();
        
        // 检查所有场景
        CheckAllScenes();
        
        // 检查Prefabs
        CheckPrefabs();
        
        Debug.Log("<color=green>✅ 对象持久化问题检查完成</color>");
    }
    
    private static int CleanupInvalidReferences()
    {
        Debug.Log("<color=yellow>🧹 清理无效的对象引用...</color>");
        
        int cleanedCount = 0;
        
        // 获取当前场景中的所有GameObject
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj == null) continue;
            
            // 检查组件是否有无效引用
            Component[] components = obj.GetComponents<Component>();
            
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Debug.LogWarning($"<color=yellow>⚠️ 发现空组件在 {obj.name}，位置 {i}</color>");
                    
                    // 移除空组件引用
                    var serializedObject = new SerializedObject(obj);
                    var componentsProperty = serializedObject.FindProperty("m_Component");
                    
                    if (componentsProperty != null && i < componentsProperty.arraySize)
                    {
                        componentsProperty.DeleteArrayElementAtIndex(i);
                        serializedObject.ApplyModifiedProperties();
                        cleanedCount++;
                        Debug.Log($"<color=green>✅ 已清理 {obj.name} 的空组件引用</color>");
                    }
                }
            }
        }
        
        return cleanedCount;
    }
    
    private static int FixInvalidComponents()
    {
        Debug.Log("<color=yellow>🔧 修复场景中的无效组件...</color>");
        
        int fixedCount = 0;
        
        // 查找所有MonoBehaviour组件
        MonoBehaviour[] allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
        
        foreach (MonoBehaviour mb in allMonoBehaviours)
        {
            if (mb == null) continue;
            
            try
            {
                // 检查组件是否有效
                var serializedObject = new SerializedObject(mb);
                var iterator = serializedObject.GetIterator();
                
                while (iterator.NextVisible(true))
                {
                    if (iterator.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (iterator.objectReferenceValue != null && 
                            iterator.objectReferenceValue.name == "Missing Prefab")
                        {
                            Debug.LogWarning($"<color=yellow>⚠️ 发现缺失的Prefab引用在 {mb.name}.{iterator.propertyPath}</color>");
                            iterator.objectReferenceValue = null;
                            fixedCount++;
                        }
                    }
                }
                
                serializedObject.ApplyModifiedProperties();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"<color=yellow>⚠️ 检查组件时出错 {mb.name}: {e.Message}</color>");
            }
        }
        
        return fixedCount;
    }
    
    private static int CleanupPrefabReferences()
    {
        Debug.Log("<color=yellow>🧹 清理Prefab中的无效引用...</color>");
        
        int cleanedCount = 0;
        
        // 查找所有Prefab文件
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        
        foreach (string guid in prefabGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            
            if (prefab == null) continue;
            
            try
            {
                // 检查Prefab的组件
                Component[] components = prefab.GetComponentsInChildren<Component>(true);
                
                bool needSave = false;
                foreach (Component comp in components)
                {
                    if (comp == null)
                    {
                        Debug.LogWarning($"<color=yellow>⚠️ Prefab {assetPath} 包含空组件</color>");
                        needSave = true;
                        cleanedCount++;
                    }
                }
                
                if (needSave)
                {
                    EditorUtility.SetDirty(prefab);
                    Debug.Log($"<color=green>✅ 已标记 {assetPath} 需要保存</color>");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"<color=yellow>⚠️ 检查Prefab时出错 {assetPath}: {e.Message}</color>");
            }
        }
        
        return cleanedCount;
    }
    
    private static void CheckCurrentScene()
    {
        Debug.Log("<color=yellow>🔍 检查当前场景...</color>");
        
        Scene currentScene = SceneManager.GetActiveScene();
        Debug.Log($"<color=white>当前场景: {currentScene.name}</color>");
        
        // 获取场景中的所有根对象
        GameObject[] rootObjects = currentScene.GetRootGameObjects();
        
        foreach (GameObject rootObj in rootObjects)
        {
            CheckGameObjectRecursively(rootObj);
        }
    }
    
    private static void CheckAllScenes()
    {
        Debug.Log("<color=yellow>🔍 检查所有场景...</color>");
        
        // 获取构建设置中的所有场景
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        
        foreach (var sceneSettings in scenes)
        {
            if (!sceneSettings.enabled) continue;
            
            try
            {
                Scene scene = EditorSceneManager.OpenScene(sceneSettings.path, OpenSceneMode.Additive);
                Debug.Log($"<color=white>检查场景: {scene.name}</color>");
                
                GameObject[] rootObjects = scene.GetRootGameObjects();
                foreach (GameObject rootObj in rootObjects)
                {
                    CheckGameObjectRecursively(rootObj);
                }
                
                EditorSceneManager.CloseScene(scene, true);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"<color=yellow>⚠️ 无法检查场景 {sceneSettings.path}: {e.Message}</color>");
            }
        }
    }
    
    private static void CheckGameObjectRecursively(GameObject obj)
    {
        if (obj == null) return;
        
        // 检查组件
        Component[] components = obj.GetComponents<Component>();
        
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == null)
            {
                Debug.LogWarning($"<color=yellow>⚠️ {obj.name} 包含空组件 (索引 {i})</color>");
            }
        }
        
        // 递归检查子对象
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            CheckGameObjectRecursively(obj.transform.GetChild(i).gameObject);
        }
    }
    
    private static void CheckPrefabs()
    {
        Debug.Log("<color=yellow>🔍 检查Prefabs...</color>");
        
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        int checkedCount = 0;
        int issueCount = 0;
        
        foreach (string guid in prefabGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            
            if (prefab == null) continue;
            
            checkedCount++;
            
            // 检查Prefab及其子对象
            if (HasInvalidComponents(prefab))
            {
                Debug.LogWarning($"<color=yellow>⚠️ Prefab有问题: {assetPath}</color>");
                issueCount++;
            }
        }
        
        Debug.Log($"<color=white>检查了 {checkedCount} 个Prefab，发现 {issueCount} 个问题</color>");
    }
    
    private static bool HasInvalidComponents(GameObject obj)
    {
        Component[] components = obj.GetComponentsInChildren<Component>(true);
        
        foreach (Component comp in components)
        {
            if (comp == null) return true;
        }
        
        return false;
    }
    
    [MenuItem("Tools/项目构建/🔄 强制刷新并重新序列化")]
    public static void ForceRefreshAndReserialize()
    {
        Debug.Log("<color=cyan>🔄 强制刷新并重新序列化所有资源...</color>");
        
        // 1. 刷新资源数据库
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        
        // 2. 重新序列化所有资源
        AssetDatabase.ForceReserializeAssets();
        
        // 3. 保存项目
        AssetDatabase.SaveAssets();
        
        // 4. 保存场景
        EditorSceneManager.SaveOpenScenes();
        
        Debug.Log("<color=green>✅ 强制刷新完成！</color>");
        Debug.Log("<color=yellow>💡 如果问题仍然存在，请重启Unity编辑器</color>");
    }
    
    #endif
} 