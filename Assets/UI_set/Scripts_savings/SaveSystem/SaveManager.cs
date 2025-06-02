using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEngine.UI;
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    
    private string saveDirectory;
    private const string SAVE_FILE_PREFIX = "save_";
    private const string SAVE_FILE_EXTENSION = ".json";
    private const int MAX_SAVE_SLOTS = 3;
    
    // Auto Save Settings
    [Header("Auto Save Settings")]
    public float autoSaveInterval = 60f; // Auto save every 60 seconds
    private float autoSaveTimer;
    private bool isAutoSaveEnabled = true;
    
    // UI References
    [Header("UI References")]
    public SaveMessageUI saveMessageUI;
    public float messageDisplayTime = 2f;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSaveSystem();
            gameObject.SetActive(true);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeSaveSystem()
    {
        // Create save directory
        saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
        
        Debug.Log($"Save Directory: {saveDirectory}");
    }
    
    private void Update()
    {
        // Auto Save Timer
        if (isAutoSaveEnabled && SceneManager.GetActiveScene().name != "MainMenuScene")
        {
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= autoSaveInterval)
            {
                AutoSave();
                autoSaveTimer = 0f;
            }
        }
        
        // Dynamically find SaveMessageUI if null
        if (saveMessageUI == null && SceneManager.GetActiveScene().name != "MainMenuScene")
        {
            saveMessageUI = FindFirstObjectByType<SaveMessageUI>();
        }
    }
    
    public void SaveGame(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
        {
            Debug.LogError($"Invalid save slot: {slotIndex}");
            return;
        }
        
        GameData gameData = CollectGameData();
        // Update save naming logic
        gameData.saveName = $"SaveSlot{slotIndex + 1}";
        gameData.saveTime = DateTime.Now;
        
        string fileName = $"{SAVE_FILE_PREFIX}{slotIndex}{SAVE_FILE_EXTENSION}";
        string filePath = Path.Combine(saveDirectory, fileName);
        
        try
        {
            string json = JsonUtility.ToJson(gameData, true);
            File.WriteAllText(filePath, json);
            Debug.Log($"Game saved to slot {slotIndex}");
            ShowSaveMessage("Game Saved");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
            ShowSaveMessage("Save Failed", true);
        }
    }
    
    public void AutoSave()
    {
        // Find the most recently used save slot or use the first slot
        int slotToUse = GetAutoSaveSlot();
        SaveGame(slotToUse);
        Debug.Log("Auto save completed");
    }
    
    private int GetAutoSaveSlot()
    {
        // Find the most recently used save slot
        List<SaveInfo> saveInfos = GetAllSaveInfos();
        int mostRecentSlot = 0;
        DateTime mostRecentTime = DateTime.MinValue;
        
        for (int i = 0; i < saveInfos.Count; i++)
        {
            if (!saveInfos[i].isEmpty && saveInfos[i].saveTime > mostRecentTime)
            {
                mostRecentTime = saveInfos[i].saveTime;
                mostRecentSlot = i;
            }
        }
        
        // If all slots are empty, use the first slot
        return mostRecentSlot;
    }
    
    public GameData LoadGame(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
        {
            Debug.LogError($"Invalid save slot: {slotIndex}");
            return null;
        }
        
        string fileName = $"{SAVE_FILE_PREFIX}{slotIndex}{SAVE_FILE_EXTENSION}";
        string filePath = Path.Combine(saveDirectory, fileName);
        
        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                GameData gameData = JsonUtility.FromJson<GameData>(json);
                Debug.Log($"Loaded game from slot {slotIndex}");
                return gameData;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game: {e.Message}");
                return null;
            }
        }
        else
        {
            Debug.Log($"Slot {slotIndex} has no save");
            return null;
        }
    }
    
    public void LoadGameAndApply(int slotIndex)
    {
        GameData gameData = LoadGame(slotIndex);
        if (gameData != null)
        {
            // Check if we're loading in the same scene
            if (SceneManager.GetActiveScene().name == gameData.sceneName)
            {
                // Same scene, apply data immediately
                Debug.Log("Loading in same scene, applying data immediately");
                ApplyGameData(gameData);
            }
            else
            {
                // Different scene, use coroutine
                StartCoroutine(LoadGameCoroutine(gameData));
            }
        }
    }
    
    // Method for quick loading in current scene (used by pause menu)
    public void LoadGameInCurrentScene(int slotIndex)
    {
        GameData gameData = LoadGame(slotIndex);
        if (gameData != null)
        {
            if (SceneManager.GetActiveScene().name == gameData.sceneName)
            {
                Debug.Log("Quick loading in current scene");
                ApplyGameData(gameData);
                ShowSaveMessage("Game Loaded");
            }
            else
            {
                Debug.LogWarning("Cannot quick load - save is from a different scene");
                // Fall back to normal loading
                StartCoroutine(LoadGameCoroutine(gameData));
            }
        }
    }
    
    private System.Collections.IEnumerator LoadGameCoroutine(GameData gameData)
    {
        Debug.Log($"LoadGameCoroutine started. Target scene from save: {gameData.sceneName}, Current scene: {SceneManager.GetActiveScene().name}");

        // Load scene
        if (SceneManager.GetActiveScene().name != gameData.sceneName)
        {
            Debug.Log($"Attempting to load scene: {gameData.sceneName}");
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameData.sceneName);

            if (asyncLoad == null) // 添加这个检查
            {
                Debug.LogError($"Failed to start loading scene '{gameData.sceneName}'. Is it in Build Settings and spelled correctly?");
                yield break; // 提前退出协程
            }

            while (!asyncLoad.isDone)
            {
                Debug.Log($"Loading progress: {asyncLoad.progress * 100}%");
                yield return null;
            }
            Debug.Log($"Scene {gameData.sceneName} loaded successfully.");
        }
        else
        {
            Debug.Log("Target scene is the same as current scene. Skipping scene load.");
        }
        
        // Wait for one frame to ensure scene is fully loaded
        yield return null;
        
        Debug.Log("Applying game data...");
        // Apply save data
        ApplyGameData(gameData);
        Debug.Log("Game data applied.");
    }
    
    private GameData CollectGameData()
    {
        GameData gameData = new GameData();
        gameData.sceneName = SceneManager.GetActiveScene().name;
        
        // Collect game progress data
        gameData.progressData.playTime = Time.timeSinceLevelLoad;
        gameData.progressData.currentLevel = GetCurrentLevel();
        
        // Collect player data - 优先使用PlayerAttackSystem
        PlayerAttackSystem playerAttackSystem = FindPlayerAttackSystem();
        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        
        if (playerAttackSystem != null)
        {
            // 使用PlayerAttackSystem的数据
            gameData.playerData = new PlayerData
            {
                currentHealth = playerAttackSystem.Health,
                maxHealth = playerAttackSystem.MaxHealth,
                position = playerAttackSystem.transform.position,
                isFacingRight = GetPlayerFacingDirection(playerAttackSystem.gameObject),
                attackDamage = GetPrivateFieldValue<int>(playerAttackSystem, "attackDamage"),
                attackCooldown = GetPrivateFieldValue<float>(playerAttackSystem, "attackCooldown"),
                attackRadius = GetPrivateFieldValue<float>(playerAttackSystem, "attackRadius"),
                isDead = GetPrivateFieldValue<bool>(playerAttackSystem, "isDead"),
                nextAttackTime = GetPrivateFieldValue<float>(playerAttackSystem, "nextAttackTime")
            };
            
            Debug.Log($"保存血量数据: {playerAttackSystem.Health}/{playerAttackSystem.MaxHealth}");
        }
        else if (playerController != null)
        {
            // 兜底方案：使用PlayerController的数据
            gameData.playerData = new PlayerData
            {
                currentHealth = playerController.currentHealth,
                maxHealth = playerController.maxHealth,
                position = playerController.transform.position,
                isFacingRight = !playerController.GetComponent<SpriteRenderer>().flipX
            };
            
            Debug.LogWarning("未找到PlayerAttackSystem，使用PlayerController数据");
        }
        else
        {
            Debug.LogError("未找到PlayerAttackSystem和PlayerController！");
        }
        
        // Collect enemy data using EnemySaveAdapter
        gameData.enemiesData = new List<EnemyData>();
        
        // 优先使用EnemySaveAdapter
        if (EnemySaveAdapter.Instance != null)
        {
            gameData.enemiesData = EnemySaveAdapter.Instance.CollectEnemyData();
            Debug.Log($"📦 通过EnemySaveAdapter收集了 {gameData.enemiesData.Count} 个敌人数据");
        }
        else
        {
            // 兜底方案：直接查找EnemyBase（保持向后兼容）
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemyObj in enemies)
            {
                EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();
                if (enemy != null && enemy.isActive)
                {
                    gameData.enemiesData.Add(enemy.GetEnemyData());
                }
                else
                {
                    // 如果没有EnemyBase组件，尝试使用SaveableEnemy
                    SaveableEnemy saveableEnemy = enemyObj.GetComponent<SaveableEnemy>();
                    if (saveableEnemy != null && saveableEnemy.IsActive())
                    {
                        EnemyData data = saveableEnemy.GetEnemyData();
                        if (data != null)
                        {
                            gameData.enemiesData.Add(data);
                        }
                    }
                }
            }
            Debug.LogWarning("⚠️ 未找到EnemySaveAdapter，使用传统方式收集敌人数据");
        }
        
        return gameData;
    }
    
    /// <summary>
    /// 查找PlayerAttackSystem组件
    /// </summary>
    private PlayerAttackSystem FindPlayerAttackSystem()
    {
        // 首先通过标签查找
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            PlayerAttackSystem playerAttackSystem = playerObject.GetComponent<PlayerAttackSystem>();
            if (playerAttackSystem != null)
                return playerAttackSystem;
        }
        
        // 如果通过标签找不到，直接查找组件
        return FindFirstObjectByType<PlayerAttackSystem>();
    }
    
    /// <summary>
    /// 获取玩家朝向
    /// </summary>
    private bool GetPlayerFacingDirection(GameObject playerObject)
    {
        SpriteRenderer spriteRenderer = playerObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            return !spriteRenderer.flipX;
        }
        return true; // 默认朝右
    }
    
    /// <summary>
    /// 通过反射获取私有字段值
    /// </summary>
    private T GetPrivateFieldValue<T>(object obj, string fieldName)
    {
        try
        {
            var field = obj.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                return (T)field.GetValue(obj);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"无法获取字段 {fieldName}: {e.Message}");
        }
        
        return default(T);
    }
    
    private int GetCurrentLevel()
    {
        // Determine current level based on scene name
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.Contains("Level1"))
            return 1;
        else if (sceneName.Contains("Level2"))
            return 2;
        // Default return 1
        return 1;
    }
    
    private void ApplyGameData(GameData gameData)
    {
        // Apply player data - 优先使用PlayerAttackSystem
        PlayerAttackSystem playerAttackSystem = FindPlayerAttackSystem();
        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        
        if (playerAttackSystem != null && gameData.playerData != null)
        {
            // 应用PlayerAttackSystem数据
            Debug.Log($"🔄 正在应用PlayerAttackSystem数据: {gameData.playerData.currentHealth}/{gameData.playerData.maxHealth}");
            Debug.Log($"🔄 当前PlayerAttackSystem状态: {playerAttackSystem.Health}/{playerAttackSystem.MaxHealth}");
            
            // 设置位置
            playerAttackSystem.transform.position = gameData.playerData.position;
            
            // 设置血量
            SetPrivateFieldValue(playerAttackSystem, "maxHealth", gameData.playerData.maxHealth);
            SetPrivateFieldValue(playerAttackSystem, "currentHealth", gameData.playerData.currentHealth);
            
            // 验证血量是否设置成功
            Debug.Log($"🩸 血量设置后状态: {playerAttackSystem.Health}/{playerAttackSystem.MaxHealth}");
            
            // 设置攻击相关数据
            SetPrivateFieldValue(playerAttackSystem, "attackDamage", gameData.playerData.attackDamage);
            SetPrivateFieldValue(playerAttackSystem, "attackCooldown", gameData.playerData.attackCooldown);
            SetPrivateFieldValue(playerAttackSystem, "attackRadius", gameData.playerData.attackRadius);
            SetPrivateFieldValue(playerAttackSystem, "isDead", gameData.playerData.isDead);
            SetPrivateFieldValue(playerAttackSystem, "nextAttackTime", gameData.playerData.nextAttackTime);
            
            // 设置朝向
            SpriteRenderer spriteRenderer = playerAttackSystem.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !gameData.playerData.isFacingRight;
            }
            
            // 延迟更新血量条显示，确保所有组件都已初始化
            StartCoroutine(UpdateHealthBarDelayed(playerAttackSystem));
            
            Debug.Log($"✅ PlayerAttackSystem数据应用完成: 血量 {playerAttackSystem.Health}/{playerAttackSystem.MaxHealth}");
        }
        else if (playerController != null && gameData.playerData != null)
        {
            // 兜底方案：应用到PlayerController
            Debug.Log($"🔄 应用PlayerController数据: 位置 {gameData.playerData.position}");
            playerController.transform.position = gameData.playerData.position;
            playerController.currentHealth = gameData.playerData.currentHealth;
            playerController.maxHealth = gameData.playerData.maxHealth;
            
            // 设置朝向
            SpriteRenderer spriteRenderer = playerController.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !gameData.playerData.isFacingRight;
            }
            
            Debug.LogWarning("⚠️ 未找到PlayerAttackSystem，数据已应用到PlayerController");
        }
        else
        {
            Debug.LogError("❌ 未找到PlayerAttackSystem或PlayerController，或者玩家数据为空！");
        }
        
        // Apply enemy data
        StartCoroutine(ApplyEnemyDataCoroutine(gameData.enemiesData));
        
        Debug.Log("📦 存档数据应用完成");
    }
    
    /// <summary>
    /// 通过反射设置私有字段值
    /// </summary>
    private void SetPrivateFieldValue(object obj, string fieldName, object value)
    {
        try
        {
            var field = obj.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(obj, value);
                Debug.Log($"✅ 设置字段 {fieldName} = {value}");
            }
            else
            {
                Debug.LogWarning($"⚠️ 未找到字段: {fieldName}");
                // 显示所有可用的私有字段以便调试
                var allFields = obj.GetType().GetFields(
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                Debug.Log($"可用的私有字段: {string.Join(", ", System.Array.ConvertAll(allFields, f => f.Name))}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 设置字段 {fieldName} 失败: {e.Message}");
        }
    }
    
    private System.Collections.IEnumerator ApplyEnemyDataCoroutine(List<EnemyData> enemiesData)
    {
        // Wait one frame to ensure scene is fully loaded
        yield return null;
        
        Debug.Log($"🔄 开始恢复敌人数据，共 {enemiesData.Count} 个敌人");
        
        // 优先使用EnemySaveAdapter
        if (EnemySaveAdapter.Instance != null)
        {
            Debug.Log("✅ 使用EnemySaveAdapter恢复敌人数据");
            EnemySaveAdapter.Instance.RestoreEnemyData(enemiesData);
        }
        else
        {
            Debug.LogWarning("⚠️ 未找到EnemySaveAdapter，使用传统方式恢复敌人数据");
            
            // 传统方式：禁用所有现有敌人
            GameObject[] existingEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in existingEnemies)
            {
                enemy.SetActive(false);
            }
            
            // Find enemy manager or spawner
            EnemyManager enemyManager = FindFirstObjectByType<EnemyManager>();
            
            if (enemyManager != null)
            {
                // If there is an enemy manager, restore enemies through manager
                enemyManager.RestoreEnemies(enemiesData);
            }
            else
            {
                // Otherwise try to restore enemy states directly
                foreach (EnemyData data in enemiesData)
                {
                    // Find matching enemy using EnemyBase
                    foreach (GameObject enemyObj in existingEnemies)
                    {
                        EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();
                        if (enemy != null && enemy.enemyID == data.enemyID)
                        {
                            enemy.LoadEnemyData(data);
                            if (data.isActive)
                            {
                                enemyObj.SetActive(true);
                            }
                            break;
                        }
                        
                        // 兜底方案：尝试使用SaveableEnemy
                        SaveableEnemy saveableEnemy = enemyObj.GetComponent<SaveableEnemy>();
                        if (saveableEnemy != null && saveableEnemy.GetEnemyID() == data.enemyID)
                        {
                            saveableEnemy.LoadEnemyData(data);
                            if (data.isActive)
                            {
                                enemyObj.SetActive(true);
                            }
                            break;
                        }
                    }
                }
            }
        }
        
        // 等待一帧确保所有敌人状态已更新
        yield return new WaitForEndOfFrame();
        
        Debug.Log("✅ 敌人数据恢复完成");
    }
    
    /// <summary>
    /// 延迟更新血量条，确保所有组件都已准备就绪
    /// </summary>
    private System.Collections.IEnumerator UpdateHealthBarDelayed(PlayerAttackSystem playerAttackSystem)
    {
        // 等待几帧，确保场景完全加载
        yield return new WaitForEndOfFrame();
        yield return new WaitForFixedUpdate();
        yield return new WaitForEndOfFrame();
        
        // 尝试多次更新血量条
        for (int attempts = 0; attempts < 5; attempts++)
        {
            HealthBarManager healthBarManager = HealthBarManager.Instance;
            if (healthBarManager == null)
            {
                healthBarManager = FindFirstObjectByType<HealthBarManager>();
            }
            
            if (healthBarManager != null)
            {
                Debug.Log($"🎯 尝试更新血量条 (第{attempts + 1}次): {playerAttackSystem.Health}/{playerAttackSystem.MaxHealth}");
                
                // 强制重新连接PlayerAttackSystem
                healthBarManager.ForceReconnectPlayerSystem();
                
                // 确保连接的是正确的PlayerAttackSystem
                var connectedPlayer = healthBarManager.GetConnectedPlayerSystem();
                if (connectedPlayer != playerAttackSystem)
                {
                    Debug.LogWarning("⚠️ HealthBarManager连接的PlayerAttackSystem不匹配，手动设置");
                    healthBarManager.SetPlayerAttackSystem(playerAttackSystem);
                }
                
                // 强制更新显示
                healthBarManager.UpdateHealthDisplay(true);
                
                // 验证血量条是否正确更新
                float healthPercentage = healthBarManager.GetHealthPercentage();
                Debug.Log($"💚 血量条更新后百分比: {healthPercentage * 100:F1}%");
                Debug.Log($"🔍 验证: PlayerAttackSystem血量 = {playerAttackSystem.Health}, 血量条显示百分比 = {healthPercentage}");
                
                if (healthPercentage > 0 || playerAttackSystem.Health <= 0)
                {
                    Debug.Log("✅ 血量条更新成功");
                    break;
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ 第{attempts + 1}次未找到HealthBarManager");
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
        // 最终验证
        yield return new WaitForSeconds(0.5f);
        Debug.Log($"🔎 最终验证 - PlayerAttackSystem: {playerAttackSystem.Health}/{playerAttackSystem.MaxHealth}");
        
        HealthBarManager finalManager = HealthBarManager.Instance;
        if (finalManager != null)
        {
            float finalPercentage = finalManager.GetHealthPercentage();
            Debug.Log($"🔎 最终验证 - 血量条百分比: {finalPercentage * 100:F1}%");
        }
    }
    
    public List<SaveInfo> GetAllSaveInfos()
    {
        List<SaveInfo> saveInfos = new List<SaveInfo>();
        
        for (int i = 0; i < MAX_SAVE_SLOTS; i++)
        {
            string fileName = $"{SAVE_FILE_PREFIX}{i}{SAVE_FILE_EXTENSION}";
            string filePath = Path.Combine(saveDirectory, fileName);
            
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    GameData gameData = JsonUtility.FromJson<GameData>(json);
                    
                    SaveInfo info = new SaveInfo
                    {
                        slotIndex = i,
                        saveName = gameData.saveName,
                        saveTime = gameData.saveTime,
                        sceneName = gameData.sceneName,
                        playerHealth = gameData.playerData.currentHealth,
                        playTime = gameData.progressData.playTime
                    };
                    
                    saveInfos.Add(info);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to read save info: {e.Message}");
                }
            }
            else
            {
                // Empty slot
                SaveInfo info = new SaveInfo
                {
                    slotIndex = i,
                    saveName = $"SaveSlot{i + 1}",
                    isEmpty = true
                };
                saveInfos.Add(info);
            }
        }
        
        return saveInfos;
    }
    
    public void DeleteSave(int slotIndex)
    {
        Debug.Log($"DeleteSave called with slotIndex: {slotIndex}");
        
        string fileName = $"{SAVE_FILE_PREFIX}{slotIndex}{SAVE_FILE_EXTENSION}";
        string filePath = Path.Combine(saveDirectory, fileName);
        
        Debug.Log($"Looking for file: {filePath}");
        
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                Debug.Log($"Successfully deleted save file: {filePath}");
                Debug.Log($"Deleted save in slot {slotIndex}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete save file: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"Save file does not exist: {filePath}");
        }
    }
    
    private void ShowSaveMessage(string message, bool isError = false)
    {
        if (saveMessageUI != null)
        {
            saveMessageUI.ShowMessage(message, isError);
        }
    }
    
    public void SetAutoSaveEnabled(bool enabled)
    {
        isAutoSaveEnabled = enabled;
        if (!enabled)
        {
            autoSaveTimer = 0f;
        }
    }
}

[System.Serializable]
public class SaveInfo
{
    public int slotIndex;
    public string saveName;
    public DateTime saveTime;
    public string sceneName;
    public int playerHealth;
    public float playTime;
    public bool isEmpty;
} 