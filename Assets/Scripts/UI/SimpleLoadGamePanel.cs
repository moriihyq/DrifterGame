using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleLoadGamePanel : MonoBehaviour
{
    [Header("UI References")]
    public Button backButton;
    public Transform contentParent; // Scroll View的Content对象
    
    [Header("Slot Settings")]
    public Color emptySlotColor = Color.gray;
    public Color filledSlotColor = Color.white;
    
    private void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClick);
            
        RefreshSaveSlots();
    }
    
    private void OnEnable()
    {
        RefreshSaveSlots();
    }
    
    public void RefreshSaveSlots()
    {
        // 获取所有存档
        List<SaveData> saves = SaveManager.Instance.GetAllSaves();
        
        // 获取Content下的所有子对象（存档槽位）
        int childCount = contentParent.childCount;
        
        for (int i = 0; i < childCount && i < saves.Count; i++)
        {
            Transform slotTransform = contentParent.GetChild(i);
            GameObject slotObj = slotTransform.gameObject;
            
            // 获取槽位的组件
            Button slotButton = slotObj.GetComponent<Button>();
            TMP_Text slotText = slotObj.GetComponentInChildren<TMP_Text>();
            Image slotImage = slotObj.GetComponent<Image>();
            
            SaveData saveData = saves[i];
            
            if (saveData != null)
            {
                // 有存档
                if (slotText != null)
                {
                    slotText.text = $"槽位 {i + 1}\n{saveData.saveName}\n{saveData.saveTime:yyyy/MM/dd HH:mm}";
                }
                
                if (slotImage != null)
                    slotImage.color = filledSlotColor;
                    
                if (slotButton != null)
                {
                    // 移除所有监听器
                    slotButton.onClick.RemoveAllListeners();
                    
                    int slotIndex = i; // 避免闭包问题
                    slotButton.onClick.AddListener(() => OnSlotClick(slotIndex, true));
                }
            }
            else
            {
                // 空槽位
                if (slotText != null)
                {
                    slotText.text = $"槽位 {i + 1}\n[空槽位]";
                }
                
                if (slotImage != null)
                    slotImage.color = emptySlotColor;
                    
                if (slotButton != null)
                {
                    // 移除所有监听器
                    slotButton.onClick.RemoveAllListeners();
                    
                    int slotIndex = i;
                    slotButton.onClick.AddListener(() => OnSlotClick(slotIndex, false));
                }
            }
        }
    }
    
    private void OnSlotClick(int slotIndex, bool hasSave)
    {
        if (hasSave)
        {
            // 加载存档
            SaveManager.Instance.LoadGame(slotIndex);
        }
        else
        {
            // 空槽位，开始新游戏并保存到该槽位
            PlayerPrefs.SetInt("PendingSaveSlot", slotIndex);
            PlayerPrefs.Save();
            UnityEngine.SceneManagement.SceneManager.LoadScene("5.5 map");
        }
    }
    
    private void OnBackButtonClick()
    {
        // 隐藏自己
        gameObject.SetActive(false);
        
        // 显示主菜单
        GameObject mainUI = GameObject.Find("MainUIPanel");
        if (mainUI != null)
            mainUI.SetActive(true);
    }
} 