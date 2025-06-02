using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
#endif

/// <summary>
/// 编译错误检查器 - 专门用于检查和修复编译错误
/// </summary>
public class 编译错误检查器 : MonoBehaviour
{
    #if UNITY_EDITOR
    
    [MenuItem("Tools/项目构建/🔍 检查编译错误")]
    public static void CheckCompilationErrors()
    {
        Debug.Log("<color=cyan>===== 🔍 开始检查编译错误 =====</color>");
        
        // 1. 强制重新编译
        AssetDatabase.Refresh();
        
        // 2. 检查常见的编译错误模式
        CheckCommonErrors();
        
        // 3. 检查缺少的using语句
        CheckMissingUsings();
        
        // 4. 检查语法错误
        CheckSyntaxErrors();
        
        Debug.Log("<color=cyan>===== 🔍 编译错误检查完成 =====</color>");
        Debug.Log("<color=yellow>💡 如果仍有编译错误，请查看Console窗口的红色错误信息</color>");
    }
    
    [MenuItem("Tools/项目构建/🛠️ 修复常见编译错误")]
    public static void FixCommonCompilationErrors()
    {
        Debug.Log("<color=cyan>🛠️ 开始修复常见编译错误...</color>");
        
        int fixedCount = 0;
        
        // 1. 检查并修复缺少的using语句
        fixedCount += FixMissingUsings();
        
        // 2. 检查并修复常见的语法错误
        fixedCount += FixCommonSyntaxErrors();
        
        // 3. 刷新资源数据库
        AssetDatabase.Refresh();
        
        Debug.Log($"<color=green>🎉 修复完成！共修复了 {fixedCount} 个问题</color>");
        
        if (fixedCount > 0)
        {
            Debug.Log("<color=yellow>请等待重新编译完成...</color>");
        }
    }
    
