using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pauseMenuPanel;
    public GameObject saveSlotPanel;
    
    [Header("Buttons")]
    public Button resumeButton;
    public Button saveButton;
    public Button loadButton;
    public Button optionsButton;
    public Button mainMenuButton;
    public Button[] saveSlotButtons; // Save slot buttons
    public Button[] deleteSlotButtons; // Delete save buttons (one per slot)
    public Button backFromSaveButton;
    
    [Header("Settings")]
    public KeyCode pauseKey = KeyCode.Escape;
    public bool showDeleteButtons = true; // Whether to show delete buttons
    
    private bool isPaused = false;
    private bool isInSaveMode = true; // true for save mode, false for load mode
    
    private void Start()
    {
        // Initialize UI
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        if (saveSlotPanel != null)
            saveSlotPanel.SetActive(false);
        
        // Bind button events
        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);
        if (saveButton != null)
            saveButton.onClick.AddListener(() => OpenSaveLoadPanel(true));
        if (loadButton != null)
            loadButton.onClick.AddListener(() => OpenSaveLoadPanel(false));
        if (optionsButton != null)
            optionsButton.onClick.AddListener(OpenOptions);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        if (backFromSaveButton != null)
            backFromSaveButton.onClick.AddListener(CloseSavePanel);
        
        // Bind save slot buttons
        for (int i = 0; i < saveSlotButtons.Length; i++)
        {
            int slotIndex = i;
            if (saveSlotButtons[i] != null)
            {
                saveSlotButtons[i].onClick.AddListener(() => OnSlotClick(slotIndex));
            }
        }
        
        // Bind delete buttons
        if (deleteSlotButtons != null)
        {
            for (int i = 0; i < deleteSlotButtons.Length; i++)
            {
                int slotIndex = i;
                if (i < deleteSlotButtons.Length && deleteSlotButtons[i] != null)
                {
                    deleteSlotButtons[i].onClick.AddListener(() => DeleteSlot(slotIndex));
                }
            }
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }
    
    public void Pause()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
            Time.timeScale = 0f;
            isPaused = true;
        }
    }
    
    public void Resume()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
            if (saveSlotPanel != null)
                saveSlotPanel.SetActive(false);
            Time.timeScale = 1f;
            isPaused = false;
        }
    }
    
    private void OpenSaveLoadPanel(bool saveMode)
    {
        isInSaveMode = saveMode;
        if (saveSlotPanel != null)
        {
            saveSlotPanel.SetActive(true);
            UpdateSaveSlotButtons();
            
            // Update panel title (if exists)
            var titleText = saveSlotPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (titleText != null && titleText.name == "Title")
            {
                titleText.text = saveMode ? "Select Save Slot" : "Select Save to Load";
            }
        }
    }
    
    private void CloseSavePanel()
    {
        if (saveSlotPanel != null)
        {
            saveSlotPanel.SetActive(false);
        }
    }
    
    private void OnSlotClick(int slotIndex)
    {
        if (isInSaveMode)
        {
            SaveToSlot(slotIndex);
        }
        else
        {
            LoadFromSlot(slotIndex);
        }
    }
    
    private void SaveToSlot(int slotIndex)
    {
        if (SaveManager.Instance != null)
        {
            // Save directly, no confirmation needed
            SaveManager.Instance.SaveGame(slotIndex);
            Debug.Log($"Saved to slot {slotIndex + 1}");
            
            // Close pause menu and resume game immediately after saving
            Resume();
        }
    }
    
    private void LoadFromSlot(int slotIndex)
    {
        if (SaveManager.Instance != null)
        {
            var saveInfos = SaveManager.Instance.GetAllSaveInfos();
            if (slotIndex < saveInfos.Count && !saveInfos[slotIndex].isEmpty)
            {
                Debug.Log($"Loading save from slot {slotIndex + 1}");
                
                // Close pause menu and resume game immediately
                Resume();
                
                // Use quick loading method for current scene
                SaveManager.Instance.LoadGameInCurrentScene(slotIndex);
            }
            else
            {
                Debug.LogWarning($"Slot {slotIndex + 1} is empty or invalid");
            }
        }
    }
    
    private void DeleteSlot(int slotIndex)
    {
        if (SaveManager.Instance != null)
        {
            var saveInfos = SaveManager.Instance.GetAllSaveInfos();
            if (slotIndex < saveInfos.Count && !saveInfos[slotIndex].isEmpty)
            {
                // Delete directly, no confirmation needed
                SaveManager.Instance.DeleteSave(slotIndex);
                UpdateSaveSlotButtons();
                Debug.Log($"Deleted save in slot {slotIndex + 1}");
            }
        }
    }
    
    private void UpdateSaveSlotButtons()
    {
        if (SaveManager.Instance == null) return;
        
        var saveInfos = SaveManager.Instance.GetAllSaveInfos();
        
        for (int i = 0; i < saveSlotButtons.Length && i < saveInfos.Count; i++)
        {
            if (saveSlotButtons[i] != null)
            {
                var buttonText = saveSlotButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    if (saveInfos[i].isEmpty)
                    {
                        // Use new naming convention for empty slots
                        if (i == 0)
                        {
                            buttonText.text = "LATEST SAVE\nEMPTY";
                        }
                        else
                        {
                            buttonText.text = $"SAVESLOT{i}\nEMPTY";
                        }
                        
                        if (isInSaveMode)
                        {
                            saveSlotButtons[i].interactable = true;
                        }
                        else
                        {
                            saveSlotButtons[i].interactable = false;
                        }
                        
                        if (deleteSlotButtons != null && i < deleteSlotButtons.Length)
                        {
                            deleteSlotButtons[i].gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        string timeStr = saveInfos[i].saveTime.ToString("MM-dd HH:mm");
                        // Use new naming convention for filled slots
                        if (i == 0)
                        {
                            buttonText.text = $"LATEST SAVE\n{timeStr}";
                        }
                        else
                        {
                            buttonText.text = $"SAVESLOT{i}\n{timeStr}";
                        }
                        saveSlotButtons[i].interactable = true;
                        
                        if (deleteSlotButtons != null && i < deleteSlotButtons.Length)
                        {
                            deleteSlotButtons[i].gameObject.SetActive(showDeleteButtons);
                        }
                    }
                }
            }
        }
    }
    
    private void OpenOptions()
    {
        // Implement options menu functionality
        Debug.Log("Options menu not implemented yet");
    }
    
    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }
    
    private void OnDestroy()
    {
        // Ensure time scale is restored when the menu is destroyed
        Time.timeScale = 1f;
    }
} 