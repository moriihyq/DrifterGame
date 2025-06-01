using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LoadGamePanelManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform saveSlotContainer;
    public GameObject saveSlotPrefab;
    public Button backButton;
    public GameObject mainUIPanel;
    public GameObject dimmer; // Dimmer background mask
    
    [Header("New Game Features")]
    public Button newGameButton; // Start Game button (loads recent save or starts new)
    public Button forceNewGameButton; // Force New Game button (always starts fresh)
    public string newGameSceneName = "YourGameSceneName"; // New game scene name
    
    [Header("Save Slot Buttons")]
    public Button[] saveSlots;
    public Button[] deleteButtons;
    
    private TMP_FontAsset customFont;
    
    private void OnEnable()
    {
        // Activate Dimmer background mask
        if (dimmer != null)
        {
            dimmer.SetActive(true);
        }
        
        // Re-bind buttons when panel is enabled
        Debug.Log("LoadGamePanelManager OnEnable called");
        BindDeleteButtons();
        RefreshSaveSlots();
    }
    
    private void OnDisable()
    {
        // Close Dimmer background mask
        if (dimmer != null)
        {
            dimmer.SetActive(false);
        }
    }
    
    private void BindDeleteButtons()
    {
        if (deleteButtons != null)
        {
            Debug.Log($"BindDeleteButtons: Found {deleteButtons.Length} delete buttons in array");
            for (int i = 0; i < deleteButtons.Length; i++)
            {
                int slotIndex = i;
                if (deleteButtons[i] != null)
                {
                    Debug.Log($"Binding delete button {i} to slot {slotIndex}");
                    deleteButtons[i].onClick.RemoveAllListeners();
                    deleteButtons[i].onClick.AddListener(() => 
                    {
                        Debug.Log($"Delete button clicked for slot {slotIndex}");
                        DeleteSelectedSave(slotIndex);
                    });
                    
                    // Add button name for debugging
                    deleteButtons[i].name = $"DeleteButton_Slot{i}";
                }
                else
                {
                    Debug.LogWarning($"Delete button at index {i} is null");
                }
            }
        }
        else
        {
            Debug.LogWarning("deleteButtons array is null in BindDeleteButtons");
        }
    }
    
    // Public method for testing via Unity Inspector
    public void TestDeleteSlot(int slotIndex)
    {
        Debug.Log($"TestDeleteSlot called with index {slotIndex}");
        DeleteSelectedSave(slotIndex);
    }
    
    private void Start()
    {
        // Load custom font
        customFont = Resources.Load<TMP_FontAsset>("UI_set/10 Font/CyberpunkCraftpixPixel SDF");
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(CloseLoadGamePanel);
        }
        
        // Bind new game button
        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(StartNewGame);
        }
        
        // Bind force new game button
        if (forceNewGameButton != null)
        {
            forceNewGameButton.onClick.AddListener(ForceStartNewGame);
        }
        
        BindDeleteButtons();
        RefreshSaveSlots();
    }
    
    public void RefreshSaveSlots()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager not found!");
            return;
        }
        
        List<SaveInfo> saveInfos = SaveManager.Instance.GetAllSaveInfos();
        
        if (saveSlots != null && saveSlots.Length > 0)
        {
            for (int i = 0; i < saveSlots.Length && i < saveInfos.Count; i++)
            {
                TextMeshProUGUI buttonText = saveSlots[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    // Set custom font
                    if (customFont != null)
                    {
                        buttonText.font = customFont;
                    }
                    
                    // Set text color to bright cyan for better visibility
                    buttonText.color = new Color(0f, 1f, 1f, 1f); // Bright cyan color
                    
                    // Increase font size for better readability
                    buttonText.fontSize = 38f; // Further increased for better readability
                    
                    if (saveInfos[i].isEmpty)
                    {
                        // Use new naming convention for empty slots with new game option
                        buttonText.text = $"SAVESLOT{i + 1}\n[Click to Create New Game]";
                        saveSlots[i].interactable = true; // Allow empty slots to be clickable
                        
                        // Set a slightly brighter color for empty slots to indicate they're clickable
                        buttonText.color = new Color(0.7f, 1f, 0.7f, 1f); // Light green
                        
                        if (deleteButtons != null && i < deleteButtons.Length && deleteButtons[i] != null)
                        {
                            deleteButtons[i].gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        // Use more concise time format
                        string timeStr = saveInfos[i].saveTime.ToString("MM-dd HH:mm");
                        // Use new naming convention for filled slots
                        buttonText.text = $"SAVESLOT{i + 1}\n{timeStr}";
                        saveSlots[i].interactable = true;
                        
                        // Set bright cyan for active save slots
                        buttonText.color = new Color(0f, 1f, 1f, 1f); // Bright cyan
                        
                        if (deleteButtons != null && i < deleteButtons.Length && deleteButtons[i] != null)
                        {
                            deleteButtons[i].gameObject.SetActive(true);
                        }
                    }
                }
                
                int slotIndex = i;
                saveSlots[i].onClick.RemoveAllListeners();
                
                // Set different click events for empty slots and existing saves
                if (saveInfos[slotIndex].isEmpty)
                {
                    saveSlots[i].onClick.AddListener(() => CreateNewSaveToSlot(slotIndex));
                }
                else
                {
                    saveSlots[i].onClick.AddListener(() => LoadSelectedSave(slotIndex));
                }
            }
        }
        else if (saveSlotPrefab != null && saveSlotContainer != null)
        {
            foreach (Transform child in saveSlotContainer)
            {
                Destroy(child.gameObject);
            }
            
            foreach (SaveInfo info in saveInfos)
            {
                GameObject slotObj = Instantiate(saveSlotPrefab, saveSlotContainer);
                SaveSlotUI slotUI = slotObj.GetComponent<SaveSlotUI>();
                if (slotUI != null)
                {
                    slotUI.SetupSlot(info, this);
                }
            }
        }
    }
    
    public void LoadSelectedSave(int slotIndex)
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadGameAndApply(slotIndex);
            CloseLoadGamePanel();
        }
    }
    
    /// <summary>
    /// Start game - loads most recent save or starts new game if no saves exist
    /// </summary>
    public void StartNewGame()
    {
        Debug.Log("Starting game from save interface");
        
        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager not found!");
            return;
        }
        
        // Check for existing saves
        var saveInfos = SaveManager.Instance.GetAllSaveInfos();
        SaveInfo mostRecentSave = null;
        DateTime mostRecentTime = DateTime.MinValue;
        
        // Find the most recent save
        foreach (var saveInfo in saveInfos)
        {
            if (!saveInfo.isEmpty && saveInfo.saveTime > mostRecentTime)
            {
                mostRecentTime = saveInfo.saveTime;
                mostRecentSave = saveInfo;
            }
        }
        
        if (mostRecentSave != null)
        {
            // Load the most recent save
            Debug.Log($"Loading most recent save: {mostRecentSave.saveName} from {mostRecentSave.saveTime}");
            SaveManager.Instance.LoadGameAndApply(mostRecentSave.slotIndex);
        }
        else
        {
            // No saves found, start new game
            Debug.Log("No existing saves found, starting new game");
            StartFreshNewGame();
        }
        
        CloseLoadGamePanel();
    }
    
    /// <summary>
    /// Start a completely fresh new game (used when no saves exist)
    /// </summary>
    private void StartFreshNewGame()
    {
        // Get the first available game scene
        if (string.IsNullOrEmpty(newGameSceneName) || newGameSceneName == "YourGameSceneName")
        {
            // Try to auto-detect game scenes - 5.26地图 has highest priority
            string[] possibleScenes = { "5.26地图", "Example1", "关卡1", "Level1", "GameScene", "MainGameScene", "可以运行的地图" };
            foreach (string sceneName in possibleScenes)
            {
                if (Application.CanStreamedLevelBeLoaded(sceneName))
                {
                    newGameSceneName = sceneName;
                    break;
                }
            }
        }

        if (string.IsNullOrEmpty(newGameSceneName))
        {
            Debug.LogError("No available game scene found! Please set newGameSceneName in LoadGamePanelManager.");
            return;
        }

        // Clear all save slots to ensure a completely new game
        for (int i = 0; i < 3; i++) // Assume there are 3 save slots
        {
            SaveManager.Instance.DeleteSave(i);
        }

        // For fresh new game, load scene directly to preserve default enemy setup
        Debug.Log($"Loading fresh game scene: {newGameSceneName}");
        SceneManager.LoadScene(newGameSceneName);
    }
    
    /// <summary>
    /// Force start a new game (always clears saves and starts fresh)
    /// </summary>
    public void ForceStartNewGame()
    {
        Debug.Log("Force starting new game - clearing all saves");
        StartFreshNewGame();
        CloseLoadGamePanel();
    }
    
    /// <summary>
    /// Create new save to specified slot
    /// </summary>
    /// <param name="slotIndex">Save slot index</param>
    public void CreateNewSaveToSlot(int slotIndex)
    {
        Debug.Log($"Creating new save to slot {slotIndex}");
        
        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager not found!");
            return;
        }
        
        // Check if slot is empty
        var saveInfos = SaveManager.Instance.GetAllSaveInfos();
        if (slotIndex < saveInfos.Count && !saveInfos[slotIndex].isEmpty)
        {
            Debug.LogWarning($"Slot {slotIndex} already has a save, will overwrite existing save");
        }
        
        // Get game scene name
        string gameScene = newGameSceneName;
        if (string.IsNullOrEmpty(gameScene) || gameScene == "YourGameSceneName")
        {
            string[] possibleScenes = { "5.26地图", "Example1", "关卡1", "Level1", "GameScene", "MainGameScene", "可以运行的地图" };
            foreach (string sceneName in possibleScenes)
            {
                if (Application.CanStreamedLevelBeLoaded(sceneName))
                {
                    gameScene = sceneName;
                    break;
                }
            }
        }
        
        if (string.IsNullOrEmpty(gameScene))
        {
            Debug.LogError("No available game scene found!");
            return;
        }
        
        // Create new game data with better initial setup
        GameData newGameData = CreateNewGameData(gameScene);
        
        // Save to specified slot
        SaveNewGameToSlot(newGameData, slotIndex);
        
        // Start coroutine to load scene and then apply save data properly
        StartCoroutine(LoadSceneAndApplySaveCoroutine(gameScene, slotIndex));
        
        CloseLoadGamePanel();
    }
    
    /// <summary>
    /// Coroutine to load scene first, then apply save data
    /// </summary>
    private System.Collections.IEnumerator LoadSceneAndApplySaveCoroutine(string sceneName, int slotIndex)
    {
        Debug.Log($"Loading scene {sceneName} for new save slot {slotIndex}");
        
        // Load the scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // Wait a few more frames for scene initialization
        yield return new WaitForEndOfFrame();
        yield return new WaitForFixedUpdate();
        yield return new WaitForEndOfFrame();
        
        // Now apply the save data
        if (SaveManager.Instance != null)
        {
            Debug.Log($"Scene loaded, applying save data for slot {slotIndex}");
            SaveManager.Instance.LoadGameAndApply(slotIndex);
        }
        else
        {
            Debug.LogWarning("SaveManager not available after scene load");
        }
    }
    
    /// <summary>
    /// Create new game data
    /// </summary>
    private GameData CreateNewGameData(string sceneName)
    {
        GameData gameData = new GameData();
        gameData.sceneName = sceneName;
        gameData.saveName = $"New Game {System.DateTime.Now:MM-dd HH:mm}";
        gameData.saveTime = System.DateTime.Now;
        
        // Initialize player data with better default position
        Vector3 defaultPlayerPosition = GetDefaultPlayerPosition(sceneName);
        gameData.playerData = new PlayerData
        {
            currentHealth = 100,
            maxHealth = 100,
            position = defaultPlayerPosition,
            isFacingRight = true,
            attackDamage = 50,
            attackCooldown = 0.2f,
            attackRadius = 3.0f,
            isDead = false,
            nextAttackTime = 0f
        };
        
        // Initialize game progress data
        gameData.progressData = new GameProgressData
        {
            currentLevel = 1,
            playTime = 0f,
            score = 0
        };
        
        // Initialize enemy data - let the scene handle enemies naturally
        // Don't force empty enemy list, let SaveManager collect existing enemies
        gameData.enemiesData = new List<EnemyData>();
        
        Debug.Log($"Created new game data: {gameData.saveName} for scene: {sceneName}");
        Debug.Log($"Player start position: {defaultPlayerPosition}");
        return gameData;
    }
    
    /// <summary>
    /// Get default player position for different scenes
    /// </summary>
    private Vector3 GetDefaultPlayerPosition(string sceneName)
    {
        // Define default starting positions for different scenes
        switch (sceneName)
        {
            case "5.26地图":
                return new Vector3(-8f, 0f, 0f); // Adjust this based on your scene layout
            case "可以运行的地图":
                return new Vector3(-10f, 0f, 0f); // Adjust this based on your scene layout
            case "Example1":
                return new Vector3(-5f, 0f, 0f);
            case "关卡1":
            case "Level1":
                return new Vector3(-6f, 0f, 0f);
            default:
                // Generic default position for unknown scenes
                return new Vector3(-5f, 0f, 0f);
        }
    }
    
    /// <summary>
    /// Save new game data to specified slot
    /// </summary>
    private void SaveNewGameToSlot(GameData gameData, int slotIndex)
    {
        try
        {
            string saveDirectory = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
            if (!System.IO.Directory.Exists(saveDirectory))
            {
                System.IO.Directory.CreateDirectory(saveDirectory);
            }
            
            string fileName = $"save_{slotIndex}.json";
            string filePath = System.IO.Path.Combine(saveDirectory, fileName);
            
            string json = JsonUtility.ToJson(gameData, true);
            System.IO.File.WriteAllText(filePath, json);
            
            Debug.Log($"New game save has been saved to slot {slotIndex}: {filePath}");
            
            // Refresh save list display
            RefreshSaveSlots();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save new game save: {e.Message}");
        }
    }
    
    public void DeleteSelectedSave(int slotIndex)
    {
        Debug.Log($"DeleteSelectedSave called with slotIndex: {slotIndex}");
        
        if (SaveManager.Instance != null)
        {
            // Check if the save actually exists before deletion
            var saveInfos = SaveManager.Instance.GetAllSaveInfos();
            if (slotIndex < saveInfos.Count)
            {
                Debug.Log($"Save info for slot {slotIndex}: isEmpty={saveInfos[slotIndex].isEmpty}, saveName={saveInfos[slotIndex].saveName}");
                
                if (!saveInfos[slotIndex].isEmpty)
                {
                    SaveManager.Instance.DeleteSave(slotIndex);
                    Debug.Log($"Delete method called for slot {slotIndex + 1}");
                    
                    // Force immediate refresh
                    RefreshSaveSlots();
                    Debug.Log("RefreshSaveSlots called after deletion");
                }
                else
                {
                    Debug.Log($"Slot {slotIndex + 1} is already empty, nothing to delete");
                }
            }
            else
            {
                Debug.LogError($"Invalid slot index: {slotIndex}");
            }
        }
        else
        {
            Debug.LogError("SaveManager.Instance is null!");
        }
    }
    
    private void CloseLoadGamePanel()
    {
        // Close Dimmer background mask
        if (dimmer != null)
        {
            dimmer.SetActive(false);
        }
        
        gameObject.SetActive(false);
        if (mainUIPanel != null)
        {
            mainUIPanel.SetActive(true);
        }
    }
}

