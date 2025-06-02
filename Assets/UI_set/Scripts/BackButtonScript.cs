using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButtonScript : MonoBehaviour
{
    public string mainMenuSceneName = "MainMenuScene"; // 主菜单场景名称

    // 在Unity编辑器中可以分配一个引用，用于关闭选项菜单而不是返回主菜单
    public GameObject optionsPanel;

    // 当Back按钮被点击时调用
    public void OnBackButtonClick()
    {
        // 尝试使用OptionsMenuManager关闭选项菜单
        OptionsMenuManager optionsMenuManager = FindFirstObjectByType<OptionsMenuManager>();
        if (optionsMenuManager != null)
        {
            // 检查选项面板是否打开
            if (optionsPanel != null && optionsPanel.activeSelf)
            {
                optionsMenuManager.CloseOptionsMenu();
                return;
            }
        }
        
        // 如果分配了选项面板且它是激活的，则关闭它（后备方案）
        if (optionsPanel != null && optionsPanel.activeSelf)
        {
            optionsPanel.SetActive(false);
        }
        // 否则返回主菜单
        else
        {
            ReturnToMainMenu();
        }
    }

    // 返回主菜单场景
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // 检测Back按键按下事件
    void Update()
    {
        // 使用Back按键（一般在Android设备上）或Escape键（PC上测试用）
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackButtonClick();
        }
    }
} 