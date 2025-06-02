using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

using UnityEngine.UI;
/// <summary>
/// Unity编辑器工具 - 角色图层修复工具
/// 提供在编辑器中快速修复角色渲染层级的功能
/// </summary>
public class LayerFixEditorTool : EditorWindow
{
    private int playerSortingOrder = 10;
    private int enemySortingOrder = 10;
    private int npcSortingOrder = 10;
    private string customTag = "Player";
    private int customSortingOrder = 10;

    [MenuItem("Tools/角色图层修复工具")]
    public static void ShowWindow()
    {
        LayerFixEditorTool window = GetWindow<LayerFixEditorTool>();
        window.titleContent = new GUIContent("角色图层修复工具");
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("角色图层修复工具", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // 分析当前场景
        if (GUILayout.Button("分析当前场景的渲染层级", GUILayout.Height(30)))
        {
            AnalyzeSceneRenderingLayers();
        }
        
        GUILayout.Space(10);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Space(10);

        // 玩家设置
        GUILayout.Label("玩家角色设置", EditorStyles.boldLabel);
        playerSortingOrder = EditorGUILayout.IntField("玩家排序顺序:", playerSortingOrder);
        
        if (GUILayout.Button("修复所有玩家角色层级"))
        {
            FixCharactersByTag("Player", playerSortingOrder);
        }
        
        GUILayout.Space(10);

        // 敌人设置
        GUILayout.Label("敌人角色设置", EditorStyles.boldLabel);
        enemySortingOrder = EditorGUILayout.IntField("敌人排序顺序:", enemySortingOrder);
        
        if (GUILayout.Button("修复所有敌人角色层级"))
        {
            FixCharactersByTag("Enemy", enemySortingOrder);
        }
        
        GUILayout.Space(10);

        // NPC设置
        GUILayout.Label("NPC角色设置", EditorStyles.boldLabel);
        npcSortingOrder = EditorGUILayout.IntField("NPC排序顺序:", npcSortingOrder);
        
        if (GUILayout.Button("修复所有NPC角色层级"))
        {
            FixCharactersByTag("NPC", npcSortingOrder);
        }
        
        GUILayout.Space(10);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Space(10);

        // 自定义标签设置
        GUILayout.Label("自定义标签设置", EditorStyles.boldLabel);
        customTag = EditorGUILayout.TextField("自定义标签:", customTag);
        customSortingOrder = EditorGUILayout.IntField("排序顺序:", customSortingOrder);
        
        if (GUILayout.Button($"修复所有 '{customTag}' 标签的对象"))
        {
            FixCharactersByTag(customTag, customSortingOrder);
        }
        
        GUILayout.Space(10);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Space(10);

        // 全局操作
        GUILayout.Label("全局操作", EditorStyles.boldLabel);
        
        if (GUILayout.Button("修复所有SpriteRenderer对象（设为排序顺序10）", GUILayout.Height(25)))
        {
            FixAllSpriteRenderers(10);
        }
        
        if (GUILayout.Button("重置所有TilemapRenderer为标准层级", GUILayout.Height(25)))
        {
            ResetTilemapRenderers();
        }
        
        GUILayout.Space(10);

        // 帮助信息
        EditorGUILayout.HelpBox(
            "推荐设置:\n" +
            "• 背景: -10 到 -1\n" +
            "• 地面/墙壁: 0 到 5\n" +
            "• 游戏对象: 6 到 8\n" +
            "• 角色: 10 到 15\n" +
            "• UI效果: 16 到 20",
            MessageType.Info
        );
    }

    /// <summary>
    /// 根据标签修复角色的渲染层级
    /// </summary>
    private void FixCharactersByTag(string tag, int sortingOrder)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        
        if (objects.Length == 0)
        {
            Debug.LogWarning($"[LayerFixEditorTool] 未找到任何带有 '{tag}' 标签的对象");
            EditorUtility.DisplayDialog("警告", $"未找到任何带有 '{tag}' 标签的对象", "确定");
            return;
        }

        int fixedCount = 0;
        
        foreach (GameObject obj in objects)
        {
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Undo.RecordObject(sr, $"Fix Layer for {obj.name}");
                sr.sortingOrder = sortingOrder;
                fixedCount++;
                
                EditorUtility.SetDirty(sr);
                Debug.Log($"<color=green>[LayerFixEditorTool] 已修复 {obj.name} 的层级 (排序顺序: {sortingOrder})</color>");
            }
        }
        
