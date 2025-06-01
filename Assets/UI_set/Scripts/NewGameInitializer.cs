using UnityEngine;

/// <summary>
/// New Game Initializer
/// Ensures proper player positioning when starting a new game
/// </summary>
public class NewGameInitializer : MonoBehaviour
{
    [Header("Player Spawn Settings")]
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Vector3 defaultSpawnPosition = new Vector3(-8f, 0f, 0f);
    [SerializeField] private bool autoFindPlayer = true;
    [SerializeField] private string playerTag = "Player";
    
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLog = true;
    [SerializeField] private bool showSpawnPointGizmo = true;
    
    private void Start()
    {
        // Check if this is a fresh scene load (no save data applied)
        if (ShouldInitializeAsNewGame())
        {
            InitializeNewGame();
        }
    }
    
    /// <summary>
    /// Check if we should initialize as new game
    /// </summary>
    private bool ShouldInitializeAsNewGame()
    {
        // If SaveManager exists, check if any saves exist
        if (SaveManager.Instance != null)
        {
            var saveInfos = SaveManager.Instance.GetAllSaveInfos();
            bool allSlotsEmpty = true;
            foreach (var saveInfo in saveInfos)
            {
                if (!saveInfo.isEmpty)
                {
                    allSlotsEmpty = false;
                    break;
                }
            }
            
            if (allSlotsEmpty && enableDebugLog)
            {
                Debug.Log("[NewGameInitializer] All save slots empty, initializing as new game");
            }
            
            // Only initialize if all slots are empty (true new game)
            return allSlotsEmpty;
        }
        
        // If no SaveManager, don't initialize (let scene load naturally)
        if (enableDebugLog)
        {
            Debug.Log("[NewGameInitializer] No SaveManager found, skipping initialization");
        }
        return false;
    }
    
    /// <summary>
    /// Initialize the game as a new game
    /// </summary>
    private void InitializeNewGame()
    {
        if (enableDebugLog)
        {
            Debug.Log("[NewGameInitializer] Initializing new game setup");
        }
        
        // Set player position
        SetPlayerToSpawnPosition();
        
        // Could add other new game initialization here:
        // - Reset UI elements
        // - Initialize game state
        // - Set up initial items/inventory
    }
    
    /// <summary>
    /// Set player to the correct spawn position
    /// </summary>
    private void SetPlayerToSpawnPosition()
    {
        GameObject player = null;
        
        // Try to find player
        if (autoFindPlayer)
        {
            player = GameObject.FindGameObjectWithTag(playerTag);
            if (player == null)
            {
                // Try finding by component
                PlayerAttackSystem playerAttackSystem = FindObjectOfType<PlayerAttackSystem>();
                if (playerAttackSystem != null)
                {
                    player = playerAttackSystem.gameObject;
                }
                else
                {
                    // Try finding PlayerController as fallback
                    PlayerController playerController = FindObjectOfType<PlayerController>();
                    if (playerController != null)
                    {
                        player = playerController.gameObject;
                    }
                }
            }
        }
        
        if (player != null)
        {
            Vector3 targetPosition = GetSpawnPosition();
            player.transform.position = targetPosition;
            
            if (enableDebugLog)
            {
                Debug.Log($"[NewGameInitializer] Set player position to: {targetPosition}");
            }
        }
        else
        {
            if (enableDebugLog)
            {
                Debug.LogWarning("[NewGameInitializer] Player not found, cannot set spawn position");
            }
        }
    }
    
    /// <summary>
    /// Get the spawn position
    /// </summary>
    private Vector3 GetSpawnPosition()
    {
        if (playerSpawnPoint != null)
        {
            return playerSpawnPoint.position;
        }
        
        // Get scene-specific spawn position based on scene name
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return GetSceneSpecificSpawnPosition(sceneName);
    }
    
    /// <summary>
    /// Get scene-specific spawn position
    /// </summary>
    private Vector3 GetSceneSpecificSpawnPosition(string sceneName)
    {
        switch (sceneName)
        {
            case "5.26地图":
                return new Vector3(-8f, 0f, 0f);
            case "可以运行的地图":
                return new Vector3(-10f, 0f, 0f);
            case "Example1":
                return new Vector3(-5f, 0f, 0f);
            case "关卡1":
            case "Level1":
                return new Vector3(-6f, 0f, 0f);
            default:
                return defaultSpawnPosition;
        }
    }
    
    /// <summary>
    /// Set custom spawn point
    /// </summary>
    public void SetSpawnPoint(Transform spawnPoint)
    {
        playerSpawnPoint = spawnPoint;
    }
    
    /// <summary>
    /// Set custom spawn position
    /// </summary>
    public void SetSpawnPosition(Vector3 position)
    {
        defaultSpawnPosition = position;
    }
    
    // Gizmo for spawn point visualization
    private void OnDrawGizmos()
    {
        if (!showSpawnPointGizmo) return;
        
        Vector3 spawnPos = GetSpawnPosition();
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(spawnPos, 0.5f);
        Gizmos.DrawLine(spawnPos + Vector3.up, spawnPos + Vector3.down);
        Gizmos.DrawLine(spawnPos + Vector3.left * 0.5f, spawnPos + Vector3.right * 0.5f);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(spawnPos, Vector3.one * 0.2f);
    }
} 