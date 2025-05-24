using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI面板")]
    public GameObject pauseMenuPanel;
    public GameObject saveSlotPanel;
    
    [Header("按钮")]
    public Button resumeButton;
    public Button saveButton;
    public Button loadButton;
    public Button optionsButton;
    public Button mainMenuButton;
    public Button[] saveSlotButtons; // 存档槽位按钮
    public Button backFromSaveButton;
    
    [Header("设置")]
    public KeyCode pauseKey = KeyCode.Escape;
    
    private bool isPaused = false;
    
    private void Start()
    {
        // 初始化UI
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        if (saveSlotPanel != null)
            saveSlotPanel.SetActive(false);
        
        // 绑定按钮事件
        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);
        if (saveButton != null)
            saveButton.onClick.AddListener(OpenSavePanel);
        if (loadButton != null)
            loadButton.onClick.AddListener(OpenLoadPanel);
        if (optionsButton != null)
            optionsButton.onClick.AddListener(OpenOptions);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        if (backFromSaveButton != null)
            backFromSaveButton.onClick.AddListener(CloseSavePanel);
        
        // 绑定存档槽位按钮
        for (int i = 0; i < saveSlotButtons.Length; i++)
        {
            int slotIndex = i;
            if (saveSlotButtons[i] != null)
            {
                saveSlotButtons[i].onClick.AddListener(() => SaveToSlot(slotIndex));
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
    
    private void OpenSavePanel()
    {
        if (saveSlotPanel != null)
        {
            saveSlotPanel.SetActive(true);
            UpdateSaveSlotButtons();
        }
    }
    
    private void CloseSavePanel()
    {
        if (saveSlotPanel != null)
        {
            saveSlotPanel.SetActive(false);
        }
    }
    
    private void SaveToSlot(int slotIndex)
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame(slotIndex);
            CloseSavePanel();
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
                var buttonText = saveSlotButtons[i].GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    if (saveInfos[i].isEmpty)
                    {
                        buttonText.text = $"空槽位 {i + 1}";
                    }
                    else
                    {
                        buttonText.text = $"{saveInfos[i].saveName}\n{saveInfos[i].saveTime:yyyy-MM-dd HH:mm}";
                    }
                }
            }
        }
    }
    
    private void OpenLoadPanel()
    {
        // 可以复用LoadGamePanelManager或创建新的加载界面
        Debug.Log("打开加载界面");
    }
    
    private void OpenOptions()
    {
        // 打开选项菜单
        Debug.Log("打开选项菜单");
    }
    
    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }
    
    private void OnDestroy()
    {
        // 确保时间缩放恢复正常
        Time.timeScale = 1f;
    }
} 