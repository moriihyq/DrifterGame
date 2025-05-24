using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadGamePanelManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject saveSlotPrefab; // 存档槽位预制体
    public Transform saveSlotContainer; // 存档槽位的父容器
    public Button backButton; // 返回按钮
    
    [Header("New Save Dialog")]
    public GameObject newSaveDialog; // 新建存档对话框
    public TMP_InputField saveNameInput; // 存档名称输入框
    public Button confirmSaveButton; // 确认存档按钮
    public Button cancelSaveButton; // 取消存档按钮
    
    private List<GameObject> saveSlotInstances = new List<GameObject>();
    private int selectedSlot = -1;
    
    private void Start()
    {
        // 确保对话框初始关闭
        if (newSaveDialog != null)
            newSaveDialog.SetActive(false);
            
        // 绑定按钮事件
        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClick);
            
        if (confirmSaveButton != null)
            confirmSaveButton.onClick.AddListener(OnConfirmNewSave);
            
        if (cancelSaveButton != null)
            cancelSaveButton.onClick.AddListener(OnCancelNewSave);
            
        RefreshSaveSlots();
    }
    
    private void OnEnable()
    {
        RefreshSaveSlots();
    }
    
    public void RefreshSaveSlots()
    {
        // 清除现有的槽位
        foreach (var slot in saveSlotInstances)
        {
            Destroy(slot);
        }
        saveSlotInstances.Clear();
        
        // 获取所有存档
        List<SaveData> saves = SaveManager.Instance.GetAllSaves();
        
        // 为每个槽位创建UI
        for (int i = 0; i < saves.Count; i++)
        {
            CreateSaveSlot(i, saves[i]);
        }
    }
    
    private void CreateSaveSlot(int slotIndex, SaveData saveData)
    {
        GameObject slotObj = Instantiate(saveSlotPrefab, saveSlotContainer);
        saveSlotInstances.Add(slotObj);
        
        // 获取槽位组件
        TMP_Text slotNumberText = slotObj.transform.Find("SlotNumber")?.GetComponent<TMP_Text>();
        TMP_Text saveNameText = slotObj.transform.Find("SaveName")?.GetComponent<TMP_Text>();
        TMP_Text saveTimeText = slotObj.transform.Find("SaveTime")?.GetComponent<TMP_Text>();
        TMP_Text playTimeText = slotObj.transform.Find("PlayTime")?.GetComponent<TMP_Text>();
        Button loadButton = slotObj.transform.Find("LoadButton")?.GetComponent<Button>();
        Button deleteButton = slotObj.transform.Find("DeleteButton")?.GetComponent<Button>();
        Button newSaveButton = slotObj.transform.Find("NewSaveButton")?.GetComponent<Button>();
        GameObject saveInfo = slotObj.transform.Find("SaveInfo")?.gameObject;
        GameObject emptySlot = slotObj.transform.Find("EmptySlot")?.gameObject;
        
        // 设置槽位编号
        if (slotNumberText != null)
            slotNumberText.text = $"槽位 {slotIndex + 1}";
        
        if (saveData != null)
        {
            // 有存档，显示存档信息
            if (saveInfo != null) saveInfo.SetActive(true);
            if (emptySlot != null) emptySlot.SetActive(false);
            
            if (saveNameText != null)
                saveNameText.text = saveData.saveName;
                
            if (saveTimeText != null)
                saveTimeText.text = saveData.saveTime.ToString("yyyy/MM/dd HH:mm");
                
            if (playTimeText != null)
            {
                TimeSpan playTime = TimeSpan.FromSeconds(saveData.playTime);
                playTimeText.text = $"游戏时间: {playTime.Hours:D2}:{playTime.Minutes:D2}:{playTime.Seconds:D2}";
            }
            
            // 绑定按钮事件
            if (loadButton != null)
            {
                int slot = slotIndex; // 避免闭包问题
                loadButton.onClick.AddListener(() => OnLoadButtonClick(slot));
            }
            
            if (deleteButton != null)
            {
                int slot = slotIndex;
                deleteButton.onClick.AddListener(() => OnDeleteButtonClick(slot));
            }
        }
        else
        {
            // 空槽位，显示新建存档按钮
            if (saveInfo != null) saveInfo.SetActive(false);
            if (emptySlot != null) emptySlot.SetActive(true);
            
            if (newSaveButton != null)
            {
                int slot = slotIndex;
                newSaveButton.onClick.AddListener(() => OnNewSaveButtonClick(slot));
            }
        }
    }
    
    private void OnLoadButtonClick(int slot)
    {
        SaveManager.Instance.LoadGame(slot);
    }
    
    private void OnDeleteButtonClick(int slot)
    {
        // 显示确认对话框
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            // 简单的确认
            SaveManager.Instance.DeleteSave(slot);
            RefreshSaveSlots();
        }
    }
    
    private void OnNewSaveButtonClick(int slot)
    {
        selectedSlot = slot;
        ShowNewSaveDialog();
    }
    
    private void ShowNewSaveDialog()
    {
        if (newSaveDialog != null)
        {
            newSaveDialog.SetActive(true);
            if (saveNameInput != null)
            {
                saveNameInput.text = $"存档 {selectedSlot + 1}";
                saveNameInput.Select();
            }
        }
    }
    
    private void OnConfirmNewSave()
    {
        if (selectedSlot >= 0 && saveNameInput != null)
        {
            string saveName = string.IsNullOrEmpty(saveNameInput.text) ? 
                $"存档 {selectedSlot + 1}" : saveNameInput.text;
                
            // 这里应该在游戏场景中调用，现在只是创建新存档
            // 实际使用时，应该先开始新游戏，然后在游戏中保存
            Debug.Log($"准备在槽位 {selectedSlot + 1} 创建新存档: {saveName}");
            
            // 暂时直接进入游戏场景
            UnityEngine.SceneManagement.SceneManager.LoadScene("5.5 map");
        }
        
        if (newSaveDialog != null)
            newSaveDialog.SetActive(false);
    }
    
    private void OnCancelNewSave()
    {
        selectedSlot = -1;
        if (newSaveDialog != null)
            newSaveDialog.SetActive(false);
    }
    
    private void OnBackButtonClick()
    {
        // 返回主菜单
        gameObject.SetActive(false);
        
        // 显示主菜单UI
        GameObject mainUI = GameObject.Find("MainUIPanel");
        if (mainUI != null)
            mainUI.SetActive(true);
    }
} 