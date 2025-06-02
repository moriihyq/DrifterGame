using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Save Slot New Game Example
/// Demonstrates complete implementation of adding new game functionality to save slots
/// </summary>
public class SaveSlotNewGameExample : MonoBehaviour
{
    [Header("UI Component References")]
    [SerializeField] private Button slotButton;           // Save slot button
    [SerializeField] private TextMeshProUGUI slotText;    // Slot display text
    [SerializeField] private TextMeshProUGUI detailText;  // Detail information text
    [SerializeField] private GameObject emptySlotPanel;   // Empty slot panel
    [SerializeField] private GameObject saveInfoPanel;    // Save info panel
    [SerializeField] private Button deleteButton;         // Delete button
    
    [Header("Slot Settings")]
    [SerializeField] private int slotIndex = 0;           // Slot index
    [SerializeField] private string emptySlotText = "[Click to Create New Game]";  // Empty slot prompt text
    
    [Header("Color Configuration")]
    [SerializeField] private Color emptySlotColor = new Color(0.7f, 1f, 0.7f, 1f);    // Empty slot color (light green)
    [SerializeField] private Color activeSaveColor = new Color(0f, 1f, 1f, 1f);       // Existing save color (cyan)
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 0.7f, 1f);          // Hover color (light yellow)
    
    private LoadGamePanelManager panelManager;
    private SaveInfo currentSaveInfo;
    private bool isEmpty = true;
    
    private void Start()
    {
        // 查找LoadGamePanelManager
        panelManager = FindFirstObjectByType<LoadGamePanelManager>();
        if (panelManager == null)
        {
            Debug.LogError($"[SaveSlotNewGameExample] Slot {slotIndex}: LoadGamePanelManager not found");
        }
        
        // 初始化按钮事件
        if (slotButton != null)
        {
            slotButton.onClick.AddListener(OnSlotButtonClick);
        }
        
        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(OnDeleteButtonClick);
        }
        
        // 初始化显示
        RefreshSlotDisplay();
    }
    
    /// <summary>
    /// Slot button click event
    /// </summary>
    private void OnSlotButtonClick()
    {
        if (panelManager == null)
        {
            Debug.LogError($"[SaveSlotNewGameExample] Slot {slotIndex}: LoadGamePanelManager not found");
            return;
        }
        
        if (isEmpty)
        {
            // Empty slot: Create new save
            Debug.Log($"🎮 Slot {slotIndex}: Creating new game");
            panelManager.CreateNewSaveToSlot(slotIndex);
        }
        else
        {
            // Existing save: Load save
            Debug.Log($"📁 Slot {slotIndex}: Loading save");
            panelManager.LoadSelectedSave(slotIndex);
        }
    }
    
    /// <summary>
    /// Delete button click event
    /// </summary>
    private void OnDeleteButtonClick()
    {
        if (panelManager == null || isEmpty)
        {
            return;
        }
        
        Debug.Log($"🗑️ Slot {slotIndex}: Deleting save");
        panelManager.DeleteSelectedSave(slotIndex);
        
        // Wait for one frame before refreshing display
        StartCoroutine(RefreshAfterFrame());
    }
    
    /// <summary>
    /// Refresh slot display
    /// </summary>
    public void RefreshSlotDisplay()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning($"[SaveSlotNewGameExample] Slot {slotIndex}: SaveManager not found");
            return;
        }
        
        // Get save information
        var saveInfos = SaveManager.Instance.GetAllSaveInfos();
        if (slotIndex >= 0 && slotIndex < saveInfos.Count)
        {
            currentSaveInfo = saveInfos[slotIndex];
            isEmpty = currentSaveInfo.isEmpty;
            
            UpdateSlotAppearance();
        }
        else
        {
            Debug.LogError($"[SaveSlotNewGameExample] Slot index {slotIndex} out of range");
        }
    }
    
    /// <summary>
    /// Update slot appearance
    /// </summary>
    private void UpdateSlotAppearance()
    {
        if (isEmpty)
        {
            ShowEmptySlot();
        }
        else
        {
            ShowSaveInfo();
        }
        
        // Update delete button display
        if (deleteButton != null)
        {
            deleteButton.gameObject.SetActive(!isEmpty);
        }
    }
    
    /// <summary>
    /// Show empty slot
    /// </summary>
    private void ShowEmptySlot()
    {
        // Show empty slot panel
        if (emptySlotPanel != null) emptySlotPanel.SetActive(true);
        if (saveInfoPanel != null) saveInfoPanel.SetActive(false);
        
        // Set main text
        if (slotText != null)
        {
            slotText.text = $"Save slot {slotIndex + 1}";
            slotText.color = emptySlotColor;
        }
        
        // Set detail text
        if (detailText != null)
        {
            detailText.text = emptySlotText;
            detailText.color = emptySlotColor;
        }
        
        // Set button interactable
        if (slotButton != null)
        {
            slotButton.interactable = true;
        }
        
        Debug.Log($"🔄 Slot {slotIndex}: Displayed as empty slot");
    }
    
    /// <summary>
    /// Show save information
    /// </summary>
    private void ShowSaveInfo()
    {
        // Show save info panel
        if (emptySlotPanel != null) emptySlotPanel.SetActive(false);
        if (saveInfoPanel != null) saveInfoPanel.SetActive(true);
        
        // Set main text
        if (slotText != null)
        {
            slotText.text = currentSaveInfo.saveName;
            slotText.color = activeSaveColor;
        }
        
        // Set detail text
        if (detailText != null)
        {
            string timeStr = currentSaveInfo.saveTime.ToString("MM-dd HH:mm");
            detailText.text = $"Save time: {timeStr}\nScene: {currentSaveInfo.sceneName}\nHealth: {currentSaveInfo.playerHealth}";
            detailText.color = activeSaveColor;
        }
        
        // Set button interactable
        if (slotButton != null)
        {
            slotButton.interactable = true;
        }
        
        Debug.Log($"🔄 Slot {slotIndex}: Displayed save information - {currentSaveInfo.saveName}");
    }
    
    /// <summary>
    /// Wait for one frame before refreshing (used after delete operation)
    /// </summary>
    private System.Collections.IEnumerator RefreshAfterFrame()
    {
        yield return null;
        RefreshSlotDisplay();
    }
    
    /// <summary>
    /// Set slot index
    /// </summary>
    public void SetSlotIndex(int index)
    {
        slotIndex = index;
        RefreshSlotDisplay();
    }
    
    /// <summary>
    /// Get current slot emptiness
    /// </summary>
    public bool IsEmpty => isEmpty;
    
    /// <summary>
    /// Get current save information
    /// </summary>
    public SaveInfo GetCurrentSaveInfo() => currentSaveInfo;
    
    // Editor debugging functionality
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        
        // Automatically find components in editor
        if (slotButton == null)
        {
            slotButton = GetComponent<Button>();
        }
        
        if (slotText == null)
        {
            slotText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }
    
    // GUI debugging information
    private void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        // Display slot status at bottom right of screen
        float x = Screen.width - 200;
        float y = Screen.height - 100 - (slotIndex * 25);
        
        string status = isEmpty ? "Empty" : "Existing save";
        GUI.Label(new Rect(x, y, 200, 20), $"Slot {slotIndex}: {status}");
    }
} 