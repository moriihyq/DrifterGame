using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// New Save Feature Tester
/// Used to test and debug new save creation functionality in save interface
/// </summary>
public class NewSaveFeatureTester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool enableTesting = true;
    [SerializeField] private KeyCode testNewGameKey = KeyCode.N; // N key to test new game
    [SerializeField] private KeyCode testSlot0Key = KeyCode.Alpha1; // 1 key to test slot 0 creation
    [SerializeField] private KeyCode testSlot1Key = KeyCode.Alpha2; // 2 key to test slot 1 creation
    [SerializeField] private KeyCode testSlot2Key = KeyCode.Alpha3; // 3 key to test slot 2 creation
    [SerializeField] private KeyCode refreshSlotsKey = KeyCode.R; // R key to refresh slot display
    
    [Header("Debug Information")]
    [SerializeField] private bool showDebugGUI = true;
    [SerializeField] private bool showConsoleInfo = true;
    
    private LoadGamePanelManager loadGamePanel;
    private Canvas testCanvas;
    private bool isTestingMode = false;
    
    private void Start()
    {
        if (!enableTesting) return;
        
        // Find LoadGamePanelManager
        loadGamePanel = FindFirstObjectByType<LoadGamePanelManager>();
        if (loadGamePanel == null)
        {
            Debug.LogWarning("[NewSaveFeatureTester] LoadGamePanelManager not found, some test features may not be available");
        }
        
        Debug.Log("[NewSaveFeatureTester] New save feature tester started");
        Debug.Log($"Test keys: [{testNewGameKey}] New Game, [{testSlot0Key}] Slot 0, [{testSlot1Key}] Slot 1, [{testSlot2Key}] Slot 2, [{refreshSlotsKey}] Refresh");
    }
    
    private void Update()
    {
        if (!enableTesting) return;
        
        // Hotkey testing
        if (Input.GetKeyDown(testNewGameKey))
        {
            TestStartNewGame();
        }
        else if (Input.GetKeyDown(testSlot0Key))
        {
            TestCreateNewSaveToSlot(0);
        }
        else if (Input.GetKeyDown(testSlot1Key))
        {
            TestCreateNewSaveToSlot(1);
        }
        else if (Input.GetKeyDown(testSlot2Key))
        {
            TestCreateNewSaveToSlot(2);
        }
        else if (Input.GetKeyDown(refreshSlotsKey))
        {
            TestRefreshSlots();
        }
        
        // Toggle testing mode
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleTestingMode();
        }
    }
    
    /// <summary>
    /// Test start new game functionality
    /// </summary>
    public void TestStartNewGame()
    {
        Debug.Log("🧪 [Test] Start new game");
        
        if (loadGamePanel != null)
        {
            loadGamePanel.StartNewGame();
        }
        else
        {
            Debug.LogError("[Test] LoadGamePanelManager not found, cannot test new game functionality");
        }
    }
    
    /// <summary>
    /// Test create new save to specified slot
    /// </summary>
    public void TestCreateNewSaveToSlot(int slotIndex)
    {
        Debug.Log($"🧪 [Test] Create new save to slot {slotIndex}");
        
        if (loadGamePanel != null)
        {
            loadGamePanel.CreateNewSaveToSlot(slotIndex);
        }
        else
        {
            Debug.LogError("[Test] LoadGamePanelManager not found, cannot test new save creation functionality");
        }
    }
    
    /// <summary>
    /// Test refresh save slot display
    /// </summary>
    public void TestRefreshSlots()
    {
        Debug.Log("🧪 [Test] Refresh save slot display");
        
        if (loadGamePanel != null)
        {
            loadGamePanel.RefreshSaveSlots();
            Debug.Log("✅ Save slots refreshed");
        }
        else
        {
            Debug.LogError("[Test] LoadGamePanelManager not found, cannot refresh slots");
        }
    }
    
    /// <summary>
    /// Test save system status
    /// </summary>
    public void TestSaveSystemStatus()
    {
        Debug.Log("🧪 [Test] Check save system status");
        
        if (SaveManager.Instance != null)
        {
            var saveInfos = SaveManager.Instance.GetAllSaveInfos();
            Debug.Log($"✅ SaveManager available, {saveInfos.Count} save slots total");
            
            for (int i = 0; i < saveInfos.Count; i++)
            {
                if (saveInfos[i].isEmpty)
                {
                    Debug.Log($"   Slot {i}: Empty");
                }
                else
                {
                    Debug.Log($"   Slot {i}: {saveInfos[i].saveName} - {saveInfos[i].saveTime}");
                }
            }
        }
        else
        {
            Debug.LogError("❌ SaveManager not found!");
        }
    }
    
    /// <summary>
    /// Toggle testing mode
    /// </summary>
    private void ToggleTestingMode()
    {
        isTestingMode = !isTestingMode;
        Debug.Log($"🧪 Testing mode: {(isTestingMode ? "ON" : "OFF")}");
    }
    
    /// <summary>
    /// Show debug GUI
    /// </summary>
    private void OnGUI()
    {
        // 图形化界面已禁用 - 如需重新启用，将下面的return注释掉
        return;
        
        if (!enableTesting || !showDebugGUI) return;
        
        // Create GUI area
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        GUILayout.Label("=== New Save Feature Tester ===");
        
        GUILayout.Space(10);
        
        // Function test buttons
        if (GUILayout.Button($"[{testNewGameKey}] Start New Game"))
        {
            TestStartNewGame();
        }
        
        GUILayout.Space(5);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button($"[{testSlot0Key}] New Save to Slot 0"))
        {
            TestCreateNewSaveToSlot(0);
        }
        if (GUILayout.Button($"[{testSlot1Key}] New Save to Slot 1"))
        {
            TestCreateNewSaveToSlot(1);
        }
        if (GUILayout.Button($"[{testSlot2Key}] New Save to Slot 2"))
        {
            TestCreateNewSaveToSlot(2);
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(5);
        
        if (GUILayout.Button($"[{refreshSlotsKey}] Refresh Save Slots"))
        {
            TestRefreshSlots();
        }
        
        if (GUILayout.Button("Check Save System Status"))
        {
            TestSaveSystemStatus();
        }
        
        GUILayout.Space(10);
        
        // Status information
        GUILayout.Label("Status Information:");
        GUILayout.Label($"LoadGamePanelManager: {(loadGamePanel != null ? "✅" : "❌")}");
        GUILayout.Label($"SaveManager: {(SaveManager.Instance != null ? "✅" : "❌")}");
        GUILayout.Label($"Testing Mode: {(isTestingMode ? "🟢" : "⚪")} (Press T to toggle)");
        
        GUILayout.EndArea();
    }
    
    /// <summary>
    /// Enable or disable testing functionality
    /// </summary>
    public void SetTestingEnabled(bool enabled)
    {
        enableTesting = enabled;
        Debug.Log($"[NewSaveFeatureTester] Testing functionality {(enabled ? "enabled" : "disabled")}");
    }
} 