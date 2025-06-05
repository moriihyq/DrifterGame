using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CollidableTileBreaker))]
public class CollidableTileBreakerEditor : Editor
{
    private SerializedProperty collidableTilemap;
    private SerializedProperty tileHitPoints;
    private SerializedProperty attackDetectionRange;
    private SerializedProperty playerLayer;
    
    private SerializedProperty enableBreakEffects;
    private SerializedProperty particleColor;
    private SerializedProperty particleCount;
    private SerializedProperty particleSpeed;
    private SerializedProperty particleLifetime;
    
    private SerializedProperty breakSounds;
    private SerializedProperty soundVolume;
    
    private SerializedProperty enableDrops;
    private SerializedProperty dropPrefabs;
    private SerializedProperty dropChance;
    
    private SerializedProperty showDebugGizmos;
    private SerializedProperty logDebugInfo;
    
    // 预览相关
    private bool showPreview = false;
    private Vector3 previewPosition = Vector3.zero;
    
    private void OnEnable()
    {
        // 获取序列化属性
        collidableTilemap = serializedObject.FindProperty("collidableTilemap");
        tileHitPoints = serializedObject.FindProperty("tileHitPoints");
        attackDetectionRange = serializedObject.FindProperty("attackDetectionRange");
        playerLayer = serializedObject.FindProperty("playerLayer");
        
        enableBreakEffects = serializedObject.FindProperty("enableBreakEffects");
        particleColor = serializedObject.FindProperty("particleColor");
        particleCount = serializedObject.FindProperty("particleCount");
        particleSpeed = serializedObject.FindProperty("particleSpeed");
        particleLifetime = serializedObject.FindProperty("particleLifetime");
        
        breakSounds = serializedObject.FindProperty("breakSounds");
        soundVolume = serializedObject.FindProperty("soundVolume");
        
        enableDrops = serializedObject.FindProperty("enableDrops");
        dropPrefabs = serializedObject.FindProperty("dropPrefabs");
        dropChance = serializedObject.FindProperty("dropChance");
        
        showDebugGizmos = serializedObject.FindProperty("showDebugGizmos");
        logDebugInfo = serializedObject.FindProperty("logDebugInfo");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        CollidableTileBreaker breaker = (CollidableTileBreaker)target;
        
        // 标题
        EditorGUILayout.Space();
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 16;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("可破坏Collideable瓦片系统", titleStyle);
        EditorGUILayout.Space();
        
        // 快速设置按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("自动查找Collideable Tilemap", GUILayout.Height(25)))
        {
            AutoFindCollidableTilemap();
        }
        if (GUILayout.Button("创建测试用破坏音效", GUILayout.Height(25)))
        {
            CreateTestBreakSounds();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // 瓦片破坏设置
        EditorGUILayout.LabelField("瓦片破坏设置", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        
        EditorGUILayout.PropertyField(collidableTilemap, new GUIContent("Collideable瓦片图", "要破坏的Tilemap对象"));
        EditorGUILayout.PropertyField(tileHitPoints, new GUIContent("瓦片耐久度", "瓦片需要多少次攻击才能被破坏"));
        EditorGUILayout.PropertyField(attackDetectionRange, new GUIContent("攻击检测范围", "攻击可以影响的范围"));
        EditorGUILayout.PropertyField(playerLayer, new GUIContent("玩家图层", "玩家所在的图层遮罩"));
        
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
        
        // 破坏效果设置
        EditorGUILayout.LabelField("破坏效果设置", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        
        EditorGUILayout.PropertyField(enableBreakEffects, new GUIContent("启用破坏效果", "是否显示粒子效果"));
        
        if (enableBreakEffects.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(particleColor, new GUIContent("碎片颜色"));
            EditorGUILayout.PropertyField(particleCount, new GUIContent("碎片数量"));
            EditorGUILayout.PropertyField(particleSpeed, new GUIContent("碎片速度"));
            EditorGUILayout.PropertyField(particleLifetime, new GUIContent("碎片存在时间"));
            
            // 预览按钮
            if (GUILayout.Button("预览破坏效果"))
            {
                if (Application.isPlaying)
                {
                    breaker.BreakTileAtPosition(breaker.transform.position);
                }
                else
                {
                    EditorUtility.DisplayDialog("预览效果", "请在游戏运行时预览破坏效果", "确定");
                }
            }
            EditorGUI.indentLevel--;
        }
        
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
        
        // 音效设置
        EditorGUILayout.LabelField("音效设置", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        
        EditorGUILayout.PropertyField(breakSounds, new GUIContent("破坏音效数组", "破坏瓦片时播放的音效"), true);
        EditorGUILayout.PropertyField(soundVolume, new GUIContent("音效音量"));
        
        // 测试音效按钮
        if (breakSounds.arraySize > 0 && GUILayout.Button("测试音效"))
        {
            AudioSource audioSource = breaker.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = breaker.gameObject.AddComponent<AudioSource>();
            }
            
            SerializedProperty firstSound = breakSounds.GetArrayElementAtIndex(0);
            AudioClip clip = firstSound.objectReferenceValue as AudioClip;
            if (clip != null)
            {
                audioSource.PlayOneShot(clip, soundVolume.floatValue);
            }
        }
        
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
        
        // 掉落设置
        EditorGUILayout.LabelField("掉落设置", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        
        EditorGUILayout.PropertyField(enableDrops, new GUIContent("启用掉落", "破坏瓦片时是否掉落物品"));
        
        if (enableDrops.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(dropPrefabs, new GUIContent("掉落物预制体"), true);
            EditorGUILayout.Slider(dropChance, 0f, 1f, new GUIContent("掉落概率"));
            EditorGUI.indentLevel--;
        }
        
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
        
        // 调试设置
        EditorGUILayout.LabelField("调试设置", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        
        EditorGUILayout.PropertyField(showDebugGizmos, new GUIContent("显示调试Gizmos", "在Scene视图中显示攻击范围"));
        EditorGUILayout.PropertyField(logDebugInfo, new GUIContent("记录调试信息", "在控制台输出调试信息"));
        
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
        
        // 运行时信息
        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField("运行时信息", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) player = GameObject.Find("Player");
            
            if (player != null)
            {
                EditorGUILayout.LabelField($"玩家位置: {player.transform.position}");
                EditorGUILayout.LabelField($"攻击检测范围: {attackDetectionRange.floatValue}");
            }
            else
            {
                EditorGUILayout.HelpBox("未找到玩家对象！", MessageType.Warning);
            }
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            
            // 运行时测试按钮
            EditorGUILayout.LabelField("运行时测试", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("显示系统状态"))
            {
                breaker.DebugShowStatus();
            }
            
            if (GUILayout.Button("强制攻击检查"))
            {
                breaker.DebugForceAttack();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("扫描所有Tilemap", GUILayout.Height(30)))
            {
                breaker.DebugScanAllTilemaps();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("测试破坏当前位置"))
            {
                breaker.BreakTileAtPosition(breaker.transform.position);
            }
            
            if (GUILayout.Button("范围破坏测试"))
            {
                breaker.BreakTilesInRange(breaker.transform.position, attackDetectionRange.floatValue);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        // 帮助信息
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "使用说明：\n" +
            "1. 将此脚本添加到场景中的任意GameObject上\n" +
            "2. 点击'自动查找Collideable Tilemap'或手动指定要破坏的Tilemap\n" +
            "3. 调整攻击检测范围和瓦片耐久度\n" +
            "4. 设置破坏效果、音效和掉落物（可选）\n" +
            "5. 玩家攻击时会自动检测并破坏范围内的瓦片", 
            MessageType.Info);
        
        serializedObject.ApplyModifiedProperties();
    }
    
    /// <summary>
    /// 自动查找Collideable Tilemap
    /// </summary>
    private void AutoFindCollidableTilemap()
    {
        UnityEngine.Tilemaps.Tilemap[] tilemaps = FindObjectsOfType<UnityEngine.Tilemaps.Tilemap>();
        UnityEngine.Tilemaps.Tilemap foundTilemap = null;
        
        foreach (var tilemap in tilemaps)
        {
            if (tilemap.name.ToLower().Contains("collideable") || 
                tilemap.name.ToLower().Contains("collision") ||
                tilemap.name.ToLower().Contains("wall"))
            {
                foundTilemap = tilemap;
                break;
            }
        }
        
        if (foundTilemap != null)
        {
            collidableTilemap.objectReferenceValue = foundTilemap;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.DisplayDialog("找到Tilemap", 
                $"成功找到Collideable Tilemap: {foundTilemap.name}", "确定");
        }
        else
        {
            EditorUtility.DisplayDialog("未找到Tilemap", 
                "未找到名称包含'collideable'、'collision'或'wall'的Tilemap", "确定");
        }
    }
    
    /// <summary>
    /// 创建测试用破坏音效
    /// </summary>
    private void CreateTestBreakSounds()
    {
        EditorUtility.DisplayDialog("创建测试音效", 
            "请手动将破坏音效拖入'破坏音效数组'字段。\n" +
            "推荐使用短促的碎裂、撞击或破碎音效。", "确定");
    }
    
    /// <summary>
    /// Scene视图中的GUI显示
    /// </summary>
    private void OnSceneGUI()
    {
        CollidableTileBreaker breaker = (CollidableTileBreaker)target;
        
        if (showDebugGizmos.boolValue)
        {
            // 绘制攻击范围
            Handles.color = Color.yellow;
            Handles.DrawWireDisc(breaker.transform.position, Vector3.forward, attackDetectionRange.floatValue);
            
            // 显示范围数值
            Handles.Label(breaker.transform.position + Vector3.up * (attackDetectionRange.floatValue + 0.5f), 
                $"攻击范围: {attackDetectionRange.floatValue:F1}");
        }
    }
} 