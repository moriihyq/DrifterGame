using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Linq;

public class TilemapBreakSystemCreator : EditorWindow
{
    [MenuItem("Tools/创建Tilemap破坏系统")]
    public static void ShowWindow()
    {
        GetWindow<TilemapBreakSystemCreator>("Tilemap破坏系统创建器");
    }
    
    private Tilemap selectedTilemap;
    private GameObject playerObject;
    private bool setupBreakableManager = true;
    private bool setupPlayerIntegration = true;
    private bool addSampleEffects = true;
    
    // 示例设置
    private int tileHealth = 2;
    private float attackRange = 2f;
    private float detectionRadius = 1.5f;
    private int debrisCount = 5;
    private float dropChance = 0.3f;
    
    private void OnGUI()
    {
        GUILayout.Label("Tilemap破坏系统创建器", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        // Tilemap选择
        GUILayout.Label("1. 选择要设置的Tilemap:");
        selectedTilemap = (Tilemap)EditorGUILayout.ObjectField("目标Tilemap", selectedTilemap, typeof(Tilemap), true);
        
        GUILayout.Space(10);
        
        // 玩家对象选择
        GUILayout.Label("2. 选择玩家对象:");
        playerObject = (GameObject)EditorGUILayout.ObjectField("玩家对象", playerObject, typeof(GameObject), true);
        
        GUILayout.Space(10);
        
        // 组件设置选项
        GUILayout.Label("3. 设置选项:");
        setupBreakableManager = EditorGUILayout.Toggle("设置BreakableTilemapManager", setupBreakableManager);
        setupPlayerIntegration = EditorGUILayout.Toggle("设置PlayerAttackTilemapIntegration", setupPlayerIntegration);
        addSampleEffects = EditorGUILayout.Toggle("添加示例效果", addSampleEffects);
        
        GUILayout.Space(10);
        
        // 参数设置
        GUILayout.Label("4. 基础参数:");
        tileHealth = EditorGUILayout.IntSlider("瓦片血量", tileHealth, 1, 10);
        attackRange = EditorGUILayout.Slider("攻击范围", attackRange, 0.5f, 5f);
        detectionRadius = EditorGUILayout.Slider("检测半径", detectionRadius, 0.5f, 3f);
        debrisCount = EditorGUILayout.IntSlider("碎片数量", debrisCount, 1, 15);
        dropChance = EditorGUILayout.Slider("掉落概率", dropChance, 0f, 1f);
        
        GUILayout.Space(20);
        
        // 创建按钮
        if (GUILayout.Button("创建破坏系统", GUILayout.Height(40)))
        {
            CreateBreakSystem();
        }
        
        GUILayout.Space(10);
        
        // 自动检测按钮
        if (GUILayout.Button("自动检测场景对象", GUILayout.Height(25)))
        {
            AutoDetectObjects();
        }
        
        GUILayout.Space(10);
        
        // 帮助信息
        EditorGUILayout.HelpBox(
            "这个工具会自动设置完整的Tilemap破坏系统。\n" +
            "1. 选择要设置破坏功能的Tilemap\n" +
            "2. 选择玩家对象（有PlayerAttackSystem的对象）\n" +
            "3. 调整参数后点击创建\n" +
            "4. 系统会自动添加必要的组件和设置",
            MessageType.Info);
    }
    
    private void AutoDetectObjects()
    {
        // 自动检测Tilemap
        if (selectedTilemap == null)
        {
            Tilemap[] tilemaps = FindObjectsOfType<Tilemap>();
            if (tilemaps.Length > 0)
            {
                // 优先选择名称包含"wall"或"破坏"的Tilemap
                selectedTilemap = tilemaps.FirstOrDefault(t => 
                    t.name.ToLower().Contains("wall") || 
                    t.name.Contains("破坏") || 
                    t.name.Contains("可破坏")) ?? tilemaps[0];
            }
        }
        
        // 自动检测玩家对象
        if (playerObject == null)
        {
            PlayerAttackSystem playerAttackSystem = FindObjectOfType<PlayerAttackSystem>();
            if (playerAttackSystem != null)
            {
                playerObject = playerAttackSystem.gameObject;
            }
            else
            {
                // 查找标签为Player的对象
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    playerObject = player;
                }
            }
        }
        
        if (selectedTilemap != null || playerObject != null)
        {
            Debug.Log($"[TilemapBreakSystemCreator] 自动检测完成：Tilemap={selectedTilemap?.name}, Player={playerObject?.name}");
        }
        else
        {
            Debug.LogWarning("[TilemapBreakSystemCreator] 未能自动检测到合适的对象");
        }
    }
    
    private void CreateBreakSystem()
    {
        if (selectedTilemap == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择一个Tilemap！", "确定");
            return;
        }
        
        if (setupPlayerIntegration && playerObject == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择玩家对象！", "确定");
            return;
        }
        
        // 记录撤销操作
        Undo.SetCurrentGroupName("创建Tilemap破坏系统");
        int undoGroup = Undo.GetCurrentGroup();
        
        try
        {
            // 设置BreakableTilemapManager
            if (setupBreakableManager)
            {
                SetupBreakableTilemapManager();
            }
            
            // 设置PlayerAttackTilemapIntegration
            if (setupPlayerIntegration)
            {
                SetupPlayerIntegration();
            }
            
            // 添加示例效果
            if (addSampleEffects)
            {
                CreateSampleEffects();
            }
            
            Debug.Log("[TilemapBreakSystemCreator] ✓ Tilemap破坏系统创建完成！");
            EditorUtility.DisplayDialog("成功", "Tilemap破坏系统创建完成！\n请查看控制台获取详细信息。", "确定");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TilemapBreakSystemCreator] 创建过程中出现错误：{e.Message}");
            EditorUtility.DisplayDialog("错误", $"创建过程中出现错误：{e.Message}", "确定");
        }
        