    private static void CheckCommonErrors()
    {
        Debug.Log("<color=yellow>📝 检查常见编译错误模式...</color>");
        
        string[] scriptPaths = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
        
        foreach (string scriptPath in scriptPaths)
        {
            string relativePath = "Assets" + scriptPath.Substring(Application.dataPath.Length);
            
            try
            {
                string content = File.ReadAllText(scriptPath);
                
                // 检查常见错误模式
                CheckForCommonErrorPatterns(relativePath, content);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"<color=yellow>⚠️ 无法读取文件: {relativePath} - {e.Message}</color>");
            }
        }
    }
    
    private static void CheckForCommonErrorPatterns(string filePath, string content)
    {
        // 检查是否有不匹配的大括号
        int openBraces = 0;
        int closeBraces = 0;
        
        foreach (char c in content)
        {
            if (c == '{') openBraces++;
            if (c == '}') closeBraces++;
        }
        
        if (openBraces != closeBraces)
        {
            Debug.LogWarning($"<color=yellow>⚠️ {filePath}: 大括号不匹配 ({{ : {openBraces}, }} : {closeBraces})</color>");
        }
        
        // 检查是否缺少分号
        string[] lines = content.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (!string.IsNullOrEmpty(line) && 
                !line.StartsWith("//") && 
                !line.StartsWith("*") &&
                !line.StartsWith("using") &&
                !line.Contains("{") && 
                !line.Contains("}") &&
                !line.EndsWith(";") &&
                !line.EndsWith(":") &&
                !line.StartsWith("#"))
            {
                if (line.Contains("=") || line.Contains("return") || line.Contains("Debug.Log"))
                {
                    Debug.LogWarning($"<color=yellow>⚠️ {filePath}:{i+1}: 可能缺少分号: {line}</color>");
                }
            }
        }
    }
    
    private static void CheckMissingUsings()
    {
        Debug.Log("<color=yellow>📦 检查缺少的using语句...</color>");
        
        string[] scriptPaths = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
        
        foreach (string scriptPath in scriptPaths)
        {
            string relativePath = "Assets" + scriptPath.Substring(Application.dataPath.Length);
            
            try
            {
                string content = File.ReadAllText(scriptPath);
                CheckMissingUsingsInFile(relativePath, content);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"<color=yellow>⚠️ 无法读取文件: {relativePath} - {e.Message}</color>");
            }
        }
    }
    
    private static void CheckMissingUsingsInFile(string filePath, string content)
    {
        // 检查常见的using语句
        Dictionary<string, string> commonUsings = new Dictionary<string, string>
        {
            { "IPointerDownHandler", "UnityEngine.EventSystems" },
            { "IDragHandler", "UnityEngine.EventSystems" },
            { "IPointerUpHandler", "UnityEngine.EventSystems" },
            { "PointerEventData", "UnityEngine.EventSystems" },
            { "RectTransformUtility", "UnityEngine" },
            { "Slider", "UnityEngine.UI" },
            { "Image", "UnityEngine.UI" },
            { "Button", "UnityEngine.UI" },
            { "Text", "UnityEngine.UI" },
            { "Toggle", "UnityEngine.UI" },
            { "Dropdown", "UnityEngine.UI" },
            { "Scrollbar", "UnityEngine.UI" },
            { "EditorWindow", "UnityEditor" },
            { "MenuItem", "UnityEditor" },
            { "SerializedProperty", "UnityEditor" },
            { "PropertyDrawer", "UnityEditor" },
            { "System.Collections.Generic", "System.Collections.Generic" },
            { "System.IO", "System.IO" },
            { "System.Linq", "System.Linq" },
            { "System.Text", "System.Text" }
        };
        
        foreach (var kvp in commonUsings)
        {
            if (content.Contains(kvp.Key) && !content.Contains($"using {kvp.Value};"))
            {
                Debug.LogWarning($"<color=yellow>⚠️ {filePath}: 可能缺少 using {kvp.Value}; (使用了 {kvp.Key})</color>");
            }
        }
    }
    
    private static void CheckSyntaxErrors()
    {
        Debug.Log("<color=yellow>🔧 检查语法错误...</color>");
        
        // 这里可以添加更多语法检查
        Debug.Log("<color=white>💡 详细的语法错误请查看Unity Console窗口</color>");
    }
    
    private static int FixMissingUsings()
    {
        Debug.Log("<color=yellow>🔧 修复缺少的using语句...</color>");
        
        int fixedCount = 0;
        string[] scriptPaths = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
        
        foreach (string scriptPath in scriptPaths)
        {
            string relativePath = "Assets" + scriptPath.Substring(Application.dataPath.Length);
            
            try
            {
                string content = File.ReadAllText(scriptPath);
                string originalContent = content;
                
                // 添加常见的缺少的using语句
                content = AddMissingUsings(content);
                
                if (content != originalContent)
                {
                    File.WriteAllText(scriptPath, content);
                    Debug.Log($"<color=green>✅ 修复了 {relativePath} 的using语句</color>");
                    fixedCount++;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"<color=yellow>⚠️ 无法修复文件: {relativePath} - {e.Message}</color>");
            }
        }
        
        return fixedCount;
    }
    
    private static string AddMissingUsings(string content)
    {
        List<string> neededUsings = new List<string>();
        
        // 检查并添加需要的using语句
        if (content.Contains("IPointerDownHandler") || content.Contains("IDragHandler") || content.Contains("IPointerUpHandler") || content.Contains("PointerEventData"))
        {
            if (!content.Contains("using UnityEngine.EventSystems;"))
                neededUsings.Add("using UnityEngine.EventSystems;");
        }
        
        if (content.Contains("Slider") || content.Contains("Image") || content.Contains("Button") || content.Contains("Text"))
        {
            if (!content.Contains("using UnityEngine.UI;"))
                neededUsings.Add("using UnityEngine.UI;");
        }
        
        if (content.Contains("EditorWindow") || content.Contains("MenuItem") || content.Contains("SerializedProperty"))
        {
            if (!content.Contains("using UnityEditor;"))
                neededUsings.Add("using UnityEditor;");
        }
        
        if (content.Contains("List<") || content.Contains("Dictionary<"))
        {
            if (!content.Contains("using System.Collections.Generic;"))
                neededUsings.Add("using System.Collections.Generic;");
        }
        
        if (content.Contains("File.") || content.Contains("Directory."))
        {
            if (!content.Contains("using System.IO;"))
                neededUsings.Add("using System.IO;");
        }
        
        if (content.Contains(".Where(") || content.Contains(".Select(") || content.Contains(".FirstOrDefault("))
        {
            if (!content.Contains("using System.Linq;"))
                neededUsings.Add("using System.Linq;");
        }
        
        // 在文件开头添加缺少的using语句
        if (neededUsings.Count > 0)
        {
            string usingSection = string.Join("\n", neededUsings) + "\n";
            
            // 找到第一个非using行
            string[] lines = content.Split('\n');
            int insertIndex = 0;
            
            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].Trim().StartsWith("using") && !string.IsNullOrWhiteSpace(lines[i].Trim()))
                {
                    insertIndex = i;
                    break;
                }
            }
            
            if (insertIndex > 0)
            {
                List<string> newLines = new List<string>(lines);
                newLines.InsertRange(insertIndex, neededUsings);
                content = string.Join("\n", newLines);
            }
            else
            {
                content = usingSection + content;
            }
        }
        
        return content;
    }
    
    private static int FixCommonSyntaxErrors()
    {
        Debug.Log("<color=yellow>🔧 修复常见语法错误...</color>");
        
        // 这里可以添加常见语法错误的自动修复
        // 例如：缺少分号、不匹配的大括号等
        
        return 0;
    }
    
    #endif
} 