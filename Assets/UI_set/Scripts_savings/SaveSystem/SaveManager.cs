using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        if (slotIndex == 0)
        {
            gameData.saveName = "Latest Save";
        }
        else
        {
            gameData.saveName = $"SaveSlot{slotIndex}";
        }
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
        
        // Collect player data
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            gameData.playerData = new PlayerData
            {
                currentHealth = player.currentHealth,
                maxHealth = player.maxHealth,
                position = player.transform.position,
                isFacingRight = !player.GetComponent<SpriteRenderer>().flipX
            };
        }
        
        // Collect enemy data
        gameData.enemiesData = new List<EnemyData>();
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemyObj in enemies)
        {
            EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();
            if (enemy != null && enemy.isActive)
            {
                gameData.enemiesData.Add(enemy.GetEnemyData());
            }
        }
        
        return gameData;
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
        // Apply player data
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null && gameData.playerData != null)
        {
            Debug.Log($"Applying player position: {gameData.playerData.position}");
            player.transform.position = gameData.playerData.position;
            player.currentHealth = gameData.playerData.currentHealth;
            player.maxHealth = gameData.playerData.maxHealth;
            
            // Ensure SpriteRenderer component exists
            SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !gameData.playerData.isFacingRight;
            }
        }
        else
        {
            Debug.LogError("Failed to find PlayerController or player data is null!");
        }
        
        // Apply enemy data
        StartCoroutine(ApplyEnemyDataCoroutine(gameData.enemiesData));
        
        Debug.Log("Save data applied successfully");
    }
    
    private System.Collections.IEnumerator ApplyEnemyDataCoroutine(List<EnemyData> enemiesData)
    {
        // Wait one frame to ensure scene is fully loaded
        yield return null;
        
        // First disable all existing enemies
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
                // Find matching enemy
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
                }
            }
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
                    saveName = i == 0 ? "Latest Save" : $"SaveSlot{i}",
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