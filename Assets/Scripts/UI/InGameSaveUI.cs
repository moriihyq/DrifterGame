using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InGameSaveUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject saveMenuPanel;
    public GameObject saveSlotPrefab;
    public Transform saveSlotContainer;
    public Button closeSaveMenuButton;
    public KeyCode saveMenuKey = KeyCode.Escape;
    
    [Header("Save Confirmation")]
    public GameObject saveConfirmDialog;
    public TMP_Text confirmText;
    public Button confirmButton;
    public Button cancelButton;
    
    private bool isMenuOpen = false;
    private List<GameObject> saveSlotInstances = new List<GameObject>();
    
    private void Start()
    {
        // 初始化UI
        if (saveMenuPanel != null)
            saveMenuPanel.SetActive(false);
            
        if (saveConfirmDialog != null)
            saveConfirmDialog.SetActive(false);
            
        // 绑定按钮事件
        if (closeSaveMenuButton != null)
            closeSaveMenuButton.onClick.AddListener(CloseSaveMenu);
            
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmSave);
            
        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelSave);
            
        // 检查是否有待保存的槽位（新游戏）
        if (PlayerPrefs.HasKey("PendingSaveSlot"))
        {
            int pendingSlot = PlayerPrefs.GetInt("PendingSaveSlot");
            PlayerPrefs.DeleteKey("PendingSaveSlot");
            PlayerPrefs.Save();
            
            // 自动保存到指定槽位
            SaveManager.Instance.SaveGame(pendingSlot, $"新游戏 {pendingSlot + 1}");
        }
    }
    
    private void Update()
    {
        // 按ESC键打开/关闭存档菜单
        if (Input.GetKeyDown(saveMenuKey))
        {
            if (isMenuOpen)
                CloseSaveMenu();
            else
                OpenSaveMenu();
        }
    }
    
    public void OpenSaveMenu()
    {
        isMenuOpen = true;
        if (saveMenuPanel != null)
        {
            saveMenuPanel.SetActive(true);
            RefreshSaveSlots();
            
            // 暂停游戏
            Time.timeScale = 0f;
        }
    }
    
    public void CloseSaveMenu()
    {
        isMenuOpen = false;
        if (saveMenuPanel != null)
            saveMenuPanel.SetActive(false);
            
        if (saveConfirmDialog != null)
            saveConfirmDialog.SetActive(false);
            
        // 恢复游戏
        Time.timeScale = 1f;
    }
    
    private void RefreshSaveSlots()
    {
        // 清除现有槽位
        foreach (var slot in saveSlotInstances)
        {
            Destroy(slot);
        }
        saveSlotInstances.Clear();
        
        // 获取所有存档
        List<SaveData> saves = SaveManager.Instance.GetAllSaves();
        
        // 创建存档槽位UI
        for (int i = 0; i < saves.Count; i++)
        {
            CreateSaveSlotUI(i, saves[i]);
        }
    }
    
    private void CreateSaveSlotUI(int slotIndex, SaveData existingSave)
    {
        GameObject slotObj = Instantiate(saveSlotPrefab, saveSlotContainer);
        saveSlotInstances.Add(slotObj);
        
        // 获取UI组件
        TMP_Text slotText = slotObj.GetComponentInChildren<TMP_Text>();
        Button saveButton = slotObj.GetComponent<Button>();
        
        // 设置显示文本
        string displayText = $"槽位 {slotIndex + 1}";
        if (existingSave != null)
        {
            displayText += $" - {existingSave.saveName} ({existingSave.saveTime:MM/dd HH:mm})";
        }
        else
        {
            displayText += " - [空]";
        }
        
        if (slotText != null)
            slotText.text = displayText;
            
        // 绑定按钮事件
        if (saveButton != null)
        {
            int slot = slotIndex; // 避免闭包问题
            saveButton.onClick.AddListener(() => OnSaveSlotClick(slot, existingSave != null));
        }
    }
    
    private int selectedSlot = -1;
    
    private void OnSaveSlotClick(int slot, bool hasExistingSave)
    {
        selectedSlot = slot;
        
        if (hasExistingSave)
        {
            // 显示覆盖确认对话框
            if (saveConfirmDialog != null && confirmText != null)
            {
                confirmText.text = $"确定要覆盖槽位 {slot + 1} 的存档吗？";
                saveConfirmDialog.SetActive(true);
            }
        }
        else
        {
            // 直接保存到空槽位
            SaveToSlot(slot);
        }
    }
    
    private void OnConfirmSave()
    {
        if (selectedSlot >= 0)
        {
            SaveToSlot(selectedSlot);
        }
        
        if (saveConfirmDialog != null)
            saveConfirmDialog.SetActive(false);
    }
    
    private void OnCancelSave()
    {
        selectedSlot = -1;
        if (saveConfirmDialog != null)
            saveConfirmDialog.SetActive(false);
    }
    
    private void SaveToSlot(int slot)
    {
        SaveManager.Instance.SaveGame(slot);
        
        // 显示保存成功提示
        Debug.Log($"游戏已保存到槽位 {slot + 1}");
        
        // 刷新存档列表
        RefreshSaveSlots();
        
        // 可以在这里添加保存成功的UI提示
    }
    
    // 快速保存功能（F5）
    private void QuickSave()
    {
        // 保存到最近使用的槽位或第一个槽位
        int quickSaveSlot = PlayerPrefs.GetInt("LastUsedSaveSlot", 0);
        SaveManager.Instance.SaveGame(quickSaveSlot, "快速存档");
        PlayerPrefs.SetInt("LastUsedSaveSlot", quickSaveSlot);
        
        Debug.Log($"快速保存到槽位 {quickSaveSlot + 1}");
    }
    
    // 可以添加快捷键
    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            QuickSave();
        }
    }
} 