// Save slot UI component
[System.Serializable]
public class SaveSlotUI : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI slotNameText;
    public TextMeshProUGUI saveTimeText;
    public TextMeshProUGUI sceneNameText;
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI playTimeText;
    public Button loadButton;
    public Button deleteButton;
    public GameObject emptySlotIndicator;
    public GameObject saveInfoContainer;
    
    private SaveInfo currentSaveInfo;
    private LoadGamePanelManager panelManager;
    
    public void SetupSlot(SaveInfo saveInfo, LoadGamePanelManager manager)
    {
        currentSaveInfo = saveInfo;
        panelManager = manager;
        
        if (saveInfo.isEmpty)
        {
            // Show empty slot with new game option
            if (emptySlotIndicator != null) emptySlotIndicator.SetActive(true);
            if (saveInfoContainer != null) saveInfoContainer.SetActive(false);
            if (loadButton != null) 
            {
                loadButton.interactable = true; // Allow empty slot buttons to be clickable
                loadButton.onClick.RemoveAllListeners();
                loadButton.onClick.AddListener(() => panelManager.CreateNewSaveToSlot(saveInfo.slotIndex));
                
                // Update button text
                var buttonText = loadButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "New Game";
                    buttonText.color = new Color(0.7f, 1f, 0.7f, 1f); // Light green
                }
            }
            if (deleteButton != null) deleteButton.gameObject.SetActive(false);
            
            if (slotNameText != null) 
            {
                slotNameText.text = $"{saveInfo.saveName} [Click to Create]";
                // Set light green color for empty slots to indicate they're clickable
                slotNameText.color = new Color(0.7f, 1f, 0.7f, 1f);
                // Increase font size for better readability
                slotNameText.fontSize = 38f; // Increased main text size
            }
        }
        else
        {
            // Show save info
            if (emptySlotIndicator != null) emptySlotIndicator.SetActive(false);
            if (saveInfoContainer != null) saveInfoContainer.SetActive(true);
            if (loadButton != null) 
            {
                loadButton.interactable = true;
                loadButton.onClick.RemoveAllListeners();
                loadButton.onClick.AddListener(() => panelManager.LoadSelectedSave(saveInfo.slotIndex));
                
                // Update button text
                var buttonText = loadButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "Load Game";
                    buttonText.color = new Color(0f, 1f, 1f, 1f); // Bright cyan
                }
            }
            if (deleteButton != null) deleteButton.gameObject.SetActive(true);
            
            // Set text and colors for all text components
            if (slotNameText != null) 
            {
                slotNameText.text = saveInfo.saveName;
                slotNameText.color = new Color(0f, 1f, 1f, 1f); // Bright cyan
                slotNameText.fontSize = 38f; // Increased main text size
            }
            if (saveTimeText != null) 
            {
                saveTimeText.text = $"Saved: {saveInfo.saveTime:MM-dd HH:mm}";
                saveTimeText.color = new Color(0f, 1f, 1f, 1f); // Bright cyan
                saveTimeText.fontSize = 28f; // Increased detail text size
            }
            if (sceneNameText != null) 
            {
                sceneNameText.text = $"Scene: {saveInfo.sceneName}";
                sceneNameText.color = new Color(0f, 1f, 1f, 1f); // Bright cyan
                sceneNameText.fontSize = 28f; // Increased detail text size
            }
            if (playerHealthText != null) 
            {
                playerHealthText.text = $"Health: {saveInfo.playerHealth}";
                playerHealthText.color = new Color(0f, 1f, 1f, 1f); // Bright cyan
                playerHealthText.fontSize = 28f; // Increased detail text size
            }
            if (playTimeText != null) 
            {
                playTimeText.text = $"Play Time: {FormatPlayTime(saveInfo.playTime)}";
                playTimeText.color = new Color(0f, 1f, 1f, 1f); // Bright cyan
                playTimeText.fontSize = 28f; // Increased detail text size
            }
            
            // Set delete button event
            if (deleteButton != null)
            {
                deleteButton.onClick.RemoveAllListeners();
                int capturedSlotIndex = saveInfo.slotIndex; // Capture the slot index
                Debug.Log($"SaveSlotUI: Binding delete button for slot {capturedSlotIndex}");
                deleteButton.onClick.AddListener(() => 
                {
                    Debug.Log($"SaveSlotUI: Delete button clicked for slot {capturedSlotIndex}");
                    panelManager.DeleteSelectedSave(capturedSlotIndex);
                });
            }
            else
            {
                Debug.LogWarning($"SaveSlotUI: deleteButton is null for slot {saveInfo.slotIndex}");
            }
        }
    }
    
    private string FormatPlayTime(float seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(seconds);
        return string.Format("{0:D2}:{1:D2}:{2:D2}", time.Hours, time.Minutes, time.Seconds);
    }
} 