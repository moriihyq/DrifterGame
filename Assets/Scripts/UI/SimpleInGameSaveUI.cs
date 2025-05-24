using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SimpleInGameSaveUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject saveMenuPanel;
    public Transform slotContainer;
    public Button closeSaveMenuButton;
    public Button saveToNewSlotButton; // 保存到新槽位按钮
    
    [Header("Settings")]
    public KeyCode saveMenuKey = KeyCode.Escape;
    public KeyCode quickSaveKey = KeyCode.F5;
    public Color emptySlotColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    public Color filledSlotColor = Color.white;
    
    private bool isMenuOpen = false;
    
    private void Start()
    {
        // 初始化UI
        if (saveMenuPanel != null)
            saveMenuPanel.SetActive(false);
            
        // 绑定按钮事件
        if (closeSaveMenuButton != null)
            closeSaveMenuButton.onClick.AddListener(CloseSaveMenu);
            
        // 检查是否有待保存的槽位（新游戏）
        if (PlayerPrefs.HasKey("PendingSaveSlot"))
        {
            int pendingSlot = PlayerPrefs.GetInt("PendingSaveSlot");
            PlayerPrefs.DeleteKey("PendingSaveSlot");
            PlayerPrefs.Save();
            
            // 自动保存到指定槽位
            SaveManager.Instance.SaveGame(pendingSlot, $"新游戏");
            ShowSaveMessage($"游戏已自动保存到槽位 {pendingSlot + 1}");
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
        
        // 快速保存
        if (Input.GetKeyDown(quickSaveKey))
        {
            QuickSave();
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
            
        // 恢复游戏
        Time.timeScale = 1f;
    }
    
    private void RefreshSaveSlots()
    {
        // 获取所有存档
        List<SaveData> saves = SaveManager.Instance.GetAllSaves();
        
        // 获取所有槽位按钮
        int childCount = slotContainer.childCount;
        
        for (int i = 0; i < childCount && i < saves.Count; i++)
        {
            Transform slotTransform = slotContainer.GetChild(i);
            GameObject slotObj = slotTransform.gameObject;
            
            // 获取组件
            Button slotButton = slotObj.GetComponent<Button>();
            TMP_Text slotText = slotObj.GetComponentInChildren<TMP_Text>();
            Image slotImage = slotObj.GetComponent<Image>();
            
            SaveData saveData = saves[i];
            
            if (saveData != null)
            {
                // 有存档
                if (slotText != null)
                {
                    slotText.text = $"槽位 {i + 1}\n{saveData.saveName}\n{saveData.saveTime:MM/dd HH:mm}\n[点击覆盖]";
                }
                
                if (slotImage != null)
                    slotImage.color = filledSlotColor;
            }
            else
            {
                // 空槽位
                if (slotText != null)
                {
                    slotText.text = $"槽位 {i + 1}\n[空槽位]\n[点击保存]";
                }
                
                if (slotImage != null)
                    slotImage.color = emptySlotColor;
            }
            
            // 绑定按钮事件
            if (slotButton != null)
            {
                slotButton.onClick.RemoveAllListeners();
                int slotIndex = i; // 避免闭包问题
                slotButton.onClick.AddListener(() => SaveToSlot(slotIndex));
            }
        }
    }
    
    private void SaveToSlot(int slot)
    {
        SaveManager.Instance.SaveGame(slot);
        PlayerPrefs.SetInt("LastUsedSaveSlot", slot);
        PlayerPrefs.Save();
        
        ShowSaveMessage($"游戏已保存到槽位 {slot + 1}");
        
        // 刷新显示
        RefreshSaveSlots();
        
        // 延迟关闭菜单
        Invoke(nameof(CloseSaveMenu), 1f);
    }
    
    private void QuickSave()
    {
        // 保存到最近使用的槽位或第一个槽位
        int quickSaveSlot = PlayerPrefs.GetInt("LastUsedSaveSlot", 0);
        SaveManager.Instance.SaveGame(quickSaveSlot, "快速存档");
        PlayerPrefs.SetInt("LastUsedSaveSlot", quickSaveSlot);
        
        ShowSaveMessage($"快速保存到槽位 {quickSaveSlot + 1}");
    }
    
    private void ShowSaveMessage(string message)
    {
        Debug.Log(message);
        
        // 创建临时UI显示保存信息
        GameObject messageObj = new GameObject("SaveMessage");
        messageObj.transform.SetParent(transform);
        
        RectTransform rect = messageObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.8f);
        rect.anchorMax = new Vector2(0.5f, 0.8f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(400, 60);
        
        Image bg = messageObj.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(messageObj.transform);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;
        
        TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = message;
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        
        // 3秒后销毁
        Destroy(messageObj, 3f);
    }
} 