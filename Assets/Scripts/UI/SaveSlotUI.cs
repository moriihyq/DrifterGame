using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text slotNumberText;
    public TMP_Text saveNameText;
    public TMP_Text saveTimeText;
    public TMP_Text playTimeText;
    public Button loadButton;
    public Button deleteButton;
    public Button newSaveButton;
    public GameObject saveInfoPanel;
    public GameObject emptySlotPanel;
    
    private int slotIndex;
    private LoadGamePanelManager panelManager;
    
    public void Initialize(int slot, SaveData data, LoadGamePanelManager manager)
    {
        slotIndex = slot;
        panelManager = manager;
        
        if (slotNumberText != null)
            slotNumberText.text = $"槽位 {slot + 1}";
        
        if (data != null)
        {
            // 显示存档信息
            if (saveInfoPanel != null) saveInfoPanel.SetActive(true);
            if (emptySlotPanel != null) emptySlotPanel.SetActive(false);
            
            if (saveNameText != null)
                saveNameText.text = data.saveName;
                
            if (saveTimeText != null)
                saveTimeText.text = data.saveTime.ToString("yyyy/MM/dd HH:mm");
                
            if (playTimeText != null)
            {
                System.TimeSpan playTime = System.TimeSpan.FromSeconds(data.playTime);
                playTimeText.text = $"游戏时间: {playTime.Hours:D2}:{playTime.Minutes:D2}:{playTime.Seconds:D2}";
            }
            
            // 绑定按钮
            if (loadButton != null)
                loadButton.onClick.AddListener(() => SaveManager.Instance.LoadGame(slotIndex));
                
            if (deleteButton != null)
                deleteButton.onClick.AddListener(OnDeleteClick);
        }
        else
        {
            // 显示空槽位
            if (saveInfoPanel != null) saveInfoPanel.SetActive(false);
            if (emptySlotPanel != null) emptySlotPanel.SetActive(true);
            
            if (newSaveButton != null)
                newSaveButton.onClick.AddListener(OnNewSaveClick);
        }
    }
    
    private void OnDeleteClick()
    {
        SaveManager.Instance.DeleteSave(slotIndex);
        if (panelManager != null)
            panelManager.RefreshSaveSlots();
    }
    
    private void OnNewSaveClick()
    {
        // 开始新游戏并记住要保存的槽位
        PlayerPrefs.SetInt("PendingSaveSlot", slotIndex);
        PlayerPrefs.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene("5.5 map");
    }
} 