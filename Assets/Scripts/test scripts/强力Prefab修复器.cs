using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
#endif

/// <summary>
/// 强力Prefab修复器 - 专门处理顽固的Prefab缺失脚本问题
/// 使用文本编辑方式直接修复Prefab文件
/// </summary>
public class 强力Prefab修复器 : MonoBehaviour
{
    #if UNITY_EDITOR
    
    [MenuItem("Tools/项目构建/💪 强力修复顽固Prefab")]
    public static void ForceFixStubbornPrefabs()
    {
        Debug.Log("<color=cyan>💪 开始强力修复顽固Prefab问题...</color>");
        
        // 专门处理这两个问题Prefab
        string[] problematicPrefabs = {
            "Assets/UI_set/Save Slot Prefab.prefab",
            "Assets/UI_set/saveMessagePrefab.prefab"
        };
        
        int fixedCount = 0;
        
        foreach (string prefabPath in problematicPrefabs)
        {
            if (ForceFixPrefabFile(prefabPath))
            {
                fixedCount++;
            }
        }
        
        if (fixedCount > 0)
        {
            // 强制刷新资源
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"<color=green>✅ 强力修复完成！成功修复 {fixedCount} 个Prefab</color>");
            Debug.Log("<color=yellow>💡 建议重启Unity编辑器以确保修复生效</color>");
        }
        else
        {
            Debug.LogWarning("<color=yellow>⚠️ 没有找到需要修复的Prefab文件</color>");
        }
    }
    
    [MenuItem("Tools/项目构建/🔥 完全重建问题Prefab")]
    public static void CompletelyRebuildPrefabs()
    {
        Debug.Log("<color=cyan>🔥 开始完全重建问题Prefab...</color>");
        
        // 重建Save Slot Prefab
        if (RebuildSaveSlotPrefab())
        {
            Debug.Log("<color=green>✅ 成功重建 Save Slot Prefab</color>");
        }
        
        // 重建saveMessagePrefab
        if (RebuildSaveMessagePrefab())
        {
            Debug.Log("<color=green>✅ 成功重建 saveMessagePrefab</color>");
        }
        
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        
        Debug.Log("<color=green>🎉 Prefab重建完成！</color>");
    }
    
    private static bool ForceFixPrefabFile(string prefabPath)
    {
        string fullPath = Path.Combine(Application.dataPath.Replace("Assets", ""), prefabPath);
        
        if (!File.Exists(fullPath))
        {
            Debug.LogWarning($"<color=yellow>⚠️ 找不到文件: {prefabPath}</color>");
            return false;
        }
        
        try
        {
            Debug.Log($"<color=white>🔧 强力修复: {prefabPath}</color>");
            
            // 读取Prefab文件内容
            string content = File.ReadAllText(fullPath);
            string originalContent = content;
            
            // 移除缺失的脚本引用
            // 匹配模式：m_Script: {fileID: 11500000, guid: [guid], type: 3}
            // 但是对应的脚本文件不存在或损坏
            
            // 1. 移除包含无效GUID的脚本引用
            content = RemoveInvalidScriptReferences(content);
            
            // 2. 移除空的组件引用
            content = RemoveEmptyComponentReferences(content);
            
            // 3. 修复组件数组索引
            content = FixComponentArrayIndices(content);
            
            // 4. 移除孤立的MonoBehaviour组件
            content = RemoveOrphanedMonoBehaviours(content);
            
            if (content != originalContent)
            {
                // 备份原文件
                string backupPath = fullPath + ".backup";
                File.Copy(fullPath, backupPath, true);
                Debug.Log($"<color=white>📦 已备份原文件到: {backupPath}</color>");
                
                // 写入修复后的内容
                File.WriteAllText(fullPath, content);
                
                Debug.Log($"<color=green>✅ 成功修复 {prefabPath}</color>");
                return true;
            }
            else
            {
                Debug.Log($"<color=white>ℹ️ {prefabPath} 无需修复</color>");
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=red>❌ 修复失败 {prefabPath}: {e.Message}</color>");
            return false;
        }
    }
    
    private static string RemoveInvalidScriptReferences(string content)
    {
        // 移除包含无效GUID的m_Script引用
        // 这些通常指向已删除的脚本文件
        
        string pattern = @"^\s*m_Script:\s*\{fileID:\s*11500000,\s*guid:\s*[a-fA-F0-9]{32},\s*type:\s*3\}\s*$";
        
        string[] lines = content.Split('\n');
        var validLines = new System.Collections.Generic.List<string>();
        
        bool skipNextFewLines = false;
        int skipCount = 0;
        
        for (int i = 0; i < lines.Length; i++)
        {
            if (skipNextFewLines && skipCount > 0)
            {
                skipCount--;
                continue;
            }
            skipNextFewLines = false;
            
            string line = lines[i];
            
            // 检查是否是脚本引用行
            if (Regex.IsMatch(line, pattern))
            {
                // 检查这个脚本引用是否有效
                // 如果下面几行包含MonoBehaviour组件定义但没有实际内容，说明脚本缺失
                bool hasValidScript = false;
                
                for (int j = i + 1; j < lines.Length && j < i + 10; j++)
                {
                    if (lines[j].Contains("m_Name:") || lines[j].Contains("m_Enabled:"))
                    {
                        hasValidScript = true;
                        break;
                    }
                    if (lines[j].StartsWith("---"))
                    {
                        break;
                    }
                }
                
                if (!hasValidScript)
                {
                    Debug.Log($"<color=yellow>移除无效脚本引用: {line.Trim()}</color>");
                    
                    // 跳过这个脚本引用和相关的MonoBehaviour定义
                    skipNextFewLines = true;
                    skipCount = 5; // 跳过接下来的几行
                    continue;
                }
            }
            
            validLines.Add(line);
        }
        
        return string.Join("\n", validLines.ToArray());
    }
    
    private static string RemoveEmptyComponentReferences(string content)
    {
        // 移除指向空对象的组件引用
        string pattern = @"^\s*-\s*component:\s*\{fileID:\s*0\}\s*$";
        
        string[] lines = content.Split('\n');
        var validLines = new System.Collections.Generic.List<string>();
        
        foreach (string line in lines)
        {
            if (!Regex.IsMatch(line, pattern))
            {
                validLines.Add(line);
            }
            else
            {
                Debug.Log($"<color=yellow>移除空组件引用: {line.Trim()}</color>");
            }
        }
        
        return string.Join("\n", validLines.ToArray());
    }
    
    private static string FixComponentArrayIndices(string content)
    {
        // 修复组件数组中的索引问题
        // 这个比较复杂，暂时简单处理
        return content;
    }
    
    private static string RemoveOrphanedMonoBehaviours(string content)
    {
        // 移除孤立的MonoBehaviour块（没有对应脚本的）
        string[] sections = content.Split(new string[] { "--- !u!" }, System.StringSplitOptions.None);
        var validSections = new System.Collections.Generic.List<string>();
        
        for (int i = 0; i < sections.Length; i++)
        {
            string section = sections[i];
            
            if (i == 0)
            {
                validSections.Add(section);
                continue;
            }
            
            // 检查是否是MonoBehaviour
            if (section.StartsWith("114 &"))
            {
                // 检查是否包含有效的脚本引用
                if (section.Contains("m_Script: {fileID: 11500000") && 
                    !section.Contains("m_Name:") && 
                    !section.Contains("m_Enabled:"))
                {
                    Debug.Log("<color=yellow>移除孤立的MonoBehaviour块</color>");
                    continue; // 跳过这个无效的MonoBehaviour
                }
            }
            
            validSections.Add("--- !u!" + section);
        }
        
        return string.Join("", validSections.ToArray());
    }
    
    private static bool RebuildSaveSlotPrefab()
    {
        try
        {
            Debug.Log("<color=cyan>🔧 重建 Save Slot Prefab...</color>");
            
            // 创建一个新的GameObject作为Save Slot
            GameObject saveSlot = new GameObject("Save Slot");
            
            // 添加基本组件
            var rectTransform = saveSlot.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 50);
            
            // 添加Image组件（如果需要背景）
            var image = saveSlot.AddComponent<UnityEngine.UI.Image>();
            
            // 添加Button组件
            var button = saveSlot.AddComponent<UnityEngine.UI.Button>();
            
            // 添加Text子对象
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(saveSlot.transform);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var text = textObj.AddComponent<UnityEngine.UI.Text>();
            text.text = "Save Slot";
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.alignment = TextAnchor.MiddleCenter;
            
            // 保存为Prefab
            string prefabPath = "Assets/UI_set/Save Slot Prefab.prefab";
            
            // 确保目录存在
            string directory = Path.GetDirectoryName(prefabPath);
            if (!AssetDatabase.IsValidFolder(directory))
            {
                Directory.CreateDirectory(directory.Replace("Assets/", Application.dataPath + "/"));
            }
            
            // 删除旧的Prefab（如果存在）
            if (File.Exists(prefabPath))
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }
            
            // 创建新的Prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(saveSlot, prefabPath);
            
            // 删除场景中的临时对象
            DestroyImmediate(saveSlot);
            
            if (prefab != null)
            {
                Debug.Log($"<color=green>✅ 成功重建 {prefabPath}</color>");
                return true;
            }
            
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=red>❌ 重建Save Slot Prefab失败: {e.Message}</color>");
            return false;
        }
    }
    
    private static bool RebuildSaveMessagePrefab()
    {
        try
        {
            Debug.Log("<color=cyan>🔧 重建 saveMessagePrefab...</color>");
            
            // 创建一个新的GameObject作为Save Message
            GameObject saveMessage = new GameObject("Save Message");
            
            // 添加基本组件
            var rectTransform = saveMessage.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(300, 100);
            
            // 添加Image组件作为背景
            var image = saveMessage.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0, 0, 0, 0.8f); // 半透明黑色背景
            
            // 添加Text子对象
            GameObject textObj = new GameObject("Message Text");
            textObj.transform.SetParent(saveMessage.transform);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);
            
            var text = textObj.AddComponent<UnityEngine.UI.Text>();
            text.text = "游戏已保存";
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.fontSize = 16;
            
            // 保存为Prefab
            string prefabPath = "Assets/UI_set/saveMessagePrefab.prefab";
            
            // 确保目录存在
            string directory = Path.GetDirectoryName(prefabPath);
            if (!AssetDatabase.IsValidFolder(directory))
            {
                Directory.CreateDirectory(directory.Replace("Assets/", Application.dataPath + "/"));
            }
            
            // 删除旧的Prefab（如果存在）
            if (File.Exists(prefabPath))
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }
            
            // 创建新的Prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(saveMessage, prefabPath);
            
            // 删除场景中的临时对象
            DestroyImmediate(saveMessage);
            
            if (prefab != null)
            {
                Debug.Log($"<color=green>✅ 成功重建 {prefabPath}</color>");
                return true;
            }
            
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=red>❌ 重建saveMessagePrefab失败: {e.Message}</color>");
            return false;
        }
    }
    
    [MenuItem("Tools/项目构建/🧹 删除损坏Prefab并重建")]
    public static void DeleteAndRebuildPrefabs()
    {
        Debug.Log("<color=cyan>🧹 删除损坏的Prefab并重建...</color>");
        
        string[] problematicPrefabs = {
            "Assets/UI_set/Save Slot Prefab.prefab",
            "Assets/UI_set/saveMessagePrefab.prefab"
        };
        
        // 删除损坏的Prefab
        foreach (string prefabPath in problematicPrefabs)
        {
            if (File.Exists(prefabPath))
            {
                AssetDatabase.DeleteAsset(prefabPath);
                Debug.Log($"<color=yellow>🗑️ 已删除损坏的Prefab: {prefabPath}</color>");
            }
        }
        
        // 刷新资源数据库
        AssetDatabase.Refresh();
        
        // 重建Prefab
        CompletelyRebuildPrefabs();
    }
    
    [MenuItem("Tools/项目构建/📊 检查Prefab文件完整性")]
    public static void CheckPrefabIntegrity()
    {
        Debug.Log("<color=cyan>📊 检查Prefab文件完整性...</color>");
        
        string[] problematicPrefabs = {
            "Assets/UI_set/Save Slot Prefab.prefab",
            "Assets/UI_set/saveMessagePrefab.prefab"
        };
        
        foreach (string prefabPath in problematicPrefabs)
        {
            CheckSinglePrefabIntegrity(prefabPath);
        }
    }
    
    private static void CheckSinglePrefabIntegrity(string prefabPath)
    {
        string fullPath = Path.Combine(Application.dataPath.Replace("Assets", ""), prefabPath);
        
        Debug.Log($"<color=white>📝 检查: {prefabPath}</color>");
        
        if (!File.Exists(fullPath))
        {
            Debug.LogWarning($"<color=red>❌ 文件不存在: {prefabPath}</color>");
            return;
        }
        
        try
        {
            // 尝试加载Prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (prefab == null)
            {
                Debug.LogError($"<color=red>❌ 无法加载Prefab: {prefabPath}</color>");
                return;
            }
            
            // 检查缺失组件
            Component[] components = prefab.GetComponentsInChildren<Component>(true);
            int missingCount = 0;
            
            foreach (Component comp in components)
            {
                if (comp == null)
                {
                    missingCount++;
                }
            }
            
            if (missingCount > 0)
            {
                Debug.LogWarning($"<color=yellow>⚠️ {prefabPath} 包含 {missingCount} 个缺失组件</color>");
            }
            else
            {
                Debug.Log($"<color=green>✅ {prefabPath} 完整性检查通过</color>");
            }
            
            // 检查文件内容
            string content = File.ReadAllText(fullPath);
            
            if (content.Contains("m_Script: {fileID: 11500000, guid: 00000000000000000000000000000000"))
            {
                Debug.LogWarning($"<color=yellow>⚠️ {prefabPath} 包含无效的脚本GUID</color>");
            }
            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=red>❌ 检查Prefab时出错 {prefabPath}: {e.Message}</color>");
        }
    }
    
    #endif
} 