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
    public GameObject dimmer; // Dimmer背景遮罩
    
    [Header("Save Slot Buttons")]
    public Button[] saveSlots;
    public Button[] deleteButtons;
    
    private TMP_FontAsset customFont;
    
    private void OnEnable()
    {
        // 激活Dimmer背景遮罩
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
        // 关闭Dimmer背景遮罩
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
                        // Use new naming convention for empty slots
                        buttonText.text = $"SAVESLOT{i + 1}\nEMPTY";
                        saveSlots[i].interactable = false;
                        
                        // Set a slightly dimmer color for empty slots
                        buttonText.color = new Color(0.5f, 0.8f, 0.8f, 1f); // Dimmed cyan
                        
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
                saveSlots[i].onClick.AddListener(() => LoadSelectedSave(slotIndex));
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
        // 关闭Dimmer背景遮罩
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
            // Show empty slot
            if (emptySlotIndicator != null) emptySlotIndicator.SetActive(true);
            if (saveInfoContainer != null) saveInfoContainer.SetActive(false);
            if (loadButton != null) loadButton.interactable = false;
            if (deleteButton != null) deleteButton.gameObject.SetActive(false);
            
            if (slotNameText != null) 
            {
                slotNameText.text = saveInfo.saveName;
                // Set dimmed cyan color for empty slots
                slotNameText.color = new Color(0.5f, 0.8f, 0.8f, 1f);
                // Increase font size for better readability
                slotNameText.fontSize = 38f; // Increased main text size
            }
        }
        else
        {
            // Show save info
            if (emptySlotIndicator != null) emptySlotIndicator.SetActive(false);
            if (saveInfoContainer != null) saveInfoContainer.SetActive(true);
            if (loadButton != null) loadButton.interactable = true;
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
            
            // Set button events
            if (loadButton != null)
            {
                loadButton.onClick.RemoveAllListeners();
                loadButton.onClick.AddListener(() => panelManager.LoadSelectedSave(saveInfo.slotIndex));
            }
            
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