        string message = $"已修复 {fixedCount} 个 '{tag}' 标签的对象\n排序顺序设置为: {sortingOrder}";
        Debug.Log($"<color=cyan>[LayerFixEditorTool] {message}</color>");
        EditorUtility.DisplayDialog("修复完成", message, "确定");
    }

    /// <summary>
    /// 修复所有SpriteRenderer对象
    /// </summary>
    private void FixAllSpriteRenderers(int sortingOrder)
    {
        SpriteRenderer[] spriteRenderers = FindObjectsOfType<SpriteRenderer>();
        
        if (spriteRenderers.Length == 0)
        {
            Debug.LogWarning("[LayerFixEditorTool] 场景中未找到任何SpriteRenderer对象");
            return;
        }

        foreach (SpriteRenderer sr in spriteRenderers)
        {
            Undo.RecordObject(sr, $"Fix Layer for {sr.gameObject.name}");
            sr.sortingOrder = sortingOrder;
            EditorUtility.SetDirty(sr);
        }
        
        string message = $"已修复 {spriteRenderers.Length} 个SpriteRenderer对象";
        Debug.Log($"<color=cyan>[LayerFixEditorTool] {message}</color>");
        EditorUtility.DisplayDialog("修复完成", message, "确定");
    }

    /// <summary>
    /// 重置所有TilemapRenderer为标准层级
    /// </summary>
    private void ResetTilemapRenderers()
    {
        TilemapRenderer[] tilemapRenderers = FindObjectsOfType<TilemapRenderer>();
        
        if (tilemapRenderers.Length == 0)
        {
            Debug.LogWarning("[LayerFixEditorTool] 场景中未找到任何TilemapRenderer对象");
            return;
        }

        int count = 0;
        foreach (TilemapRenderer tr in tilemapRenderers)
        {
            Undo.RecordObject(tr, $"Reset Tilemap Layer for {tr.gameObject.name}");
            
            // 根据名称设置标准层级
            if (tr.gameObject.name.ToLower().Contains("background"))
            {
                tr.sortingOrder = -1;
            }
            else if (tr.gameObject.name.ToLower().Contains("ground") || tr.gameObject.name.ToLower().Contains("floor"))
            {
                tr.sortingOrder = 0;
            }
            else if (tr.gameObject.name.ToLower().Contains("wall"))
            {
                tr.sortingOrder = count < 3 ? count : 2; // 墙壁层级0-2
            }
            else
            {
                tr.sortingOrder = 1; // 默认层级
            }
            
            EditorUtility.SetDirty(tr);
            count++;
        }
        
        string message = $"已重置 {tilemapRenderers.Length} 个TilemapRenderer对象的层级";
        Debug.Log($"<color=cyan>[LayerFixEditorTool] {message}</color>");
        EditorUtility.DisplayDialog("重置完成", message, "确定");
    }

    /// <summary>
    /// 分析场景中的渲染层级情况
    /// </summary>
    private void AnalyzeSceneRenderingLayers()
    {
        Debug.Log("<color=cyan>========== 场景渲染层级分析 ==========</color>");
        
        // 分析TilemapRenderer
        TilemapRenderer[] tilemapRenderers = FindObjectsOfType<TilemapRenderer>();
        Debug.Log($"<color=yellow>发现 {tilemapRenderers.Length} 个 TilemapRenderer:</color>");
        
        foreach (TilemapRenderer tr in tilemapRenderers)
        {
            Debug.Log($"- {tr.gameObject.name}: 排序层={tr.sortingLayerName}, 排序顺序={tr.sortingOrder}");
        }
        
        // 分析SpriteRenderer
        SpriteRenderer[] spriteRenderers = FindObjectsOfType<SpriteRenderer>();
        Debug.Log($"<color=yellow>发现 {spriteRenderers.Length} 个 SpriteRenderer:</color>");
        
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            string tag = sr.gameObject.tag;
            Debug.Log($"- {sr.gameObject.name} (Tag: {tag}): 排序层={sr.sortingLayerName}, 排序顺序={sr.sortingOrder}");
        }
        
        Debug.Log("<color=cyan>========================================</color>");
        
        EditorUtility.DisplayDialog("分析完成", "渲染层级分析完成，请查看Console面板获取详细信息", "确定");
    }
} 