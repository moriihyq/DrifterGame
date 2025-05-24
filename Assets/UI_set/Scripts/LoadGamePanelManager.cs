using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadGamePanelManager : MonoBehaviour
{
    [Header("UI引用")]
    public Transform saveSlotContainer; // 存档槽位的父对象
    public GameObject saveSlotPrefab; // 存档槽位预制体
    public Button backButton; // 返回按钮
    public GameObject mainUIPanel; // 主UI面板引用
    
    [Header("存档槽位按钮")]
    public Button[] saveSlots; // 存档槽位按钮数组
    
    private void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(CloseLoadGamePanel);
        }
        
        // 初始化时刷新存档列表
        RefreshSaveSlots();
    }
    
    public void RefreshSaveSlots()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager未找到！");
            return;
        }
        
        List<SaveInfo> saveInfos = SaveManager.Instance.GetAllSaveInfos();
        
        // 如果使用预设的槽位
        if (saveSlots != null && saveSlots.Length > 0)
        {
            for (int i = 0; i < saveSlots.Length && i < saveInfos.Count; i++)
            {
                // 获取按钮上的文本组件
                TextMeshProUGUI buttonText = saveSlots[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    if (saveInfos[i].isEmpty)
                    {
                        buttonText.text = $"空槽位 {i + 1}";
                        saveSlots[i].interactable = false;
                    }
                    else
                    {
                        buttonText.text = $"{saveInfos[i].saveName}\n{saveInfos[i].saveTime:yyyy-MM-dd HH:mm}\n场景: {saveInfos[i].sceneName}";
                        saveSlots[i].interactable = true;
                    }
                }
                
                // 设置按钮点击事件
                int slotIndex = i; // 创建局部变量以在lambda中使用
                saveSlots[i].onClick.RemoveAllListeners();
                saveSlots[i].onClick.AddListener(() => LoadSelectedSave(slotIndex));
            }
        }
        // 如果使用动态生成
        else if (saveSlotPrefab != null && saveSlotContainer != null)
        {
            // 清除现有的槽位
            foreach (Transform child in saveSlotContainer)
            {
                Destroy(child.gameObject);
            }
            
            // 创建新的槽位
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
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSave(slotIndex);
            RefreshSaveSlots();
        }
    }
    
    private void CloseLoadGamePanel()
    {
        gameObject.SetActive(false);
        if (mainUIPanel != null)
        {
            mainUIPanel.SetActive(true);
        }
    }
}

// 存档槽位UI组件
[System.Serializable]
public class SaveSlotUI : MonoBehaviour
{
    [Header("UI组件")]
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
            // 显示空槽位
            if (emptySlotIndicator != null) emptySlotIndicator.SetActive(true);
            if (saveInfoContainer != null) saveInfoContainer.SetActive(false);
            if (loadButton != null) loadButton.interactable = false;
            if (deleteButton != null) deleteButton.gameObject.SetActive(false);
            
            if (slotNameText != null) slotNameText.text = saveInfo.saveName;
        }
        else
        {
            // 显示存档信息
            if (emptySlotIndicator != null) emptySlotIndicator.SetActive(false);
            if (saveInfoContainer != null) saveInfoContainer.SetActive(true);
            if (loadButton != null) loadButton.interactable = true;
            if (deleteButton != null) deleteButton.gameObject.SetActive(true);
            
            // 设置文本
            if (slotNameText != null) slotNameText.text = saveInfo.saveName;
            if (saveTimeText != null) saveTimeText.text = saveInfo.saveTime.ToString("yyyy-MM-dd HH:mm:ss");
            if (sceneNameText != null) sceneNameText.text = $"场景: {saveInfo.sceneName}";
            if (playerHealthText != null) playerHealthText.text = $"生命值: {saveInfo.playerHealth}";
            if (playTimeText != null) playTimeText.text = $"游戏时间: {FormatPlayTime(saveInfo.playTime)}";
            
            // 设置按钮事件
            if (loadButton != null)
            {
                loadButton.onClick.RemoveAllListeners();
                loadButton.onClick.AddListener(() => panelManager.LoadSelectedSave(saveInfo.slotIndex));
            }
            
            if (deleteButton != null)
            {
                deleteButton.onClick.RemoveAllListeners();
                deleteButton.onClick.AddListener(() => 
                {
                    // 可以添加确认对话框
                    panelManager.DeleteSelectedSave(saveInfo.slotIndex);
                });
            }
        }
    }
    
    private string FormatPlayTime(float seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(seconds);
        return string.Format("{0:D2}:{1:D2}:{2:D2}", time.Hours, time.Minutes, time.Seconds);
    }
} 