        Undo.CollapseUndoOperations(undoGroup);
    }
    
    private void SetupBreakableTilemapManager()
    {
        // 检查是否已经有TilemapCollider2D
        TilemapCollider2D tilemapCollider = selectedTilemap.GetComponent<TilemapCollider2D>();
        if (tilemapCollider == null)
        {
            Undo.AddComponent<TilemapCollider2D>(selectedTilemap.gameObject);
            Debug.Log("[TilemapBreakSystemCreator] ✓ 添加了TilemapCollider2D组件");
        }
        
        // 检查是否已经有BreakableTilemapManager
        BreakableTilemapManager manager = selectedTilemap.GetComponent<BreakableTilemapManager>();
        if (manager == null)
        {
            manager = Undo.AddComponent<BreakableTilemapManager>(selectedTilemap.gameObject);
            Debug.Log("[TilemapBreakSystemCreator] ✓ 添加了BreakableTilemapManager组件");
        }
        
        // 配置BreakableTilemapManager参数
        SerializedObject serializedManager = new SerializedObject(manager);
        
        serializedManager.FindProperty("breakableTilemap").objectReferenceValue = selectedTilemap;
        serializedManager.FindProperty("detectionRadius").floatValue = detectionRadius;
        serializedManager.FindProperty("useHealthSystem").boolValue = true;
        serializedManager.FindProperty("tileHealth").intValue = tileHealth;
        serializedManager.FindProperty("createDebris").boolValue = true;
        serializedManager.FindProperty("debrisCount").intValue = debrisCount;
        serializedManager.FindProperty("dropChance").floatValue = dropChance;
        
        serializedManager.ApplyModifiedProperties();
        
        Debug.Log("[TilemapBreakSystemCreator] ✓ 配置了BreakableTilemapManager参数");
    }
    
    private void SetupPlayerIntegration()
    {
        // 检查是否已经有PlayerAttackTilemapIntegration
        PlayerAttackTilemapIntegration integration = playerObject.GetComponent<PlayerAttackTilemapIntegration>();
        if (integration == null)
        {
            integration = Undo.AddComponent<PlayerAttackTilemapIntegration>(playerObject);
            Debug.Log("[TilemapBreakSystemCreator] ✓ 添加了PlayerAttackTilemapIntegration组件");
        }
        
        // 配置PlayerAttackTilemapIntegration参数
        SerializedObject serializedIntegration = new SerializedObject(integration);
        
        // 设置图层遮罩（包含Tilemap所在的图层）
        int tilemapLayer = selectedTilemap.gameObject.layer;
        LayerMask layerMask = 1 << tilemapLayer;
        serializedIntegration.FindProperty("tilemapLayers").intValue = layerMask;
        serializedIntegration.FindProperty("tilemapAttackRange").floatValue = attackRange;
        serializedIntegration.FindProperty("showAttackRange").boolValue = true;
        
        serializedIntegration.ApplyModifiedProperties();
        
        Debug.Log($"[TilemapBreakSystemCreator] ✓ 配置了PlayerAttackTilemapIntegration参数（图层：{tilemapLayer}）");
    }
    
    private void CreateSampleEffects()
    {
        // 创建示例音效目录
        string audioPath = "Assets/Audio/TilemapBreak";
        if (!AssetDatabase.IsValidFolder(audioPath))
        {
            string[] folders = audioPath.Split('/');
            string currentPath = folders[0];
            
            for (int i = 1; i < folders.Length; i++)
            {
                string newPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = newPath;
            }
            
            Debug.Log($"[TilemapBreakSystemCreator] ✓ 创建了音效目录：{audioPath}");
        }
        
        // 创建示例特效预制体目录
        string effectPath = "Assets/Prefabs/Effects";
        if (!AssetDatabase.IsValidFolder(effectPath))
        {
            string[] folders = effectPath.Split('/');
            string currentPath = folders[0];
            
            for (int i = 1; i < folders.Length; i++)
            {
                string newPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = newPath;
            }
            
            Debug.Log($"[TilemapBreakSystemCreator] ✓ 创建了特效目录：{effectPath}");
        }
        
        Debug.Log("[TilemapBreakSystemCreator] ✓ 示例效果目录创建完成，您可以在这些目录中添加音效和特效");
    }
    
    /// <summary>
    /// 验证系统设置
    /// </summary>
    private void ValidateSystemSetup()
    {
        if (selectedTilemap == null) return;
        
        bool hasCollider = selectedTilemap.GetComponent<TilemapCollider2D>() != null;
        bool hasManager = selectedTilemap.GetComponent<BreakableTilemapManager>() != null;
        bool hasIntegration = playerObject != null && playerObject.GetComponent<PlayerAttackTilemapIntegration>() != null;
        
        if (hasCollider && hasManager && hasIntegration)
        {
            Debug.Log("[TilemapBreakSystemCreator] ✓ 系统验证通过，所有组件都已正确设置");
        }
        else
        {
            Debug.LogWarning($"[TilemapBreakSystemCreator] ⚠ 系统验证：Collider={hasCollider}, Manager={hasManager}, Integration={hasIntegration}");
        }
    }
} 